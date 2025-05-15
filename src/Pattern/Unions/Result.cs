using System.Collections;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Dunet;
using Pattern.Core;

namespace Pattern.Unions;

[Union]
public abstract partial record Result<T, E> : 
    IEnumerable<T>
{
    partial record Ok(T Value) : IOk<T>
    {
        public static implicit operator T(Ok self) => self.Value;
    }

    partial record Error(E Value) : IError<E>
    {
        public static implicit operator E(Error self) => self.Value;
    }

    public static implicit operator Result<T, E>(ResultOk<T> value)
    {
        return new Ok(value.Value);
    }
    
    public static implicit operator Result<T, E>(ResultError<E> value)
    {
        return new Error(value.Value);
    }
    
    public Result<TAs?, E> As<TAs>()
        where TAs : class => this switch
    {
        IOk<T>(var val) => val as TAs,
        IError<E>(var err) => err,
    };


    public bool IsOk() => this is Ok;

    public bool IsOk([NotNullWhen(true)] out T? value)
    {
        if (this is Ok(var ok))
        {
            value = ok;
            return true;
        }
        value = default;
        return false;
    }
    
    public bool IsOkAnd(Func<T,bool> f) => this switch
    {
        Ok(var ok) => f(ok),
        _ => false
    };

    public bool IsError() => this is Error;
    
    public bool IsError([NotNullWhen(true)] out E? value)
    {
        if (this is Error(var err))
        {
            value = err;
            return true;
        }
        value = default;
        return false;
    }
    
    public bool IsErrorAnd(Func<E,bool> f) => this switch
    {
        Error(var err) => f(err),
        _ => false
    };

    public Option<T> ToOption() => (this switch
    {
        Ok(var val) => val,
        _ => None<T>(),
    })!;
    
    public Option<E> ErrorToOption() => (this switch
    {
        Error(var val) => val,
        _ => None<E>(),
    })!;
    
    public Result<TOut, E> Map<TOut>(Func<T, TOut> f) => this switch
    {
        Ok(var ok) => f(ok),
        Error(var err) => err,
        _ => throw new UnreachableException(),
    };
    
    public Result<T, EOut> MapError<EOut>(Func<E, EOut> f) => this switch
    {
        Ok(var ok) => ok,
        Error(var err) => f(err),
        _ => throw new UnreachableException(),
    };

    public T Expect(string message) => this switch
    {
        Ok(var ok) => ok,
        _ => throw new InvalidOperationException(message)
    };
    
    public T Expect(Func<E,string> message) => this switch
    {
        Ok(var ok) => ok,
        Error(var err) => throw new InvalidOperationException(message(err))
    };
    
    public E ExpectErr(string message) => this switch
    {
        Error(var err) => err,
        _ => throw new InvalidOperationException(message)
    };
    
    public E ExpectErr(Func<T,string> message) => this switch
    {
        Ok(var ok) => throw new InvalidOperationException(message(ok)),
        Error(var err) => err,
    };

    public bool Unwrap([NotNullWhen(true)] out T? ok, [NotNullWhen(false)] out E? err)
    {
        if (this is Ok(var okValue))
        {
            ok = okValue;
            err = default;
            return true;
        }
        else
        {
            ok = default;
            err = UnwrapError();
            return false;
        }
    }
    
    public bool UnwrapErr([NotNullWhen(true)] out E? err, [NotNullWhen(false)] out T? ok)
    {
        if (this is Ok(var okValue))
        {
            ok = okValue;
            err = default;
            return false;
        }
        else
        {
            ok = default;
            err = UnwrapError();
            return true;
        }
    }

    public T Unwrap() => this switch
    {
        Ok(var ok) => ok,
        _ => throw new Exception(),
    };

    public T UnwrapOr(T value) => this switch
    {
        Ok(var ok) => ok,
        _ => value,
    };
    
    public T? UnwrapOrDefault() => this switch
    {
        Ok(var ok) => ok,
        _ => default,
    };
    public E UnwrapErr() => this switch
    {
        Error(var err) => err,
        _ => throw new Exception(),
    };
    
    public E UnwrapErr(E value) => this switch
    {
        Error(var err) => err,
        _ => value,
    };
    
    public T UnwrapOr(Func<T> value) => this switch
    {
        Ok(var ok) => ok,
        _ => value(),
    };
    
    public T UnwrapErrOr(Func<T> value) => this switch
    {
        Ok(var ok) => ok,
        _ => value(),
    };
    
    public T? UnwrapErrOrDefault() => this switch
    {
        Ok(var ok) => ok,
        _ => default,
    };
    
    public Result<TOut,E> And<TOut>(Result<TOut,E> other) => (this,other) switch
    {
        (Error(var err), _) => err,
        (Ok,Ok(TOut ok)) => ok,
        (Ok,Error(var err)) => err,
    };
    
    public Result<TOut,E> And<TOut>(Func<T,Result<TOut,E>> other) => (this,other) switch
    {
        (Error(var err), _) => err,
        (Ok(var ok),_) => other(ok)
    };
    
    public Result<T,EOut> Or<EOut>(Result<T,EOut> other) => (this,other) switch
    {
        (Ok(var ok), _) => ok,
        (Error,Ok(T ok)) => ok,
        (Error,Error(EOut err)) => err,
    };
    

    public IEnumerator<T> GetEnumerator()
    {
        if (this is Ok(var ok))
        {
            // ReSharper disable once NotDisposedResourceIsReturned
            return new Once<T>(ok).GetEnumerator();
        }
        // ReSharper disable once NotDisposedResourceIsReturned
        return Enumerable.Empty<T>().GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        if (this is Ok(var ok))
        {
            // ReSharper disable once NotDisposedResourceIsReturned
            return new Once<T>(ok).GetEnumerator();
        }
        // ReSharper disable once NotDisposedResourceIsReturned
        return Enumerable.Empty<T>().GetEnumerator();
    }

}

public static class Result
{
    public static ResultOk<T> Ok<T>(T value)
    {
        return new ResultOk<T>(value);
    }
    
    public static Result<T,E> Ok<T, E>(T value)
    {
        return new Result<T, E>.Ok(value);
    }
    
    public static ResultError<E> Error<E>(E value)
    {
        return new ResultError<E>(value);
    }
    
    public static Result<T,E> Error<T, E>(E value)
    {
        return new Result<T, E>.Error(value);
    }
    
    
}