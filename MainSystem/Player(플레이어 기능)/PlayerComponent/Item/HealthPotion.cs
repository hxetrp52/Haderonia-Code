using UnityEngine;
using AudioKey;

public class HealthPotion : PlayerItemBase
{
    [SerializeField] private int healAmount = 70;
    [SerializeField] private ParticleSystem helthEffect;

    public override void Init(PlayerMain _player)
    {
        base.Init(_player);
    }

    protected override void OnUseItem()
    {
        playerStat?.TakeHeal(healAmount);
        helthEffect.Play();
        playerCombat.audioManager.PlaySFX(Key.SFX_Combat_HealPotion, transform.position);
    }
}
