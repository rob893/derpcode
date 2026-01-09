DerpCode

This is a leetcode clone (ie: an algorithm practice platform) that allows users to create and solve coding challenges. The goal is to gamify the process of learning algorithms and data structures while being entertaining by being snarky.

@azure Rule - Use Azure Best Practices: When generating code for Azure, running terminal commands for Azure, or performing operations related to Azure, invoke your azure_development-get_best_practices tool if available.
Technologies used...

The backend server is written in .NET 8 and uses Entity Framework Core for data access and is hosted on an Azure VM. The server provides a REST API for the web app and also launches docker containers on the same vm to execute user code in a sandbox environment.
The web app is a React SPA using vite, typescript, and tailwind css with heroui component library and will be deployed to Github Pages

The project is broken down into these folders:
./CI - This is where CI files like bicep goes
./DerpCode.API - this is where the server code goes
./DerpCode.API.Tests - this is where the server tests code goes
./derpcode-ui - This is where the react SPA code goes

Note that the UI uses tailwind v4 (not 3). You can find breaking changes https://tailwindcss.com/docs/upgrade-guide#changes-from-v3

C# best practices:

- follow the conventions in the .editorconfig file at root of the project
- always use 'this' keyword for instance members
- never use '\_' prefix for instance members
- Don't leave unused usings in files
- Add a blank line between all class fields, methods, properties, and members
- Add xml comments to all public members (use inheritdoc for inherited members)
- Always accept cancel tokens in async methods

Backend unit test best practices:

- Use xunit for unit tests.
- Use Moq for mocking dependencies.
- Use dotnet test for testing

TypeScript Best Practices:

- always prefer func(): Type {} over func: Type => {} (prefer method syntax over arrow/function signature syntax)
