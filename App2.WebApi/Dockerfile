FROM mcr.microsoft.com/dotnet/sdk:5.0-buster-slim AS build-env
WORKDIR /app

# Set intermediate stage as build
LABEL stage=app-build

# Copy csproj and restore dependencies
COPY App2.WebApi.csproj ./src/
RUN dotnet restore "./src/App2.WebApi.csproj"

# Copy everything, build and publish
COPY . ./src/
RUN dotnet publish src/*.csproj -c Release -o /app/publish


# Build runtime imagedock
FROM mcr.microsoft.com/dotnet/aspnet:5.0-buster-slim
WORKDIR /app
COPY --from=build-env /app/publish .
ENTRYPOINT ["dotnet", "App2.WebApi.dll"]