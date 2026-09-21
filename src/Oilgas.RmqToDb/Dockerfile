# DbWriter: споживач RabbitMQ -> Azure SQL (EF Core). Контекст збірки = КОРІНЬ репозиторію.
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY global.json ./
COPY src/Oilgas.Model/Oilgas.Model.csproj               src/Oilgas.Model/
COPY src/Oilgas.Ef/Oilgas.Ef.csproj         src/Oilgas.Ef/
COPY src/Oilgas.RmqToDb/Oilgas.RmqToDb.csproj         src/Oilgas.RmqToDb/
RUN dotnet restore src/Oilgas.RmqToDb/Oilgas.RmqToDb.csproj

COPY src/ src/
RUN dotnet publish src/Oilgas.RmqToDb/Oilgas.RmqToDb.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/runtime:10.0 AS runtime
WORKDIR /app
COPY --from=build /app ./
ENTRYPOINT ["dotnet", "Oilgas.RmqToDb.dll"]
