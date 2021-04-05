FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["App2.WebApi/App2.WebApi.csproj", "App2.WebApi/"]
RUN dotnet restore "App2.WebApi/App2.WebApi.csproj"
COPY . .
WORKDIR "/src/App2.WebApi"
RUN dotnet build "App2.WebApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "App2.WebApi.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "App2.WebApi.dll"]