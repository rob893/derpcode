# DerpCode

LeetCode-style algorithm practice platform (snarky/gamified). .NET 10 API + React SPA; the API executes user code in Docker containers.

## Repo Layout

- `DerpCode.API/` — .NET 10 Web API (EF Core + Identity + Postgres)
- `DerpCode.API.Tests/` — xUnit tests (Moq), mirrored folder structure
- `derpcode-ui/` — React + Vite + TypeScript + Tailwind v4 + PWA
- `Docker/` — per-language runner images (CSharp, Java, JavaScript, Python, Rust, TypeScript)
- `CI/Azure/` — Bicep infra, cloud-init, deployment automation

## Commands

Run from repo root unless noted.

**API:** `dotnet build DerpCode.API/DerpCode.API.csproj` · `dotnet test` · `npm run start` (hot reload) · `npm run build-api`

**UI** (from `derpcode-ui/`): `npm i` · `npm run dev` · `npm run lint` · `npm run test` (Jest) · `npm run test:e2e` (Playwright) · `npm run build`

## API Architecture

- Extension-driven startup: `Program.cs` → `ApplicationStartup/ServiceCollectionExtensions/*`.
- Config: `appsettings.json` → env-specific → `appsettings.Local.json` → Azure Key Vault (non-Dev).
- Global auth; use `[AllowAnonymous]` to opt out. URL-segment versioning: `/api/v1/…`, `/api/v2/…`.
- Errors: `ProblemDetailsWithErrors` + `X-Correlation-Id` on every response.
- Service-result pattern: services return `Result<T>`; controllers map failures via `ServiceControllerBase.HandleServiceFailureResult`.
- All async methods must accept and pass `CancellationToken`.

## Data & Seeding

- DbContext: `Data/DataContext.cs` (Postgres, jsonb columns, enum conversions).
- Startup seeder runs `SyncProblemsFromFolderAsync()` from `DerpCode.API/Data/SeedData/Problems/`.
- CLI seeder mode: args `seeder` + flags `drop`, `migrate`, `clear`, `seed`, `--password <…>`.

## Code Execution (Docker)

- `Services/Core/CodeExecutionService.cs` via `Docker.DotNet`.
- Bind-mounts temp dir → `/home/runner/submission`; expects `results.json`, `output.txt`, `error.txt`.
- New language = Docker image + driver templates under `Docker/` + `ProblemDriver.Image`.

## UI ↔ API Integration

- Base URL: `VITE_DERPCODE_API_BASE_URL`.
- `axiosConfig.ts`: `X-Correlation-Id`, refresh-token on 401 (`x-token-expired`), double-submit CSRF, `withCredentials: true`.
- API calls in `services/api.ts`, **always** cached/managed via TanStack React Query in `hooks/api.ts`. No direct API calls outside React Query.

## UI Styling

- Dark mode default (`<html class="dark">`), `darkMode: 'class'`.
- Use HeroUI theme tokens: `bg-background`, `text-foreground`, `bg-content1..4`, `bg-default-*`, `primary` (green), `secondary` (purple).
- Brand palette in `tailwind.config.js` (`brand-green-*`, `brand-purple-*`).
- Typography: Inter font stack (`index.css`), `prose dark:prose-invert` for markdown, Highlight.js + KaTeX for code.

## Coding Style

- **C#:** Follow `.editorconfig`. `PascalCase` types/members, `I`-prefixed interfaces, `camelCase` fields (no `_` prefix). `this.` for instance members. XML docs on public members (`<inheritdoc/>` where applicable). Non-entity POCOs → `record` with `get; init;`. Repos extend `Repository<…>` / `IRepository<…>`.
- **TypeScript:** ESLint via `eslint.config.js`, format with `npm run prettier`. Prefer method syntax `func(): Type {}` over arrow signatures.

## Testing

- **Backend:** xUnit + Moq in `DerpCode.API.Tests/` (mirrored folders).
- **UI unit:** Jest (`*.test.ts(x)` / `*.spec.ts(x)` under `src/`).
- **UI e2e:** Playwright in `derpcode-ui/e2e/`. Use Playwright MCP for local testing; test credentials in `appsettings.Local.json`.
- Include tests for new behavior and regressions.

## Commits & PRs

- Conventional Commits: `feat:`, `fix:`, `chore:`, `test:`. One logical change per commit.
- PRs: clear summary, linked issue, commands run, screenshots for UI changes.
- Flag migration/config impacts.

## Azure

Use Azure best-practices tooling when working on Bicep/VM/Key Vault/App Insights.
