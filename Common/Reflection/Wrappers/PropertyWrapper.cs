using System.Reflection;

namespace Common.Reflection;

internal class PropertyWrapper
{
    private readonly PropertyInfo propertyInfo;
    private MethodBase setter, getter;

    public PropertyWrapper(PropertyInfo propertyInfo)
    {
        this.propertyInfo = propertyInfo;
    }

    public void Set(object value)
    {
        Set(null, value);
    }

    public void Set(object obj, object value)
    {
        setter ??= propertyInfo?.GetSetMethod();
        _ = (setter?.Invoke(obj, [value]));
    }

    public object Get(object obj = null)
    {
        getter ??= propertyInfo?.GetGetMethod();

        return getter?.Invoke(obj, null);
    }

    public T Get<T>(object obj = null)
    {
        return Get(obj).Cast<T>();
    }
}