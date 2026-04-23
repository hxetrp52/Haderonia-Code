using EnumTypes;
using UnityEngine;

public abstract class IconDisplayBase : MonoBehaviour // 키가이드 띄워주는 스크립트
{
    [SerializeField] protected InputActionIconData iconData;
    [Inject] protected GameManager gameManager;

    protected virtual void Start()
    {
        gameManager.GetManager<InputModeManager>().OnDeviceChanged += Refresh;
        Refresh(gameManager.GetManager<InputModeManager>().CurrentDevice);
    }

    protected virtual void OnEnable() 
    {
        if (gameManager.GetManager<InputModeManager>() == null) return;

        gameManager.GetManager<InputModeManager>().OnDeviceChanged += Refresh;
        Refresh(gameManager.GetManager<InputModeManager>().CurrentDevice);
    }


    protected virtual void OnDisable()
    {
        gameManager.GetManager<InputModeManager>().OnDeviceChanged -= Refresh;
    }

    private void Refresh(InputDeviceType device)
    {
        if (iconData == null)
            return;

        ApplySprite(iconData.GetSprite(device));
    }

    protected abstract void ApplySprite(Sprite sprite);
}
