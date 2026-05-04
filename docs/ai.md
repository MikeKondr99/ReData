# AI Notes

## Обязательный контекст для ИИ
- При работе в этом репозитории сначала прочитать `AGENTS.md`, затем этот файл.

## Aspire запуск
- Основной сценарий разработки: запуск через AppHost.
- Команда: `dotnet run --project src/apps/ReData.AppHost`.
- Для запуска только API (без оркестрации Aspire): `dotnet run --project src/apps/ReData.DemoApp`.
- Angular фронт поднимается как npm ресурс `redata-angular` из AppHost.
- Для Angular в AppHost зарегистрирован HTTP endpoint через `PORT`, поэтому у ресурса есть External URL.
- Keycloak в AppHost поднят на стабильном порту `8080` (важно для предсказуемого OIDC в dev).

## Aspire MCP
- Проверка доступных MCP tools: `aspire mcp tools --apphost src/apps/ReData.AppHost/ReData.AppHost.csproj`.
- Если команда пишет `No running apphost found`, сначала запустить AppHost.
- Через AppHost публикуются resource MCP tools для БД ресурсов (`redata-mcp`, `dwh-mcp`, `tickerq-mcp`).

## ConnectionStrings при запуске через AppHost
- Имена connection string берутся из `AddDatabase("<name>")` в `src/apps/ReData.AppHost/AppHost.cs`.
- Сейчас используются:
  - `ConnectionStrings__redata`
  - `ConnectionStrings__tickerq`
  - `ConnectionStrings__dwh`
- Для кода, который ожидает `DwhRead`/`DwhWrite`, нужен fallback на `dwh` или явный маппинг переменных.

## Angular proxy в dev
- `src/apps/ReData.Angular/scripts/start.mjs` запускает `ng serve` с `--proxy-config proxy.conf.cjs`.
- `src/apps/ReData.Angular/proxy.conf.cjs` берет target API из env:
  - `services__redata_demoapp__http__0` или
  - `services__redata-demoapp__http__0`
- Fallback для локального запуска без AppHost: `http://localhost:5223`.
- Тот же proxy конфиг проксирует Keycloak через префикс `/kc`.
- Target Keycloak берется из `services__keycloak__http__0` или `KEYCLOAK_HTTP`.
- Предпочтительный источник target: `KEYCLOAK_HTTP` (проставляется из AppHost через `keycloak.GetEndpoint("http")`).
