# DemoApp Data Access Conventions

## EF Core Tracking

- In `ReData.DemoApp`, `ApplicationDatabaseContext` is configured with `QueryTrackingBehavior.NoTracking` in `Program.cs`.
- Do not add `AsNoTracking()` in repository queries by default because it is already global behavior.
- Use explicit tracking only when the query really requires tracked entities for updates via change tracker.

## Validation Queries

- Request validators may call repository methods (for example, dataset name uniqueness checks).
- `GetByNameAsync(name)` is cached with a point tag (`dataset:name:<hash(name)>`).
- Name tags are invalidated by `DatasetChangedEvent` using affected old/new names.
- Database unique index remains the final consistency guarantee (`DataSets.Name` is unique).

## Dataset Read Consistency

- `GetByIdAsync(id)` uses EF Plus cache with point tag `dataset:{id}`.
- On `Create/Update/Delete`, dataset cache is invalidated by:
  - point/list/name tags;
  - `ExpireType<DataSetEntity>()` and `ExpireType<TransformationEntity>()` as a safety net.

## Dataset Transformations

- Current EF model uses a shadow foreign key `DataSetEntityId` for `DataSetEntity -> TransformationEntity`.
- When transformations are inserted separately from dataset graph (for example in `Update` replace-flow), repository must explicitly set that shadow FK, otherwise `GetById().Include(ds => ds.Transformations)` may return an empty collection.
