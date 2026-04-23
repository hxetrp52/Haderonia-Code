using EnumTypes;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using AudioKey;

public class GreatSwordManCombatData : OutfitCombatBase
{
    private const float PositionMatchEpsilonSqr = 0.0001f;
    private const float EffectDestroyBuffer = 0.2f;

    private const string SlashEffect_KEY = "Combat_GreatSwordMan_SlashEffect";
    private const string BuffEffect_KEY = "Combat_GreatSwordMan_BuffEffect";
    private const string HitEffect_KEY = "Combat_GreatSwordMan_Hit_Effect";

    private static readonly Color SkillEffectColor = Color.red;
    private static readonly Color DefaultEffectColor = Color.white;

    private GameObject slashEffect;
    private GameObject buffEffect;
    private GameObject hitEffectPrefab;

    private ParticleSystem slashParticle;
    private ParticleSystem.MainModule slashMainModule;
    private ParticleSystem.SubEmittersModule slashSubEmittersModule;

    private ParticleSystem sparkParticle;
    private ParticleSystem.MainModule sparkMainModule;

    private bool isSkillActive = false;
    private Color currentHitEffectColor = DefaultEffectColor;

    private float skillBuffDuration = 5f;

    [Header("GreatSword Attack")]
    [SerializeField] private float attackRadius = 1.5f;     // 공격 반경
    [SerializeField] private float attackAngle = 100f;      // 부채꼴 각도(전체각)
    [SerializeField] private Transform attackPivot;         // 판정 시작점(없으면 transform)

    public override void Enter(PlayerCombat combat, PlayerStat stat, CharacterOutfitData data)
    {
        base.Enter(combat, stat, data);

        LoadByKey<GameObject>(SlashEffect_KEY, prefab =>
        {
            slashEffect = Instantiate(prefab, transform);
            slashEffect.SetActive(false);
            slashParticle = slashEffect.GetComponent<ParticleSystem>();
            slashMainModule = slashParticle.main;
            slashSubEmittersModule = slashParticle.subEmitters;

            sparkParticle = slashSubEmittersModule.GetSubEmitterSystem(0);
            sparkMainModule = sparkParticle.main;
        });

        LoadByKey<GameObject>(BuffEffect_KEY, prefab =>
        {
            buffEffect = Instantiate(prefab, transform);
            buffEffect.SetActive(false);
        });

        LoadByKey<GameObject>(HitEffect_KEY, prefab =>
        {
            hitEffectPrefab = prefab;
        });
    }

    public override void Attack(InputAction.CallbackContext context)
    {
        if (!context.started) return;

        Vector3 attackDir = combat.GetAttackDirection();
        attackDir.y = 0f;
        attackDir.Normalize();

        animator.PlayAttackAnimation(attackDir);

        Transform pivot = attackPivot != null ? attackPivot : transform;
        Vector3 origin = pivot.position;

        SpawnSlashEffect(origin, attackDir);

        PlayHaptic(attackHaptic);
        combat.audioManager.PlaySFX(data.sfxData.GetSFXEvent(0), transform.position); 

        Collider[] hits = Physics.OverlapSphere(origin, attackRadius, combat.targetMask);

        HashSet<object> hitTargets = new HashSet<object>();

        // 스킬 활성 중에는 고정 피해(DamageType.Fixed) 적용 → 방어력 무시
        DamageType damageType = isSkillActive ? DamageType.Fixed : DamageType.Physical;

        foreach (Collider hit in hits)
        {
            Vector3 toTarget = hit.transform.position - origin;
            toTarget.y = 0f;

            if (toTarget.sqrMagnitude <= 0.0001f) continue;

            float angle = Vector3.Angle(attackDir, toTarget.normalized);
            if (angle > attackAngle * 0.5f) continue; // 부채꼴 밖

            int finalDamage = Mathf.RoundToInt(stat.GetStat(StatType.AttackPower).Value);

            var damageAble = hit.GetComponentInParent<IDamageAble>();
            if (damageAble != null)
            {
                if (hitTargets.Add(damageAble))
                {
                    damageAble.OnDamage(finalDamage, damageType);
                    SpawnHitEffect(hit, attackDir, origin);
                }
                continue;
            }
        }
    }

    private void SpawnHitEffect(Collider hit, Vector3 attackDir, Vector3 attackOrigin)
    {
        if (hitEffectPrefab == null) return;

        Vector3 spawnPosition = hit.ClosestPoint(attackOrigin);
        if ((spawnPosition - attackOrigin).sqrMagnitude <= PositionMatchEpsilonSqr)
            spawnPosition = hit.bounds.center;

        Quaternion rotation = Quaternion.LookRotation(-attackDir, Vector3.up);
        GameObject hitEffectInstance = Instantiate(hitEffectPrefab, spawnPosition, rotation);
        ApplyHitEffectColor(hitEffectInstance, currentHitEffectColor);
        combat.audioManager.PlaySFX(data.sfxData.GetSFXEvent(1), spawnPosition);
        Destroy(hitEffectInstance, CalculateEffectLifetime(hitEffectInstance));
    }

    private void ApplyHitEffectColor(GameObject effectInstance, Color color)
    {
        ParticleSystem[] particles = effectInstance.GetComponentsInChildren<ParticleSystem>(true);
        foreach (ParticleSystem particle in particles)
        {
            ParticleSystem.MainModule main = particle.main;
            main.startColor = color;
        }
    }

    private float CalculateEffectLifetime(GameObject effectInstance)
    {
        ParticleSystem[] particles = effectInstance.GetComponentsInChildren<ParticleSystem>(true);
        float maxLifetime = 0.5f;

        foreach (ParticleSystem particle in particles)
        {
            ParticleSystem.MainModule main = particle.main;
            maxLifetime = Mathf.Max(maxLifetime, main.duration + main.startLifetime.constantMax);
        }

        return maxLifetime + EffectDestroyBuffer;
    }

    private void SpawnSlashEffect(Vector3 origin, Vector3 attackDir)
    {
        Vector3 effectPos = origin + attackDir * 0.5f;
        effectPos.y += 0.2f;

        // 공격 방향(yaw)에 x축 -45도 기울기를 적용
        Quaternion baseRotation = Quaternion.LookRotation(attackDir, Vector3.up);
        Quaternion rot = baseRotation * Quaternion.Euler(-45f, 90f, 0f);

        slashEffect.transform.SetPositionAndRotation(effectPos, rot);
        slashEffect.SetActive(false); // 파티클 재생 리셋용
        slashEffect.SetActive(true);

        // 파티클이면 명시적으로 Play
       
        slashParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        slashParticle.Play();
    }

    public override void Skill(InputAction.CallbackContext context)
    {
        if (!context.started) return;
        if (isSkillActive) return; // 이미 스킬 활성화 중이면 재발동 불가

        Debug.Log("GreatSwordMan 스킬 발동: 불사의 힘");

        PlayHaptic(skillHaptic);

        var buff = new GreatSwordManBuff(
            skillBuffDuration,
            stat,
            combat,
            (active) => isSkillActive = active,
            () => GreatSwordManBuffEnded()
        );

        stat.playerBuffSystem.AddBuff(buff);

        buffEffect.gameObject.SetActive(true);

        slashMainModule.startColor = SkillEffectColor;
        sparkMainModule.startColor = SkillEffectColor;
        currentHitEffectColor = SkillEffectColor;

    }

    public void GreatSwordManBuffEnded()
    {
        combat.SetEndSkill();
        buffEffect.gameObject.SetActive(false);
        slashMainModule.startColor = DefaultEffectColor; 
        sparkMainModule.startColor = DefaultEffectColor;
        currentHitEffectColor = DefaultEffectColor;
    }

    public override void Exit()
    {
        if (slashEffect != null)
            Destroy(slashEffect);
        
        Destroy(buffEffect);

        // 아웃핏 변경 시 스킬 상태 및 공격속도 배율 초기화
        if (isSkillActive)
        {
            isSkillActive = false;
            combat?.SetAttackSpeedMultiplier(1f);
            stat.SetSkillInvincibility(false);
        }

        base.Exit();
    }
}
