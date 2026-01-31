using BuildingBlocks.Infrastructure.Middleware;
using ContratacaoService.Api.Endpoints;
using ContratacaoService.Application;
using ContratacaoService.Infrastructure;
using ContratacaoService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new()
    {
        Title = "Contratacao Service API",
        Version = "v1",
        Description = "Insurance Contract Management Service"
    });
});

// Add Application and Infrastructure layers
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// Add Health Checks
builder.Services.AddHealthChecks()
    .AddNpgSql(builder.Configuration.GetConnectionString("ContratacaoDb")!);

var app = builder.Build();

// Configure the HTTP request pipeline
app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Contratacao Service API v1");
    });

    // Apply migrations in development
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<ContratacaoDbContext>();
    await dbContext.Database.MigrateAsync();
}

app.UseSerilogRequestLogging();

// Map endpoints
app.MapContractEndpoints();
app.MapHealthChecks("/health");

app.Run();

namespace ContratacaoService.Api
{
    /// <summary>
    /// Marker class for integration tests with WebApplicationFactory.
    /// </summary>
    public partial class Program { }
}
