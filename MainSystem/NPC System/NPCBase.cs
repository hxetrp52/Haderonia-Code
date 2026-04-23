using System;
using UnityEngine;

public abstract class NPCBase : MonoBehaviour
{
    public NPCModuleBase[] modules;

    [Inject] public GameManager gameManager;
    [Inject] public PlayerMain player;

    public Action Onfocus;
    public Action Onunfocus;
    public Action Oninteract;

    private void Start()
    {
        if (modules == null) return;
        foreach(var module in modules)
        {
            if (module != null)
                module.Initialize(this);
        }
    }

    private void OnDestroy()
    {
        Onfocus = null;
        Onunfocus = null;
        Oninteract = null;
    }

    /// <summary>플레이어 기준 가장 가까운 NPC로 선택되었을 때</summary>
    public virtual void OnFocused()
    {
        Onfocus?.Invoke();
    }

    /// <summary>선택 해제되었을 때</summary>
    public virtual void OnUnfocused()
    {
        Onunfocus?.Invoke();
    }

    /// <summary>상호작용 키 눌렀을 때</summary>
    public virtual void Interact()
    {
        Oninteract?.Invoke();
    }
}
