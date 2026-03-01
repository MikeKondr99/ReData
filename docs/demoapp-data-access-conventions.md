# DemoApp Data Access Conventions

## EF Core Tracking

- In `ReData.DemoApp`, `ApplicationDatabaseContext` is configured with `QueryTrackingBehavior.NoTracking` in `Program.cs`.
- Do not add `AsNoTracking()` in repository queries by default because it is already global behavior.
- Use explicit tracking only when the query really requires tracked entities for updates via change tracker.

## Validation Queries

- Request validators may call repository methods (for example, dataset name uniqueness checks).
- `GetByNameAsync(name)` is a direct DB query.
- Database unique index remains the final consistency guarantee (`DataSets.Name` is unique).

## Dataset Read Consistency

- Dataset repository reads (`GetAllAsync`, `GetByIdAsync`, `GetByNameAsync`) execute directly against the database.
- `DatasetChangedEvent` is still published on create/update/delete and can be used by external handlers (for example, distributed cache invalidation).

## Dataset Transformations

- Current EF model uses FK `TransformationEntity.DataSetId` for `DataSetEntity -> TransformationEntity`.
- Repository keeps backward compatibility for in-place updates on legacy schemas by setting shadow `DataSetEntityId` only if that property still exists in the model.
