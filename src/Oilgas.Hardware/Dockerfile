# Багатоетапна збірка Hardware. Контекст збірки = КОРІНЬ репозиторію.
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY global.json ./
COPY src/Oilgas.Contracts/Oilgas.Contracts.csproj       src/Oilgas.Contracts/
COPY src/Oilgas.Measurements/Oilgas.Measurements.csproj src/Oilgas.Measurements/
COPY src/Oilgas.Hardware/Oilgas.Hardware.csproj         src/Oilgas.Hardware/
RUN dotnet restore src/Oilgas.Hardware/Oilgas.Hardware.csproj

COPY src/ src/
RUN dotnet publish src/Oilgas.Hardware/Oilgas.Hardware.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app ./
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "Oilgas.Hardware.dll"]
