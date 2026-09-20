# RawLog: споживач RabbitMQ + веб-морда. Контекст збірки = КОРІНЬ репозиторію.
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY global.json ./
COPY src/Oilgas.RawLog/Oilgas.RawLog.csproj src/Oilgas.RawLog/
RUN dotnet restore src/Oilgas.RawLog/Oilgas.RawLog.csproj

COPY src/ src/
RUN dotnet publish src/Oilgas.RawLog/Oilgas.RawLog.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app ./
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "Oilgas.RawLog.dll"]
