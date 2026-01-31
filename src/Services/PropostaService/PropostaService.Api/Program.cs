using BuildingBlocks.Infrastructure.Middleware;
using Microsoft.EntityFrameworkCore;
using PropostaService.Api.Endpoints;
using PropostaService.Application;
using PropostaService.Infrastructure;
using PropostaService.Infrastructure.Persistence;
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
        Title = "Proposta Service API",
        Version = "v1",
        Description = "Insurance Proposal Management Service"
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
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Proposta Service API v1");
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
