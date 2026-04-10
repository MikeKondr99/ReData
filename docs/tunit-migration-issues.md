# Переход на TUnit: проблемы и мелочи

## Контекст
- Репозиторий переведен с `ReData.DemoApp.Tests` (xUnit) на TUnit-проект с тем же именем: `src/tests/ReData.DemoApp.Tests`.
- Текущий запуск TUnit выполняется как исполняемый тестовый хост (`dotnet run --project ...`).

## 1) Изменился способ запуска тестов
- Где: `src/tests/ReData.DemoApp.Tests/ReData.DemoApp.Tests.csproj`
- Что:
  - Проект тестов теперь `OutputType=Exe` и работает через Microsoft Testing Platform (MTP).
  - `dotnet test` в legacy-режиме может падать в окружении с .NET 10 SDK (`Testing with VSTest target is no longer supported...`).
- Последствие:
  - Нужно использовать `dotnet run --project src/tests/ReData.DemoApp.Tests/ReData.DemoApp.Tests.csproj -c Release` или перейти на новый MTP-режим `dotnet test` через `global.json`.
- Нюанс CI:
  - Текущий `.github/workflows/dotnet.yml` использует `dotnet test`. На .NET 9 это работает, но при обновлении раннера/SDK до .NET 10 потребуется корректировка.

## 2) Параллельность усилила flaky-эффекты
- Где:
  - `src/tests/ReData.DemoApp.Tests/Datasets/CreateDatasetTests.cs`
  - `src/tests/ReData.DemoApp.Tests/DataConnectors/CreateDataConnectorTests.cs`
- Что:
  - Один тест помечен skip с TODO:
    - `CreateDataset_FromEmptyConnector_ShouldSaveRowsCountZero`
    - `[Skip("TODO: investigate parallel metadata race (RowsCount intermittently null/non-zero in parallel run)")]`
  - Для DataConnectors оставлен `[NotInParallel]`.
- Последствие:
  - При параллельном прогоне часть сценариев по метаданным/записи в БД нестабильна.

## 3) Конкурентный DbContext в command-тестах (корневая причина)
- Где:
  - `src/apps/ReData.DemoApp/Commands/CreateDataConnectorCommand.cs`
  - внутреннее поведение FastEndpoints `CommandExtensions.ExecuteAsync()` / `ServiceResolver`
- Что:
  - Вызов command-хендлеров из тестового контекста без `HttpContext` может резолвить зависимости из root provider.
  - Из-за этого scoped `ApplicationDatabaseContext` может использоваться конкурентно между тестами.
- Симптом:
  - `A second operation was started on this context instance...`
- Практический вывод:
  - Для таких сценариев лучше оставлять `NotInParallel` или запускать эти тесты только endpoint-через-HTTP путем.

## 4) Смена паттернов тестов: меньше Arrange, больше intent
- Где:
  - `src/tests/ReData.DemoApp.Tests/Datasets/UpdateDatasetTests.cs`
  - `src/tests/ReData.DemoApp.Tests/Extensions.cs`
- Что:
  - Введен паттерн `Request with` для сокращения Arrange.
  - Внедрены helper-проверки `IsSuccess()` и `IsValidationError()` вместо FluentAssertions.
- Нюанс:
  - Часть старых тестовых привычек (большой Arrange и прямые многострочные проверки) теперь стоит вычищать поэтапно.

## 5) DbContext lifecycle: один контекст на тест
- Где:
  - `src/tests/ReData.DemoApp.Tests/Datasets/DatasetTestBase.cs`
- Что:
  - `Before(HookType.Test)` создает scoped `ApplicationDatabaseContext`.
  - `After(HookType.Test)` корректно dispose-ит scope и контекст.
- Плюс:
  - Уменьшает шанс неявной повторной конкуренции внутри одного теста.
- Ограничение:
  - Не решает автоматически конкуренцию в сторонних executor-ах/командах, если они живут вне request-scope.

## 6) AOT/trim предупреждения на HTTP JSON extension
- Где:
  - `src/tests/ReData.DemoApp.Tests/Datasets/UpdateDatasetTests.cs` (строка с `PutAsJsonAsync`)
- Что:
  - Предупреждения `IL2026`/`IL3050` для методов с `RequiresUnreferencedCode`/`RequiresDynamicCode`.
- Вывод:
  - Проект собирается с AOT-совместимыми флагами, но это не равно гарантии AOT-исполняемости без доработок сериализации.

## 7) Операционные мелочи
- В консоли местами видны проблемы кодировки для русских `DisplayName`.
- Для анализа удобнее использовать HTML/JUnit/TRX отчеты, а не только raw stdout.

## 8) Текущее состояние DemoApp.Tests
- Всего тестов: 122.
- Наблюдались:
  - один skip с TODO (RowsCount race),
  - единичный невоспроизводимый flaky на сценарии с отключенной `WHERE`.

## Источники
- TUnit test filters: https://tunit.dev/docs/execution/test-filters/
- TUnit engine modes: https://tunit.dev/docs/execution/engine-modes/
- `dotnet test` и MTP:
  - https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-test-vstest
  - https://learn.microsoft.com/en-us/dotnet/core/testing/migrating-vstest-microsoft-testing-platform
