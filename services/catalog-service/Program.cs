var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));
app.MapGet("/videos", () => Results.Ok(new[] { new { id = 1, name = "stub" } }));

app.Run();