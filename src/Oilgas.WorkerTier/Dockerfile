# Міст Mosquitto -> RabbitMQ. Контекст збірки = КОРІНЬ репозиторію.
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY global.json ./
COPY src/Oilgas.WorkerTier/Oilgas.WorkerTier.csproj src/Oilgas.WorkerTier/
RUN dotnet restore src/Oilgas.WorkerTier/Oilgas.WorkerTier.csproj

COPY src/ src/
RUN dotnet publish src/Oilgas.WorkerTier/Oilgas.WorkerTier.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/runtime:10.0 AS runtime
WORKDIR /app
COPY --from=build /app ./
ENTRYPOINT ["dotnet", "Oilgas.WorkerTier.dll"]
