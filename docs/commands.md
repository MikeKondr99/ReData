
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
