namespace ReData.Query;


public interface IArg
{
    int Index { get; }
    
}


public record struct Unknown(int Index) : IArg;

public record struct Number(int Index) : IArg;

public record struct Text(int Index) : IArg;

public record struct DateTime(int Index) : IArg;

public record struct Integer(int Index) : IArg;

public record struct Null(int Index) : IArg;

public record struct Bool(int Index) : IArg;
