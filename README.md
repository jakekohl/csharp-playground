# C# Playground

A basic ASP.NET Core Razor Pages web app for learning and experimenting.

## Run

```bash
dotnet run
```

Then open http://localhost:5111 (or the HTTPS URL shown in the terminal).

## Layout

| Path | Purpose |
|------|---------|
| `Program.cs` | App startup + Minimal API endpoints |
| `Pages/` | Razor Pages (UI + page models) |
| `Pages/Sandbox.cshtml` | Sample interactive page to edit |
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

Example usage:

```csharp
using Playground.Util;
using Microsoft.Data.SqlClient;

// Query
var table = await DB.ExecuteQueryAsync(
    "SELECT Id, Name FROM dbo.Items WHERE Active = @active",
    [DB.Param("@active", true)]);

// Non-query
await DB.ExecuteNonQueryAsync(
    "UPDATE dbo.Items SET Name = @name WHERE Id = @id",
    [DB.Param("@name", "Widget"), DB.Param("@id", 1)]);

// Transaction
await DB.ExecuteInTransactionAsync(async (conn, tx) =>
{
    await using var cmd = conn.CreateCommand();
    cmd.Transaction = tx;
    cmd.CommandText = "INSERT INTO dbo.Items (Name) VALUES (@name)";
    cmd.Parameters.Add(DB.Param("@name", "Gadget"));
    await cmd.ExecuteNonQueryAsync();
});
```

## Try next

- Edit `Pages/Sandbox.cshtml` / `.cshtml.cs` and refresh
- Add endpoints in `Program.cs` (see `/api/hello`)
- Add a new page: `dotnet new page -n MyPage -o Pages -n Playground`
