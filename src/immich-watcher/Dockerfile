# Create base image with NPM and immich cli
FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
RUN apt update && apt install -y curl
RUN curl -fsSL https://deb.nodesource.com/setup_18.x | bash - && apt-get install -y nodejs
RUN npm i -g @immich/cli
USER $APP_UID
WORKDIR /app

# Build source 
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG TARGETARCH
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["immich-watcher/immich-watcher.csproj", "immich-watcher/"]
RUN dotnet restore "immich-watcher/immich-watcher.csproj" -a $TARGETARCH 
COPY . .
WORKDIR "/src/immich-watcher"
RUN dotnet publish "immich-watcher.csproj" -c $BUILD_CONFIGURATION -o /app/publish  -a $TARGETARCH  

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "immich-watcher.dll"]
