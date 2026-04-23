using UnityEngine;
using UnityEngine.Serialization;
using AudioKey;

public class PlayerRollSystem : MonoBehaviour, IPlayerComponent
{
    [FormerlySerializedAs("itemCooltime")]
    [SerializeField] private float rollCooldownSeconds = 3f;
    [SerializeField] private float dashSpeed = 18f;
    [SerializeField] private float dashDuration = 0.18f;

    private PlayerMain player;
    private PlayerMovement playerMovement;
    private TimeManager timeManager;
    private PlayerControlLockManager controlLockManager;
    private PlayerAnimator playerAnimator;

    private GameTimer cooldownTimer;
    private bool canRoll = true;
    private Vector3 pendingDashDirection = Vector3.zero;

    public void Init(PlayerMain _player)
    {
        player = _player;
    }

    public void LateInit()
    {
        playerMovement = player.GetPlayerComponent<PlayerMovement>();
        playerAnimator = player.GetPlayerComponent<PlayerAnimator>();
        timeManager = player.GetGameManager().GetManager<TimeManager>();
        controlLockManager = player.GetGameManager().GetManager<PlayerControlLockManager>();
    }

    public bool TryRoll()
    {
        if (!canRoll) return false;

        Vector3 moveDirection = playerMovement.GetMoveDirection();
        if (moveDirection.sqrMagnitude <= Mathf.Epsilon) return false;

        pendingDashDirection = moveDirection.normalized;
        ExecuteRoll();
        playerAnimator.PlayRollingAnimation();
        player.GetGameManager().GetManager<AudioManager>().PlaySFX(AudioKey.Key.SFX_Roll, transform.position);
        StartCooldown();
        return true;
    }

    private void ExecuteRoll()
    {
        if (pendingDashDirection.sqrMagnitude <= Mathf.Epsilon) return;

        Vector3 currentVelocity = playerMovement.rb.linearVelocity;
        Vector3 dashVelocity = new Vector3(
            pendingDashDirection.x * dashSpeed,
            currentVelocity.y,
            pendingDashDirection.z * dashSpeed);

        playerMovement.ApplyDashVelocity(dashVelocity, dashDuration);
        pendingDashDirection = Vector3.zero;
    }

    private void StartCooldown()
    {
        if (rollCooldownSeconds <= 0f)
        {
            canRoll = true;
            cooldownTimer = null;
            return;
        }

        canRoll = false;
        cooldownTimer = timeManager.StartTimer(rollCooldownSeconds, false, () =>
        {
            canRoll = true;
            cooldownTimer = null;
        });
    }

    public float GetCooldownProgress()
    {
        if (canRoll || cooldownTimer == null || cooldownTimer.Duration <= 0f)
            return 1f;

        return 1f - Mathf.Clamp01(cooldownTimer.GetRemainingTime() / cooldownTimer.Duration);
    }
}
