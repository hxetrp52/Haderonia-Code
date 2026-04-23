using UnityEngine;

public class PlayerItemSystem : MonoBehaviour, IPlayerComponent
{
    private PlayerMain player;
    private PlayerCombat playerCombat;
    private bool cachedItemReady;
    private float cachedCooldownNormalized;
    private bool isCacheInitialized;

    public PlayerItemBase currentItem;

    public void Init(PlayerMain _player)
    {
        player = _player;
        playerCombat = player.GetPlayerComponent<PlayerCombat>();
    }

    public void LateInit()
    {
        if (currentItem == null) return;

        currentItem.Init(player);
        playerCombat?.combatUI?.BindItem(currentItem);
        SyncCachedState(currentItem.CanUseItem(), currentItem.GetCooldownRemainingNormalized());
    }

    public void OnUseItem()
    {
        if (currentItem == null) return;

        currentItem.UseItem();
        playerCombat?.combatUI?.RefreshItemCooldownUI();
    }

    private void Update()
    {
        if (currentItem == null) return;

        bool canUseItem = currentItem.CanUseItem();
        float cooldownNormalized = GetCooldownNormalized(canUseItem);

        if (ShouldRefreshUI(canUseItem, cooldownNormalized))
        {
            playerCombat?.combatUI?.RefreshItemCooldownUI();
        }

        SyncCachedState(canUseItem, cooldownNormalized);
    }

    private float GetCooldownNormalized(bool canUseItem)
    {
        bool shouldRecalculate = !canUseItem || !cachedItemReady;

        return shouldRecalculate
            ? currentItem.GetCooldownRemainingNormalized()
            : cachedCooldownNormalized;
    }

    private bool ShouldRefreshUI(bool canUseItem, float cooldownNormalized)
    {
        if (!isCacheInitialized) return true;

        if (cachedItemReady != canUseItem) return true;

        bool cooldownChanged = (!canUseItem || !cachedItemReady) &&
                               !Mathf.Approximately(cachedCooldownNormalized, cooldownNormalized);

        return cooldownChanged;
    }

    private void SyncCachedState(bool canUseItem, float cooldownNormalized)
    {
        cachedItemReady = canUseItem;
        cachedCooldownNormalized = cooldownNormalized;
        isCacheInitialized = true;
    }
}
