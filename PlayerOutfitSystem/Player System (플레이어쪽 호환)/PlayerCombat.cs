using EnumTypes;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCombat : MonoBehaviour, IPlayerComponent
{
    public const float MinAttackSpeedMultiplier = 0.01f;

    public IOutFitComponent outfitCombat;
    [HideInInspector] public PlayerMain player;
    [HideInInspector] public TimeManager timeManager;
    [HideInInspector] public GamePadManager gamePadManager;
    [HideInInspector] public AudioManager audioManager;
    private PlayerControlLockManager playerControlLockManager;

    private PlayerAttackDirector attackDirector = new PlayerAttackDirector();
    public RuntimeObjectPool playerPool;

    public LayerMask targetMask; // 보스/적 레이어

    public PlayerCombatUI combatUI; 
    private PlayerStat playerStat;
    public PlayerAnimator playerAnimator;

    private GameTimer attackTimer;
    private GameTimer skillTimer;

    private int attackPower;

    private float attackCooldown;
    private float skillCooldown;
    private float attackSpeedMultiplier = 1f;

    private bool canCombat = true;

    private bool canAttack = true;
    private bool canUseSkill = true;

    private CharacterOutfitData outfitData;

    private int cachedHp = -1;
    private float cachedMaxHp = -1f;
    private bool cachedSkillReady;

    public void Init(PlayerMain _player)
    {
        player = _player;
        targetMask = LayerMask.GetMask("Enemy", "Boss");
        playerStat = player.GetPlayerComponent<PlayerStat>();


        combatUI.Initialize(playerStat, this);
        combatUI.SetCanvas();
    }

    public void LateInit()
    {
        timeManager = player.GetGameManager().GetManager<TimeManager>();
        gamePadManager = player.GetGameManager().GetManager<GamePadManager>();
        audioManager = player.GetGameManager().GetManager<AudioManager>();
        playerControlLockManager = player.GetGameManager().GetManager<PlayerControlLockManager>();
    }

    private void Update()
    {
        if (combatUI == null || playerStat == null) return;

        UpdateHpUiIfChanged();
        UpdateSkillUiIfNeeded();
    }

    public void SetAttackPower(int attackPower)
    {
        this.attackPower = attackPower;
    }

    public void SetCanCombat(bool canCombat) // 전투 가능하게 설정
    {
        this.canCombat = canCombat;
    }

    public void SetOutfitCombat(IOutFitComponent combat) // 컴뱃 스크립트 설정
    {
        outfitCombat = combat;
    }

    public void SetOutfitData(CharacterOutfitData data) // 공속, 스킬 쿨타임 받아오는 의상 데이터 설정
    {
        outfitData = data;
        attackCooldown = data.attackSpeed;
        skillCooldown = data.skillCoolTime;
        attackDirector.currentType = data.attackDirectionType;
        canAttack = true;
        canUseSkill = true;
    }

    public Vector3 GetAttackDirection() 
    {
        Vector3 moveDir = player.GetPlayerComponent<PlayerMovement>().GetMoveDirection();
        return attackDirector.GetAttackDirection(moveDir);
    }

    public void SetEndSkill() // 스킬 끝났을 시점부터 스킬 타이머 작동
    {
        canUseSkill = false;

        skillTimer = timeManager.StartTimer(outfitData.skillCoolTime, false, () =>
        {
            canUseSkill = true;
        });
    }

    public bool CanUseSkill()
    {
        return canUseSkill;
    }

    public float GetSkillCooldownFillAmount()
    {
        if (canUseSkill || skillTimer == null) return 1f;

        if (skillTimer.Duration <= 0f) return 1f;

        return Mathf.Clamp01(skillTimer.GetNormalized());
    }

    public void SetAttackSpeedMultiplier(float multiplier) // 공격속도 배율 설정 (버프/디버프 연동용)
    {
        attackSpeedMultiplier = Mathf.Max(MinAttackSpeedMultiplier, multiplier);
        playerStat?.SetAttackSpeedMultiplier(attackSpeedMultiplier);
    }

    private void UpdateAttackCooldown() //공격 속도 관리 
    {
        float cd = 1f / (outfitData.attackSpeed * attackSpeedMultiplier);
        attackTimer = timeManager.StartTimer(cd, false, () =>
        {
            canAttack = true;
        });
    }


    public void OnPerformAttack(InputAction.CallbackContext context) // 공격 실행
    {
        if (!canAttack || !canCombat) return;
        if (playerControlLockManager.IsLocked(PlayerControlLock.Attack)) return;

        outfitCombat.Attack(context);
        canAttack = false;
        UpdateAttackCooldown();
    }

    public void OnPerformSkill(InputAction.CallbackContext context) // 스킬 실행 
    {
        if (!canUseSkill || !canCombat) return;
        if (playerControlLockManager.IsLocked(PlayerControlLock.Skill)) return;


        outfitCombat.Skill(context);
    }

    private void UpdateHpUiIfChanged()
    {
        int currentHp = playerStat.GetCurrentHp();
        float maxHp = playerStat.GetStat(StatType.MaxHP)?.Value ?? 0f;

        if (cachedHp == currentHp && Mathf.Approximately(cachedMaxHp, maxHp))
            return;

        combatUI.UpdateHpSlider();
        cachedHp = currentHp;
        cachedMaxHp = maxHp;
    }

    private void UpdateSkillUiIfNeeded()
    {
        bool isSkillCoolingDown = !canUseSkill;
        bool skillReadyChanged = cachedSkillReady != canUseSkill;

        if (isSkillCoolingDown || skillReadyChanged)
            combatUI.UpdateSkillCooldown();

        cachedSkillReady = canUseSkill;
    }
}
