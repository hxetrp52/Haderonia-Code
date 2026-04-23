using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class UIHapticWatcher : MonoBehaviour
{
    [Header("Input Action (Submit)")]
    [SerializeField] private InputActionReference submitAction;

    [SerializeField] private HapticProfile transitionHaptic;
    [SerializeField] private HapticProfile pressHaptic;

    private EventSystem _eventSystem;
    private GameObject _lastSelected;

    [SerializeField] private GamePadManager gamepadManager;

    private void OnEnable()
    {
        if (submitAction != null || Gamepad.current != null)
        {
            submitAction.action.performed += OnSubmit;
            submitAction.action.Enable();
        }
    }

    private void OnDisable()
    {
        if (submitAction != null)
        {
            submitAction.action.performed -= OnSubmit;
            submitAction.action.Disable();
        }
    }
    public void SetEventSystem()
    {
        _eventSystem = EventSystem.current;
    }

    public void UIWatcherUpdate()
    {
        if (_eventSystem == null) SetEventSystem();

        var current = _eventSystem.currentSelectedGameObject;

        if (current == null) return;

        if (!current.activeInHierarchy) return;

        if (current != _lastSelected)
        {
            _lastSelected = current;
            gamepadManager.gamepadHaptic.PlayHaptic(transitionHaptic);
        }
    }

    private void OnSubmit(InputAction.CallbackContext ctx)
    {
        var current = _eventSystem.currentSelectedGameObject;

        if (current == null)return;

        if (!current.activeInHierarchy) return;

        if (current.TryGetComponent<Selectable>(out var selectable) &&
            selectable.IsInteractable())
        {
            gamepadManager.gamepadHaptic.PlayHaptic(pressHaptic);
        }
    }

}
