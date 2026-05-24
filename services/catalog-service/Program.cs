using Microsoft.EntityFrameworkCore;
using catalog_service.Data;

var builder = WebApplication.CreateBuilder(args);

string postgreSqlConnection = builder.Configuration.GetConnectionString("PostgreSqlConnection")?.ToString() ?? throw new InvalidOperationException("Connection string 'PostgreSqlConnection' not found.");

builder.Services.AddHealthChecks().AddNpgSql(postgreSqlConnection);
builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(postgreSqlConnection));

var app = builder.Build();

app.MapHealthChecks("/health");
app.MapGet("/videos", () => Results.Ok(new[] { new { id = 1, name = "stub" } }));

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

app.Run();