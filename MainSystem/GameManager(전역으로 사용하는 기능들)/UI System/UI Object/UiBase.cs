using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class UIBase : MonoBehaviour
{
    protected bool isInitialize = false;
    public CanvasGroup canvas;
    protected GameManager gameManager; 
    
    
    public bool IsShown { get; set; }
    protected bool _isShowing;
    
    protected virtual void OnEnable()
    {
        canvas.alpha = 0;
    }
    
    public virtual void SetData()
    {
        // 데이터 설정을 위한 가상 함수
    }
    
    protected virtual void Reset()
    {
        canvas = transform.GetComponent<CanvasGroup>();
    }
    
    public virtual void OnShow(Action<UIBase> finished = null)
    {
        IsShown = true;
        gameObject.SetActive(true);
        canvas.alpha = 1;
        gameManager.GetManager<PlayerControlLockManager>().AddLock(PlayerLockReason.UI, PlayerControlLock.All);
        finished?.Invoke(this);
    }

    public virtual void OnClose(Action<UIBase> finished = null)
    {
        IsShown = false;
        canvas.alpha = 0;
        gameObject.SetActive(false);

        var lockManager = gameManager.GetManager<PlayerControlLockManager>();
        if (lockManager == null)
        {
            Debug.LogError($"{nameof(PlayerControlLockManager)} not found while closing {name}");
            finished?.Invoke(this);
            return;
        }

        var uiManager = gameManager.GetManager<UIManager>();
        if (!uiManager.HasAnyActiveUi())
        {
            lockManager.RemoveLock(PlayerLockReason.UI);
        }
        finished?.Invoke(this);
    }

    public virtual void GetGameManager(GameManager gameManager)
    {
        this.gameManager = gameManager;
    }
}
