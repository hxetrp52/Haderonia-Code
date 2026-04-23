using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering.Universal;
using UIStringKey;

public class UIManager : ManagerBase // UI 담당하는 매니저
{
    [HideInInspector] public Camera overlayCamera;
    [SerializeField] private RectTransform _panelCanvas;
    [SerializeField] private RectTransform _popupCanvas;

    [SerializeField] private GameManager gameManager;
    [SerializeField] private AddressableLoadManager loadManager;

    public Transform panelCanvas => _panelCanvas;
    public Transform popupCanvas => _popupCanvas;

    public Dictionary<string, UIBase> panelList = new Dictionary<string, UIBase>();
    public Dictionary<string, UIPopup> popupList = new Dictionary<string, UIPopup>();

    private readonly Stack<UIPopup> popupStack = new();

    private bool _initialized = false;

    public override void Init()
    {
        Initialize();
    }

    public override void ManagerUpdate()
    {

    }

    private void Initialize()
    {
        _initialized = true;
    }

    private void EnsureInitialized()
    {
        if (_initialized) return;

        Initialize();

        if (_panelCanvas != null && _panelCanvas.parent != transform)
        {
            _panelCanvas.SetParent(transform, false);
        }

        if (_popupCanvas != null && _popupCanvas.parent != transform)
        {
            _popupCanvas.SetParent(transform, false);
        }

    }

    private void PreloadPopup(string popupKey)
    {
        if (_popupCanvas == null) return;
        if (popupList.ContainsKey(popupKey)) return;

        loadManager.LoadPrefab(popupKey, prefab =>
        {
            if (prefab == null)
            {
                Debug.LogWarning($"Popup preload failed : {popupKey}");
                return;
            }

            var instance = Instantiate(prefab, _popupCanvas);
            instance.name = prefab.name;
            instance.SetActive(true);
            instance.SetActive(false);

            var UIPopup = instance.GetComponent<UIPopup>();
            if (UIPopup == null)
            {
                Debug.LogWarning($"UIPopup missing : {popupKey}");
                Destroy(instance);
                return;
            }

            popupList.Add(popupKey, UIPopup);
        });
    }


    #region Panel Management
    public void OpenPanel(string panelKey, Action<UIBase> finished = null)
    {
        EnsureInitialized();

        if (panelList.TryGetValue(panelKey, out var panel))
        {
            panel.transform.SetParent(_panelCanvas);
            panel.OnShow(finished);
            return;
        }

        loadManager.InstantiatePrefab(panelKey, _panelCanvas, instance =>
        {
            if (instance == null)
            {
                Debug.LogError($"Panel load failed : {panelKey}");
                return;
            }

            instance.name = panelKey;

            var UIBase = instance.GetComponent<UIBase>();

            panelList.Add(panelKey, UIBase);
            UIBase.GetGameManager(gameManager);
            UIBase.OnShow(finished);
        });
    }

    public void OpenPanel<Tdata>(string panelKey, Tdata payload, Action<UIBase> finished = null)
    {
        EnsureInitialized();

        void ShowWithPayload(UIBase ui)
        {
            if (ui is IUiSetup<Tdata> setup)
                setup.Setup(payload);
            ui.OnShow(finished);
        }

        if (panelList.TryGetValue(panelKey, out var panel))
        {
            panel.transform.SetParent(_panelCanvas);
            ShowWithPayload(panel);
            return;
        }

        loadManager.InstantiatePrefab(panelKey, _panelCanvas, instance =>
        {
            if (instance == null)
            {
                Debug.LogError($"Panel load failed : {panelKey}");
                return;
            }

            instance.name = panelKey;

            var UIBase = instance.GetComponent<UIBase>();
            panelList.Add(panelKey, UIBase);
            UIBase.GetGameManager(gameManager);

            ShowWithPayload(UIBase);
        });
    }


    public void ClosePanel(string panelName, bool destroy = false)
    {
        if (panelList.ContainsKey(panelName))
        {
            panelList[panelName].OnClose((uiPanel) =>
            {
                if (destroy)
                {
                    panelList.Remove(panelName);
                    Destroy(uiPanel.gameObject);
                    loadManager.ReleasePrefab(panelName);
                }
                else
                {
                    uiPanel.gameObject.SetActive(false);
                }
            });
        }
    }

    public UIBase GetPanel(string panelName)
    {
        if (panelList.ContainsKey(panelName))
        {
            return panelList[panelName];
        }
        return null;
    }
    #endregion

    #region Popup Management
    public void OpenPopup(string popupKey, Action<UIBase> finished = null)
    {
        EnsureInitialized();

        if (popupList.TryGetValue(popupKey, out var popup))
        {
            if (popup.IsShown) return;
            popup.transform.SetParent(_popupCanvas);
            popup.OnShow(finished);
            popupStack.Push(popup);
            return;
        }

        loadManager.InstantiatePrefab(popupKey, _popupCanvas, instance =>
        {
            if (instance == null)
            {
                Debug.LogError($"Popup load failed : {popupKey}");
                return;
            }

            instance.name = popupKey;

            var UIPopup = instance.GetComponent<UIPopup>();

            popupList.Add(popupKey, UIPopup);
            UIPopup.GetGameManager(gameManager);
            UIPopup.OnShow(finished);

            popupStack.Push(UIPopup);
        });
    }

    public void OpenPopup<Tdata>(string popupKey, Tdata payload, Action<UIBase> finished = null)
    {
        EnsureInitialized();

        void ShowWithPayload(UIPopup ui)
        {
            if (ui is IUiSetup<Tdata> setup)
                setup.Setup(payload);
            ui.OnShow(finished);
        }

        if (popupList.TryGetValue(popupKey, out var popup))
        {
            if (popup.IsShown) return;
            popup.transform.SetParent(_popupCanvas);
            ShowWithPayload(popup);
            popupStack.Push(popup);
            return;
        }

        loadManager.InstantiatePrefab(popupKey, _popupCanvas, instance =>
        {
            if (instance == null)
            {
                Debug.LogError($"Popup load failed : {popupKey}");
                return;
            }

            instance.name = popupKey;

            var UIPopup = instance.GetComponent<UIPopup>();
            if (UIPopup == null)
            {
                Debug.LogError($"UIPopup component missing : {popupKey}");
                Destroy(instance);
                return;
            }

            if (popupList.ContainsKey(popupKey))
            {
                Destroy(instance);
                return;
            }

            popupList.Add(popupKey, UIPopup);
            UIPopup.GetGameManager(gameManager);

            ShowWithPayload(UIPopup);
            popupStack.Push(UIPopup);
        });
    }

    public void ClosePopup(string popupName, bool destroy = false)
    {
        if (popupList.ContainsKey(popupName))
        {
            popupList[popupName].OnClose((UIPopup) =>
            {
                if (destroy)
                {
                    popupList.Remove(popupName);
                    Destroy(UIPopup.gameObject);
                    loadManager.ReleasePrefab(popupName);
                }
                else
                {
                    UIPopup.gameObject.SetActive(false);
                }
            });
        }
    }

    public void CloseTopPopup(bool destroy = false)
    {
        if (popupStack.Count == 0)
            return;

        var top = popupStack.Pop();
        ClosePopup(top.name, destroy);
    }

    public UIPopup GetPopup(string popupName)
    {
        if (popupList.ContainsKey(popupName))
        {
            return popupList[popupName];
        }
        return null;
    }
    #endregion

    public bool HandleEscape()
    {
        if (popupStack.Count > 0)
        {
            CloseTopPopup();
            return true;
        }

        OpenPopup(UIKey.LobbySettingUI);
        return false;
    }

    #region Utility
    public bool HasAnyActiveUi()
    {
        foreach (var panel in panelList)
        {
            if (panel.Value?.gameObject.activeSelf == true)
                return true;
        }

        foreach (var popup in popupList)
        {
            if (popup.Value?.gameObject.activeSelf == true)
                return true;
        }

        return false;
    }


    public void AllShow()
    {
        foreach (var panel in panelList)
        {
            if (!panel.Value.gameObject.activeSelf)
                panel.Value.OnShow();
        }

        foreach (var popup in popupList)
        {
            if (!popup.Value.gameObject.activeSelf)
                popup.Value.OnShow();
        }
    }
    public void AllHide()
    {
        foreach (var panel in panelList)
        {
            if (panel.Value.gameObject.activeSelf)
                panel.Value.OnClose();
        }

        foreach (var popup in popupList)
        {
            if (popup.Value.gameObject.activeSelf)
                popup.Value.OnClose();
        }
    }

    public void AllClear()
    {
        foreach (var panel in panelList)
        {
            panel.Value.OnClose((uiPanel) =>
            {
                Destroy(panel.Value.gameObject);
                loadManager.ReleasePrefab(panel.Key);
            });
        }
        panelList.Clear();

        foreach (var popup in popupList)
        {
            popup.Value.OnClose((UIPopup) =>
                {
                    Destroy(popup.Value.gameObject);
                    loadManager.ReleasePrefab(popup.Key);
                });
        }
        popupList.Clear();
    }
    #endregion
}
