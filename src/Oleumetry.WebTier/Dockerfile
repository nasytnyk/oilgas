# Багатоетапна збірка WebTier. Контекст збірки = КОРІНЬ репозиторію
# (щоб бачити global.json і всі проекти графа залежностей).

# ── етап 1: build (повний SDK) ──
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# спершу лише .csproj — щоб шар restore кешувався й не перезбирався без потреби
COPY global.json ./
COPY src/Oleumetry.Domain/Oleumetry.Domain.csproj       src/Oleumetry.Domain/
COPY src/Oleumetry.Contracts/Oleumetry.Contracts.csproj src/Oleumetry.Contracts/
COPY src/Oleumetry.UseCases/Oleumetry.UseCases.csproj   src/Oleumetry.UseCases/
COPY src/Oleumetry.Postgres/Oleumetry.Postgres.csproj   src/Oleumetry.Postgres/
COPY src/Oleumetry.RabbitMq/Oleumetry.RabbitMq.csproj   src/Oleumetry.RabbitMq/
COPY src/Oleumetry.Redis/Oleumetry.Redis.csproj         src/Oleumetry.Redis/
COPY src/Oleumetry.WebTier/Oleumetry.WebTier.csproj     src/Oleumetry.WebTier/
RUN dotnet restore src/Oleumetry.WebTier/Oleumetry.WebTier.csproj

# потім увесь код і publish
COPY src/ src/
RUN dotnet publish src/Oleumetry.WebTier/Oleumetry.WebTier.csproj -c Release -o /app --no-restore

# ── етап 2: runtime (легкий ASP.NET-образ, без SDK) ──
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app ./
# Container Apps очікує, що застосунок слухає визначений порт
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "Oleumetry.WebTier.dll"]
