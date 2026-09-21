# Hardware: headless-симулятор (Worker, без HTTP). Контекст збірки = КОРІНЬ репозиторію.
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY global.json ./
COPY src/Oilgas.Model/Oilgas.Model.csproj       src/Oilgas.Model/
COPY src/Oilgas.Hardware/Oilgas.Hardware.csproj src/Oilgas.Hardware/
RUN dotnet restore src/Oilgas.Hardware/Oilgas.Hardware.csproj

COPY src/ src/
RUN dotnet publish src/Oilgas.Hardware/Oilgas.Hardware.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/runtime:10.0 AS runtime
WORKDIR /app
COPY --from=build /app ./
ENTRYPOINT ["dotnet", "Oilgas.Hardware.dll"]
