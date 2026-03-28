Railway deployment notes
=======================

1) PostgreSQL service
- In Railway add a PostgreSQL plugin/instance. Railway will provide a `DATABASE_URL` environment variable (format: `postgres://user:pass@host:port/dbname`).

2) Environment variables
- Railway provides `DATABASE_URL` automatically when you add Postgres. No need to set `ConnectionStrings:DefaultConnection` manually, but you may set it if you prefer.
- Ensure `ASPNETCORE_URLS` is set to `http://+:8080` (the included Dockerfile exposes 8080).

3) Dockerfile
- The project includes a multi-stage `Dockerfile` at repo root. Railway will detect and build it.

4) Notes about EF Core migrations
- Current migrations were generated targeting SQL Server and contain SQL Server-specific annotations (identity columns). When switching providers you should:
  - Regenerate migrations for PostgreSQL (recommended): delete existing migrations in `LogiCore.Infrastructure/Migrations` and run `dotnet ef migrations add Initial` targeting the PostgreSQL provider.
  - Or carefully review and adapt migration code (advanced).

5) Quick local test
```bash
docker build -t logicore:latest .
docker run --rm -p 8080:8080 -e DATABASE_URL="postgres://postgres:postgres@host:5432/logicore_dev" logicore:latest
```

6) Troubleshooting
- If EF throws provider compatibility errors when applying migrations, regenerate migrations as explained above.
