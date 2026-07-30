# C# Playground

A basic ASP.NET Core Razor Pages web app for learning and experimenting.

## Getting Started
```bash
# Clone Repo
git clone https://github.com/jakekohl/csharp-playground.git

# Build Solution
dotnet build

# Run Solution
dotnet run
```

Then open http://localhost:5111 (or the HTTPS URL shown in the terminal).

## Layout

| Path | Purpose |
|------|---------|
| `Program.cs` | App startup + Minimal API endpoints |
| `Pages/` | Razor Pages (UI + page models) |
| `Pages/Sandbox.cshtml` | Sample interactive page to edit |
| `Pages/Users.cshtml` | Sample CRUD Testing Page for Database |
| `util/DB.cs` | Azure SQL helper (queries + transactions) |
| `wwwroot/` | Static CSS, JS, and other assets |
| `appsettings.json` | Configuration |
| `.env` | Local secrets (gitignored) — SQL credentials |

## Azure SQL

Copy your server/database into `.env`:

```
sql_server=myserver.database.windows.net
sql_database=mydb
sql_user=...
sql_password=...
```

Or set a full `sql_connection_string` instead. Then hit `GET /api/db/health` to verify connectivity.

## Try next

- Edit `Pages/Sandbox.cshtml` / `.cshtml.cs` and refresh
- Add endpoints in `Program.cs` (see `/api/hello`)
- Add a new page: `dotnet new page -n MyPage -o Pages -n Playground`
