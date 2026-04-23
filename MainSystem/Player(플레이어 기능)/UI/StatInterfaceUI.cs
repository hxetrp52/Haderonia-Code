using EnumTypes;
using TMPro;
using UnityEngine;
using System.Collections;

public class StatInterfaceUI : MonoBehaviour
{
    [SerializeField] private TMP_Text attackPowerText;
    [SerializeField] private TMP_Text moveSpeedText;
    [SerializeField] private TMP_Text attackSpeedText;
    private PlayerStat playerStat;
    private Stat attackPowerStat;
    private Stat moveSpeedStat;
    private Stat attackSpeedStat;
    private bool subscribed = false;


    public void Initialize(PlayerStat stat)
    {
        playerStat = stat;
        TrySubscribeStatEvents();
        RefreshAll();
    }

    private void OnDisable()
    {
        UnsubscribeStatEvents(); // 스탯 이벤트 일괄 해제
    }

    private void TrySubscribeStatEvents()
    {
        // Outfit change recreates Stat instances, so existing subscriptions must be rebound.
        if (subscribed)
            UnsubscribeStatEvents();

        attackPowerStat = playerStat.GetStat(StatType.AttackPower);
        moveSpeedStat = playerStat.GetStat(StatType.MoveSpeed);
        attackSpeedStat = playerStat.GetStat(StatType.AttackSpeed);

        attackPowerStat.OnValueChanged += HandleAttackPowerChanged;
        moveSpeedStat.OnValueChanged += HandleMoveSpeedChanged;
        attackSpeedStat.OnValueChanged += HandleAttackSpeedChanged;
        subscribed = true;
    }

    private void UnsubscribeStatEvents()
    {
        if (!subscribed)
            return;

        if (attackPowerStat != null) attackPowerStat.OnValueChanged -= HandleAttackPowerChanged;
        if (moveSpeedStat != null) moveSpeedStat.OnValueChanged -= HandleMoveSpeedChanged;
        if (attackSpeedStat != null) attackSpeedStat.OnValueChanged -= HandleAttackSpeedChanged;
        attackPowerStat = null;
        moveSpeedStat = null;
        attackSpeedStat = null;
        subscribed = false;
    }

    private void HandleAttackPowerChanged(float value)
    {
        if (attackPowerText != null)
            attackPowerText.text = $"{value:F0}";
    }

    private void HandleMoveSpeedChanged(float value)
    {
        if (moveSpeedText != null)
            moveSpeedText.text = $"{value:F1}";
    }

    private void HandleAttackSpeedChanged(float value)
    {
        if (attackSpeedText != null)
            attackSpeedText.text = $"{value:F2}";
    }

    private void RefreshAll()
    {
        HandleAttackPowerChanged(attackPowerStat.Value);
        HandleMoveSpeedChanged(moveSpeedStat.Value);
        HandleAttackSpeedChanged(attackSpeedStat.Value);
    }
}
