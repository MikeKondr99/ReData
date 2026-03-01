
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