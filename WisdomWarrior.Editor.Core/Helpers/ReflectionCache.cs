using System.Collections.Concurrent;
using System.Reflection;
using WisdomWarrior.Engine.Core.Attributes;

namespace WisdomWarrior.Editor.Core.Helpers;

public static class ReflectionCache
{
    private static readonly ConcurrentDictionary<Type, PropertyInfo[]> PropertyCache = new();

    public static PropertyInfo[] GetTrackableProperties(Type componentType)
    {
        return PropertyCache.GetOrAdd(componentType, type =>
        {
            if (ShouldTreatAsLeafValue(type))
            {
                return [];
            }

            return type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(property => property.CanRead
                                   && property.CanWrite
                                   && property.GetIndexParameters().Length == 0)
                .ToArray();
        });
    }

    private static bool ShouldTreatAsLeafValue(Type type)
    {
        if (type == typeof(string))
        {
            return true;
        }

        return type.Namespace == "System.Numerics"
               && type.IsValueType;
    }
}
