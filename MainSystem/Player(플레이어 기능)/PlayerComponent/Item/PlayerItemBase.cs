using UnityEngine;

public class PlayerItemBase : MonoBehaviour
{
    protected PlayerMain player;
    protected PlayerStat playerStat; 
    protected PlayerCombat playerCombat;
    protected TimeManager timeManager;

    [SerializeField] private Sprite itemSprite;
    [SerializeField] protected float itemCooltime;
    private bool canUseItem = true;
    private GameTimer cooldownTimer;
    private Sprite cachedItemSprite;
    private bool hasTriedSpriteLookup;

    public virtual void Init(PlayerMain _player)
    {
        player = _player;
        playerStat = player.GetPlayerComponent<PlayerStat>();
        playerCombat = player.GetPlayerComponent<PlayerCombat>();
        timeManager = player.GetGameManager().GetManager<TimeManager>();
    }

    public virtual void UseItem()
    {
        if (!canUseItem) return;

        OnUseItem();
        StartCooldown();
    }

    protected virtual void OnUseItem()
    {
    }

    private void StartCooldown()
    {
        if (itemCooltime <= 0f)
        {
            canUseItem = true;
            cooldownTimer = null;
            return;
        }

        canUseItem = false;
        cooldownTimer = timeManager.StartTimer(itemCooltime, false, () =>
        {
            canUseItem = true;
            cooldownTimer = null;
        });
    }

    public bool CanUseItem()
    {
        return canUseItem;
    }

    public float GetCooldownRemainingNormalized()
    {
        if (canUseItem || cooldownTimer == null || cooldownTimer.Duration <= 0f)
            return 1f;

        return 1f - Mathf.Clamp01(cooldownTimer.GetRemainingTime() / cooldownTimer.Duration);
    }

    public float GetCooldownRemainingTime()
    {
        if (canUseItem || cooldownTimer == null)
            return 0f;

        return cooldownTimer.GetRemainingTime();
    }

    public Sprite GetItemSprite() => itemSprite;
   
}
