using HarmonyLib;
using System;
using System.Globalization;
using System.Reflection;

namespace Common.Reflection;

internal static class ObjectExtensions
{
    public static T Cast<T>(this object obj)
    {
        try
        {
            $"cast<{typeof(T)}>(): object is null, default value is used".LogDebug(obj == null);
            return obj == null ? default : (T)obj;
        }
        catch
        {
            string msg = $"cast error: {obj}; {obj.GetType()} -> {typeof(T)}";
            Debug.Assert(false, msg);
            msg.LogError();

            return default;
        }
    }

    public static T Convert<T>(this object obj)
    {
        return obj.Convert(typeof(T)).Cast<T>();
    }

    public static object Convert(this object obj, Type targetType)
    {
        if (obj == null)
        {
            return null;
        }

        try
        {
            if (targetType.IsEnum)
            {
                if (obj is string str)
                {
                    return Enum.Parse(targetType, str, true);
                }

                targetType = Enum.GetUnderlyingType(targetType);
            }
            else if (Nullable.GetUnderlyingType(targetType) is Type underlyingType)
            {
                return Activator.CreateInstance(targetType, obj.Convert(underlyingType));
            }

            return System.Convert.ChangeType(obj, targetType, CultureInfo.InvariantCulture);
        }
        catch (Exception e)
        {
            Log.Message(e);
            return null;
        }
    }

    // assigns value to field with types conversion
    // returns `false` if field have equal value after conversion (or in case of exception)
    // returns 'true' in case of successful assignment
    public static bool SetFieldValue(this object obj, FieldInfo field, object value)
    {
        try
        {
            object newValue = value.Convert(field.FieldType);

            if (Equals(field.GetValue(obj), newValue))
            {
                return false;
            }

            field.SetValue(obj, newValue);

            return true;
        }
        catch (Exception e)
        {
            Log.Message(e); 
            return false;
        }
    }

    public static void SetFieldValue(this object obj, string fieldName, object value)
    {
        obj.GetType().Field(fieldName)?.SetValue(obj, value);
    }

    public static object GetFieldValue(this object obj, string fieldName)
    {
        return obj.GetType().Field(fieldName)?.GetValue(obj);
    }

    public static T GetFieldValue<T>(this object obj, string fieldName)
    {
        return obj.GetFieldValue(fieldName).Cast<T>();
    }

    public static object GetPropertyValue(this object obj, string propertyName)
    {
        return obj.GetType().Property(propertyName)?.GetValue(obj);
    }

    public static T GetPropertyValue<T>(this object obj, string propertyName)
    {
        return obj.GetPropertyValue(propertyName).Cast<T>();
    }
}

internal static class TypeExtensions
{
    private static MethodInfo Method(this Type type, string name, BindingFlags bf, Type[] types)
    {
        try
        {
            return types is null
                ? type.GetMethod(name, bf)
                : type.GetMethod(name, bf, null, types, null);
        }
        catch (AmbiguousMatchException)
        {
            $"Ambiguous method: {type.Name}.{name}".LogError();
        }
        catch (Exception e)
        {
            Log.Message(e);
        }

        return null;
    }

    public static MethodInfo Method(this Type type, string name, BindingFlags bf = ReflectionHelper.bfAll)
    {
        return Method(type, name, bf, null);
    }

    public static MethodInfo Method(this Type type, string name, params Type[] types)
    {
        return Method(type, name, ReflectionHelper.bfAll, types);
    }

    public static MethodInfo Method<T>(this Type type, string name, params Type[] types)
    {
        return Method(type, name, ReflectionHelper.bfAll, types)?.MakeGenericMethod(typeof(T));
    }

    public static EventInfo Event(this Type type, string name, BindingFlags bf = ReflectionHelper.bfAll)
    {
        return type.GetEvent(name, bf);
    }

    public static FieldInfo Field(this Type type, string name, BindingFlags bf = ReflectionHelper.bfAll)
    {
        return type.GetField(name, bf);
    }

    public static PropertyInfo Property(this Type type, string name, BindingFlags bf = ReflectionHelper.bfAll)
    {
        return type.GetProperty(name, bf);
    }

    public static FieldInfo[] Fields(this Type type, BindingFlags bf = ReflectionHelper.bfAll)
    {
        return type.GetFields(bf);
    }

    public static MethodInfo[] Methods(this Type type, BindingFlags bf = ReflectionHelper.bfAll)
    {
        return type.GetMethods(bf);
    }

    public static PropertyInfo[] Properties(this Type type, BindingFlags bf = ReflectionHelper.bfAll)
    {
        return type.GetProperties(bf);
    }
}

internal static class MemberInfoExtensions
{
    public static string FullName(this MemberInfo memberInfo)
    {
        if (memberInfo == null)
        {
            return "[null]";
        }

        if ((memberInfo.MemberType & (MemberTypes.Method | MemberTypes.Field | MemberTypes.Property)) != 0)
        {
            return $"{memberInfo.DeclaringType.FullName}.{memberInfo.Name}";
        }

        return (memberInfo.MemberType & (MemberTypes.TypeInfo | MemberTypes.NestedType)) != 0
            ? (memberInfo as Type).FullName
            : memberInfo.Name;
    }

    public static EventWrapper Wrap(this EventInfo evnt, object obj = null)
    {
        return new(evnt, obj);
    }

    public static MethodWrapper Wrap(this MethodInfo method)
    {
        return new(method);
    }

    public static MethodWrapper<D> Wrap<D>(this MethodInfo method, object obj = null) where D : Delegate
    {
        return new(method, obj);
    }

    public static PropertyWrapper Wrap(this PropertyInfo property)
    {
        return new(property);
    }

    public static A GetAttribute<A>(this MemberInfo memberInfo, bool includeDeclaringTypes = false) where A : Attribute
    {
        A[] attrs = null;
        memberInfo.GetAttributes(ref attrs, includeDeclaringTypes, earlyExit: true);

        return attrs.Length > 0 ? attrs[0] : null;
    }

    public static A[] GetAttributes<A>(this MemberInfo memberInfo, bool includeDeclaringTypes = false) where A : Attribute
    {
        A[] attrs = null;
        memberInfo.GetAttributes(ref attrs, includeDeclaringTypes, earlyExit: false);

        return attrs;
    }

    private static void GetAttributes<A>(this MemberInfo memberInfo, ref A[] attrs, bool includeDeclaringTypes, bool earlyExit) where A : Attribute
    {
        attrs = attrs.AddRangeToArray(Attribute.GetCustomAttributes(memberInfo, typeof(A)) as A[]);

        if (!includeDeclaringTypes)
        {
            return;
        }

        Type declaringType = memberInfo.DeclaringType;

        while (declaringType != null && (!earlyExit || attrs.Length == 0))
        {
            declaringType.GetAttributes(ref attrs, false, false);
            declaringType = declaringType.DeclaringType;
        }
    }

    public static bool CheckAttribute<A>(this MemberInfo memberInfo) where A : Attribute
    {
        return Attribute.IsDefined(memberInfo, typeof(A));
    }
}