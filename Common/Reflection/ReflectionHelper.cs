using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace Common.Reflection;

static class ReflectionHelper
{
    public const BindingFlags bfAll = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;

    public static Type GetCallingType() => new StackTrace().GetFrame(2).GetMethod().ReflectedType;

    public static Type GetCallingDerivedType()
    {
        StackTrace stackTrace = new();
        Type baseType = stackTrace.GetFrame(1).GetMethod().ReflectedType;
        Type resultType = baseType;

        for (int i = 2; i < stackTrace.FrameCount; i++)
        {
            var currentType = stackTrace.GetFrame(i).GetMethod().ReflectedType;

            if (baseType.IsAssignableFrom(currentType))
                resultType = currentType;
            else
                return resultType;
        }

        return resultType;
    }

    // for getting mod's defined types, don't include any of Common projects types (or types without namespace)
    public static readonly List<Type> definedTypes = Assembly.GetExecutingAssembly().GetTypes().Where(type => type.Namespace?.StartsWith(nameof(Common)) is false).ToList();
}