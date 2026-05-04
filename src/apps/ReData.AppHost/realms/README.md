# Realm import для Keycloak

`AppHost` монтирует эту папку в Keycloak через `WithRealmImport("./realms", isReadOnly: true)`.

Чтобы зафиксировать текущий realm `redata` и пользователей:

1. Остановите `AppHost`.
2. Запустите export команду в контейнере Keycloak:

```powershell
docker exec <keycloak-container-name> /opt/keycloak/bin/kc.sh export `
  --realm redata `
  --dir /opt/keycloak/data/export `
  --users realm_file
```

3. Скопируйте файлы на хост:

```powershell
docker cp <keycloak-container-name>:/opt/keycloak/data/export/. ./src/apps/ReData.AppHost/realms
```

4. Проверьте, что в папке появились файлы вида:
- `redata-realm.json`
- `redata-users-0.json` (и дополнительные chunks, если есть)

После этого при следующем старте `AppHost` realm и пользователи загрузятся автоматически.
