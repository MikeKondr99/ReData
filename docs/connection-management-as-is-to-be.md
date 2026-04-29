# Управление подключениями в ReData DemoApp (AS-IS -> TO-BE)

## Контекст
После перевода `/api/transform` на потоковую выдачу (`IAsyncEnumerable`) жизненный цикл `DbConnection` и `DbDataReader` стал длиннее и теперь частично живет до завершения HTTP-ответа. Это правильно для стриминга, но текущие абстракции управления соединениями фрагментированы.

Важно: `Connector` и `DbConnection` в этой модели не одно и то же.
- `Connector` — доменная сущность-источник данных (сейчас в основном CSV/таблица в DWH после загрузки, в будущем может быть внешний источник).
- `ConnectionService` — инфраструктурный сервис, который открывает/закрывает физические подключения к конкретной БД для выполнения этапа пайплайна.

## AS-IS (сейчас)

### Что уже хорошо
- Для стрим-ответов `TransformEndpoint` не материализует данные в `IValue`, а отдает `IAsyncEnumerable`.
- Для длинного ответа соединение и ридер корректно регистрируются на disposal через `HttpContext.Response.RegisterForDisposeAsync(...)`.
- В большинстве write/short-read сценариев используются `await using`, что снижает риск утечек.

### Что сейчас проблемно
- Создание соединений размазано по коду (`new NpgsqlConnection(...)` в командах, эндпоинтах, job).
- Политика времени жизни ресурса не централизована: где-то соединение закрывает команда, где-то endpoint, где-то `finally` внутри iterator.
- `DwhService` смешивает ответственность:
  - хранит connection strings,
  - строит `QueryBuilder` (метаданные),
  - частично участвует в runtime трансформаций.
- Нет явной модели «этапа выполнения и целевой БД подключения» (сейчас по сути hardcoded DWH read/write).
- Для будущих пользовательских подключений (multi-connector/multi-tenant) не хватает слоя маршрутизации и кэширования provider-фабрик.

## Оценка идеи `Scoped DbConnectionService`

Идея: `ConnectionService.GetReadConnection(ConnectionSource.Dwh)` в scope запроса + автозакрытие ресурсов в конце scope.

### Плюсы
- Сильно упрощает код потребителей (команды/эндпоинты).
- Единая точка для политик открытия, retry, таймаутов, тегирования, обсервабилити.
- Хорошо ложится на схему «открыть -> выполнить -> закрыть», что нормально при pooling у Npgsql.

### Минусы и риски
- Если реализовать как «один shared connection на scope», это создаст скрытую связанность:
  - нельзя безопасно делать параллельные запросы на одном соединении,
  - long-lived stream может блокировать остальные операции scope.
- Для стриминга lifetime соединения должен быть привязан к lifetime ответа, а не только к lifetime команды.
- Авто-dispose «в конце запроса» полезен как safety net, но не должен заменять явное владение в местах стриминга.

### Вывод по идее
Идея правильная, но сервис должен быть **factory/lease-oriented**, а не «контейнер с одним соединением на scope».

## TO-BE (целевая модель)

### 1) Разделить обязанности
- `IConnectionTargetResolver`:
  - по контексту выполнения возвращает целевую БД и режим доступа.
  - пример контекста: `ExecutionStage.TransformRead`, `ExecutionStage.IngestWrite`, `ExecutionStage.ExternalRead`.
- `IConnectionStringResolver`:
  - по target возвращает строку подключения и provider (Postgres/MySql/...).
- `IQueryConnectionFactory`:
  - создает `DbConnection` под read/write и нужный provider.
- `ConnectionLease` (`IAsyncDisposable`):
  - обертка над `DbConnection` + мета (source, mode, openedAt, tags).

### 2) Явная модель владения
- Любой слой, который получает lease, обязан его закрыть (`await using` или `RegisterForDisposeAsync`).
- Для стрим-ответов lease передается в response pipeline и живет до конца отправки.
- Для коротких операций lease закрывается в том же методе.

### 3) Команды и endpoint
- Команда `ExecuteQuery` должна возвращать не «сырой `NpgsqlConnection`», а `ConnectionLease` + `DomainDbDataReader`.
- Тип результата лучше сделать provider-agnostic (`DbConnection`, не `NpgsqlConnection`) на границе DemoApp.

### 4) Поддержка будущих источников
- Не связывать напрямую `ConnectorId -> DbConnection`.
- На этапе transform по текущей модели `Connector` может оставаться доменной сущностью, а физическое подключение идти в DWH.
- После появления внешних источников резолвер target выбирает:
  - до загрузки: внешняя БД/источник,
  - после загрузки/нормализации: DWH.

## Рекомендованный план внедрения

### Этап 1 (без поломок API)
- Вынести `new NpgsqlConnection(...)` в `IQueryConnectionFactory`.
- Перевести текущие места на фабрику без изменения контрактов endpoint.
- Добавить метрики по lease lifecycle (создано/открыто/закрыто/длительность).

### Этап 2 (потоковые сценарии)
- Заменить возврат `NpgsqlConnection` на lease в `ExecuteQueryCommandResult`.
- Зафиксировать единый паттерн:
  - stream: `RegisterForDisposeAsync(reader)` + `RegisterForDisposeAsync(lease)`;
  - non-stream: `await using`.

### Этап 3 (multi-source)
- Добавить `ExecutionStage -> ConnectionTarget` resolver.
- Сохранить текущий default: для transform/read путь идет через DWH.
- Для внешних источников добавить отдельный stage (без ломки текущего пути).

### Этап 4 (чистка слоев)
- Убрать из `DwhService` построение `QueryBuilder` в отдельный сервис метаданных/коннектора.
- `DwhService` оставить только как legacy adapter или удалить после миграции.

## Что делать с вашим опасением про «долгий scope»
Это валидное опасение. Решение:
- не держать singleton-connection внутри scope,
- выдавать независимые leases,
- scope использовать только для резолверов/фабрик, а не для хранения активных соединений.

## Дополнительно по производительности
- Для read-only stream целесообразно выставлять provider-специфичные `CommandBehavior.SequentialAccess` там, где это безопасно.
- С учетом ваших замеров (stream << array по памяти) приоритет — сохранить streaming lifecycle и не возвращаться к materialize.

## Вопросы для фиксации дизайна
## Зафиксированные решения на сейчас
1. Параллельные запросы внутри одного HTTP-запроса считаем редким/отдельным сценарием, в базовый `ConnectionService` не закладываем.
2. В будущем появится доработка `Connector` или отдельная `Source` сущность с пользовательским `ConnectionString`.
3. Write-операции с высокой вероятностью остаются отдельными сценариями, не смешиваются с long-running stream.
4. Единая retry/timeout policy пока не обязательна.
5. Breaking changes допустимы (pet project), можно менять контракты без спринтовых ограничений.
