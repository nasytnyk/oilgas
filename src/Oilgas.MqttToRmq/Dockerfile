# Міст Mosquitto -> RabbitMQ. Контекст збірки = КОРІНЬ репозиторію.
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY global.json ./
COPY src/Oilgas.MqttToRmq/Oilgas.MqttToRmq.csproj src/Oilgas.MqttToRmq/
RUN dotnet restore src/Oilgas.MqttToRmq/Oilgas.MqttToRmq.csproj

COPY src/ src/
RUN dotnet publish src/Oilgas.MqttToRmq/Oilgas.MqttToRmq.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/runtime:10.0 AS runtime
WORKDIR /app
COPY --from=build /app ./
ENTRYPOINT ["dotnet", "Oilgas.MqttToRmq.dll"]
