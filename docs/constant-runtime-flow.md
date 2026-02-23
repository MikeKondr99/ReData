# Constant Runtime Flow

Документ описывает, как скриптовые `const` проходят через:

- `QueryBuilder`
- `ExpressionResolver`
- `IConstantRuntime` (`DisabledConstantRuntime` / `RunnerConstantRuntime`)
- исполнение SQL

## 1. Общий поток

```mermaid
flowchart TD
    A["Текст скрипта<br/>const ...; expr"] --> B["Expr.ParseScript"]
    B --> C["ExpressionScript:<br/>Constants + Expression"]
    C --> D["QueryBuilder.ResolveScript"]
    D --> E["ExpressionResolver.ResolveScript"]
    E --> F["ResolveLocalConstants"]
    F --> G{"Константа literal?"}
    G -- "Да" --> H["QueryConstant.Value = literal"]
    G -- "Нет" --> I{"Есть ConstantQuerySource?"}
    I -- "Нет" --> J["ExprError"]
    I -- "Да" --> K["IConstantRuntime.Create(...)"]
    K --> L["QueryConstant{Query=scalar query, Value=null}"]
    H --> M["Resolve финального expr"]
    L --> M
    M --> N["ResolvedScriptExpr"]
    N --> O["QueryBuilder merges constants<br/>для следующего scope"]
```

## 2. Кто за что отвечает

- `QueryBuilder`:
  - передает `QuerySource` и `ConstantQuerySource`;
  - хранит глобальные константы запроса;
  - после `Where` сохраняет константы из скрипта в общий scope.
- `ExpressionResolver`:
  - валидирует объявления;
  - строит локальный scope;
  - решает, когда взять значение сразу, а когда использовать runtime.
- `IConstantRuntime`:
  - `Create`: как представить вычисляемую константу (обычно scalar query);
  - `Resolve`: как получить значение (кеш или вычисление).

## 3. Вычисление константы при обращении

```mermaid
sequenceDiagram
    participant R as ResolveName
    participant C as TryResolveConstant
    participant RT as IConstantRuntime
    participant Q as QueryConstant
    participant DB as Database

    R->>C: имя константы
    C->>RT: Resolve(Q)
    alt Value уже в кеше
        RT-->>C: Ok(Value)
    else Нужен расчет
        RT->>DB: Выполнить scalar query
        DB-->>RT: IValue
        RT-->>C: Ok(Value)
        C->>Q: сохранить Value в кеше
    end
    C-->>R: ResolvedExpr на основе literal(Value)
```

## 4. Реализации runtime

- `DisabledConstantRuntime`:
  - `Create`: формирует scalar `QueryConstant`;
  - `Resolve`: возвращает ошибку, если `Value` еще не вычислено.
- `RunnerConstantRuntime`:
  - `Create`: формирует scalar `QueryConstant`;
  - `Resolve`: при пустом кеше выполняет query через `IQueryExecutor`, потом кеширует `Value`.

## 5. Важные правила

- Локальная константа может ссылаться только на ранее объявленные константы.
- Константа должна быть либо literal/constant-expression, либо агрегатным выражением.
- Без `ConstantQuerySource` сложную константу создать нельзя.
- Без активного runtime сложная константа не вычисляется.
- Кеш константы живет в рамках жизненного цикла текущего `QueryBuilder`/запроса.

