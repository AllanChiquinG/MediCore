using MediCore.Api.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Conexion a PostgreSQL. La cadena de conexion NO va en el repo (es publico):
// se guarda en user-secrets (desarrollo) o en variables de entorno (QA / produccion).
var connectionString = builder.Configuration.GetConnectionString("MediCore")
    ?? throw new InvalidOperationException("Falta la cadena de conexion 'ConnectionStrings:MediCore'.");
builder.Services.AddDbContext<MediCoreDbContext>(options => options.UseNpgsql(connectionString));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
