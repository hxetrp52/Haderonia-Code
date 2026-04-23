using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.InputSystem;
using UnityEngine.ResourceManagement.AsyncOperations;
using AudioKey;

public abstract class OutfitCombatBase : MonoBehaviour, IOutFitComponent
{
    protected PlayerCombat combat;
    protected PlayerStat stat;
    protected PlayerAnimator animator;
    protected CharacterOutfitData data;
    protected bool isAlive; // 어드레서블이 살아있는지 확인
    private readonly List<AsyncOperationHandle> handles = new(); // 불러온 어드레서블 리스트
    protected PlayerAttackDirector attackDirector;
    protected RuntimeObjectPool playerPool;

    protected TimeManager timeManager;
    protected GamePadManager gamePadManager;

    protected HapticProfile attackHaptic;
    protected HapticProfile skillHaptic;

    protected string combatUIFrameKey = "CombatFrame/"; 
    protected GameObject combatUIFrame;

    protected void LoadByKey<T>( // stiring으로 키 받아서 오브젝트 로드
    string key,
    System.Action<T> onLoaded) where T : Object
    {
        var handle = Addressables.LoadAssetAsync<T>(key);
        handles.Add(handle);

        handle.Completed += h =>
        {
            if (!isAlive) return;

            if (h.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.LogError($"[Combat] Addressable Load Failed (Key): {key}");
                return;
            }

            onLoaded?.Invoke(h.Result);
        };
    }

    public virtual void Enter(PlayerCombat combat, PlayerStat stat, CharacterOutfitData data)
    {
        this.combat = combat;
        animator = combat.playerAnimator;
        this.data = data;
        this.stat = stat;
        attackHaptic = data.attackHaptic;
        skillHaptic = data.skillHaptic;
        isAlive = true;
        timeManager = combat.timeManager;
        gamePadManager = combat.gamePadManager;
        playerPool = combat.playerPool;
        
        combat.combatUI.ChangeSkillIcon(data.skillSprite);

        LoadByKey<GameObject>(combatUIFrameKey + data.codeName, go =>
        {
            combatUIFrame = Instantiate(go);
            combat.combatUI.SetCombatFrame(combatUIFrame);
        });
    }

    public abstract void Attack(InputAction.CallbackContext context);
    public abstract void Skill(InputAction.CallbackContext context);

    protected virtual void PlayHaptic(HapticProfile haptic) => gamePadManager.gamepadHaptic.PlayHaptic(haptic); 
    
    public virtual void Exit()
    {
        isAlive = false;

        Destroy(combatUIFrame);

        foreach (var h in handles)
        {
            if (h.IsValid())
                Addressables.Release(h);
        }

        handles.Clear();
    }
}
