# BUG-016: Лишние поля в JSON тихо игнорируются при `additionalProperties: false`

## Описание что не так
- В OpenAPI/схемах для многих объектов указан `additionalProperties: false`.
- Фактически лишние поля в payload принимаются и молча игнорируются:
  - `POST /api/transform` проходит с extra-полями на верхнем уровне и внутри `transformation/items`;
  - `POST /api/datasets` также проходит с extra-полями;
  - `PUT /api/datasets/{id}` также проходит с extra-полями.
- Это скрывает ошибки клиентов и расходится с контрактом.

## Шаги воспроизведения
- Отправить `POST /api/transform` с лишними полями:
  - на верхнем уровне (`unexpected`);
  - в `select` (`abc`);
  - в `items` (`zzz`).
- Получить `200`.
- Отправить `POST /api/datasets` с extra-полями (`extraTop`, `foo`, `abc`, `zzz`).
- Получить `201`.
- Отправить `PUT /api/datasets/{id}` с такими же extra-полями.
- Получить `200`.

## Ожидаемое поведение
- При `additionalProperties: false` запрос должен отклоняться с `400` и явной ошибкой по лишним полям.

## Возвращенный ответ API
- `POST /api/transform` с extra-полями: `Status 200`.
- `POST /api/datasets` с extra-полями: `Status 201`.
- `PUT /api/datasets/{id}` с extra-полями: `Status 200`.

## Ссылка на созданный во время тест-сессии набор
- `http://localhost:5223/api/datasets/3a29fe7d-22e9-479b-8dd7-9d61d2f7812a`
