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

app.Run();
