using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour, IPlayerComponent
{
    private float speed;

    public Rigidbody rb;

    private PlayerMain player;
    private PlayerAnimator playerAnimator;

    private PlayerControlLockManager controlLockController;

    private float x;
    private float z;

    private float dashOverrideRemainingTime;
    private Vector3 dashOverrideVelocity;

    private bool isBack = true;


    public void Init(PlayerMain _player)
    {
        player = _player;
        playerAnimator = player.GetPlayerComponent<PlayerAnimator>();
    }

    public void LateInit()
    {
        controlLockController = player.GetGameManager().GetManager<PlayerControlLockManager>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (controlLockController.IsLocked(PlayerControlLock.Move)) return;

        if (dashOverrideRemainingTime > 0f)
        {
            dashOverrideRemainingTime -= Time.fixedDeltaTime;
            rb.linearVelocity = dashOverrideVelocity;
            return;
        }

        rb.linearVelocity = new Vector3(x, 0, z) * speed;

    }

    private void Update()
    {
        if (controlLockController.IsLocked(PlayerControlLock.Move))
        {
            StopMovement();
            return;
        }

        playerAnimator.ControlMoveAnimation(new Vector3(x, 0, z));
    }

    public void SetMoveSpeed(float newSpeed)
    {
        speed = newSpeed;
    }

    public Vector3 GetMoveDirection()
    {
        return new Vector3(x, 0, z);
    }

    public void ApplyDashVelocity(Vector3 velocity, float duration)
    {
        dashOverrideVelocity = velocity;
        dashOverrideRemainingTime = Mathf.Max(0f, duration);
    }


    public void OnMove(InputAction.CallbackContext context)
    {
        if (context.performed || context.canceled)
        {
            Vector2 moveInput = context.ReadValue<Vector2>();
            x = moveInput.x;
            z = moveInput.y;
        }
    }

    public void StopMovement()
    {
        dashOverrideRemainingTime = 0f;
        dashOverrideVelocity = Vector3.zero;
        rb.linearVelocity = Vector3.zero;
        playerAnimator.StopAnimation();
    }
}
