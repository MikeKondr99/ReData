
# Полезные Команды

## Собрать докер файл и запустить

Эти команды нужны для проверки верного деплоя,
так как он происходит через Dockerfile

```
docker build . -t redata
```

```
docker run -p 8080:8080 redata
```

```
dotnet ef migrations add DataSetFkFix --project ./src/ReData.DemoApp --context ApplicationDatabaseContext    
```

## Frontend (SvelteKit)

Новый фронтенд: `src/ReData.Svelte` (SvelteKit + TypeScript + Tailwind + Prettier).

```
cd src/ReData.Svelte
npm install
npm run dev
```

```
cd src/ReData.Svelte
npm run check
npm run build
```

```
cd src/ReData.Svelte
npm run format
npm run format:check
```

## Publish DemoApp с выбором фронта

По умолчанию фронтенд не собирается:

```
dotnet publish src/ReData.DemoApp/ReData.DemoApp.csproj -c Release
```

Собрать и вложить Angular в `wwwroot`:

```
dotnet publish src/ReData.DemoApp/ReData.DemoApp.csproj -c Release -p:FrontendFlavor=angular
```

Собрать и вложить Svelte в `wwwroot`:

```
dotnet publish src/ReData.DemoApp/ReData.DemoApp.csproj -c Release -p:FrontendFlavor=svelte
```

## Docker с выбором фронта

```
docker build . -t redata --build-arg FRONTEND_FLAVOR=none
```

```
docker build . -t redata --build-arg FRONTEND_FLAVOR=angular
```

```
docker build . -t redata --build-arg FRONTEND_FLAVOR=svelte
```

## Aspire Dashboard (Standalone + MCP)

Запуск dashboard с OTLP (gRPC/HTTP) и MCP endpoint:

```powershell
docker rm -f redata-dashboard 2>$null
docker run -d --name redata-dashboard `
  -e "Dashboard__ApplicationName=ReData Телеметрия" `
  -e ASPIRE_DASHBOARD_UNSECURED_ALLOW_ANONYMOUS=true `
  -e ASPNETCORE_URLS=http://0.0.0.0:18888 `
  -e ASPIRE_DASHBOARD_OTLP_ENDPOINT_URL=http://0.0.0.0:18889 `
  -e ASPIRE_DASHBOARD_OTLP_HTTP_ENDPOINT_URL=http://0.0.0.0:18890 `
  -e ASPIRE_DASHBOARD_MCP_ENDPOINT_URL=http://0.0.0.0:18891 `
  -p 18888:18888 `
  -p 4317:18889 `
  -p 4318:18890 `
  -p 18891:18891 `
  mcr.microsoft.com/dotnet/aspire-dashboard:13
```

Проверить, что контейнер поднялся:

```powershell
docker ps --filter "name=redata-dashboard"
```

Проверить MCP endpoint:

```powershell
Invoke-WebRequest -UseBasicParsing http://localhost:18891/mcp
```

Остановить и удалить dashboard:

```powershell
docker rm -f redata-dashboard
```
