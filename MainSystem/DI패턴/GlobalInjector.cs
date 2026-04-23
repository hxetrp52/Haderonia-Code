using System;
using System.Linq;
using UnityEngine;

[DefaultExecutionOrder(-200)]
public class GlobalInjector : MonoBehaviour
{
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        RegisterGlobalProviders();
    }

    private void RegisterGlobalProviders()
    {
        var providers = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None)
            .Where(m => m.GetType().IsDefined(typeof(GlobalProvideAttribute), true));

        foreach (var provider in providers)
        {
            DependencyContainer.RegisterGlobal(provider.GetType(), provider);
        }
    }
}

[AttributeUsage(AttributeTargets.Class)]
public class GlobalProvideAttribute : Attribute { }
