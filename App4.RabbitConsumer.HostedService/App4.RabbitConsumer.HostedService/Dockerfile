FROM mcr.microsoft.com/dotnet/runtime:5.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["App4.RabbitConsumer.HostedService/App4.RabbitConsumer.HostedService/App4.RabbitConsumer.HostedService.csproj", "App4.RabbitConsumer.HostedService/App4.RabbitConsumer.HostedService/"]
RUN dotnet restore "App4.RabbitConsumer.HostedService/App4.RabbitConsumer.HostedService/App4.RabbitConsumer.HostedService.csproj"
COPY . .
WORKDIR "/src/App4.RabbitConsumer.HostedService/App4.RabbitConsumer.HostedService"
RUN dotnet build "App4.RabbitConsumer.HostedService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "App4.RabbitConsumer.HostedService.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "App4.RabbitConsumer.HostedService.dll"]