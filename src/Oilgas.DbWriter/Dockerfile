# DbWriter: споживач RabbitMQ -> Azure SQL (EF Core). Контекст збірки = КОРІНЬ репозиторію.
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY global.json ./
COPY src/Oilgas.Contracts/Oilgas.Contracts.csproj       src/Oilgas.Contracts/
COPY src/Oilgas.Model/Oilgas.Model.csproj               src/Oilgas.Model/
COPY src/Oilgas.Measurements/Oilgas.Measurements.csproj src/Oilgas.Measurements/
COPY src/Oilgas.UseCases/Oilgas.UseCases.csproj         src/Oilgas.UseCases/
COPY src/Oilgas.SqlServer/Oilgas.SqlServer.csproj         src/Oilgas.SqlServer/
COPY src/Oilgas.DbWriter/Oilgas.DbWriter.csproj         src/Oilgas.DbWriter/
RUN dotnet restore src/Oilgas.DbWriter/Oilgas.DbWriter.csproj

COPY src/ src/
RUN dotnet publish src/Oilgas.DbWriter/Oilgas.DbWriter.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/runtime:10.0 AS runtime
WORKDIR /app
COPY --from=build /app ./
ENTRYPOINT ["dotnet", "Oilgas.DbWriter.dll"]
