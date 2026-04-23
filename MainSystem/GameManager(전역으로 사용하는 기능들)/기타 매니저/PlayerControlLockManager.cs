using System;
using System.Collections.Generic;
using UnityEngine;

[Flags]
public enum PlayerControlLock
{
    None = 0,
    Move = 1 << 0,
    Interact = 1 << 1,
    Attack = 1 << 2,
    Skill = 1 << 3,
    Item = 1 << 4,   
    All = ~0
}

public enum PlayerLockReason // lock 거는 원인
{
    UI, // Move, Interact 막음
    Cutscene,
    Stun,
    Tutorial,
    GameFail
}

public class PlayerControlLockManager: ManagerBase
{
    private readonly Dictionary<PlayerLockReason, PlayerControlLock> activeLocks = new();

    public event Action<PlayerControlLock> OnLockChanged;

    public PlayerControlLock CurrentLockState { get; private set; } = PlayerControlLock.None;

    public override void Init()
    {
        activeLocks.Clear();
        CurrentLockState = PlayerControlLock.None;
    }

    public override void ManagerUpdate()
    {
        
    }

    public void AddLock(PlayerLockReason reason, PlayerControlLock lockType) // 특정 reason으로 lock 추가 또는 갱신
    {
        activeLocks[reason] = lockType;   // 같은 reason 재등록 시 갱신
        Recalculate();
    }

    public void RemoveLock(PlayerLockReason reason) // 특정 reason의 lock 해제
    {
        if (activeLocks.Remove(reason))
            Recalculate();
    }

    public bool IsLocked(PlayerControlLock lockType) // 특정 lock이 활성화되어 있는지 확인
    {
        return (CurrentLockState & lockType) != 0;
    }

    private void Recalculate() // 모든 active lock을 합산하여 최종 lock 상태 계산
    {
        PlayerControlLock merged = PlayerControlLock.None;
        foreach (var pair in activeLocks)
            merged |= pair.Value;

        if (merged == CurrentLockState) return;

        CurrentLockState = merged;
        OnLockChanged?.Invoke(CurrentLockState);
    }
}