using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

[GlobalProvide]
public class PlayerMain : MonoBehaviour
{

    private static PlayerMain instance = null;

    private Vector3 stadPos;

    public static PlayerMain Instance
    {
        get
        {
            if (instance == null)
            {
                return null;
            }
            return instance;
        }
    }
    protected Dictionary<Type, IPlayerComponent> playerComponents = new Dictionary<Type, IPlayerComponent>();

    [Inject] private GameManager gameManager;

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

        AddComponents();
        InitComponents();
        stadPos = transform.position;
        SceneManager.sceneLoaded += (scene, mode) =>
        {
            transform.position = scene.name.Contains("Tutorial") ? new Vector3(0f, -0.075f, -6.4f) : Vector3.zero;
        };
    }

    private void Start()
    {
        LateInitComponents();
    }

    protected virtual void AddComponents()
    {
        GetComponentsInChildren<IPlayerComponent>().ToList()
            .ForEach(component => playerComponents.Add(component.GetType(), component));
    }

    protected virtual void InitComponents()
    {
        playerComponents.Values.ToList().ForEach(component => component.Init(this));
    }

    protected virtual void LateInitComponents()
    {
        playerComponents.Values.ToList().ForEach(component => component.LateInit());
    }

    public T GetPlayerComponent<T>() where T : IPlayerComponent
    {
        if (playerComponents.TryGetValue(typeof(T), out var comp))
            return (T)comp;

        return default;
    }

    public IPlayerComponent GetPlayerComponent(Type type)
    {
        playerComponents.TryGetValue(type, out var comp);
        return comp;
    }

    public GameManager GetGameManager()
    {
        return gameManager;
    }

    public void OnReset()
    {
        transform.position = stadPos;
        PlayerStat stat = GetPlayerComponent<PlayerStat>();
        stat.SetMaxHp();
        GetPlayerComponent<PlayerMovement>()?.StopMovement();
        GetPlayerComponent<PlayerCombat>()?.SetCanCombat(true);
    }
}
