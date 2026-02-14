# BUG-018: `POST /api/transform` на `null`-структурах возвращает внутренние ошибки вместо валидации

## Описание что не так
- Для невалидных `null`-структур в `transformations` endpoint возвращает `400`, но с текстом:
  - `Непредвиденная ошибка при применении трансформаций: ...`
  - и внутренними исключениями (`Object reference...`, `Value cannot be null...`).
- Это не валидационная ошибка уровня контракта, а утечка внутренних причин выполнения.

## Шаги воспроизведения
- Вызвать `POST /api/transform` с одним из payload:
  - `select.items = null`
  - `orderBy.items = null`
  - `groupBy.items = null`
  - `groupBy.groups = null`
  - `transformations: [null]`
- Получить `400` с `message = "Непредвиденная ошибка..."` и `errors = null`.

## Ожидаемое поведение
- Возвращать структурированную валидацию по полям (`400`) без внутренних exception-текстов.

## Возвращенный ответ API
- `Status 400`
- Примеры `message`:
  - `Object reference not set to an instance of an object.`
  - `Value cannot be null. (Parameter 'source'/'first'/'second')`
- `errors: null`.

## Ссылка на созданный во время тест-сессии набор
- `http://localhost:5223/api/datasets/52bf144e-b457-421d-9e4f-8af2f7455a65`
