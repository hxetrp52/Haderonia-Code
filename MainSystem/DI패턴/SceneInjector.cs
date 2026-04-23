using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

[DefaultExecutionOrder(-100)]
public class SceneInjector : MonoBehaviour
{
    private SceneDependencyContainer _sceneContainer;

    private void Awake()
    {
        _sceneContainer = new SceneDependencyContainer();

        RegisterSceneProviders();
        InjectAll();
    }

    private void RegisterSceneProviders()
    {
        var providers = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None)
            .Where(m => m.GetType().IsDefined(typeof(SceneProvideAttribute), true));

        foreach (var provider in providers)
        {
            _sceneContainer.Register(provider.GetType(), provider);
        }
    }

    private void InjectAll()
    {
        var monos = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);

        foreach (var mono in monos)
        {
            Inject(mono);
        }
    }

    private void Inject(MonoBehaviour mono)
    {
        var members = mono.GetType()
            .GetMembers(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .Where(m => m.IsDefined(typeof(InjectAttribute), true));

        foreach (var member in members)
        {
            if (member is FieldInfo field)
            {
                field.SetValue(mono, _sceneContainer.Resolve(field.FieldType));
            }
            else if (member is MethodInfo method)
            {
                var args = method.GetParameters()
                    .Select(p => _sceneContainer.Resolve(p.ParameterType))
                    .ToArray();

                method.Invoke(mono, args);
            }
        }
    }
}

[AttributeUsage(AttributeTargets.Class)]
public class SceneProvideAttribute : Attribute { }