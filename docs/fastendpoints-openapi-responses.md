# FastEndpoints: явные Response и отправка ошибок

Для endpoint-ов с `HandleAsync(...)` (которые сами пишут поток/файл в ответ) OpenAPI может показывать `204` по умолчанию, если явно не задать варианты ответов.

Рекомендуемый шаблон:

1. В `Configure()` явно описывать ответы через `Description(...Produces...)`.
2. Для JSON-ошибок использовать `Send.ResponseAsync(...)`, а не ручной `WriteAsJsonAsync(...)`.

Пример:

```csharp
Description(d => d
    .Produces(StatusCodes.Status200OK, contentType: "text/csv")
    .Produces<ExportDatasetErrorResponse>(StatusCodes.Status400BadRequest, "application/json")
    .Produces(StatusCodes.Status404NotFound)
    .Produces(StatusCodes.Status500InternalServerError));
```

```csharp
await Send.ResponseAsync(
    new ExportDatasetErrorResponse { ... },
    StatusCodes.Status400BadRequest,
    ct);
return;
```

`Send.*` методы не завершают выполнение метода автоматически, поэтому после отправки ответа нужен явный `return`.
