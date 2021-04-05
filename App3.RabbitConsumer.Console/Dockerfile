FROM mcr.microsoft.com/dotnet/runtime:5.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["App3.RabbitConsumer.Console/App3.RabbitConsumer.Console.csproj", "App3.RabbitConsumer.Console/"]
RUN dotnet restore "App3.RabbitConsumer.Console/App3.RabbitConsumer.Console.csproj"
COPY . .
WORKDIR "/src/App3.RabbitConsumer.Console"
RUN dotnet build "App3.RabbitConsumer.Console.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "App3.RabbitConsumer.Console.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "App3.RabbitConsumer.Console.dll"]