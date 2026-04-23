using EnumTypes;
using System.Collections.Generic;

public class BuffSystem
{
    private readonly Dictionary<string, Buff> activeBuffs = new(); // 적용중인 버프들
    private readonly IStatHolder target; // 버프 대상
    private readonly TimeManager timeManager; // 타이머 진행시킬 타임 매니저

    public BuffSystem(IStatHolder target, TimeManager timeManager) // 버프 매니저 
    {
        this.target = target;
        this.timeManager = timeManager;
    }

    public void AddBuff(Buff buff)// 버프 추가
    {
        if (activeBuffs.TryGetValue(buff.BuffId, out var existing)) // 이미 있는 버프인가?
        {
            switch (existing.StackType) // 갱신, 스택, 갱신+스택인지 따라 처리
            {
                case BuffStackType.Stack:
                    existing.AddStack();
                    break;

                case BuffStackType.StackRefresh:
                    existing.AddStack();
                    existing.Refresh(() => RemoveBuff(existing.BuffId));
                    break;

                case BuffStackType.Refresh:
                    existing.Refresh(() => RemoveBuff(existing.BuffId));
                    break;
            }
        }
        else
        {
            activeBuffs.Add(buff.BuffId, buff);
            buff.Apply(target, timeManager, () => RemoveBuff(buff.BuffId));
        }
    }

    public void RemoveBuff(string buffId) // 특정 버프 제거
    {
        if (!activeBuffs.TryGetValue(buffId, out var buff))
            return;

        buff.Cancel();
        buff.OnRemove();
        activeBuffs.Remove(buffId);
    }

    public IEnumerable<Buff> GetActiveBuffs() => activeBuffs.Values; // 적용중인 버프들 확인

    public void RemoveAllDebuffs() // 모든 디버프 제거
    {
        RemoveBuffByCategory(BuffCategory.Debuff); 
    }

    public void RemoveAllBuffs() // 모든 버프 제거 
    {
        RemoveBuffByCategory(BuffCategory.Buff);
    }

    public void RemoveBuffByPredicate(System.Func<Buff, bool> predicate) // 조건에 맞는 버프 제거
    {
        var removeList = new List<string>();

        foreach (var pair in activeBuffs)
        {
            if (predicate(pair.Value))
                removeList.Add(pair.Key);
        }

        foreach (var id in removeList)
            RemoveBuff(id);
    }

    private void RemoveBuffByCategory(BuffCategory category) // 버프/디버프 제거
    {
        var removeList = new List<string>();

        foreach (var pair in activeBuffs)
        {
            if (pair.Value.Category == category)
                removeList.Add(pair.Key);
        }

        foreach (var id in removeList)
            RemoveBuff(id);
    }
}