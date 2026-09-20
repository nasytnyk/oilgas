# Міст Mosquitto -> RabbitMQ. Контекст збірки = КОРІНЬ репозиторію.
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY global.json ./
COPY src/Oilgas.MqttToRabbitMq/Oilgas.MqttToRabbitMq.csproj src/Oilgas.MqttToRabbitMq/
RUN dotnet restore src/Oilgas.MqttToRabbitMq/Oilgas.MqttToRabbitMq.csproj

COPY src/ src/
RUN dotnet publish src/Oilgas.MqttToRabbitMq/Oilgas.MqttToRabbitMq.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/runtime:10.0 AS runtime
WORKDIR /app
COPY --from=build /app ./
ENTRYPOINT ["dotnet", "Oilgas.MqttToRabbitMq.dll"]
