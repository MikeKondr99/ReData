# BUG-013: `export` с числовым `fileType` вне enum возвращает `500` вместо `400`

## Описание что не так
- Для `GET /api/datasets/{id}/export?fileType=...`:
  - строковый невалидный `fileType=abc` корректно дает `400`;
  - числовые значения вне enum (`-1`, `3`, `99`) приводят к `500`.
- Это непоследовательная валидация одного и того же параметра.

## Шаги воспроизведения
- Взять валидный набор.
- Вызвать `export` с `fileType=-1`, `fileType=3`, `fileType=99`.
- Получить `500`.
- Вызвать `export` с `fileType=abc`.
- Получить `400` с валидационной ошибкой.

## Ожидаемое поведение
- Любой невалидный `fileType` должен возвращать `400` с понятной ошибкой валидации.

## Возвращенный ответ API
- `fileType=-1`: `Status 500`, body пустой.
- `fileType=3`: `Status 500`, body пустой.
- `fileType=99`: `Status 500`, body пустой.
- `fileType=abc`: `Status 400`, `Value [abc] is not valid for a [ExportFileType] property!`.

## Ссылка на созданный во время тест-сессии набор
- `http://localhost:5223/api/datasets/31ee1deb-9826-4b43-8d1c-83a2c573087d`
