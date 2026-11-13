FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY src/ ./

RUN dotnet publish Guildly.Identity.Database.Migration.Job/Guildly.Identity.Database.Migration.Job.csproj \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

COPY --from=build /app/publish/ ./
COPY --from=build /src/Guildly.Identity.Database.Migration.Job/appsettings.json ./

ENTRYPOINT ["dotnet", "Guildly.Identity.Database.Migration.Job.dll"]