FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Install Libreoffice for pdf conversion
RUN apt-get update && apt-get install -y \
    libreoffice-core \
    libreoffice-calc \
    libreoffice-common \
    && rm -rf /var/lib/apt/lists/* 

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["WimpeyTrack.Api.csproj", "./"]
RUN dotnet restore "WimpeyTrack.Api.csproj"
COPY . .
WORKDIR "/src"
RUN dotnet build "./WimpeyTrack.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./WimpeyTrack.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Copy the seed data
COPY Data/SeedData/central-belt-locations.tsv /app/Data/SeedData/central-belt-locations.tsv

RUN mkdir -p /app/wwwroot

ENTRYPOINT ["dotnet", "WimpeyTrack.Api.dll"]
