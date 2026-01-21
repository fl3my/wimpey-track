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
COPY ["WimpeyTrack.Api/WimpeyTrack.Api.csproj", "WimpeyTrack.Api/"]
RUN dotnet restore "WimpeyTrack.Api/WimpeyTrack.Api.csproj"
COPY . .
WORKDIR "/src/WimpeyTrack.Api"
RUN dotnet build "./WimpeyTrack.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./WimpeyTrack.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "WimpeyTrack.Api.dll"]
