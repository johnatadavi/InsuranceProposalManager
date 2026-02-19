using BuildingBlocks.Infrastructure.Middleware;
using Microsoft.EntityFrameworkCore;
using PropostaService.Api.Endpoints;
using PropostaService.Application;
using PropostaService.Infrastructure;
using PropostaService.Infrastructure.Persistence;
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
        document.Info.Title = "Proposta Service API";
        document.Info.Version = "v1";
        document.Info.Description = "Insurance Proposal Management Service";
        return Task.CompletedTask;
    });
});

// Add Application and Infrastructure layers
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// Add Health Checks
builder.Services.AddHealthChecks()
    .AddNpgSql(builder.Configuration.GetConnectionString("PropostaDb")!);

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
            .WithTitle("Proposta Service API")
            .WithTheme(ScalarTheme.BluePlanet)
            .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
    });

    // Apply migrations in development
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<PropostaDbContext>();
    await dbContext.Database.MigrateAsync();
}

app.UseSerilogRequestLogging();

// Map endpoints
app.MapProposalEndpoints();
app.MapHealthChecks("/health");

app.Run();

namespace PropostaService.Api
{
    /// <summary>
    /// Marker class for integration tests with WebApplicationFactory.
    /// </summary>
    public partial class Program { }
}
