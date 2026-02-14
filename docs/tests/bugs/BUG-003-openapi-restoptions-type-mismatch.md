# BUG-003: Несоответствие OpenAPI и фактического типа `restOptions`

## Описание что не так
- В OpenAPI `SelectTransformation.restOptions` описан как enum integer.
- В ответе `GET /api/datasets/{id}` API возвращает строковое значение (`"Delete"` / `"NoAction"`).
- Из-за этого строгие клиенты, сгенерированные по схеме, могут падать на десериализации.

## Шаги воспроизведения
- Создать набор с `select` и `restOptions = 2`.
- Получить набор через `GET /api/datasets/{id}`.
- Увидеть `restOptions: "Delete"` вместо integer.

## Ожидаемое поведение
- Либо схема и ответы должны быть integer.
- Либо схема должна быть string enum, если API возвращает строки.

## Возвращенный ответ API
- Status `200` на `GET /api/datasets/{id}`.
- Фрагмент body: `"restOptions":"Delete"` (при том, что по OpenAPI ожидается integer enum).

## Ссылка на созданный во время тест-сессии набор
- `http://localhost:5223/api/datasets/632247a5-38e8-42b3-a8c6-d326452d88e3`
