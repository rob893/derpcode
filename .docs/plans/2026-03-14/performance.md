# Performance Research — 2026-03-14

## Executive Summary
The backend is generally structured well, but several hot paths still scale linearly because they materialize full datasets in memory before filtering or pagination. The highest-risk issues are unbounded Docker executions, XP-history queries that load every row for a user, missing supporting indexes on frequently filtered tables, and problem-list endpoints that do most of their work outside the database. On the frontend, the production build shows a very large initial payload, driven by eager route loading, Monaco/Shiki-related dependencies, and missing chunk-splitting.

## Previous Plan Status
No previous plan exists. This is the initial performance audit.

## Findings

### 1. Docker code execution has no hard timeout
- **Description:** `CodeExecutionService` creates and starts a container, then waits on `WaitContainerAsync` using only the incoming request cancellation token. A user submission that never terminates can keep the container alive until the request is aborted upstream, which risks CPU starvation and stuck runner capacity.
- **Location:** `DerpCode.API\Services\Core\CodeExecutionService.cs:106-124`
- **Impact:** Critical
- **Effort:** Medium
- **Dependencies:** None
- **Breaking Changes:** No
- **Recommendation:** Add a linked `CancellationTokenSource` with a configurable hard timeout, kill timed-out containers explicitly, and return a deterministic timeout result to the caller.

### 2. XP history loads all events into memory before sorting and pagination
- **Description:** `GetXpHistoryAsync` fetches every `ExperienceEvent` row for the current user, materializes the full list, sorts it in memory, maps it, and only then builds the cursor response. This is O(n) per request and will degrade steadily as active users accumulate more XP history.
- **Location:** `DerpCode.API\Services\Domain\ProgressionQueryService.cs:38-62`
- **Impact:** High
- **Effort:** Medium
- **Dependencies:** Best addressed alongside Finding 4
- **Breaking Changes:** No
- **Recommendation:** Add a repository method that filters, sorts, and cursor-paginates `ExperienceEvent` rows in SQL so the API only reads the current page.

### 3. Problem submission queries are missing a composite index for their hottest filters
- **Description:** personalized problem projections compute `LastSubmissionDate` and `LastPassedSubmissionDate` by filtering `ProblemSubmissions` on `UserId`, `Pass`, and `CreatedAt`, and submission queries also filter by `ProblemId` and `UserId`. `ProblemSubmission` currently has column type configuration only, with no explicit supporting index for those predicates.
- **Location:** `DerpCode.API\Data\Repositories\ProblemRepository.cs:33-38`; `DerpCode.API\Data\Repositories\ProblemSubmissionRepository.cs:28-44`; `DerpCode.API\Data\DataContext.cs:133-137`
- **Impact:** High
- **Effort:** Low
- **Dependencies:** Requires an EF Core migration
- **Breaking Changes:** No
- **Recommendation:** Add a composite index such as `(UserId, ProblemId, Pass, CreatedAt)` and validate the generated SQL plans for the personalized and submission-history endpoints.

### 4. XP-history lookups do not have an index on `ExperienceEvent.UserId`
- **Description:** `ExperienceEvent` only defines a unique index for `IdempotencyKey`, while the progression history path filters by `UserId`. Without a user-oriented index, the table will require broader scans as event volume grows.
- **Location:** `DerpCode.API\Services\Domain\ProgressionQueryService.cs:49-58`; `DerpCode.API\Data\DataContext.cs:152-159`
- **Impact:** High
- **Effort:** Low
- **Dependencies:** Requires an EF Core migration
- **Breaking Changes:** No
- **Recommendation:** Add an index on `UserId` or a composite `(UserId, CreatedAt)` index to support both filtering and descending-history reads.

### 5. Article comment queries are under-indexed for pagination and thread filtering
- **Description:** comment lookups filter on `ArticleId`, `ParentCommentId`, and `QuotedCommentId`, then paginate by `CreatedAt` or `UpVotes`. The model configuration defines relationships but no explicit composite indexes tailored to those access patterns.
- **Location:** `DerpCode.API\Data\Repositories\ArticleRepository.cs:17-67`; `DerpCode.API\Data\DataContext.cs:204-215`
- **Impact:** Medium
- **Effort:** Low
- **Dependencies:** Requires an EF Core migration
- **Breaking Changes:** No
- **Recommendation:** Add composite indexes for threaded comment reads, starting with `(ArticleId, ParentCommentId)` and, if voting-based pagination is important, a supporting index for article-scoped vote ordering.

### 6. Problem list endpoints filter and paginate in memory instead of in SQL
- **Description:** both public and personalized problem list paths fetch cached collections, apply `Where` filters in process, and then build cursor pages from those in-memory enumerables. That keeps database round-trips low at today’s scale, but it makes request cost proportional to the full problem catalog and duplicates per-user list work for personalized views.
- **Location:** `DerpCode.API\Services\Domain\ProblemService.cs:71-132`; `DerpCode.API\Services\Domain\ProblemService.cs:663-748`; `DerpCode.API\Data\Repositories\ProblemRepository.cs:15-42`
- **Impact:** High
- **Effort:** High
- **Dependencies:** Works best with Findings 3 and 7
- **Breaking Changes:** No
- **Recommendation:** Move filtering, sorting, and cursor pagination into repository queries so each request reads only the requested window; reserve caching for small reference data or per-entity detail documents.

### 7. The shared problem cache stores more data than limited endpoints need
- **Description:** `GetProblemsFromCacheAsync` loads problems with explanation articles, tags, and drivers, then stores the full entity graph for a day. Limited list endpoints only need a fraction of that payload, so large markdown bodies and driver content occupy memory even when the caller only needs names, difficulties, and tags.
- **Location:** `DerpCode.API\Services\Domain\ProblemService.cs:663-678`
- **Impact:** Medium
- **Effort:** Medium
- **Dependencies:** Easier after Finding 6
- **Breaking Changes:** No
- **Recommendation:** Split the cache into lightweight list-view projections and full problem-detail entries, or stop caching large article bodies on list-oriented paths.

### 8. `UserRepository` eagerly includes multiple navigation graphs on common lookups
- **Description:** the repository’s default include graph always loads roles, linked accounts, and progress. That means simple username/email lookups and many authorization-oriented reads bring back more relational data than they need, increasing query size and object materialization cost.
- **Location:** `DerpCode.API\Data\Repositories\UserRepository.cs:70-116`; `DerpCode.API\Data\Repositories\UserRepository.cs:216-223`
- **Impact:** Medium
- **Effort:** Medium
- **Dependencies:** None
- **Breaking Changes:** No
- **Recommendation:** shrink the default include set to the minimum required for auth/authorization and add explicit, task-specific query methods for paths that truly need linked accounts or progress data.

### 9. Favorite-state lookup fetches the user’s entire favorites list to answer a boolean
- **Description:** the favorites API returns the full list of a user’s favorite problems, and `ProblemView` loads that list just to compute `isFavorited` for one problem via `.some(...)`. As a user favorites more problems, every problem page visit transfers and caches an increasingly large payload for a single yes/no decision.
- **Location:** `DerpCode.API\Controllers\V1\UserFavoritesController.cs:31-42`; `DerpCode.API\Services\Domain\UserFavoriteService.cs:49-60`; `DerpCode.API\Data\Repositories\UserRepository.cs:163-170`; `derpcode-ui\src\hooks\api.ts:111-119`; `derpcode-ui\src\components\ProblemView\ProblemView.tsx:46-50`
- **Impact:** Medium
- **Effort:** Medium
- **Dependencies:** None
- **Breaking Changes:** No
- **Recommendation:** expose a targeted favorite-state endpoint or include `isFavorited` directly in the problem payload used by `ProblemView`, while keeping the full favorites list only for pages that actually render the list.

### 10. Response compression is not configured for API payloads
- **Description:** the API startup pipeline registers memory cache, auth, database, HTTP client, and Docker services, but there is no `AddResponseCompression` service registration and no `UseResponseCompression` middleware. JSON-heavy responses such as problem detail, personalized lists, and comments are therefore sent uncompressed over HTTPS.
- **Location:** `DerpCode.API\Program.cs:62-77`; `DerpCode.API\Program.cs:134-151`
- **Impact:** High
- **Effort:** Low
- **Dependencies:** None
- **Breaking Changes:** No
- **Recommendation:** enable Brotli and Gzip response compression for HTTPS responses, and place the middleware early enough in the pipeline to cover controller responses.

### 11. The Monaco flame effect drives frequent React re-renders around the editor
- **Description:** `CodeEditor` stores flame particles in component state, adds particles while typing, and updates them again inside `requestAnimationFrame`. That animation loop repeatedly calls `setFlames`, which forces React work near the Monaco host component during active editing.
- **Location:** `derpcode-ui\src\components\CodeEditor.tsx:58-61`; `derpcode-ui\src\components\CodeEditor.tsx:82-107`; `derpcode-ui\src\components\CodeEditor.tsx:109-180`; `derpcode-ui\src\components\CodeEditor.tsx:219-248`
- **Impact:** High
- **Effort:** Medium
- **Dependencies:** None
- **Breaking Changes:** No
- **Recommendation:** move flame animation out of React state into a memoized overlay or imperative canvas/DOM layer so typing does not trigger animation-driven component churn.

### 12. Routes are eagerly imported and the build does not split vendor code aggressively
- **Description:** `App.tsx` statically imports every page, including heavy editor/admin flows, and the Vite build config does not define manual chunk boundaries. The production build emitted a ~2.3 MB main JS bundle plus many large dependency chunks, which increases download, parse, and startup cost even for lightweight routes.
- **Location:** `derpcode-ui\src\App.tsx:1-18`; `derpcode-ui\src\App.tsx:22-102`; `derpcode-ui\vite.config.ts:74-88`
- **Impact:** High
- **Effort:** Medium
- **Dependencies:** Pairs well with Finding 13
- **Breaking Changes:** No
- **Recommendation:** convert route components to `React.lazy`/`Suspense`, isolate editor/admin paths into separate chunks, and add `manualChunks` for major dependency groups.

### 13. The frontend ships overlapping syntax-highlighting stacks
- **Description:** the UI depends on Monaco, Shiki, `@shikijs/monaco`, and `rehype-highlight`. Monaco already provides syntax highlighting for the editor, so shipping multiple highlight engines and language/theme assets inflates bundle size and parse time without equivalent runtime benefit.
- **Location:** `derpcode-ui\package.json:18-43`; `derpcode-ui\src\components\CodeEditor.tsx:2`; `derpcode-ui\src\components\CodeEditor.tsx:5-6`; `derpcode-ui\src\components\CodeEditor.tsx:40-47`; `derpcode-ui\src\components\CodeEditor.tsx:194-198`; `derpcode-ui\src\components\MarkdownRenderer.tsx:1-5`; `derpcode-ui\src\components\MarkdownRenderer.tsx:15-17`
- **Impact:** High
- **Effort:** Medium
- **Dependencies:** Pairs well with Finding 12
- **Breaking Changes:** No
- **Recommendation:** standardize on one highlighting strategy per surface, lazy-load markdown-only highlighters, and strongly consider removing Shiki from the Monaco path unless its visual fidelity is mission-critical.

### 14. Tag loading uses a sequential waterfall loop
- **Description:** `useAllTags` issues paginated requests inside a `while` loop, waiting for each page before requesting the next. That is acceptable for tiny tag sets, but it still creates avoidable round trips on the main problem-list screen and scales poorly if tags grow.
- **Location:** `derpcode-ui\src\hooks\api.ts:508-529`; `derpcode-ui\src\components\ProblemList.tsx:53-56`; `derpcode-ui\src\components\ProblemList.tsx:97-98`
- **Impact:** Medium
- **Effort:** Low
- **Dependencies:** None
- **Breaking Changes:** No
- **Recommendation:** request a single large page for tags, or add a backend `all tags` endpoint backed by the existing server-side tag cache.

### 15. React Query devtools are included in production startup
- **Description:** `main.tsx` imports and renders `ReactQueryDevtools` unconditionally. Even with the panel closed, it still adds extra code and runtime work to production boot for a debugging aid that only helps in development.
- **Location:** `derpcode-ui\src\main.tsx:4-5`; `derpcode-ui\src\main.tsx:30-37`
- **Impact:** Low
- **Effort:** Low
- **Dependencies:** None
- **Breaking Changes:** No
- **Recommendation:** guard the devtools behind `import.meta.env.DEV` and load them dynamically only in development builds.

### 16. Docker runner images are pulled on demand instead of being warmed at startup
- **Description:** Docker client registration only creates a client; there is no startup warm-up for runner images. If a language image is absent or outdated on the host, the first submission for that language pays the full image-pull latency, which can turn a normal execution into a multi-second or multi-minute cold start.
- **Location:** `DerpCode.API\ApplicationStartup\ServiceCollectionExtensions\DockerServiceCollectionExtensions.cs:9-19`; `DerpCode.API\Services\Core\CodeExecutionService.cs:115-121`
- **Impact:** Medium
- **Effort:** Medium
- **Dependencies:** None
- **Breaking Changes:** No
- **Recommendation:** add a hosted startup task that validates/pulls the configured runner images ahead of user traffic and records readiness in health checks.

### 17. `UserPreferences` change tracking serializes JSON repeatedly
- **Description:** the custom `ValueComparer<Preferences>` compares and hashes preference objects by serializing them to JSON. That adds extra allocations and repeated string work on each tracked save involving `UserPreferences`, which is small today but avoidable.
- **Location:** `DerpCode.API\Data\DataContext.cs:78-81`
- **Impact:** Low
- **Effort:** Low
- **Dependencies:** None
- **Breaking Changes:** No
- **Recommendation:** replace JSON-string comparison with property-based equality or an explicit immutable value object that can be compared without repeated serialization.


