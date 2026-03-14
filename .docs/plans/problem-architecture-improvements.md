# Problem Architecture Improvements Plan

## Context

Review of how problems are authored, built, and executed across 6 languages (C#, Java, JavaScript, Python, Rust, TypeScript). The current design uses a clean 3-layer split — base driver (infrastructure) → driver code (problem-specific glue) → user code (solution) — with Docker containers per language. Test data (input.json, expectedOutput.json) is language-agnostic.

The architecture is solid, but the **6× language amplification** of driver code is the main scaling concern. Every new problem requires 22 files, ~18 of which are per-language. The patterns are regular enough to warrant template-driven automation.

---

## Wave 1 — Base Driver Consistency

Align the base driver interface across all 6 languages so problem authors have the same experience regardless of language.

### 1.1 Standardize `compareResults` default implementation

**Problem:** C#, JavaScript, and Python provide a default equality-based `compareResults`. Java, Rust, and TypeScript require it to be implemented even though most problems use simple equality.

**Action:** Add default `compareResults` implementations to Java (`ProblemDriverBase`), Rust (`ProblemDriver` trait), and TypeScript (`ProblemDriverBase`) that use value equality. Problem authors can still override when needed (e.g., unordered array comparison, floating-point tolerance).

- Java: Change `compareResults` from `abstract` to a concrete method using `Objects.equals()`
- Rust: Provide a default trait method using `actual == expected` on `serde_json::Value`
- TypeScript: Change `compareResults` from `abstract` to a concrete method using deep equality

### 1.2 Standardize `formatErrorMessage` across all languages

**Problem:** C#, JavaScript, and Python have `formatErrorMessage` as an extension point. Java, Rust, and TypeScript don't expose it at all — the base driver hardcodes the format.

**Action:** Add `formatErrorMessage` with a sensible default to Java, Rust, and TypeScript base drivers. All 6 languages should have the same 4 extension points:

| Method               | Required? | Default behavior                         |
| -------------------- | --------- | ---------------------------------------- |
| `parseTestCases`     | Required  | —                                        |
| `executeTestCase`    | Required  | —                                        |
| `compareResults`     | Optional  | Value equality                           |
| `formatErrorMessage` | Optional  | `"Expected {expected} but got {actual}"` |

### 1.3 Update Docker base driver copies

Both `.github/skills/create-derpcode-problem/base-drivers/` and `Docker/*/` contain copies of the base driver. Ensure both locations are updated in sync. Consider whether the Docker copy should be the single source of truth with the skill referencing it, or vice versa.

---

## Wave 2 — Driver Archetypes

Introduce parameterized driver templates for common problem types to reduce per-problem DriverCode authoring from ~80 lines to ~10-15 lines per language.

### 2.1 Identify and define archetypes

Based on the 23 existing problems, categorize into archetypes:

| Archetype           | Pattern                                                                      | Example problems                                                                                                                       |
| ------------------- | ---------------------------------------------------------------------------- | -------------------------------------------------------------------------------------------------------------------------------------- |
| **Simple Function** | Parse flat arrays → call `Solution.func(args)` → compare scalar/array        | AddTwoNumbers, FizzBuzz, SubtractTwoNumbers, ValidPalindrome, ContainsDuplicate, ClimbingStairs, MaximumSubarray, FibonacciNumber      |
| **Stateful Class**  | Parse operation arrays → instantiate → call methods sequentially             | LRUCache                                                                                                                               |
| **Tree Operation**  | Deserialize tree from array → call function → serialize result back to array | InvertBinaryTree, BinaryTreeInorderTraversal, BinaryTreePreorderTraversal, BinaryTreePostorderTraversal, BinaryTreeLevelOrderTraversal |
| **Linked List**     | Deserialize linked list from array → call function → serialize result        | ReverseLinkedList, AddTwoNumbersII, MergeTwoSortedLists                                                                                |
| **Custom**          | Doesn't fit a standard pattern                                               | TargetPair, AtMost, Mirror2DArray, XORBasisRangeQueries, LongestSubstringWithoutRepeatingCharacters, ValidParentheses                  |

### 2.2 Build archetype templates

For each archetype, create a parameterized template per language in `.github/skills/create-derpcode-problem/archetypes/`. The template takes:

- Function/class name
- Parameter types and names
- Return type
- Input JSON shape mapping

**Example — Simple Function archetype (TypeScript):**

```typescript
// Auto-generated from archetype: simple-function
// Function: ${functionName}(${params}) -> ${returnType}
// Input shape: flat array of argument tuples

class ${ProblemName}Driver extends ProblemDriverBase {
    parseTestCases(input: any, expectedOutput: any): TestCase[] {
        return input.map((args: any, i: number) => ({
            input: args,
            expectedOutput: expectedOutput[i]
        }));
    }

    executeTestCase(testCase: TestCase, index: number): any {
        return ${functionCall};
    }
}
```

### 2.3 Update the `create-derpcode-problem` skill

Teach the skill to:

1. Identify which archetype a new problem fits
2. Auto-generate DriverCode from the archetype template
3. Fall back to hand-writing for Custom archetype problems

---

## Wave 3 — Runner Improvements ✅ Completed

### 3.1 Separate compile and run phases for all compiled languages ✅

All compiled languages now follow the same pattern: compile (15s timeout) → exit-on-failure → run (20s timeout).

**Changes made:**
- **C#** (`Docker/CSharp/run.sh`): Split `dotnet run` into `dotnet build --no-restore -v:q` + `dotnet bin/Debug/net10.0/App.dll`. Added `dotnet restore` to Dockerfile for pre-caching.
- **Rust** (`Docker/Rust/run.sh`): Added 15s `timeout` to `cargo build --release` and exit-on-compile-failure check.
- **Java**: Already had this pattern — no changes needed.

### 3.2 Replace `ts-node` with `esbuild` for TypeScript ✅

Replaced `ts-node` + `typescript` + `@types/node` with `esbuild` for near-instant TS→JS transpilation.

**Changes made:**
- **`Docker/TypeScript/package.json`**: Replaced 3 devDependencies with just `esbuild ^0.25.0`.
- **`Docker/TypeScript/run.sh`**: Split into `esbuild index.ts --bundle --platform=node --outfile=dist/index.js` (15s) → `node dist/index.js` (20s), with compile failure check.

### 3.3 Updated all runners to latest stable SDK/language versions ✅

| Runner | Before | After |
|--------|--------|-------|
| C# | dotnet/sdk:8.0 | dotnet/sdk:10.0 |
| Java | temurin:17-jdk-jammy, Gson 2.10.1 | temurin:25-jdk (LTS), Gson 2.13.2 |
| JavaScript | node:22-slim | node:24-slim (LTS) |
| Python | python:3.13-slim | python:3.14-slim |
| Rust | rust:1.87-slim, serde 1.0.219, serde_json 1.0.140 | rust:1.94-slim, serde 1.0.228, serde_json 1.0.149 |
| TypeScript | node:22-slim | node:24-slim (LTS) |

---

## Wave 4 — Test Case Enhancements

### 4.1 Leverage `isHidden` for hidden/premium test cases

**Problem:** The `isHidden` field exists in `TestCaseResult` but is always `false` from the container. It appears to be a placeholder.

**Action:** Define how hidden test cases work:

- Hidden test cases are still executed but their input/expectedOutput/actualOutput are redacted in the API response
- Problem.json gains a `hiddenTestCaseCount` or the input/expectedOutput arrays are split into visible and hidden sections
- Base drivers set `isHidden = true` for test cases beyond a threshold index

### 4.2 Add test case categories

**Action:** Introduce optional test case metadata in Problem.json:

```json
{
  "testCaseCategories": [
    { "name": "Examples", "startIndex": 0, "endIndex": 2 },
    { "name": "Edge Cases", "startIndex": 3, "endIndex": 5 },
    { "name": "Performance", "startIndex": 6, "endIndex": 9 }
  ]
}
```

This enables the UI to group test results and could enable per-category time limits in the future.

---

## Wave 5 — Base Driver Source of Truth

### 5.1 Eliminate duplicate base driver files

**Problem:** Base drivers exist in two places:

- `Docker/<Language>/base_driver.txt` (used in Docker image build)
- `.github/skills/create-derpcode-problem/base-drivers/<language>/base-driver.txt` (used by the skill for reference)

These can drift out of sync.

**Action:** Pick one canonical location and have the other reference it. Options:

- **Option A:** `Docker/` is source of truth. The skill reads from there.
- **Option B:** Shared `lib/base-drivers/` directory. Both Docker and skill reference it. Docker COPY uses a relative path or build context includes it.

---

## Priority & Dependencies

```
Wave 1 (Base Driver Consistency)
  └─→ Wave 2 (Driver Archetypes) — archetypes depend on consistent interfaces
  └─→ Wave 5 (Source of Truth) — single location before making interface changes

Wave 3 (Runner Improvements) — independent, can proceed in parallel

Wave 4 (Test Case Enhancements) — independent, can proceed in parallel
```

**Recommended order:** Wave 1 → Wave 5 → Wave 2 → Wave 3 & 4 (parallel)
