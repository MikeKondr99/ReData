# VariableRuntime: цепочка взаимодействия

Документ описывает, как новая модель переменных проходит через:

- `QueryBuilder`
- `ExpressionResolver`
- `IVariableRuntime` (`DisabledVariableRuntime` / `RunnerVariableRuntime`)
- исполнение SQL

## 1. Общий поток

```mermaid
flowchart TD
    A["Текст скрипта<br/>var ...; expr"] --> B["Expr.ParseScript"]
    B --> C["ExpressionScript:<br/>Variables + Expression"]
    C --> D["QueryBuilder.ResolveScript"]
    D --> E["ExpressionResolver.ResolveScript"]
    E --> F["ResolveLocalVariables"]
    F --> G{"Переменная литерал?"}
    G -- "Да" --> H["QueryVariable.Value = literal"]
    G -- "Нет" --> I{"Есть VariableQuerySource?"}
    I -- "Нет" --> J["ExprError"]
    I -- "Да" --> K["IVariableRuntime.Create(...)"]
    K --> L["QueryVariable{Query=scalar query, Value=null}"]
    H --> M["Resolve финального expr"]
    L --> M
    M --> N["ResolvedScriptExpr"]
    N --> O["QueryBuilder merges variables<br/>только для Where"]
```

## 2. Кто за что отвечает

```mermaid
flowchart LR
    QB["QueryBuilder"] -->|формирует ResolutionContext| ER["ExpressionResolver"]
    ER -->|Create/Resolve| VR["IVariableRuntime"]
    VR -->|Runner impl| QR["IQueryRunner"]
    QR --> DB["Database"]
```

- `QueryBuilder`:
  - передает `QuerySource` и `VariableQuerySource`;
  - хранит глобальные переменные запроса;
  - после `Where` сохраняет переменные из скрипта в общий scope.
- `ExpressionResolver`:
  - валидирует объявления;
  - строит локальный scope;
  - решает, когда переменную можно взять сразу как `Value`, а когда нужен runtime.
- `IVariableRuntime`:
  - `Create`: как представить вычисляемую переменную (обычно scalar query);
  - `Resolve`: как получить значение (кеш или вычисление).

## 3. Вычисление переменной при обращении

```mermaid
sequenceDiagram
    participant R as ResolveName
    participant V as TryResolveVariable
    participant RT as IVariableRuntime
    participant Q as QueryVariable
    participant DB as Database

    R->>V: имя переменной
    V->>RT: Resolve(Q)
    alt Value уже в кеше
        RT-->>V: Ok(Value)
    else Нужен расчет
        RT->>DB: Выполнить scalar query
        DB-->>RT: IValue
        RT-->>V: Ok(Value)
        V->>Q: сохранить Value в кеш
    end
    V-->>R: ResolvedExpr на основе literal(Value)
```

## 4. Какие runtime бывают

```mermaid
flowchart TD
    A["IVariableRuntime"] --> B["DisabledVariableRuntime"]
    A --> C["RunnerVariableRuntime"]

    B --> B1["Create: формирует scalar QueryVariable"]
    B --> B2["Resolve: возвращает ошибку,<br/>если Value пустой"]

    C --> C1["Create: формирует scalar QueryVariable"]
    C --> C2["Resolve: при пустом кеше<br/>выполняет query через IQueryRunner"]
    C2 --> C3["Кладет значение в кеш QueryVariable.Value"]
```

## 5. Важные правила

- Локальная переменная может ссылаться только на ранее объявленные переменные.
- Переменная должна быть:
  - либо литералом/константным выражением;
  - либо агрегацией.
- Без `VariableQuerySource` сложную переменную создать нельзя.
- Без активного runtime сложная переменная не вычисляется.
- Кеш переменной живет в рамках жизненного цикла текущего `QueryBuilder`/запроса.
