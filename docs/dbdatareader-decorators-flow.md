# DbDataReader Decorators Flow

## Purpose

This note documents how `DbDataReader` decorators are composed and where data goes in:

- `api/transform`
- dataset export
- scalar collection in tests

It also fixes one important behavioral rule:

- `DbDataReaderDecorator` must forward async APIs (`ReadAsync`, `NextResultAsync`) to the inner provider reader to keep true async I/O and cancellation support.

## Decorators

- `DbDataReaderDecorator` is the base wrapper.
- `DomainDbDataReader` is the runner output contract (`IQueryExecutor.GetDataReaderAsync`).
  It composes CLR normalization + alias mapping and exposes domain metadata via `GetDomainType(...)`.
- `LegacyIValueDbDataReader` (`WrapInValue`) maps CLR values to `IValue` for current legacy contracts.
- `FieldAliasDbDataReader` is used inside `DomainDbDataReader` composition.

## Runtime Flow (Mermaid, simplified)

```mermaid
flowchart TD
    A[Provider DbDataReader]
    B[DomainDbDataReader]
    C[LegacyIValueDbDataReader<br/>WrapInValue]
    C2[LegacyIValueDbDataReader<br/>WrapInValue]

    A --> B

    B --> C2
    C2 --> T[Transform: CollectToObjects]
    T --> T2[Transform data]
    T --> T3[Transform TOTAL]

    B --> E2[Export file writer]

    B --> C
    C --> S1[Tests: CollectToScalar]
    S1 --> S2[IValue scalar]
```

## Notes About TOTAL and Scalar

- In current `TransformEndpoint`, `TOTAL` is fetched from the first row of `CollectToObjects(...)` result via `data[0].Int("TOTAL")`.
- `CollectToScalar(...)` is currently used in expression tests (`ExprTests.GetScalarAsync`) and other scalar scenarios, but not in `TransformEndpoint`.
