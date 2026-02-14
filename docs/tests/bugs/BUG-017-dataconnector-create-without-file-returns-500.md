# BUG-017: `POST /api/data-connectors` без файла падает `500`

## Описание что не так
- Endpoint создания data connector должен требовать multipart-файл `file`.
- Если отправить multipart без `file` (например, только `foo=bar`), сервер падает с `500`.
- В ответ утекает стек с `ArgumentNullException` (`stream` is null).

## Шаги воспроизведения
- Вызвать:
  - `POST /api/data-connectors?name=QA_dc_nofile3&separator=,&withHeader=true`
  - multipart body: `foo=bar` (без `file`).
- Получить `500` со stack trace.

## Ожидаемое поведение
- Возвращать `400` с валидационной ошибкой о том, что `file` обязателен.

## Возвращенный ответ API
- `Status 500`
- Ошибка: `System.ArgumentNullException: Value cannot be null. (Parameter 'stream')`

## Ссылка на созданный во время тест-сессии набор
- `http://localhost:5223/api/datasets/19c1282a-5624-463c-b004-a3f149b03054`
