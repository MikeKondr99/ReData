FROM mcr.microsoft.com/dotnet/sdk:9.0 AS dotnet-build
WORKDIR /app

# Frontend is built from dotnet publish when FrontendFlavor is specified.
ARG FRONTEND_FLAVOR=none

# Node is required when FrontendFlavor=angular/svelte.
RUN apt-get update \
    && apt-get install -y --no-install-recommends curl ca-certificates gnupg \
    && mkdir -p /etc/apt/keyrings \
    && curl -fsSL https://deb.nodesource.com/gpgkey/nodesource-repo.gpg.key \
    | gpg --dearmor -o /etc/apt/keyrings/nodesource.gpg \
    && echo "deb [signed-by=/etc/apt/keyrings/nodesource.gpg] https://deb.nodesource.com/node_22.x nodistro main" \
    > /etc/apt/sources.list.d/nodesource.list \
    && apt-get update \
    && apt-get install -y --no-install-recommends nodejs \
    && rm -rf /var/lib/apt/lists/*

COPY src/ ./src

RUN dotnet restore src/ReData.DemoApp/ReData.DemoApp.csproj

RUN dotnet publish src/ReData.DemoApp/ReData.DemoApp.csproj \
    -c Release \
    --no-restore \
    -o out \
    -p:WebRootPath=wwwroot \
    -p:FrontendFlavor=$FRONTEND_FLAVOR

FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app

COPY --from=dotnet-build /app/out .

EXPOSE 8080

ENTRYPOINT ["dotnet", "ReData.DemoApp.dll"]