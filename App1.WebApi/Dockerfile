FROM mcr.microsoft.com/dotnet/sdk:6.0-bullseye-slim AS build-env
WORKDIR /app

# Set intermediate stage as build
LABEL stage=app-build

# Copy csproj and restore dependencies
COPY App1.WebApi.csproj ./src/
RUN dotnet restore "./src/App1.WebApi.csproj"

# Copy everything, build and publish
COPY . ./src/
RUN dotnet publish src/*.csproj -c Release -o /app/publish


# Build runtime imagedock
FROM mcr.microsoft.com/dotnet/aspnet:6.0-bullseye-slim
WORKDIR /app
COPY --from=build-env /app/publish .
ENTRYPOINT ["dotnet", "App1.WebApi.dll"]