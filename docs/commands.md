
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

## Генерация большого CSV для нагрузочного POC

Быстрая генерация `1_000_000` строк и `20` колонок в потоковом режиме (без накопления в памяти):

```powershell
$out = "C:\temp\bench_1m_20cols.csv"
$rows = 1000000
$cols = 20

$sw = [System.IO.StreamWriter]::new($out, $false, [System.Text.UTF8Encoding]::new($false), 1MB)
try {
  $header = 1..$cols | ForEach-Object { "c$_" }
  $sw.WriteLine(($header -join ","))

  for ($i = 1; $i -le $rows; $i++) {
    $vals = [string[]]::new($cols)
    $vals[0]  = $i
    $vals[1]  = ($i % 1000)
    $vals[2]  = [string]::Format([System.Globalization.CultureInfo]::InvariantCulture, "{0:F4}", ($i * 0.01))
    $vals[3]  = "name_$i"
    $vals[4]  = (($i % 2) -eq 0).ToString().ToLowerInvariant()
    $vals[5]  = (Get-Date "2024-01-01").AddSeconds($i).ToString("O")
    $vals[6]  = ($i % 7)
    $vals[7]  = [string]::Format([System.Globalization.CultureInfo]::InvariantCulture, "{0:F6}", ($i * 0.001))
    $vals[8]  = "group_$($i % 100)"
    $vals[9]  = (($i % 3) -eq 0).ToString().ToLowerInvariant()
    $vals[10] = ($i * 2)
    $vals[11] = ($i * 3)
    $vals[12] = ($i % 11)
    $vals[13] = [string]::Format([System.Globalization.CultureInfo]::InvariantCulture, "{0:F2}", ($i / 3.0))
    $vals[14] = "city_$($i % 500)"
    $vals[15] = (($i % 5) -eq 0).ToString().ToLowerInvariant()
    $vals[16] = (1000000 - $i)
    $vals[17] = [string]::Format([System.Globalization.CultureInfo]::InvariantCulture, "{0:F3}", [Math]::Sqrt($i))
    $vals[18] = "tag_$($i % 1000)"
    $vals[19] = (($i % 13) -eq 0).ToString().ToLowerInvariant()

    $sw.WriteLine(($vals -join ","))
  }
}
finally {
  $sw.Dispose()
}

Get-Item $out | Select-Object FullName,Length,LastWriteTime
```
