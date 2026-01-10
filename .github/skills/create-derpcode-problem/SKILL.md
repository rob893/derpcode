---
name: create-derpcode-problem
description: This skill allows the agent to create new coding problems for the DerpCode platform. This skill should be used when asked to create new DerpCode coding problems.
---

# Skill Instructions

Context:

You are creating a new coding problem for a leetcode style platform called DerpCode. The .txt files you create along with the base driver code will all be injected into the docker containers at runtime for on demand remote code execution. The drivers, answers, and ui templates you write will not be buildable or testable as they depend on the docker run scripts and run time injection.

1. Review the base drivers in the [base drivers](./base-drivers/) folder to understand how each problem will start with. All problem drivers must inherit from a base driver. They are all intentinally txt files. The drivers for the respective languages are located:

   - [csharp](./base-drivers/csharp/base-driver.txt)
   - [java](./base-drivers/java/base-driver.txt)
   - [javascript](./base-drivers/javascript/base-driver.txt)
   - [python](./base-drivers/python/base-driver.txt)
   - [rust](./base-drivers/rust/base-driver.txt)
   - [typescript](./base-drivers/typescript/base-driver.txt)

2. Review the example problem [here](./examples/LRUCache-4/) to understand how problems are constructed. Review every file in the folder. Every single one is important and required (pay close attention to the sections in the problem description and explanation).

2.1. (Optional) You can review more problems in the `DerpCode.API/Data/SeedData/Problems` folder.
2.2. (Optional) If you want to know how the code gets injected into the containers at runtime, see the language folders in `Docker/{language}/` for each language.

3. Determine the id for the new problem. Ids are positive integers. Determine the new id by looking at the existing problems in the `DerpCode.API/Data/SeedData/Problems` folder. Each folder in that folder is a problem and is named `{ProblemName}-{Id}`. Find the problem with the highest Id and add 1 to that id. This will be the new Id.

4. Create a new folder in `DerpCode.API/Data/SeedData/Problems` and name id `{NewProblemName}-{NewProblemIdFromStep3}`

5. Create the `Description.md` file, the `Explanation.md` file, and the `Problem.json` in the folder created in step 4 file following the conventions you discovered in step 2 when reviewing the [example problem](./examples/LRUCache-4/). Do not provide hints in the problem description.

6. Create a `Drivers` folder in the folder created in step 4.

7. For each language (CSharp, Java, JavaScript, Python, TypeScript, and Rust), create a new folder in the `Drivers` folder created in step 6.

8. For each language (CSharp, Java, JavaScript, Python, TypeScript, and Rust), create an `Answer.txt`, a `DriverCode.txt`, and an `UITemplate.txt` file in the respective language folder created in step 7 (there will be 3 files per language). It is IMPORTANT that the file extensions are `.txt`. Follow the conventions from the [example problem](./examples/LRUCache-4/) when creating these files. All problem drivers MUST inherit from the base drivers from step 1. Drivers should not use any 3rd party libraries (like lodash) unless absolutely necessary.

9. Validate the problem and its drivers against the [example problem](./examples/LRUCache-4/) for consistency. Ensure the new problem has working driver for each language (CSharp, Java, JavaScript, Python, TypeScript, and Rust). Validate each driver has an `Answer.txt`, a `DriverCode.txt`, and an `UITemplate.txt` file.

Notes:

- User submitted data and problem drivers are in seperate files when run in the container (see Docker/ folder for docker files). Meaning don't assume that user code is injected into the same file as the driver when you are making new drivers. Don't forget import statemenst for user files (like import { add } form 'solution.js' for javasctipt)
- TypeScript drivers import from `./solution` (the injected `solution.ts`). Ensure the `UITemplate.txt` (and the seeded `Answer.txt`) exports the function/class the driver imports, e.g. `export function climbStairs(...)`. Otherwise `ts-node` fails with TS2306 (`solution.ts` "is not a module").
- The DerpCode UI supports math rendering (KaTeX) in markdown. In `Explanation.md`, feel free to use `$...$` for inline math and `$$...$$` for block math (for example: `$GF(2)$`, `$O(N \cdot W)$`, `$O(\log N \cdot W)$`).
- For JavaScript code, always use class syntax over 'function ClassName ()' syntax. Never use var for javascript. Prefer function foo() syntax over const foo = () syntax
