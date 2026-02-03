FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Install Libreoffice for pdf conversion
RUN apt-get update && apt-get install -y --no-install-recommends \
    libreoffice-core \
    libreoffice-calc \
    && rm -rf /var/lib/apt/lists/* 

# build frontend
FROM node:25-alpine AS frontend-build
WORKDIR /frontend

# Install deps (cache-friendly)
COPY frontend/package*.json ./
RUN npm ci

# Copy frontend source
COPY frontend/ .
RUN npm run build

# Build backend
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["backend/WimpeyTrack.Api/WimpeyTrack.Api.csproj", "./"]
RUN dotnet restore "WimpeyTrack.Api.csproj"
COPY backend/WimpeyTrack.Api/ .
WORKDIR "/src"
RUN dotnet build "WimpeyTrack.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "WimpeyTrack.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# frontend copy to wwwroot
RUN mkdir -p /app/wwwroot
COPY --from=frontend-build /frontend/dist /app/wwwroot

# Copy the seed data
RUN mkdir -p /app/seed
COPY backend/WimpeyTrack.Api/Data/SeedData/central-belt-locations.tsv /app/seed/central-belt-locations.tsv

ENTRYPOINT ["dotnet", "WimpeyTrack.Api.dll"]
