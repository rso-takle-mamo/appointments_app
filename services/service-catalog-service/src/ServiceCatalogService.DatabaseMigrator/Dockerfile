FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["ServiceCatalogService.DatabaseMigrator/ServiceCatalogService.DatabaseMigrator.csproj", "ServiceCatalogService.DatabaseMigrator/"]
RUN dotnet restore "ServiceCatalogService.DatabaseMigrator/ServiceCatalogService.DatabaseMigrator.csproj"
COPY . .
WORKDIR "/src/ServiceCatalogService.DatabaseMigrator"
RUN dotnet build "./ServiceCatalogService.DatabaseMigrator.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./ServiceCatalogService.DatabaseMigrator.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ServiceCatalogService.DatabaseMigrator.dll"]
