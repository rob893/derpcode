# DerpCode (repo guidance for AI coding agents)

DerpCode is a LeetCode-style algorithm practice platform (snarky/gamified). It’s a .NET API + React SPA; the API also executes user code in Docker containers.

## Repo layout

- `DerpCode.API/`: .NET 10 Web API (EF Core + Identity + Postgres)
- `DerpCode.API.Tests/`: xUnit tests (Moq)
- `derpcode-ui/`: React + Vite + TypeScript + Tailwind v4 + PWA
- `Docker/`: per-language runners/images used by code execution
- `CI/Azure/`: infra (Bicep, cloud-init)

## Daily workflows

- Build API: `dotnet build DerpCode.API/DerpCode.API.csproj`
- Run API: `dotnet run --project DerpCode.API/DerpCode.API.csproj`
- Hot reload: `dotnet watch run --project DerpCode.API/DerpCode.API.csproj`
- Tests: `dotnet test`

## API architecture & conventions

- Startup is extension-driven: `DerpCode.API/Program.cs` wires everything via `ApplicationStartup/ServiceCollectionExtensions/*`.
- Config precedence: `appsettings.json` → env-specific JSON → `appsettings.Local.json` → (non-Development) Azure Key Vault.
- Global auth by default: controllers require auth unless `[AllowAnonymous]` is used (see `ControllerServiceCollectionExtensions`).
- API versioning is URL-segment based: `/api/v1/...` and `/api/v2/...`.
- Errors are standardized to `ProblemDetailsWithErrors` and correlation IDs are returned on every response.
  - Correlation header name: `X-Correlation-Id` (see `CorrelationIdMiddleware` + `Constants/AppHeaderNames.cs`).
- Prefer the service-result pattern for domain ops:
  - Services return `Result<T>` (see `Core/Result.cs`)
  - Controllers map failures via `ServiceControllerBase.HandleServiceFailureResult`.
- All async methods should accept and pass through `CancellationToken` (typically `HttpContext.RequestAborted`).

## Data & seeding (important)

- EF Core DbContext: `Data/DataContext.cs` (Postgres + jsonb columns, enum conversions).
- On app startup, the seeder runs `SyncProblemsFromFolderAsync()` to keep DB problems in sync with the folder-backed seed data.
  - Seed data lives under `DerpCode.API/Data/SeedData/Problems/` and is copied to output by the csproj.
- There is a guarded “seeder” CLI mode in `Program.cs` using args: `seeder` plus optional flags `drop`, `migrate`, `clear`, `seed` and `--password <...>`.

## Code execution (Docker)

- User code runs inside Docker via `Services/Core/CodeExecutionService.cs` using `Docker.DotNet`.
- The API bind-mounts a temp submission dir into the container at `/home/runner/submission` and expects output files like `results.json`, `output.txt`, `error.txt`.
- Adding/changing a language usually involves updating the Docker image + driver templates under `Docker/` and the problem’s `ProblemDriver.Image`/driver code.

## UI ↔ API integration patterns

- API base URL comes from `VITE_DERPCODE_API_BASE_URL`.
- `derpcode-ui/src/services/axiosConfig.ts`:
  - Adds `X-Correlation-Id` for every request.
  - Uses a refresh-token flow on 401 when `x-token-expired` is present.
  - Uses double-submit CSRF (cookie `csrf_token` + header `X-CSRF-Token`) and sends cookies (`withCredentials: true`).
- API calls are centralized in `derpcode-ui/src/services/api.ts` (typed wrappers around `/api/v1/...`).

## UI styling (typography + color scheme)

- Dark mode is the default: `derpcode-ui/index.html` sets `<html class="dark">` and Tailwind uses `darkMode: 'class'`.
- Prefer HeroUI theme tokens over hard-coded hex values:
  - Page base: `bg-background text-foreground` (see `derpcode-ui/src/App.tsx`, `derpcode-ui/src/layouts/AppLayout.tsx`).
  - Surfaces: `bg-content1..4`, `bg-default-*`, `text-default-*`, `border-default-*`.
  - Accents: `primary` (green) and `secondary` (purple) from `derpcode-ui/tailwind.config.js`.
- Brand palette lives in `derpcode-ui/tailwind.config.js` as `brand-green-*` and `brand-purple-*` (HeroUI `primary`/`secondary` map to these).
- Typography:
  - Global font stack is set in `derpcode-ui/src/index.css` (Inter + system fallbacks).
  - For rich text/markdown, use `prose ... dark:prose-invert` (see `derpcode-ui/src/components/MarkdownRenderer.tsx`).
  - Code styling relies on Highlight.js + KaTeX CSS imports in `derpcode-ui/src/index.css`.

## Style rules already in this repo

- C#: follow root `.editorconfig`; use `this.` for instance members; no `_` prefix fields; no unused usings; blank line between members; XML docs for public members (`<inheritdoc/>` where applicable).
- TypeScript: prefer method syntax `func(): Type {}` over arrow-signature style.

## Azure note

When working on Azure (Bicep/VM/Key Vault/App Insights), use the Azure best-practices tooling first.
