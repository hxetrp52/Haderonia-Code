using System;
using System.Collections.Generic;

public class SceneDependencyContainer
{
    private readonly Dictionary<Type, object> _local = new();

    public void Register(Type type, object instance)
    {
        if (!_local.ContainsKey(type))
            _local.Add(type, instance);
    }

    public object Resolve(Type type)
    {
        if (_local.TryGetValue(type, out var local))
            return local;

        return DependencyContainer.Resolve(type);
    }

    public void Clear()
    {
        _local.Clear();
    }
}
