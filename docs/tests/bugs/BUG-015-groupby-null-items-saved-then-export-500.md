# BUG-015: `null`-коллекции в трансформациях сохраняются, затем `export` падает `500`

## Описание что не так
- `POST /api/datasets` принимает невалидные `null`-коллекции внутри трансформаций и возвращает `201`.
- Набор сохраняется и читается через `GET`.
- При `export` такие наборы падают с `500`.
- Подтвержденные варианты:
  - `groupBy.items = null`
  - `groupBy.groups = null`
  - `orderBy.items = null`
  - `select.items = null`

## Шаги воспроизведения
- Создать набор с одной из невалидных конфигураций выше (`... = null`).
- Получить `201`.
- Выполнить `GET /api/datasets/{id}` и убедиться, что `null` сохранен.
- Выполнить `GET /api/datasets/{id}/export?fileType=0`.
- Получить `500`.

## Ожидаемое поведение
- Для `select/orderBy/groupBy` обязательные коллекции должны валидироваться как непустые массивы (или как минимум не `null`).
- При невалидном значении ожидать `400` на этапе `POST/PUT`.

## Возвращенный ответ API
- Для всех перечисленных вариантов:
  - `POST /api/datasets`: `Status 201`.
  - `GET /api/datasets/{id}`: `Status 200`, `null`-коллекция присутствует.
  - `export`: `Status 500`, тело пустое.

## Ссылки на созданные во время тест-сессии наборы
- `http://localhost:5223/api/datasets/59331a40-b00d-4b01-a007-8b57e3cd5b3d` (`groupBy.items = null`)
- `http://localhost:5223/api/datasets/ce1d7e4c-a8e6-47b7-b2bc-717f4c28fa5e` (`groupBy.groups = null`)
- `http://localhost:5223/api/datasets/f2e7f719-35aa-417f-ae94-b6b66512d13f` (`orderBy.items = null`)
- `http://localhost:5223/api/datasets/d3ec08e9-20a7-49ad-990f-d3082e8beb5e` (`select.items = null`)
