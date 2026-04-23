using EnumTypes;
using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(menuName = "KeyGuide/Icon Data")]
public class InputActionIconData : ScriptableObject
{
    public InputActionReference action;

    [Header("Icons")]
    public Sprite keyboardMouse;
    public Sprite xbox;
    public Sprite playStation;
    public Sprite nintendo;

    public Sprite GetSprite(InputDeviceType device)
    {
        return device switch
        {
            InputDeviceType.KeyboardMouse => keyboardMouse,
            InputDeviceType.Xbox => xbox,
            InputDeviceType.PlayStation => playStation,
            InputDeviceType.Nintendo => nintendo,
            _ => keyboardMouse
        };
    }
}
