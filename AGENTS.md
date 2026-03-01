# Repository Guidelines

## Project Structure & Module Organization
- `DerpCode.API/`: ASP.NET Core backend (controllers, services, data layer, settings, seed data).
- `DerpCode.API.Tests/`: xUnit test project organized by backend domains (`Services/*`, `Extensions/*`).
- `derpcode-ui/`: React + TypeScript + Vite frontend (`src/` for app code, `e2e/` for Playwright).
- `Docker/`: language-specific code execution images (`CSharp`, `Java`, `JavaScript`, `Python`, `Rust`, `TypeScript`).
- `CI/` and `.github/workflows/`: service unit file, Azure infra, and deployment automation.

## Build, Test, and Development Commands
Run from repo root unless noted.

- `dotnet restore && dotnet build -c Release`: restore and build solution.
- `dotnet test -c Release`: run backend tests (`DerpCode.API.Tests`).
- `npm run start`: run API with hot reload (`dotnet watch run` in `DerpCode.API`).
- `npm run build-api`: build API project quickly.

Frontend (`derpcode-ui/`):
- `npm i`: install dependencies.
- `npm run dev`: start Vite dev server.
- `npm run lint`: run ESLint checks.
- `npm run test`: run Jest unit tests.
- `npm run test:e2e`: run Playwright end-to-end tests.
- `npm run build`: type-check and build production assets.

## Coding Style & Naming Conventions
- Follow `.editorconfig` for C#: 4-space indentation, braces required, `this.` qualification required, analyzers enforced as errors.
- C# naming rules: `PascalCase` for types/members, interfaces prefixed with `I`, fields in `camelCase` with no underscore prefix.
- Frontend code follows ESLint rules in `derpcode-ui/eslint.config.js`; format with `npm run prettier`.
- Use descriptive file names that match existing patterns (for example, `ProblemServiceTests.cs`, `userService.test.ts`).

## Testing Guidelines
- Backend: xUnit + Moq; place tests in mirrored folders under `DerpCode.API.Tests/`.
- Frontend unit tests: Jest files named `*.test.ts(x)` or `*.spec.ts(x)` under `src/`.
- Frontend e2e tests: Playwright specs in `derpcode-ui/e2e/*.spec.ts`.
- No hard coverage gate is configured; include tests for new behavior and regressions before opening a PR.

## Commit & Pull Request Guidelines
- Use concise Conventional Commit-style subjects seen in history: `feat:`, `fix:`, `chore:`, `test:`.
- Keep each commit focused on one logical change.
- PRs should include a clear summary, linked issue/task when applicable, commands/tests run, and screenshots for UI changes.
- Call out migration or configuration impacts when changing database schema, environment settings, or deployment scripts.
