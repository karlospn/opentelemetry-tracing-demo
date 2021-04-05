FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["App1.WebApi/App1.WebApi.csproj", "App1.WebApi/"]
RUN dotnet restore "App1.WebApi/App1.WebApi.csproj"
COPY . .
WORKDIR "/src/App1.WebApi"
RUN dotnet build "App1.WebApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "App1.WebApi.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "App1.WebApi.dll"]