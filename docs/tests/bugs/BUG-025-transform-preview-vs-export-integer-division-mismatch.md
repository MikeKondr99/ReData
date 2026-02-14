# BUG-025: Разная семантика деления в preview (`/api/transform`) и `export` для одного и того же пайплайна

## Описание что не так
- Для одинакового пайплайна с формулой `sum_amt / cnt` результаты различаются между:
  - `POST /api/transform` (preview)
  - `GET /api/datasets/{id}/export` (CSV)
- Preview считает как целочисленное деление (`100 / 3 -> 33`), а export отдает дробное (`33.3333333333333333`).
- Дополнительно в preview поле `ratio` типизируется как `int`, что конфликтует с фактическим export-значением.

## Шаги воспроизведения
- Источник: `QA_PIPE_3abd4d13` (`dataConnectorId = fa009d79-144f-460a-9399-2dddeb5e6769`).
- Пайплайн:
  - `groupBy team` с `cnt = COUNT()`, `sum_amt = SUM(amount)`
  - `select ratio = sum_amt / cnt`
  - `orderBy team asc`
- Выполнить сначала `POST /api/transform`, затем сохранить тот же пайплайн в dataset и вызвать `export`.

## Ожидаемое поведение
- Одинаковая арифметика и типы в preview и export для идентичного пайплайна.

## Возвращенный ответ API
- Preview (`POST /api/transform`):
  - `A -> ratio = 60`
  - `B -> ratio = 33`
  - `C -> ratio = 113`
  - `fields.ratio.type = int`
- Export (`GET /api/datasets/{id}/export?fileType=0`):
  - `A -> ratio = 60.0000000000000000`
  - `B -> ratio = 33.3333333333333333`
  - `C -> ratio = 113.3333333333333333`

## Ссылка на созданный во время тест-сессии набор
- `http://localhost:5223/api/datasets/e90bb73c-d945-4b03-87f8-345a79b934e0`
