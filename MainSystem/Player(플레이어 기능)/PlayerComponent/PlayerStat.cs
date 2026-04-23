using UnityEngine;
using UnityEngine.Events;
using EnumTypes;

// PlayerStat를 IStatHolder로 구현하여 Stat 시스템을 사용합니다.
// 기존 IPlayerComponent Init/InitializeStatData 호출 방식은 유지됩니다.
public class PlayerStat : MonoBehaviour, IPlayerComponent, IStatHolder
{
    private const string AttackSpeedBuffModifierId = "BuffAttackSpeed";

    private PlayerMain playerMain;
    private PlayerMovement playerMovement;
    private PlayerCombat playerCombat;
    [HideInInspector] public BuffSystem playerBuffSystem;


    // Stat 객체들
    private Stat statMaxHp;
    private Stat statMoveSpeed;
    private Stat statAttackPower;
    private Stat statAttackSpeed;
    private Stat statSkillCoolTime;

    // 현재 체력
    private int nowHp;

    private bool isSkillInvincibility = false; // 스킬로 인한 무적 상태 여부
    private bool isInvincibility = false; // 일반적인 무적 상태 여부 (데미지 후 잠시 무적 등)
    private float defaultInvincibilityDuration = 0.1f;
    private TimeManager timeManager;
    private GameTimer invincibilityTimer;

    public UnityEvent OnDeathEvent;

    public void Init(PlayerMain _player)
    {
        playerMain = _player;
        playerMovement = _player.GetPlayerComponent<PlayerMovement>();
        playerCombat = _player.GetPlayerComponent<PlayerCombat>();

        OnDeathEvent.AddListener(Death);
    }

    public void LateInit()
    {
        timeManager = playerMain.GetGameManager().GetManager<TimeManager>();
        playerBuffSystem = new BuffSystem(this, timeManager);
    }

    public void InitializeStatData(CharacterOutfitData statData)
    {
        statMaxHp = new Stat(statData.maxHP);
        statMoveSpeed = new Stat(statData.moveSpeed);
        statAttackPower = new Stat(statData.attackPower);
        statAttackSpeed = new Stat(statData.attackSpeed);
        statSkillCoolTime = new Stat(statData.skillCoolTime);

        nowHp = Mathf.RoundToInt(statMaxHp.Value);

        statMoveSpeed.OnValueChanged += (v) => playerMovement?.SetMoveSpeed(v);
        statAttackPower.OnValueChanged += (v) => playerCombat?.SetAttackPower(Mathf.RoundToInt(v));

        playerMovement?.SetMoveSpeed(statMoveSpeed.Value);
        playerCombat?.SetAttackPower(Mathf.RoundToInt(statAttackPower.Value));

        playerCombat.combatUI.statInterfaceUI?.Initialize(this);

    }
    public void TakeHeal(int heal)
    {
        nowHp += heal;
        int max = Mathf.RoundToInt(statMaxHp?.Value ?? 0);
        if (nowHp > max) nowHp = max;
    }

    public void SetInvincibility(bool value)
    {
        isInvincibility = value;
    }

    public void SetSkillInvincibility(bool value)
    {
        isSkillInvincibility = value;
    }

    public int GetCurrentHp() => nowHp;

    public void TakeDamage(int damage, float invincibilityDuration = -1f)
    {
        if (nowHp <= 0) return; // 이미 사망 상태이면 적용하지 않음
        bool canTakeDamage = !isInvincibility && !isSkillInvincibility;
        if (!canTakeDamage) damage = 0;
        else
        {
            SetInvincibility(true);
            StartInvincibilityTimer(invincibilityDuration);
        }
        nowHp -= damage;
        if (nowHp <= 0)
        {
            nowHp = 0;
            Death();
        }
    }

    // 생존 보호가 있는 데미지: HP가 데미지 이하여도 1로 살아남는다 (이미 사망 상태면 무시)
    public void TakeDamageOrSurvive(int damage)
    {
        if (nowHp <= 0) return; // 이미 사망 상태이면 적용하지 않음
        if (nowHp <= damage)
            nowHp = 1;
        else
            nowHp -= damage;
    }

    public void Death()
    {
        // 기존 처리 유지
        GameFailCanvas.instance.SetPannel(1000,10000,1000,1000);
    }

    public void SetMaxHp()
    {
        nowHp = Mathf.RoundToInt(statMaxHp.Value);
    }

    #region 스탯Change헬퍼(호환성)
    public void ChangeMoveSpeed(float addValue)
    {
        if (statMoveSpeed == null) return;
        statMoveSpeed.SetBaseValue(statMoveSpeed.BaseValue + addValue);
    }

    public void ChangeMaxHP(int addValue)
    {
        if (statMaxHp == null) return;
        statMaxHp.SetBaseValue(statMaxHp.BaseValue + addValue);
        int max = Mathf.RoundToInt(statMaxHp.Value);
        if (nowHp > max) nowHp = max;
    }

    public void ChangeAttackSpeed(float addValue)
    {
        if (statAttackSpeed == null) return;
        statAttackSpeed.SetBaseValue(statAttackSpeed.BaseValue + addValue);
    }

    public void ChangeAttackPower(int addValue)
    {
        if (statAttackPower == null) return;
        statAttackPower.SetBaseValue(statAttackPower.BaseValue + addValue);
        playerCombat?.SetAttackPower(Mathf.RoundToInt(statAttackPower.Value));
    }

    public void ChangeSkillCollTime(float addValue)
    {
        if (statSkillCoolTime == null) return;
        statSkillCoolTime.SetBaseValue(statSkillCoolTime.BaseValue - addValue);
        if (statSkillCoolTime.Value < 2f) statSkillCoolTime.SetBaseValue(2f);
    }

    public void SetAttackSpeedMultiplier(float multiplier)
    {
        if (statAttackSpeed == null) return;
        float safeMultiplier = Mathf.Max(PlayerCombat.MinAttackSpeedMultiplier, multiplier);

        // Base multiplier (1f) means pure base stat state with buff/debuff modifier removed.
        statAttackSpeed.RemoveModifierById(AttackSpeedBuffModifierId);

        if (!Mathf.Approximately(safeMultiplier, 1f))
        {
            statAttackSpeed.AddModifier(new StatModifier(AttackSpeedBuffModifierId, StatOperation.Multiply, safeMultiplier));
        }
    }
    #endregion

    // IStatHolder 구현
    public Stat GetStat(StatType statType)
    {
        return statType switch
        {
            StatType.MaxHP => statMaxHp,
            StatType.MoveSpeed => statMoveSpeed,
            StatType.AttackPower => statAttackPower,
            StatType.AttackSpeed => statAttackSpeed,
            StatType.SkillCooldown => statSkillCoolTime,
            _ => null
        };
    }

    private void StartInvincibilityTimer(float duration)
    {
        StopInvincibilityTimer();

        float finalDuration = duration < 0f ? defaultInvincibilityDuration : duration;
        if (finalDuration <= 0f)
        {
            SetInvincibility(false);
            return;
        }

        invincibilityTimer = timeManager.StartTimer(finalDuration, false, () => 
        {
            SetInvincibility(false);
            invincibilityTimer = null;
        });
    }

    private void StopInvincibilityTimer()
    {
        if (timeManager != null && invincibilityTimer != null)
        {
            timeManager.StopTimer(invincibilityTimer);
            invincibilityTimer = null;
        }
    }

    private void OnDestroy()
    {
        StopInvincibilityTimer();
    }
}
