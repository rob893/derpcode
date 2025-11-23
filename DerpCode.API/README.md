# DerpCode API

Backend server for DerpCode - an algorithm practice platform built with .NET 10 and PostgreSQL.

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [PostgreSQL 14+](https://www.postgresql.org/download/)
- [Docker](https://www.docker.com/get-started) (for code execution sandboxing)

## First Time Database Setup

### 1. Install PostgreSQL

Download and install PostgreSQL from the official website. Make note of the superuser (postgres) password you set during installation.

### 2. Create Database and User

Connect to PostgreSQL as the superuser and run the following SQL commands:

```sql
-- Create the database
CREATE DATABASE derpcode;

-- Create the application user
CREATE USER derpcodeapp_user WITH PASSWORD 'your_secure_password_here';

-- Allow the app user to connect
GRANT CONNECT ON DATABASE derpcode TO derpcodeapp_user;

ALTER DATABASE derpcode OWNER TO derpcodeapp_user;

-- Connect to the derpcode database
\c derpcode

-- Make sure the app user owns the schema (required for DROP/CREATE SCHEMA)
ALTER SCHEMA public OWNER TO derpcodeapp_user;

-- Allow schema usage
GRANT USAGE ON SCHEMA public TO derpcodeapp_user;

-- Grant privileges on existing tables (if any exist before migrations)
GRANT SELECT, INSERT, UPDATE, DELETE ON ALL TABLES IN SCHEMA public TO derpcodeapp_user;

-- Grant privileges on existing sequences (if any exist before migrations)
GRANT USAGE, SELECT ON ALL SEQUENCES IN SCHEMA public TO derpcodeapp_user;

-- Default permissions for all future tables
ALTER DEFAULT PRIVILEGES IN SCHEMA public
    GRANT SELECT, INSERT, UPDATE, DELETE
    ON TABLES TO derpcodeapp_user;

-- Default permissions for all future sequences
ALTER DEFAULT PRIVILEGES IN SCHEMA public
    GRANT USAGE, SELECT
    ON SEQUENCES TO derpcodeapp_user;
```

### 3. Update Configuration

Update your `appsettings.Local.json` (not checked into source control) with your database connection:

```json
{
  "Postgres": {
    "DefaultConnection": "Host=localhost;Database=derpcode;Username=derpcodeapp_user;Password=your_secure_password_here;CommandTimeout=120"
  }
}
```

### 4. Run Migrations

Apply the database migrations to create all tables and seed initial data:

```bash
dotnet ef database update
```

This will:

- Create all necessary tables
- Set up relationships and indexes
- Seed initial data (problems, tags, etc.)

## Development

### Running Locally

```bash
dotnet run
```

The API will be available at:

- HTTPS: `https://localhost:7059`
- HTTP: `http://localhost:5059`

### Running with Hot Reload

```bash
dotnet watch
```

### Building

```bash
dotnet build
```

### Running Tests

```bash
dotnet test
```

## Database Management

### Creating a New Migration

After modifying entity models:

```bash
dotnet ef migrations add MigrationName
```

### Reverting the Last Migration

```bash
dotnet ef migrations remove
```

### Updating the Database

```bash
dotnet ef database update
```

### Rolling Back to a Specific Migration

```bash
dotnet ef database update MigrationName
```

## Configuration

The application uses a hierarchical configuration system:

1. `appsettings.json` - Base configuration
2. `appsettings.Development.json` - Development overrides
3. `appsettings.Production.json` - Production overrides
4. `appsettings.Local.json` - Local developer overrides (not in source control)
5. Azure Key Vault - Secrets in non-development environments

### Required Configuration Sections

- **Postgres**: Database connection settings
- **Authentication**: JWT settings, OAuth client IDs/secrets
- **Email**: Azure Communication Services settings
- **Swagger**: API documentation settings
- **GitHub**: Repository integration settings
- **KeyVaultUrl**: Azure Key Vault URL for production

## Docker Code Execution

The API uses Docker to execute user-submitted code in isolated containers. Supported languages:

- C#
- Java
- JavaScript
- Python
- TypeScript
- Rust

Each language has its own Dockerfile and base driver in the `Docker/` directory at the project root.

## API Versioning

The API supports multiple versions:

- v1: Current stable version
- v2: Preview/beta features

Access via URL path: `/api/v1/...` or `/api/v2/...`

## Health Checks

Health check endpoints:

- `/health` - Basic health check
- `/health/ready` - Readiness probe (includes database)

## Swagger Documentation

When `Swagger:Enabled` is `true`, access interactive API documentation at:

- `https://localhost:7059/swagger`

## Troubleshooting

### Database Connection Issues

1. Verify PostgreSQL is running:

   ```bash
   # Windows
   Get-Service postgresql*

   # Linux/Mac
   sudo systemctl status postgresql
   ```

2. Test connection with psql:

   ```bash
   psql -h localhost -U derpcodeapp_user -d derpcode
   ```

3. Check connection string format in configuration

### Migration Issues

If migrations fail, ensure:

- Database user has sufficient privileges
- No other applications are holding locks on tables
- Connection string is correct

### Docker Issues

Ensure Docker Desktop is running before starting the API, as it's required for code execution features.

## License

Copyright (c) 2025 Robert Herber. All rights reserved.
