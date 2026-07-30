// Load .env into process environment (sql_server, sql_user, etc.) before anything reads them
DotNetEnv.Env.Load();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

// Sample Minimal API — add more endpoints here as you experiment
app.MapGet("/api/hello", () => Results.Ok(new
{
    message = "Hello from the playground API",
    timestamp = DateTimeOffset.UtcNow
}));

app.MapGet("/api/health", () => Results.Ok(new
{
    status = "healthy",
    timestamp = DateTimeOffset.UtcNow
}));

app.MapGet("/api/db/health", async (CancellationToken ct) =>
{
    var ok = await Playground.Util.DB.CanConnectAsync(ct);
    return ok
        ? Results.Ok(new { status = "healthy", database = "reachable", timestamp = DateTimeOffset.UtcNow })
        : Results.Json(new { status = "unhealthy", database = "unreachable", timestamp = DateTimeOffset.UtcNow }, statusCode: 503);
});

app.Run();
