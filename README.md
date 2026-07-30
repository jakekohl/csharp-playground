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
| `wwwroot/` | Static CSS, JS, and other assets |
| `appsettings.json` | Configuration |

## Try next

- Edit `Pages/Sandbox.cshtml` / `.cshtml.cs` and refresh
- Add endpoints in `Program.cs` (see `/api/hello`)
- Add a new page: `dotnet new page -n MyPage -o Pages -n Playground`
