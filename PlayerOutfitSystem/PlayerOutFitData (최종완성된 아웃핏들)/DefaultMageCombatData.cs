using System.Collections.Generic;
using EnumTypes;
using UnityEngine;
using UnityEngine.InputSystem;

public class DefaultMageCombatData : OutfitCombatBase
{
    private const float DIRECTION_EPSILON = 0.0001f;

    private const string SHADOW_KEY = "Combat_DefaultMage_Shadow";
    private const string TeleportEffect_KEY = "Combat_DefaultMage_TeleportEffect";
    private const string MagicBullet_KEY = "Combat_DefaultMage_MagicBullet";

    private GameObject shadowObject;
    private GameObject teleportEffect;
    private GameObject magicBulletPoolPrefab;
    private Transform magicBulletPoolRoot;

    private const float MAX_DURATION = 8f;
    private bool isShadowActive;
    private GameTimer shadowDurationTimer;

    [Header("DefaultMage Attack")]
    [SerializeField] private int bulletPoolCount = 8;
    [SerializeField] private int bulletMagicDamage = 10;
    [SerializeField] private float bulletLifetime = 2f;
    [SerializeField] private float bulletSpeed = 8f;
    [SerializeField] private float bulletSpawnOffset = 0.2f;
    [SerializeField] private float bulletSpawnHeight = 0f;

    public override void Enter(PlayerCombat combat, PlayerStat stat, CharacterOutfitData data)
    {
        base.Enter(combat, stat, data);

        LoadByKey<GameObject>(SHADOW_KEY, prefab =>
        {
            shadowObject = Instantiate(prefab, transform);
            shadowObject.transform.SetParent(null);
            shadowObject.SetActive(false);
        });
        LoadByKey<GameObject>(TeleportEffect_KEY, prefab =>
        {
            teleportEffect = Instantiate(prefab, transform);
            teleportEffect.transform.position = transform.position;
        });
        LoadByKey<GameObject>(MagicBullet_KEY, prefab =>
        {
            magicBulletPoolRoot = new GameObject("DefaultMageMagicBulletPool").transform;
            magicBulletPoolRoot.SetParent(null);

            magicBulletPoolPrefab = Instantiate(prefab, magicBulletPoolRoot);
            magicBulletPoolPrefab.SetActive(false);

            if (magicBulletPoolPrefab.GetComponent<DefaultMageMagicBullet>() == null)
                magicBulletPoolPrefab.AddComponent<DefaultMageMagicBullet>();

            playerPool.CreatePool(magicBulletPoolPrefab, magicBulletPoolRoot, bulletPoolCount);
        });
    }

  

    public override void Attack(InputAction.CallbackContext context)
    {
        if (!context.started) return;

        Vector3 attackDir = combat.GetAttackDirection();
        attackDir.y = 0f;
        if (attackDir.sqrMagnitude <= DIRECTION_EPSILON)
            attackDir = Vector3.forward;
        attackDir.Normalize();

        animator.PlayAttackAnimation(attackDir);

        if (magicBulletPoolPrefab == null)
            return;

        Vector3 spawnPos = transform.position + attackDir * bulletSpawnOffset + Vector3.up * bulletSpawnHeight;

        PoolableObject spawned = playerPool.Spawn(magicBulletPoolPrefab, spawnPos, Quaternion.LookRotation(attackDir, Vector3.zero));
        DefaultMageMagicBullet bullet = spawned as DefaultMageMagicBullet;
        if (bullet == null)
        {
            Debug.LogWarning("[DefaultMageCombatData] Spawned bullet is not DefaultMageMagicBullet.");
            return;
        }

        combat.audioManager.PlaySFX(data.sfxData.GetSFXEvent(0), transform.position);
        PlayHaptic(attackHaptic);

        bullet.Fire(attackDir, bulletSpeed, bulletLifetime, bulletMagicDamage, combat.targetMask);
    }

    public override void Skill(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            ActivateShadow();
        }
        else if (context.canceled)
        {
            combat.audioManager.PlaySFX(data.sfxData.GetSFXEvent(1), transform.position);
            ReturnToShadow();
        }
    }

    #region 스킬 구현
    private void ActivateShadow()
    {
        if (isShadowActive || shadowObject == null) return;

        shadowObject.transform.position = transform.position;
        shadowObject.SetActive(true);

        isShadowActive = true;

        shadowDurationTimer = timeManager.StartTimer(MAX_DURATION, false, ReturnToShadow);
    }
    private void ReturnToShadow()
    {
        if (!isShadowActive) return;

        transform.position = shadowObject.transform.position;
        shadowObject.SetActive(false);
        teleportEffect.GetComponent<ParticleSystem>().Play();

        combat.SetEndSkill();
        isShadowActive = false;
    }
    #endregion

    public override void Exit()
    {
        if (magicBulletPoolPrefab != null)
        {
            playerPool.DespawnAll(magicBulletPoolPrefab);
            playerPool.ClearPool(magicBulletPoolPrefab);
            Destroy(magicBulletPoolPrefab);
            magicBulletPoolPrefab = null;
        }

        if (magicBulletPoolRoot != null)
        {
            Destroy(magicBulletPoolRoot.gameObject);
            magicBulletPoolRoot = null;
        }

        if (shadowObject != null)
            Destroy(shadowObject);

        if (teleportEffect != null)
            Destroy(teleportEffect);

        base.Exit();
    }
}

public class DefaultMageMagicBullet : PoolableObject
{
    private const float DIRECTION_EPSILON = 0.0001f;
    private const int HIT_BUFFER_SIZE = 16;
    private static readonly Collider[] HitBuffer = new Collider[HIT_BUFFER_SIZE];

    private readonly HashSet<IDamageAble> hitTargets = new();
    private Vector3 moveDirection;
    private LayerMask targetMask;
    private float moveSpeed;
    private float lifeTime;
    private float elapsedTime;
    private int damage;
    private float hitRadius = 0.2f;

    private void Awake()
    {
        var sphere = GetComponent<SphereCollider>();
        if (sphere != null)
            hitRadius = Mathf.Max(0.05f, sphere.radius * Mathf.Max(transform.lossyScale.x, transform.lossyScale.y, transform.lossyScale.z));
    }

    public void Fire(Vector3 direction, float speed, float duration, int damage, LayerMask targetMask)
    {
        moveDirection = direction.sqrMagnitude <= DIRECTION_EPSILON ? Vector3.forward : direction.normalized;
        moveSpeed = speed;
        lifeTime = duration;
        this.damage = damage;
        this.targetMask = targetMask;
        elapsedTime = 0f;
        hitTargets.Clear();
        transform.rotation = Quaternion.LookRotation(moveDirection, Vector3.up);
    }

    public override void OnSpawn()
    {
        elapsedTime = 0f;
        hitTargets.Clear();
    }

    private void Update()
    {
        transform.position += moveDirection * (moveSpeed * Time.deltaTime);
        elapsedTime += Time.deltaTime;

        int hitCount = Physics.OverlapSphereNonAlloc(transform.position, hitRadius, HitBuffer, targetMask);
        for (int i = 0; i < hitCount; i++)
        {
            Collider hit = HitBuffer[i];
            if (hit == null) continue;

            var damageAble = hit.GetComponentInParent<IDamageAble>();
            if (damageAble == null || !hitTargets.Add(damageAble))
                continue;

            damageAble.OnDamage(damage, DamageType.Magic);
            ReturnToPool();
            return;
        }

        if (elapsedTime >= lifeTime)
            ReturnToPool();
    }

    public override void OnDespawn()
    {
        elapsedTime = 0f;
        hitTargets.Clear();
    }
}
