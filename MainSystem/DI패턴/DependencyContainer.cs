using System;
using System.Collections.Generic;

public static class DependencyContainer
{
    private static readonly Dictionary<Type, object> _global = new();

    public static void RegisterGlobal(Type type, object instance)
    {
        if (!_global.ContainsKey(type))
            _global.Add(type, instance);
    }

    public static object Resolve(Type type)
    {
        if (_global.TryGetValue(type, out var global))
            return global;

        throw new Exception($"Global dependency not found: {type.Name}");
    }
}