# Progression System (XP + Levels) — Updated Implementation Plan

## Decision Summary

Progression now uses a three-part model:

- `UserProgress`: one-row snapshot for fast reads (`TotalXp`, `Level`, `UpdatedAt`).
- `UserProblemProgress`: per-problem state for monthly eligibility, attempts, and best XP.
- `ExperienceEvents`: append-only XP ledger for auditability and future XP sources.

This replaces storing XP totals directly on the `User` entity.

## Product Rules (Locked)

- XP is awarded only on accepted submissions (`POST /api/v1/problems/{problemId}/submissions` with `Pass=true`).
- `/run` never changes XP.
- XP from a problem is non-stacking: store best earned value per user/problem (`BestXp`).
- XP improvement is allowed once per anchored monthly cycle (anchor = `FirstXpAwardedAt`).
- A worse future result never decreases total XP.

## XP and Level Math

- Difficulty max XP: `VeryEasy=50`, `Easy=75`, `Medium=110`, `Hard=160`, `VeryHard=230`.
- Earned XP:
  - `earned = round(maxXp * attemptFactor * timeFactor * hintFactor)`.
- Factors (clamped):
  - `attemptFactor = max(0.60, 1.00 - 0.10 * attemptsBeforePass)`
  - `timeFactor = max(0.60, min(1.00, targetMinutes[difficulty] / max(1, solveMinutes)))`
  - `hintFactor = max(0.50, 1.00 - 0.12 * uniqueHintsOpened)`
- Level curve:
  - `xpToReach(level) = 100 * (level - 1)^2`
  - derived fields: `level`, `xpIntoLevel`, `xpForNextLevel`.

## Data Model

1. `UserProgress` (`UserId` PK/FK to `AspNetUsers`)
- `TotalXp`, `Level`, `UpdatedAt`.

2. `UserProblemProgress` (`UserId + ProblemId` composite PK)
- `BestXp`, `FirstXpAwardedAt`, `LastAwardedCycleIndex`, `FirstSubmitAtCurrentCycle`, `SubmitAttemptsCurrentCycle`, `OpenedHintIndicesCurrentCycle` (`jsonb`), `LastSolvedAt`.

3. `ExperienceEvents`
- `Id`, `UserId`, `EventType`, `SourceType`, `SourceId`, `XpDelta`, `IdempotencyKey` (unique), `Metadata` (`jsonb`), `CreatedAt`.

## Backend Flow

- `POST /api/v1/problems/{problemId}/hints/{hintIndex}/open`
  - validates hint index and records unique hint usage in active cycle state.
- `SubmitSolutionAsync`
  - load/create `UserProgress` + `UserProblemProgress`
  - track attempts/time for active cycle
  - on eligible pass: compute XP, apply positive delta vs `BestXp`, update `UserProgress`, append `ExperienceEvent`, reset cycle tracking
  - on cooldown pass: append zero-delta `ExperienceEvent` for traceability

## Contracts and Tests

- `ProblemSubmissionDto` and `UserDto` include progression fields (`totalXp`, `level`, level progress, XP delta metadata).
- Tests cover:
  - `ProgressionMath` formulas and boundaries
  - service-level award/cooldown/delta behavior
  - submission DTO progression fields

## Follow-up Extensions

- Add new `ExperienceEventType` producers (achievements, streaks, etc.) without schema changes.
- Build analytics and audit views directly from `ExperienceEvents`.
