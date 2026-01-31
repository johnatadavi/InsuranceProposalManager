# Dockerfile otimizado com multi-stage build
# ContratacaoService

# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files first (for better layer caching)
COPY ["Directory.Build.props", "."]
COPY ["Directory.Packages.props", "."]

# Copy BuildingBlocks projects
COPY ["src/BuildingBlocks/BuildingBlocks.Domain/BuildingBlocks.Domain.csproj", "src/BuildingBlocks/BuildingBlocks.Domain/"]
COPY ["src/BuildingBlocks/BuildingBlocks.Application/BuildingBlocks.Application.csproj", "src/BuildingBlocks/BuildingBlocks.Application/"]
COPY ["src/BuildingBlocks/BuildingBlocks.Infrastructure/BuildingBlocks.Infrastructure.csproj", "src/BuildingBlocks/BuildingBlocks.Infrastructure/"]

# Copy ContratacaoService projects
COPY ["src/Services/ContratacaoService/ContratacaoService.Domain/ContratacaoService.Domain.csproj", "src/Services/ContratacaoService/ContratacaoService.Domain/"]
COPY ["src/Services/ContratacaoService/ContratacaoService.Application/ContratacaoService.Application.csproj", "src/Services/ContratacaoService/ContratacaoService.Application/"]
COPY ["src/Services/ContratacaoService/ContratacaoService.Infrastructure/ContratacaoService.Infrastructure.csproj", "src/Services/ContratacaoService/ContratacaoService.Infrastructure/"]
COPY ["src/Services/ContratacaoService/ContratacaoService.Api/ContratacaoService.Api.csproj", "src/Services/ContratacaoService/ContratacaoService.Api/"]

# Restore dependencies
RUN dotnet restore "src/Services/ContratacaoService/ContratacaoService.Api/ContratacaoService.Api.csproj"

# Copy source code
COPY . .

# Build and publish
WORKDIR "/src/src/Services/ContratacaoService/ContratacaoService.Api"
RUN dotnet build "ContratacaoService.Api.csproj" -c Release -o /app/build

# Stage 2: Publish
FROM build AS publish
RUN dotnet publish "ContratacaoService.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 3: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Create non-root user for security
RUN adduser --disabled-password --gecos "" appuser && chown -R appuser /app
USER appuser

# Copy published output
COPY --from=publish /app/publish .

# Expose port
EXPOSE 8080

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "ContratacaoService.Api.dll"]
