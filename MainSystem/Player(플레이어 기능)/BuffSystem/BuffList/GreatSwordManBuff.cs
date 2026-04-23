using UnityEngine;
using EnumTypes;

public class GreatSwordManBuff : Buff
{
    private PlayerStat playerStat;
    private PlayerCombat playerCombat;
    private System.Action<bool> onFixedDamageChanged; // 고정 피해 모드 토글 콜백
    private System.Action onSkillEnd;                 // 스킬 종료 시 쿨타임 시작 콜백

    private const int SKILL_DAMAGE = 50;

    /// <summary>
    /// 불사의 힘 버프
    /// </summary>
    /// <param name="duration">버프 지속 시간</param>
    /// <param name="playerStat">무적·데미지 처리용 플레이어 스탯</param>
    /// <param name="playerCombat">공격속도 배율 변경용 플레이어 컴뱃</param>
    /// <param name="onFixedDamageChanged">true = 고정 피해 모드 ON, false = OFF</param>
    /// <param name="onSkillEnd">버프 만료 후 스킬 쿨타임 시작</param>
    public GreatSwordManBuff(
        float duration,
        PlayerStat playerStat,
        PlayerCombat playerCombat,
        System.Action<bool> onFixedDamageChanged,
        System.Action onSkillEnd)
        : base("불사의 힘",
            duration: duration,
            stackType: BuffStackType.Stack,
            BuffCategory.Buff)
    {
        this.playerStat = playerStat;
        this.playerCombat = playerCombat;
        this.onFixedDamageChanged = onFixedDamageChanged;
        this.onSkillEnd = onSkillEnd;
    }

    protected override void OnApply()
    {
        // 무적 활성화
        playerStat?.SetSkillInvincibility(true);

        // 공격속도 2배
        playerCombat?.SetAttackSpeedMultiplier(2f);

        // 체력 100 감소 (100 이하면 피 1로 생존)
        playerStat?.TakeDamageOrSurvive(SKILL_DAMAGE);

        // 공격 타입을 고정 피해(Fixed)로 전환
        onFixedDamageChanged?.Invoke(true);
    }

    public override void OnRemove()
    {
        // 무적 해제
        playerStat?.SetSkillInvincibility(false);

        // 공격속도 원상복구
        playerCombat?.SetAttackSpeedMultiplier(1f);

        // 공격 타입 원상복구
        onFixedDamageChanged?.Invoke(false);

        // 스킬 쿨타임 시작
        onSkillEnd?.Invoke();
    }
}
