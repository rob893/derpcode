# Quality Research — 2026-03-14

## Executive Summary
DerpCode’s current baseline is stable — `dotnet test` passed 451 backend tests, and the UI’s existing lint, test, and build commands all passed — but the codebase still has several structural quality risks that will get more expensive to change over time. The biggest issues are a broken Azure bootstrap path for the current .NET target, an overgrown `AuthController` with duplicated OAuth logic and swallowed exceptions, large untested controller/component layers, and duplicated pagination and authorization patterns spread across the backend. The platform has solid architectural conventions overall, but those conventions are applied inconsistently in several high-churn areas.

## Previous Plan Status
No previous plan exists. This is the initial quality audit.

## Findings

### 1. Azure bootstrap targets the wrong .NET runtime
- **Description:** The checked-in Azure bootstrap path is internally inconsistent. `cloud-init.sh` installs `dotnet-sdk-8.0`, while the API project targets `net10.0`, and the Bicep template still provisions Ubuntu 20.04. Fresh infrastructure created from the repository is therefore likely to fail during restore/build or require undocumented manual fixes.
- **Location:** `CI/Azure/cloud-init.sh:25-30`; `DerpCode.API/DerpCode.API.csproj:2-4`; `CI/Azure/infrastructure.bicep:7-12`
- **Impact:** Critical
- **Effort:** Low
- **Dependencies:** Confirm the supported Ubuntu image and package source for .NET 10 in the Azure deployment path.
- **Breaking Changes:** Yes
- **Recommendation:** Upgrade the bootstrap script and VM image together, then validate the end-to-end Azure deployment flow from a clean environment.

### 2. AuthController swallows unexpected OAuth failures and has no logger
- **Description:** The Google and GitHub login endpoints both end in bare `catch` blocks that discard all exception details and return generic 500 responses. `AuthController` also does not inject `ILogger<AuthController>`, so the most security-sensitive controller in the API cannot emit structured diagnostics when external login fails.
- **Location:** `DerpCode.API/Controllers/V1/AuthController.cs:43-59`; `DerpCode.API/Controllers/V1/AuthController.cs:264-267`; `DerpCode.API/Controllers/V1/AuthController.cs:384-387`
- **Impact:** Critical
- **Effort:** Low
- **Dependencies:** None.
- **Breaking Changes:** No
- **Recommendation:** Inject `ILogger<AuthController>`, replace both bare catches with `catch (Exception ex)`, and log provider-specific failure details before returning sanitized responses.

### 3. AuthController duplicates OAuth business logic instead of delegating to a service
- **Description:** `LoginGoogleAsync` and `LoginGitHubAsync` implement nearly identical controller-level workflows: exchange code, load linked account, fall back to email lookup, link or create a user, issue tokens, and return a response. The callback actions are duplicated too. This bypasses the service-result pattern used elsewhere in the API and makes AuthController both oversized and hard to test.
- **Location:** `DerpCode.API/Controllers/V1/AuthController.cs:169-268`; `DerpCode.API/Controllers/V1/AuthController.cs:283-388`; `DerpCode.API/Controllers/V1/AuthController.cs:400-441`
- **Impact:** High
- **Effort:** High
- **Dependencies:** Introduce an authentication service abstraction for external providers.
- **Breaking Changes:** No
- **Recommendation:** Extract external-login orchestration into a dedicated service plus provider adapters so the controller becomes a thin HTTP mapper like the rest of the API.

### 4. ProblemSubmissionService is a god class with duplicated XP result construction
- **Description:** `ProblemSubmissionService` takes ten constructor dependencies and mixes submission execution, authorization, progress mutation, XP calculation, cooldown handling, cache invalidation, and experience-event persistence. The XP path also duplicates large `XpResult` object creation blocks and inline `ExperienceEvent` construction, which increases change risk in one of the platform’s core progression flows.
- **Location:** `DerpCode.API/Services/Domain/ProblemSubmissionService.cs:59-80`; `DerpCode.API/Services/Domain/ProblemSubmissionService.cs:280-436`; `DerpCode.API/Services/Domain/ProblemSubmissionService.cs:338-348`; `DerpCode.API/Services/Domain/ProblemSubmissionService.cs:356-367`; `DerpCode.API/Services/Domain/ProblemSubmissionService.cs:384-395`; `DerpCode.API/Services/Domain/ProblemSubmissionService.cs:425-436`
- **Impact:** High
- **Effort:** High
- **Dependencies:** Extract an XP/cooldown service or helper layer and move event creation behind a focused abstraction.
- **Breaking Changes:** No
- **Recommendation:** Split XP awarding and progression updates into a dedicated service, then centralize `XpResult` and event creation behind factory helpers.

### 5. The HTTP layer and UI component layer have no meaningful regression coverage
- **Description:** The repository has good backend service-test coverage in places, but there is no controller test suite and no React component test coverage for the main UI surface. That leaves authorization attributes, response mapping, cookie behavior, conditional rendering, modal flows, and error states unverified even though they are user-facing and change frequently.
- **Location:** `DerpCode.API.Tests/` (no `Controllers/` test directory); `derpcode-ui/src/components/` (no `*.test.tsx` files); `DerpCode.API/Controllers/V1/AuthController.cs`; `derpcode-ui/src/components/ProblemList.tsx`; `derpcode-ui/src/components/ProblemView/ProblemView.tsx`
- **Impact:** Critical
- **Effort:** Very High
- **Dependencies:** Controller coverage will likely need `WebApplicationFactory`; component coverage will need React Testing Library around existing flows.
- **Breaking Changes:** No
- **Recommendation:** Prioritize tests for `AuthController`, submission endpoints, `ProblemList`, and `ProblemView`, then add targeted coverage for high-risk account-management components.

### 6. Three major frontend components are already monoliths
- **Description:** `CreateEditProblem`, `AccountSection`, and `ProblemList` each exceed 690 lines and combine state orchestration, mutation logic, validation, and large rendering trees in single files. `AccountSection` alone manages many independent modal/form states, while `ProblemList` mixes filters, pagination state, and list rendering, and `CreateEditProblem` mixes editing, validation, driver editing, and result rendering.
- **Location:** `derpcode-ui/src/components/CreateEditProblem.tsx:1-786`; `derpcode-ui/src/components/AccountSection.tsx:45-77`; `derpcode-ui/src/components/ProblemList.tsx:27-210`
- **Impact:** High
- **Effort:** High
- **Dependencies:** Decide on extraction boundaries for child components and shared hooks before refactoring.
- **Breaking Changes:** No
- **Recommendation:** Split these files into focused subcomponents and hooks, starting with modal extraction in `AccountSection`, pagination/filter hooks in `ProblemList`, and editor/result sections in `CreateEditProblem`.

### 7. Production TypeScript still relies heavily on `any` and unsafe assertions
- **Description:** Core frontend types still use `any` for problem input/output payloads, callback parameters, caught errors, and validation renderers. This weakens static checking around exactly the kinds of dynamic data that most need guardrails and encourages unsafe `as`-based coercion instead of proper narrowing.
- **Location:** `derpcode-ui/src/types/models.ts:23-35`; `derpcode-ui/src/types/models.ts:130-141`; `derpcode-ui/src/types/models.ts:208-215`; `derpcode-ui/src/components/ProblemList.tsx:148-176`; `derpcode-ui/src/components/AccountSection.tsx:95-97`; `derpcode-ui/src/hooks/api.ts:71-79`; `derpcode-ui/src/components/CreateEditProblem.tsx:788-793`
- **Impact:** Medium
- **Effort:** Medium
- **Dependencies:** Some callback types depend on HeroUI/TanStack types and may require small wrapper types.
- **Breaking Changes:** No
- **Recommendation:** Replace `any` with `unknown`, library-specific types, or JSON-safe domain types, and use narrowing helpers instead of unguarded casts.

### 8. Cursor pagination is duplicated across two large extension files
- **Description:** Cursor pagination logic exists in two separate, very large implementations: one for `IEnumerable` and one for EF Core `IQueryable`. Both files define similar pipelines, overload families, and ordering concepts, so behavioral changes to pagination semantics must be replicated by hand in two of the largest backend source files.
- **Location:** `DerpCode.API/Extensions/CollectionExtensions.cs:12-697`; `DerpCode.API/Extensions/EntityFrameworkExtensions.cs:18-526`
- **Impact:** High
- **Effort:** High
- **Dependencies:** A shared pagination pipeline or options object should be designed before consolidation.
- **Breaking Changes:** No
- **Recommendation:** Extract the shared pagination algorithm into a reusable internal pipeline and keep the source-specific `IEnumerable`/`IQueryable` adapters thin.

### 9. ProblemService mixes unrelated responsibilities and duplicates domain logic
- **Description:** `ProblemService` handles querying, creation, updates, cloning, patching, validation, caching, and also exposes static comparison helpers used by seeding logic. It also duplicates tag-reconciliation code between create/update paths and repeats the `OrderBy` pagination switch in multiple query methods, all of which make the service harder to reason about and more brittle to extend.
- **Location:** `DerpCode.API/Services/Domain/ProblemService.cs:71-132`; `DerpCode.API/Services/Domain/ProblemService.cs:200-205`; `DerpCode.API/Services/Domain/ProblemService.cs:304-309`; `DerpCode.API/Services/Domain/ProblemService.cs:468-554`
- **Impact:** Medium
- **Effort:** Medium
- **Dependencies:** Moving comparison helpers will require updating `ProblemSeedDataService` references.
- **Breaking Changes:** No
- **Recommendation:** Move comparison utilities out of `ProblemService`, extract shared tag-reconciliation helpers, and consolidate pagination-order selection into one private path.

### 10. hooks/api.ts is a monolithic integration file with poor domain boundaries
- **Description:** The frontend’s React Query hooks for problems, submissions, tags, comments, favorites, preferences, and auth-adjacent flows are concentrated in one large file. This mirrors the same “everything in one file” pattern seen in `services/api.ts`, increasing merge-conflict risk and making domain ownership unclear.
- **Location:** `derpcode-ui/src/hooks/api.ts:1-467`; `derpcode-ui/src/services/api.ts:1-262`
- **Impact:** Medium
- **Effort:** Medium
- **Dependencies:** Existing imports should be preserved through barrel exports to avoid churn.
- **Breaking Changes:** No
- **Recommendation:** Split hooks and service clients by domain, keep `api.ts` as a barrel, and centralize query keys in a dedicated module.

### 11. Owner-or-admin authorization checks are copied across multiple domain services
- **Description:** The same `currentUserService.UserId != userId && !currentUserService.IsAdmin` guard pattern appears repeatedly across user-facing domain services. That duplication makes policy changes expensive and increases the odds of drift when roles or exception rules change later.
- **Location:** `DerpCode.API/Services/Domain/UserService.cs:66-67`; `DerpCode.API/Services/Domain/UserService.cs:86-87`; `DerpCode.API/Services/Domain/UserFavoriteService.cs:51-52`; `DerpCode.API/Services/Domain/UserPreferencesServices.cs:43-46`; `DerpCode.API/Services/Domain/ProblemService.cs:142-145`
- **Impact:** Medium
- **Effort:** Low
- **Dependencies:** None.
- **Breaking Changes:** No
- **Recommendation:** Centralize these guards behind a reusable authorization helper or `ICurrentUserService` method so policy changes happen in one place.

### 12. Progression math is important but only lightly tested
- **Description:** `ProgressionService` drives XP awards, cycle timing, and level progression, but its dedicated test file is only 55 lines long and covers a narrow subset of the decision space. Boundary behavior around difficulty tiers, minimum-factor clamping, time extremes, hint penalties, and level transitions is under-specified relative to the gameplay importance of this logic.
- **Location:** `DerpCode.API/Services/Domain/ProgressionService.cs:18-69`; `DerpCode.API.Tests/Services/Domain/ProgressionServiceTests.cs:8-55`
- **Impact:** Medium
- **Effort:** Medium
- **Dependencies:** None.
- **Breaking Changes:** No
- **Recommendation:** Add table-driven tests for each difficulty, min-factor floor, cycle edge, and level-boundary scenario before evolving the progression system further.

### 13. Some user-input and seed paths still prefer silence over explicit error handling
- **Description:** `CreateEditProblem` swallows invalid JSON input errors in two textareas, making bad input invisible to the user, and `DriverTemplateData` ships a seeded C# template with `#pragma warning disable CS8602` baked directly into generated starter code. These patterns normalize hiding problems instead of surfacing them clearly.
- **Location:** `derpcode-ui/src/components/CreateEditProblem.tsx:417-445`; `DerpCode.API/Data/SeedData/DriverTemplateData.cs:14-16`
- **Impact:** Medium
- **Effort:** Low
- **Dependencies:** None.
- **Breaking Changes:** No
- **Recommendation:** Surface validation feedback for malformed JSON in the UI and remove the blanket warning suppression from the seeded driver template unless it is narrowly justified.

### 14. Debug leftovers and naming inconsistencies are still shipping in production code
- **Description:** The UI still contains a stray `//asdfsadf` comment, production `console.log` calls, an empty `Extensions/CursorConverters.cs` file alongside the real implementation in `Utilities/CursorConverters.cs`, and the unusually plural `UserPreferencesServices` naming. None of these are catastrophic alone, but together they show weak cleanup discipline and inconsistent naming standards.
- **Location:** `derpcode-ui/src/components/ProblemList.tsx:35-38`; `derpcode-ui/src/contexts/AuthContext.tsx:167`; `derpcode-ui/src/hooks/usePWA.ts:37-40`; `derpcode-ui/src/utils/localStorageUtils.ts:202`; `DerpCode.API/Extensions/CursorConverters.cs`; `DerpCode.API/Services/Domain/UserPreferencesServices.cs:19-33`
- **Impact:** Low
- **Effort:** Low
- **Dependencies:** Renaming the preferences service requires updating interface, registrations, and references together.
- **Breaking Changes:** No
- **Recommendation:** Remove debug leftovers, delete dead files, and normalize service naming to match the rest of the codebase before these inconsistencies spread.
