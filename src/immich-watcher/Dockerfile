# Build source 
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG TARGETARCH
ARG BUILD_CONFIGURATION=Release
COPY . /src
RUN dotnet publish "src/immich-watcher/immich-watcher.csproj" -c $BUILD_CONFIGURATION -o /app/publish  -a $TARGETARCH  

# Create final runtime image with NPM and immich-wather
FROM mcr.microsoft.com/dotnet/runtime:8.0 AS final
RUN apt update && apt install -y curl
RUN curl -fsSL https://deb.nodesource.com/setup_20.x | bash - && apt-get install -y nodejs
RUN npm i -g @immich/cli
RUN mkdir /data
USER $APP_UID
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "immich-watcher.dll"]
