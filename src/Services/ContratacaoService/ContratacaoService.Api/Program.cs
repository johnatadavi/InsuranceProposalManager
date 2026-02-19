using BuildingBlocks.Infrastructure.Middleware;
using ContratacaoService.Api.Endpoints;
using ContratacaoService.Application;
using ContratacaoService.Infrastructure;
using ContratacaoService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
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
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, ct) =>
    {
        document.Info.Title = "Contratacao Service API";
        document.Info.Version = "v1";
        document.Info.Description = "Insurance Contract Management Service";
        return Task.CompletedTask;
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
    // OpenAPI document endpoint
    app.MapOpenApi();
    
    // Scalar API Reference UI
    app.MapScalarApiReference(options =>
    {
        options
            .WithTitle("Contratacao Service API")
            .WithTheme(ScalarTheme.BluePlanet)
            .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
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
