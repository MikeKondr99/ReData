// ReSharper disable once CheckNamespace

namespace ReData.Database.Entities;

public record DataSet
{
    public Guid Id { get; init; }
    
    public required string Name { get; init; }

    public Guid? ParentId { get; init; }

    public required ICollection<Transformation> Transformations { get; init; }
}


// Товары 

// Id: int | Name: str | Age: int

//  * Create
// DataSource1 | Good | Name:str Age:int LOL: Guid

// Фильтр 
// Name = 'str'

// RemoveFields
// [ LOL ]

//! Name:str Age:int



// Preview


    