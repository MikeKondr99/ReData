namespace Pattern;

public interface ITryFrom<TSelf, in TFrom, TError> : IFrom<Unions.Result<TSelf, TError>, TFrom>;

public interface ITryFrom<TSelf, in TFrom> : ITryFrom<TSelf, TFrom, Exception>;
