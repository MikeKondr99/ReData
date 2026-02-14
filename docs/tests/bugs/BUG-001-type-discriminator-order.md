# BUG-001: Порядок полей JSON ломает десериализацию трансформации

## Описание что не так
- При `PUT /api/datasets/{id}` (и аналогично при `POST`) сервер возвращает `500`, если в объекте `transformation` поле `"$type"` стоит не первым.
- Отсутствие `"$type"` в `transformation` также приводит к `500` (в `POST`, `PUT` и `POST /api/transform`) вместо валидационной ошибки `400`.
- JSON-объект по спецификации не должен зависеть от порядка полей.
- Фактически это приводит к ошибке десериализации полиморфного типа и падению с `System.NotSupportedException`.

## Шаги воспроизведения
- Взять существующий набор.
- Отправить `PUT /api/datasets/{id}` с `transformation`, где сначала `condition`, потом `"$type":"where"`.
- Получаем `500` вместо корректной обработки/валидации.
- Отправить `POST`/`PUT`/`transform` с `transformation` без `"$type"` и получить `500`.

## Ожидаемое поведение
- Порядок полей не влияет на обработку.
- При ошибке входных данных возвращается `400` без stack trace.

## Возвращенный ответ API
- Status: `500`
- Body (начало): `System.NotSupportedException: The JSON payload for polymorphic interface or abstract type 'ReData.DemoApp.Transformations.Transformation' must specify a type discriminator. Path: $.transformations[0].transformation ...`

## Ссылка на созданный во время тест-сессии набор
- `http://localhost:5223/api/datasets/add5ac3e-5d7d-475b-a8b5-26e43805d851`
