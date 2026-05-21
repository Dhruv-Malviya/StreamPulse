var builder = WebApplication.CreateBuilder(args);


string postgreSqlConnection = builder.Configuration.GetConnectionString("PostgreSqlConnection");

builder.Services.AddHealthChecks()
    .AddNpgSql(postgreSqlConnection);


var app = builder.Build();

app.MapHealthChecks("/health");
app.MapGet("/users", () => Results.Ok(new[] { new { id = 1, name = postgreSqlConnection } }));

app.Run();