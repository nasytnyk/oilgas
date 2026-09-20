# WebTier: GraphQL (HotChocolate) + React-консоль + RabbitMQ consumer + Azure SQL read.
# Контекст збірки = КОРІНЬ репозиторію.
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY global.json ./
COPY src/Oilgas.Model/Oilgas.Model.csproj               src/Oilgas.Model/
COPY src/Oilgas.SqlServer/Oilgas.SqlServer.csproj       src/Oilgas.SqlServer/
COPY src/Oilgas.Ui/Oilgas.Ui.csproj           src/Oilgas.Ui/
RUN dotnet restore src/Oilgas.Ui/Oilgas.Ui.csproj

COPY src/ src/
RUN dotnet publish src/Oilgas.Ui/Oilgas.Ui.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app ./
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "Oilgas.Ui.dll"]
