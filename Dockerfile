# syntax=docker/dockerfile:1.7

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS restore
WORKDIR /src
COPY AuthService_SB.sln ./
COPY src/AuthService_SB.Domain/AuthService_SB.Domain.csproj src/AuthService_SB.Domain/
COPY src/AuthService_SB.Application/AuthService_SB.Application.csproj src/AuthService_SB.Application/
COPY src/AuthService_SB.Persistence/AuthService_SB.Persistence.csproj src/AuthService_SB.Persistence/
COPY src/AuthService_SB.Api/AuthService_SB.Api.csproj src/AuthService_SB.Api/
COPY src/AuthService_SB.Tests/AuthService_SB.Tests.csproj src/AuthService_SB.Tests/
RUN dotnet restore AuthService_SB.sln

FROM restore AS build
COPY . .
RUN dotnet publish src/AuthService_SB.Api/AuthService_SB.Api.csproj \
    --configuration Release \
    --output /app/publish \
    /p:UseAppHost=false

FROM restore AS development
WORKDIR /src
COPY . .
ENV ASPNETCORE_URLS=http://+:5127
ENV ASPNETCORE_ENVIRONMENT=Development
EXPOSE 5127
CMD ["dotnet", "watch", "--project", "src/AuthService_SB.Api/AuthService_SB.Api.csproj", "run", "--urls", "http://+:5127"]

# Kept last on purpose: builders that don't get an explicit --target (e.g.
# Railway's `railway up` without a target set in railway.toml) build the
# final stage in the file by default, and that must be `production`, not
# the `dotnet watch` dev stage above.
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS production
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:5127
ENV ASPNETCORE_ENVIRONMENT=Production
RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*
COPY --from=build /app/publish .
# AddSecurityPolicies() persists DataProtection keys to ./keys at startup;
# create it with the right ownership now, since $APP_UID has no write access
# to /app (owned by root from the COPY above) once we drop privileges below.
RUN mkdir -p /app/keys && chown -R $APP_UID:$APP_UID /app/keys
EXPOSE 5127
USER $APP_UID
ENTRYPOINT ["dotnet", "AuthService_SB.Api.dll"]
