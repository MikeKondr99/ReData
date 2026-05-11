# TickerQ jobs

## Текущая схема

- `ReData.Jobs` — библиотека с TickerQ handlers.
- `ReData.JobWorker` — отдельный процесс, который исполняет handlers из `ReData.Jobs`.
- `ReData.DemoApp` — producer/dashboard-host. Он ссылается на `ReData.Jobs`, чтобы TickerQ видел объявления функций и dashboard мог показывать их в списке, но сам jobs не исполняет.

## Что уже сделано

- Добавлен проект `src/libs/ReData.Jobs`.
- `TestJob` переведен на interface-based registration через `ITickerFunction`.
- Общая настройка TickerQ вынесена в `ReData.Jobs` через `builder.AddReDataJobs(...)`.
- `ReData.DemoApp` использует режим `ReDataJobsMode.ProducerDashboard`.
- `ReData.JobWorker` использует режим `ReDataJobsMode.Worker`.
- `ReData.JobWorker` получает reference на `redata-demoapp` через Aspire и редиректит `GET /` на основной dashboard `/api/tickerq`.
- Если стандартные Aspire env для `redata-demoapp` не пришли, worker на `GET /` отвечает текстом `I am a worker, dashboard not found.`.
- В `ReData.DemoApp` добавлен временный minimal API endpoint `POST /api/dev/tickerq/test-job`, который ставит `ReData test job` на выполнение через 30 секунд.
- Отдельные тесты на `TestJob` удалены, потому что это debug-вспомогательная функция, а не доменная логика.

## Следующий шаг

1. Переносить реальные jobs из `ReData.DemoApp/Jobs` в `ReData.Jobs` по одной.
2. Оставить `DemoApp` только как producer/dashboard-host.
3. Использовать `ReData.JobWorker` как основной executor-host.
