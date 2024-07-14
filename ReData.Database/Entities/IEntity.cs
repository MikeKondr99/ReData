namespace ReData.Database.Entities;

public interface IEntity : IEntity<Guid>;

public interface IEntity<TKey>
{
    public TKey Id { get; init; }
}