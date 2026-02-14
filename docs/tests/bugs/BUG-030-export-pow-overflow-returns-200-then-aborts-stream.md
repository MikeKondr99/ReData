# BUG-030: При overflow в степенном пайплайне `export` отдает `200 OK`, но обрывает поток данных

## Описание что не так
- Пайплайн с каскадным `Pow` приводит к переполнению decimal.
- В preview (`POST /api/transform`) это видно как контролируемый `500`:
  - `Value was either too large or too small for a Decimal.`
- В export поведение хуже:
  - сервер отвечает `200 OK` и начинает отдавать CSV,
  - затем соединение обрывается (тело не дочитывается, broken/chunked stream).

## Шаги воспроизведения
- Источник: `dataConnectorId = c03ee60d-f898-4a28-897d-722139f90e75` (одна строка).
- Создать dataset с трансформациями:
  - `x1 = Pow(10,10)`
  - `x2 = Pow(x1,10)` (overflow)
- Вызвать:
  - `POST /api/transform` для того же набора трансформаций,
  - `GET /api/datasets/{id}/export?fileType=0`.

## Ожидаемое поведение
- Если вычисление не может быть выполнено, возвращать управляемую ошибку (`4xx/5xx`) до начала стриминга.
- Не отправлять `200 OK` при последующем обрыве тела ответа.

## Возвращенный ответ API
- `POST /api/transform`: `500` с сообщением `Value was either too large or too small for a Decimal.`
- `export`: `HTTP/1.1 200 OK`, затем клиент получает обрыв потока:
  - `curl: (18) transfer closed with outstanding read data remaining`
  - в PowerShell: `Unable to read data from the transport connection: The connection was closed.`

## Ссылка на созданный во время тест-сессии набор
- `http://localhost:5223/api/datasets/41a33caa-ba83-484e-92c6-057c32824e56`
