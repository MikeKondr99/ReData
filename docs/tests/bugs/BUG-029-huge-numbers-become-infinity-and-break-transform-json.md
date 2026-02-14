# BUG-029: Сверхбольшие `num` превращаются в `Infinity/-Infinity`, после чего `transform` падает `500`

## Описание что не так
- При импорте сверхбольших чисел (например, `10^400`) значения в данных становятся `Infinity` и `-Infinity`.
- `POST /api/transform` падает `500` на сериализации JSON:
  - `System.ArgumentException: .NET number values such as positive and negative infinity cannot be written as valid JSON`
- При этом `export` возвращает CSV с `Infinity/-Infinity`, то есть невалидное с точки зрения ожидаемой числовой семантики.

## Шаги воспроизведения
- Импортировать CSV с `val = 1` + `400` нулей и `val = -1` + `400` нулей.
- Вызвать `POST /api/transform` с простым `select val`.
- Получить `500`.
- Сохранить dataset с `select val` и выполнить `export`.

## Ожидаемое поведение
- Либо валидировать/отклонять значения вне поддерживаемого диапазона на входе (`400`).
- Либо хранить в поддерживаемом типе и не допускать `Infinity` в доменной модели.
- Но не падать `500` на обычном preview-запросе.

## Возвращенный ответ API
- `POST /api/transform`: `500` + stack trace c `JsonSerializer` и `Infinity`.
- `export` CSV: строки `Infinity` и `-Infinity`.

## Ссылка на созданный во время тест-сессии набор
- `http://localhost:5223/api/datasets/0b095133-e4f5-4fa5-86f7-670286f9752a`
