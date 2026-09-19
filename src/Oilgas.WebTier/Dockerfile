# Багатоетапна збірка WebTier. Контекст збірки = КОРІНЬ репозиторію
# (щоб бачити global.json і всі проекти графа залежностей).

# ── етап 1: build (повний SDK) ──
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# спершу лише .csproj — щоб шар restore кешувався й не перезбирався без потреби
COPY global.json ./
COPY src/Oilgas.Domain/Oilgas.Domain.csproj       src/Oilgas.Domain/
COPY src/Oilgas.Contracts/Oilgas.Contracts.csproj src/Oilgas.Contracts/
COPY src/Oilgas.UseCases/Oilgas.UseCases.csproj   src/Oilgas.UseCases/
COPY src/Oilgas.Postgres/Oilgas.Postgres.csproj   src/Oilgas.Postgres/
COPY src/Oilgas.RabbitMq/Oilgas.RabbitMq.csproj   src/Oilgas.RabbitMq/
COPY src/Oilgas.Redis/Oilgas.Redis.csproj         src/Oilgas.Redis/
COPY src/Oilgas.WebTier/Oilgas.WebTier.csproj     src/Oilgas.WebTier/
RUN dotnet restore src/Oilgas.WebTier/Oilgas.WebTier.csproj

# потім увесь код і publish
COPY src/ src/
RUN dotnet publish src/Oilgas.WebTier/Oilgas.WebTier.csproj -c Release -o /app --no-restore

# ── етап 2: runtime (легкий ASP.NET-образ, без SDK) ──
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app ./
# Container Apps очікує, що застосунок слухає визначений порт
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "Oilgas.WebTier.dll"]
