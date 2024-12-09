using System;
using System.Reflection;

namespace Common.Reflection;

internal abstract class _MethodWrapper
{
    protected readonly MethodInfo method;
    protected _MethodWrapper(MethodInfo method)
    {
        this.method = method;
    }

    public static implicit operator bool(_MethodWrapper mw)
    {
        return mw?.method != null;
    }
}

// slow wrapper that uses reflection
internal class MethodWrapper : _MethodWrapper
{
    public MethodWrapper(MethodInfo method) : base(method) { }

    public object Invoke()
    {
        Debug.Assert(method != null && method.IsStatic);
        return method?.Invoke(null, null);
    }

    public object Invoke(object obj)
    {
        Debug.Assert(method != null);
        return method.IsStatic
            ? method?.Invoke(null, [obj])
            : method?.Invoke(obj, null);
    }

    public object Invoke(object obj, params object[] parameters)
    {
        Debug.Assert(method != null);
        return method?.Invoke(obj, parameters ?? new object[1]); // null check in case we need to pass one 'null' as a parameter
    }

    public T Invoke<T>()
    {
        return Invoke().Cast<T>();
    }

    public T Invoke<T>(object obj)
    {
        return Invoke(obj).Cast<T>();
    }

    public T Invoke<T>(object obj, params object[] parameters)
    {
        return Invoke(obj, parameters).Cast<T>();
    }
}


// fast wrapper that uses delegate
internal class MethodWrapper<D> : _MethodWrapper where D : Delegate
{
    private readonly object obj;
    public MethodWrapper(MethodInfo method, object obj = null) : base(method)
    {
        this.obj = obj;
    }

    public static implicit operator bool(MethodWrapper<D> mw)
    {
        return mw?.Invoke != null;
    }

    public D Invoke => _delegate ??= Initialize();

    private D _delegate;

    private D Initialize()
    {
        try
        {
            return (D)Delegate.CreateDelegate(typeof(D), obj, method);
        }
        catch (Exception e)
        {
            Log.Message(e);
            return null;
        }
    }
}