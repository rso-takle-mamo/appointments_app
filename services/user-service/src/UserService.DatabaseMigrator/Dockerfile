FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["UserService.DatabaseMigrator/UserService.DatabaseMigrator.csproj", "UserService.DatabaseMigrator/"]
RUN dotnet restore "UserService.DatabaseMigrator/UserService.DatabaseMigrator.csproj"
COPY . .
WORKDIR "/src/UserService.DatabaseMigrator"
RUN dotnet build "./UserService.DatabaseMigrator.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./UserService.DatabaseMigrator.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "UserService.DatabaseMigrator.dll"]
