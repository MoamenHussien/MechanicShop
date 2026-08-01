# Stage 1: Build & Publish (SDK)
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution and configuration files first for layer caching
COPY MechanicShop.slnx Directory.Build.props Directory.Packages.props ./

# Copy csproj files for all projects maintaining directory structure
COPY Src/MechanicShop.Domain/MechanicShop.Domain.csproj Src/MechanicShop.Domain/
COPY Src/MechanicShop.Contracts/MechanicShop.Contracts.csproj Src/MechanicShop.Contracts/
COPY Src/MechanicShop.Application/MechanicShop.Application.csproj Src/MechanicShop.Application/
COPY Src/MechanicShop.Infrastructure/MechanicShop.Infrastructure.csproj Src/MechanicShop.Infrastructure/
COPY Src/MechanicShop.Client/MechanicShop.Client.csproj Src/MechanicShop.Client/
COPY Src/MechanicShop.Api/MechanicShop.Api.csproj Src/MechanicShop.Api/

# Restore dependencies
RUN dotnet restore Src/MechanicShop.Api/MechanicShop.Api.csproj

# Copy source files
COPY Src/ Src/

# Build and publish application
WORKDIR /src/Src/MechanicShop.Api
RUN dotnet publish MechanicShop.Api.csproj -c Release -o /app/publish /p:UseAppHost=false /p:EnableRequestDelegateGenerator=false

# Stage 2: Runtime Environment
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# OCI Labels
LABEL org.opencontainers.image.title="MechanicShop.Api" \
      org.opencontainers.image.description="MechanicShop ASP.NET Core Web API with Blazor WASM" \
      org.opencontainers.image.version="1.0.0" \
      org.opencontainers.image.source="https://github.com/user/mechanicshop"

# Configure listening port
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

# Copy published binaries from publish stage
COPY --from=build /app/publish .

# Run as non-root user
USER app

ENTRYPOINT ["dotnet", "MechanicShop.Api.dll"]
