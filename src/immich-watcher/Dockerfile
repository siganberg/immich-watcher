# Create base image with NPM and immich cli
FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
RUN apt update && apt install -y curl
RUN curl -fsSL https://deb.nodesource.com/setup_18.x | bash - && apt-get install -y nodejs
RUN npm i -g @immich/cli
USER $APP_UID
WORKDIR /app

# Build immich watcher
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:8.0 AS build

ARG TARGETARCH
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["immich-watcher/immich-watcher.csproj", "immich-watcher/"]

RUN if [ "$TARGETPLATFORM" = "linux/arm64 " ] ; then DOTNET_TARGET=linux-arm64 ; else DOTNET_TARGET=linux-x64 ; fi \
    && echo $DOTNET_TARGET > /tmp/rid

RUN dotnet restore "immich-watcher/immich-watcher.csproj" -a $TARGETARCH 
#-r $(cat /tmp/rid) /p:PublishReadyToRun=true
COPY . .
WORKDIR "/src/immich-watcher"
RUN dotnet build "immich-watcher.csproj" -c $BUILD_CONFIGURATION -o /app/build -a $TARGETARCH  
#-r $(cat /tmp/rid) --self-contained true --no-restore /p:PublishTrimmed=true /p:PublishReadyToRun=true /p:PublishSingleFile=true

# Package image watcher
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "immich-watcher.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "immich-watcher.dll"]
