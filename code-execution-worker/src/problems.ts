import { Language, Problem } from './models';

export const problems: Problem[] = [
  {
    id: '1',
    name: 'Add Two Numbers',
    input: '5,1,6,4',
    expectedOutput: '6,10',
    drivers: [
      {
        id: '1',
        language: Language.CSharp,
        image: 'code-executor-csharp',
        uiTemplate: `using System;

public class Solution
{
    public static int Add(int a, int b)
    {
        // Your code here
    }
}
`,
        driverCode: `using System;

public class Program
{
    public static void Main()
    {
        var str = Console.ReadLine();
        var split = str.Split(',');
        var result = string.Empty;

        for (var i = 0; i < split.Length; i += 2)
        {
            var a = int.Parse(split[i]);
            var b = int.Parse(split[i + 1]);
            result += Solution.Add(a, b).ToString();

            if (i + 2 < split.Length)
            {
                result += ",";
            }
        }

        Console.WriteLine(result);
    }
}`
      },
      {
        id: '2',
        language: Language.JavaScript,
        image: 'code-executor-js',
        uiTemplate: `export function add(a, b) {
    // Your code here
}
`,
        driverCode: `import { add } from './solution.js';

let input = '';

process.stdin.on('data', (chunk) => {
    input += chunk;
});

process.stdin.on('end', () => {
    const str = input.trim(); // Removing extra newline characters
    const split = str.split(',');
    let result = '';

    for (let i = 0; i < split.length; i += 2) {
        const a = parseInt(split[i]);
        const b = parseInt(split[i + 1]);
        result += add(a, b).toString();

        if (i + 2 < split.length) {
            result += ",";
        }
    }

    console.log(result);
});`
      },
      {
        id: '3',
        language: Language.TypeScript,
        image: 'code-executor-ts',
        uiTemplate: `export function add(a: number, b: number): number {
    // Your code here
}
`,
        driverCode: `import { add } from './solution';

let input = '';

process.stdin.on('data', (chunk) => {
    input += chunk;
});

process.stdin.on('end', () => {
    const str = input.trim(); // Removing extra newline characters
    const split = str.split(',');
    let result = '';

    for (let i = 0; i < split.length; i += 2) {
        const a = parseInt(split[i]);
        const b = parseInt(split[i + 1]);
        result += add(a, b).toString();

        if (i + 2 < split.length) {
            result += ",";
        }
    }

    console.log(result);
});`
      }
    ]
  },
  {
    id: '2',
    name: 'FizzBuzz',
    input: '5,1,3,4,15',
    expectedOutput: 'buzz,,fizz,,fizzbuzz',
    drivers: [
      {
        id: '4',
        language: Language.TypeScript,
        image: 'code-executor-ts',
        uiTemplate: `export function fizzBuzz(a: number): string {
    // Your code here
}
`,
        driverCode: `import { fizzBuzz } from './solution';

let input = '';

process.stdin.on('data', (chunk) => {
    input += chunk;
});

process.stdin.on('end', () => {
    const str = input.trim(); // Removing extra newline characters
    const split = str.split(',');
    let result = '';

    for (let i = 0; i < split.length; i++) {
        const a = parseInt(split[i]);
        result += fizzBuzz(a).toString();

        if (i + 1 < split.length) {
            result += ",";
        }
    }

    console.log(result);
});`
      }
    ]
  }
];
