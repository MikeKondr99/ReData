using Pattern.Unions;

namespace Pattern;


public static class ResultCollectionExtensions
{
    public static Unions.Result<IReadOnlyCollection<T>, E> ToResult<T, E>(this IEnumerable<Unions.Result<T, E>> collection)
    {
        var items = new List<T>();
        foreach (var item in collection)
        {
            if (item is IError<E>(var err))
            {
                return err;
            }
            items.Add(item.Unwrap());
        }
        return items;
    }
    
    
}

public static class ResultExtensions
{

    public static T Unwrap<T>(this Unions.Result<T, Never> result) => result.UnwrapOk();


    public static Result<T, E> Flatten<T, E>(Result<Result<T, E>, E> result) => result switch
    {
        IOk<Result<T, E>>(var ok) => ok,
        IError<E>(var err) => err,
    };

    public static Option<Result<T, E>> Transpose<T, E>(this Result<Option<T>, E> result)
    {
        return result switch
        {
            IOk<Option<T>>(ISome<T>(var val)) => Some(Ok(val).WithError<E>()),
            IError<E>(var err) => Some(Error(err).WithOk<T>()),
            IOk<Option<T>>(INone) => None(),
        };
    }
}

public static class OptionExtensions
{
    public static Option<T> Get<T>(this IReadOnlyList<T> list, int index)
    {
        if (index >= 0 && index < list.Count)
        {
            return list[index];
        }
        return None();
    }

    public static Option<T> Then<T>(this bool self, T value)
    {
        if (self)
        {
            return Some(value);
        }
        return None();
    }
    
    public static Option<T> Then<T>(this bool self, Func<T> value)
    {
        if (self)
        {
            return Some(value());
        }
        return None();
    }

    public static Option<T> Get<TKey, T>(this Dictionary<TKey, T> dict, TKey key)
    {
        if(dict.TryGetValue(key, out var res))
        {
            return Some(res);
        }
        return None();
    }
}
