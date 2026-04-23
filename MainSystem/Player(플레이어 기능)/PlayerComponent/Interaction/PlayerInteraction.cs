using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerInteraction : MonoBehaviour, IPlayerComponent
{
    public bool canInteraction = true;

    public PlayerInteractionController controller;


    public PlayerControlLockManager controlLockController;
    [SerializeField] private PlayerItemSystem itemSystem;
    [SerializeField] private PlayerRollSystem rollSystem;

    private GameManager gameManager;
    private PlayerMain player;

    public void Init(PlayerMain _player)
    {
        player = _player;
        gameManager = player.GetGameManager();
    }

    public void LateInit()
    {
        controlLockController = gameManager.GetManager<PlayerControlLockManager>();
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        if (controlLockController.IsLocked(PlayerControlLock.Interact)) return;

        if (context.performed)
        {
            controller.CloseNPCInteract();
        }
    }

    public void OnRoll(InputAction.CallbackContext context)
    {
        if (controlLockController.IsLocked(PlayerControlLock.Move)) return;

        if (context.performed)
        {
            rollSystem.TryRoll();
        }
    }

    public void OnUseItem(InputAction.CallbackContext context)
    {
        if (controlLockController.IsLocked(PlayerControlLock.Item)) return;

        if (context.performed)
        {
            itemSystem.OnUseItem();
        }
    }

    public void OnOpenManuWindow(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            gameManager.GetManager<UIManager>().HandleEscape();
        }
    }
}
