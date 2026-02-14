# BUG-021: Дубликат имени data connector приводит к `500` и stack trace

## Описание что не так
- `POST /api/data-connectors` при повторном `name` падает `500`.
- В ответ уходит `DbUpdateException`/`PostgresException` по уникальному индексу `IX_DataConnectors_Name`.
- Конфликт бизнес-данных обрабатывается как внутренняя ошибка сервера.

## Шаги воспроизведения
- Взять существующее имя data connector (например, `QA_dc_ok`).
- Вызвать `POST /api/data-connectors?...` с тем же `name` и валидным файлом.
- Получить `500` со стеком EF/Npgsql.

## Ожидаемое поведение
- Возвращать управляемую бизнес-ошибку (`409` или `400`) без stack trace.

## Возвращенный ответ API
- `Status 500`
- Текст: `duplicate key value violates unique constraint "IX_DataConnectors_Name"` + stack trace.

## Ссылка на созданный во время тест-сессии набор
- `http://localhost:5223/api/datasets/b73c7341-e26f-4240-a840-f26425a3456d`
