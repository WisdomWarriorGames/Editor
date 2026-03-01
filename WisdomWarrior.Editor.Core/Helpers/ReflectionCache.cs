using System.Collections.Concurrent;
using System.Reflection;

namespace WisdomWarrior.Editor.Core.Helpers;

public static class ReflectionCache
{
    private static readonly ConcurrentDictionary<Type, PropertyInfo[]> _propertyCache = new();

    public static PropertyInfo[] GetTrackableProperties(Type componentType)
    {
        return _propertyCache.GetOrAdd(componentType, type =>
        {
            return type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanRead && p.CanWrite)
                .ToArray();
        });
    }
}