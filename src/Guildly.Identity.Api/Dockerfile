FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["src/Guildly.Identity.Api/Guildly.Identity.Api.csproj", "src/Guildly.Identity.Api/"]
COPY ["src/Guildly.Identity.Application/Guildly.Identity.Application.csproj", "src/Guildly.Identity.Application/"]
COPY ["src/Guildly.Identity.Domain/Guildly.Identity.Domain.csproj", "src/Guildly.Identity.Domain/"]
COPY ["src/Guildly.Identity.Infrastructure/Guildly.Identity.Infrastructure.csproj", "src/Guildly.Identity.Infrastructure/"]
COPY ["src/Common/Guildly.Common.Api/Guildly.Common.Api.csproj", "src/Common/Guildly.Common.Api/"]
COPY ["src/Common/Guildly.Common.Application/Guildly.Common.Application.csproj", "src/Common/Guildly.Common.Application/"]
COPY ["src/Common/Guildly.Common.Database/Guildly.Common.Database.csproj", "src/Common/Guildly.Common.Database/"]
COPY ["src/Common/Guildly.Common.Domain/Guildly.Common.Domain.csproj", "src/Common/Guildly.Common.Domain/"]
COPY ["src/Common/Guildly.Common/Guildly.Common.csproj", "src/Common/Guildly.Common/"]

RUN dotnet restore "src/Guildly.Identity.Api/Guildly.Identity.Api.csproj" --disable-parallel

COPY . .
RUN dotnet publish "src/Guildly.Identity.Api/Guildly.Identity.Api.csproj" \
    -c Release \
    -o /app/publish \
    --no-restore \
    -m:1 \
    /p:UseAppHost=false

FROM runtime AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Guildly.Identity.Api.dll"]
