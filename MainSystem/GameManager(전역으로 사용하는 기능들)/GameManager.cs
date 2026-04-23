using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using AudioKey;

[GlobalProvide]
public class GameManager : MonoBehaviour
{
    private static GameManager instance = null;

    protected Dictionary<Type, ManagerBase> managerContainer = new Dictionary<Type, ManagerBase>();

    public List<ManagerBase> managers = new List<ManagerBase>();
    public static GameManager Instance
    {
        get
        {
            if(instance == null)
            {
                return null;
            }
            return instance;
        }
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
        }


        AddManagers();

        GetManager<UserDataManager>().AddPlayerOutFitList(GetManager<OutFitDataManager>().GetOutFit("기본 마법사 의상"));
        GetManager<UserDataManager>().AddPlayerOutFitList(GetManager<OutFitDataManager>().GetOutFit("대검사 의상"));
    }

    private void Update()
    {
        UpdateManagers();
    }

    private void AddManagers()
    {
        for (int i = 0; i < managers.Count; i++)
        {
            managerContainer.Add(managers[i].GetType(), managers[i]);
            managers[i].Init();
        }
    }

    private void UpdateManagers()
    {
        foreach (var manager in managerContainer.Values)
        {
            manager.ManagerUpdate();
        }
    }

    public T GetManager<T>() where T : ManagerBase
    {
        if (managerContainer.TryGetValue(typeof(T), out var comp))
            return (T)comp;

        return default;
    }

    public ManagerBase GetManager(Type type)
    {
        managerContainer.TryGetValue(type, out var comp);
        return comp;
    }
}
