# Master Research Plan — 2026-03-14

## Executive Summary

This is the first comprehensive research sweep of the DerpCode platform. Five specialized audits (performance, security, quality, UX, features) analyzed the entire codebase — .NET 10 API, React/TypeScript frontend, Docker runner infrastructure, and Azure deployment.

**Key themes across all areas:**

1. **Docker Security is the #1 Priority** — Containers run user code as root with no PID limits, no seccomp profile, and the API process holds unrestricted Docker socket access. A fork bomb or container escape is trivially achievable.
2. **Frontend Bundle & UX Debt** — No code splitting, no lazy loading of Monaco/Shiki/KaTeX, dark-mode-hostile colors throughout, and no mobile navigation. The initial bundle includes the entire app.
3. **Code Quality Ceiling** — Several god classes (AuthController 499 lines, CreateEditProblem 786 lines, ProblemList 690 lines), ~200 lines of duplicated OAuth logic, and zero controller tests.
4. **Gamification Framework Built but Incomplete** — XP/leveling/achievements exist in the schema, but zero achievement trigger logic is wired up and XP events are limited to a single source type.
5. **Performance Quick Wins Available** — No response compression, only 2 database indexes, systematic `.ToList()` over-materialization across services.

**Recommended approach:** Address critical security issues first (Wave 1), then tackle high-impact performance and quality wins (Wave 2), followed by UX improvements and feature buildout (Waves 3-4).

---

## Execution Waves

### Wave 1: Critical Security — Immediate Action Required

Items that must be addressed before any other work. These represent active vulnerability vectors.

| # | Area | Finding | Effort | Impact | Dependencies |
|---|------|---------|--------|--------|-------------|
| 1 | Security | Docker containers run user code as `root` — change to `runner` user | Low | Critical | None |
| 2 | Security | No PID limit on containers — fork bomb DoS trivially possible | Low | Critical | None |
| 3 | Security | No seccomp profile or capability dropping in runner containers | Medium | Critical | #1 |
| 4 | Security | API process holds unrestricted Docker socket — full daemon takeover on RCE | Medium | Critical | None |
| 5 | Security | SQL injection via string interpolation in `DatabaseSeeder.FixSequenceAsync` | Low | Critical | None |
| 6 | Security | Plaintext secrets in `appsettings.Local.json` on disk | Low | Critical | None |
| 7 | Security | `.env` committed to git — OAuth client IDs in public history | Low | High | None |
| 8 | Security | No maximum length on user code submission — memory exhaustion | Low | High | None |
| 9 | Security | ForwardedHeaders trusts all proxies — rate limiter IP spoofing bypass | Low | High | None |
| 10 | Security | Exception messages serialized into API responses — internal detail leakage | Medium | High | None |

### Wave 2: High-Impact Performance & Quality — No Cross-Dependencies

Items that deliver the most value per effort hour. Each is independent.

| # | Area | Finding | Effort | Impact | Dependencies |
|---|------|---------|--------|--------|-------------|
| 11 | Performance | Add response compression (Brotli + Gzip) | Low | High | None |
| 12 | Performance | Add database indexes on frequently filtered columns | Medium | High | None |
| 13 | Performance | Add route-based code splitting (React.lazy for all pages) | Medium | High | None |
| 14 | Performance | Lazy load Monaco/Shiki/KaTeX + configure Vite manualChunks | Medium | High | #13 |
| 15 | Performance | Paginate user favorites endpoint (currently unbounded) | Low | High | None |
| 16 | Quality | Fix bare `catch` blocks in AuthController (swallowing exceptions) | Low | High | None |
| 17 | Quality | Add AuthController tests (499 lines of complex OAuth logic, zero tests) | High | High | None |
| 18 | Security | No API-level container wait timeout — thread pool exhaustion DoS | Low | High | None |
| 19 | Security | CORS fallback to wildcard `["*"]` origin | Low | High | None |
| 20 | Security | Missing security headers (CSP, X-Frame-Options, X-Content-Type-Options) | Medium | High | None |

### Wave 3: UX Critical & Quality Refactors

Items addressing user-facing issues and maintainability. Some depend on Wave 2 completion.

| # | Area | Finding | Effort | Impact | Dependencies |
|---|------|---------|--------|--------|-------------|
| 21 | UX | Mobile navigation missing — nav links vanish with no hamburger menu | Medium | Critical | None |
| 22 | UX | Dark-mode-hostile color tokens (`*-50`/`*-200`) used extensively | Medium | High | None |
| 23 | UX | No React Error Boundary for crash recovery | Low | High | None |
| 24 | UX | Native `<button>` elements without focus rings / keyboard accessibility | Medium | High | None |
| 25 | UX | Missing ARIA labels on interactive elements (vote buttons, PWA banner) | Low | High | None |
| 26 | Quality | Deduplicate ~200 lines of OAuth login logic in AuthController | Medium | High | #16 |
| 27 | Quality | Decompose ProblemSubmissionService (10 constructor dependencies, SRP) | High | High | None |
| 28 | Quality | Break down frontend god components (CreateEditProblem 786, ProblemList 690, AccountSection 742 lines) | High | High | #13 |
| 29 | Quality | Replace 21 `any` type usages in TypeScript with proper types | Medium | Medium | None |
| 30 | Quality | Cloud-init script installs .NET 8 but project targets .NET 10 | Low | Medium | None |

### Wave 4: UX Polish & Performance Tuning

Refinements that improve user experience and runtime efficiency. Depend on earlier waves.

| # | Area | Finding | Effort | Impact | Dependencies |
|---|------|---------|--------|--------|-------------|
| 31 | UX | No toast/notification system for action feedback | Medium | High | None |
| 32 | UX | XP feedback after submission is understated and easy to miss | Medium | Medium | #31 |
| 33 | UX | No onboarding flow for first-time users | High | Medium | None |
| 34 | UX | ResizableSplitter mouse-only — no touch or keyboard support | Medium | Medium | None |
| 35 | UX | Missing document title updates for SPA navigation | Low | Medium | None |
| 36 | UX | Massive code duplication in ProblemCodeEditor (normal vs fullscreen) | Medium | Medium | #28 |
| 37 | UX | ProblemList filter bar not responsive on mobile | Medium | Medium | #21 |
| 38 | Performance | Fix systematic `.ToList()` over-materialization in 5+ services | Medium | Medium | None |
| 39 | Performance | Flame animation GC pressure (60 allocs/sec) | Medium | Medium | None |
| 40 | Performance | ResizableSplitter reflow on every mouse move | Low | Medium | None |
| 41 | Performance | O(n²) filter operations in CreateEditProblem | Low | Medium | None |
| 42 | Performance | Add gcTime config to React Query | Low | Medium | None |
| 43 | Quality | Add missing service tests (8 services without tests) | High | Medium | None |
| 44 | Quality | Eliminate 12 duplicated owner-or-admin authorization guards | Medium | Medium | None |

### Wave 5: Feature Buildout — Gamification & Engagement

New features that leverage existing infrastructure. Depend on stability from Waves 1-4.

| # | Area | Finding | Effort | Impact | Dependencies |
|---|------|---------|--------|--------|-------------|
| 45 | Features | Wire up achievement trigger logic (10 types defined, zero triggers) | Medium | Critical | None |
| 46 | Features | Expand XP event types beyond ProblemSolved | Medium | High | #45 |
| 47 | Features | Daily solve streak system | Medium | High | None |
| 48 | Features | Daily problem / Problem of the Day | Medium | High | None |
| 49 | Features | User statistics & progress dashboard | High | High | None |
| 50 | Features | Leaderboard system (global, monthly, by-difficulty) | High | High | #49 |
| 51 | Features | XP level progress bar in app header | Low | Medium | None |
| 52 | Features | Snarky toast notification system | Medium | Medium | #31 |
| 53 | Features | Random problem / Problem Roulette | Low | Medium | None |
| 54 | Features | Problem rating system (crowdsourced difficulty) | Medium | Medium | None |

### Wave 6: Feature Expansion — Content & Social

Larger features requiring new entities and significant frontend work.

| # | Area | Finding | Effort | Impact | Dependencies |
|---|------|---------|--------|--------|-------------|
| 55 | Features | Public user profiles with stats, badges, activity | High | High | #49, #45 |
| 56 | Features | Problem collections / learning tracks | High | High | None |
| 57 | Features | Solution comparison / community solutions | Medium | Medium | None |
| 58 | Features | Discussion/comments on problems | Medium | Medium | None |
| 59 | Features | Go and C++ language support | High | Medium | None |
| 60 | Features | Email update flow | Medium | Medium | None |
| 61 | Features | Additional achievement types (streak-based, language-based) | Low | Medium | #45, #47 |
| 62 | Features | Admin analytics dashboard | High | Medium | None |
| 63 | Features | Interview simulation mode | Very High | Medium | None |
| 64 | Features | Problem hints enhancement — interactive/progressive hints | Medium | Low | None |

---

## Cross-Dependency Map

```
Wave 1 (Security Critical)
  └─→ Wave 2 (Performance + Quality High-Impact)
       ├─→ #13 (Code splitting) → #14 (Lazy load heavy deps)
       ├─→ #16 (Fix bare catches) → #26 (Deduplicate OAuth logic)
       └─→ Wave 3 (UX Critical + Quality Refactors)
            ├─→ #21 (Mobile nav) → #37 (Responsive filter bar)
            ├─→ #13 → #28 (Decompose frontend god components) → #36 (Dedup ProblemCodeEditor)
            └─→ Wave 4 (UX Polish + Perf Tuning)
                 ├─→ #31 (Toast system) → #32 (XP feedback) → #52 (Snarky toasts)
                 └─→ Wave 5 (Gamification)
                      ├─→ #45 (Achievement triggers) → #46 (XP event types) → #61 (More achievements)
                      ├─→ #49 (User stats dashboard) → #50 (Leaderboards) → #55 (Public profiles)
                      └─→ Wave 6 (Content & Social)
```

---

## Effort Summary

| Wave | Items | Estimated Total Effort | Priority |
|------|-------|----------------------|----------|
| Wave 1 | 10 | ~12 hours | 🔴 Immediate |
| Wave 2 | 10 | ~20 hours | 🔴 This sprint |
| Wave 3 | 10 | ~30 hours | 🟡 Next sprint |
| Wave 4 | 14 | ~25 hours | 🟡 Following sprint |
| Wave 5 | 10 | ~25 hours | 🟠 Planned |
| Wave 6 | 10 | ~40 hours | 🟢 Backlog |

---

## Previous Plan Status
No previous plans exist. This is the initial comprehensive research sweep.

---

## Research Area File Index

| Area | File | Findings |
|------|------|----------|
| Performance | [performance.md](performance.md) | 15 findings |
| Security | [security.md](security.md) | 19 findings |
| Quality | [quality.md](quality.md) | 34 findings |
| UX | [ux.md](ux.md) | 50 findings |
| Features | [features.md](features.md) | 30 findings |
