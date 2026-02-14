# BUG-014: `null` в `transformations` приводит к необработанным `500`

## Описание что не так
- API не валидирует `null`-элементы/`null`-трансформации на уровне запроса и падает с `500`.
- В ответе утечки внутренних деталей:
  - `NullReferenceException` в endpoint-слое;
  - `DbUpdateException`/`PostgresException` по constraint БД.

## Шаги воспроизведения
- `POST /api/datasets` с `transformations: [null]` -> `500`.
- `PUT /api/datasets/{id}` с `transformations: [null]` -> `500`.
- `PUT /api/datasets/{id}` с `transformations: [{ enabled: true, transformation: null }]` -> `500`.

## Ожидаемое поведение
- Возвращать `400` с явной ошибкой валидации входных данных.
- Не допускать падений с stack trace/деталями БД.

## Возвращенный ответ API
- `POST` с `[null]`: `Status 500`, `NullReferenceException` (`CreateDatasetEndpoint`).
- `PUT` с `[null]`: `Status 500`, `NullReferenceException` (`UpdateDatasetEndpoint`).
- `PUT` с `transformation: null`: `Status 500`, `DbUpdateException` + `PostgresException 23502` (`Transformations.Data` not-null).

## Ссылка на созданный во время тест-сессии набор
- `http://localhost:5223/api/datasets/4fb83ef0-4180-404a-b1c2-b9652ca65b27`
