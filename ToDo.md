- Bicep templates for vm, acs, and associated roles
- xp system with unlocks (themes, titles, editor animations, etc)
- programs (guided learnings based on subject)
- User can update email (endpoint to send update email link to new email)
- implement solutions tab (endpoint for CRUD articles)
- move mapping logic into seperate class instead of dtos?
- implement soft deleting for problems, users, articles (user soft deletes should null email. anon username and other pii. Article, problem, reply will keep original content in db but service will remove it)
- Keep refactoring controllers out into services (auth)
- move pick random problem to backend (due to paging)
- add tags endpoint to return all tags so UI can use that for filtering
- add search by name for problems
- try aspire
- migrate to postges?
-- Allow the app user to connect
GRANT CONNECT ON DATABASE derpcode TO derpcodeapp_user;

-- Make sure the app user owns the schema (required for DROP/CREATE SCHEMA)
ALTER SCHEMA public OWNER TO derpcodeapp_user;

-- Allow schema usage
GRANT USAGE ON SCHEMA public TO derpcodeapp_user;

-- Default permissions for all future tables
ALTER DEFAULT PRIVILEGES IN SCHEMA public
    GRANT SELECT, INSERT, UPDATE, DELETE
    ON TABLES TO derpcodeapp_user;

-- Default permissions for all future sequences
ALTER DEFAULT PRIVILEGES IN SCHEMA public
    GRANT USAGE, SELECT
    ON SEQUENCES TO derpcodeapp_user;

- use built in openai tooling (maybe replace swagger?)
- on vm, wait for .net 10 sdk to show in apt (apt-cache search dotnet-sdk). Once it does, remove manual .net install via
  sudo rm -rf /usr/share/dotnet/sdk/10._
  sudo rm -rf /usr/share/dotnet/shared/Microsoft.NETCore.App/10._
  sudo rm -rf /usr/share/dotnet/shared/Microsoft.AspNetCore.App/10._
  sudo rm -rf /usr/share/dotnet/shared/Microsoft.NETAppHost/10._
  sudo rm -rf /usr/share/dotnet/shared/Microsoft.WindowsDesktop.App/10._ # if present (none on Linux usually)
  sudo rm -rf /usr/share/dotnet/templates/10._
  sudo rm -rf /usr/share/dotnet/packs/Microsoft.NETCore.App.Ref/10._
  sudo rm -rf /usr/share/dotnet/packs/Microsoft.AspNetCore.App.Ref/10._
  sudo rm -rf /usr/share/dotnet/packs/Microsoft.NETCore.App.Host.linux-x64/10.\*
  sudo apt install dotnet-sdk-10.0
