using EnumTypes;
using UnityEngine;

// 기존 Buff를 IStatHolder와 연동 가능한 범용 구조로 변경했습니다.
public abstract class Buff
{
    public string BuffId { get; } // 버프 이름
    public float Duration { get; } // 지속시간
    public BuffStackType StackType { get; } // 버프 중첩 타입 
    public BuffCategory Category { get; } // 버프/디버프 카테고리
    
    protected IStatHolder target; // 버프 대상
    protected TimeManager timeManager; // 시간 체크해줄 매니저(전역 사용)
    protected GameTimer durationTimer; // 버프 지속시간 타이머

    protected int stackCount = 1; // 스택 카운트 1부터 시작

    protected Buff(
        string buffId, // 버프 이름 
        float duration, // 버프 지속 시간
        BuffStackType stackType, // 버프 중첩 타입
        BuffCategory category) // 버프/디버프 카테고리
    {
        BuffId = buffId;
        Duration = duration;
        StackType = stackType;
        Category = category;
    }

    // onExpireCallback: called when this buff actually expires (after OnExpire)
    public void Apply(IStatHolder target, TimeManager timeManager, System.Action onExpireCallback = null)
    {
        this.target = target; 
        this.timeManager = timeManager;

        OnApply(); // 버프가 적용될 때 실행될 함수

        // Start timer and ensure the BuffSystem is notified when this buff expires
        durationTimer = timeManager.StartTimer(Duration, false, () => // 버프 시간 초기화
        {
            OnExpire(); // 버프가 끝날 때 실행될 함수
            onExpireCallback?.Invoke(); // 만료 콜백 호출
        });
    }

    public void AddStack() // 스택 중첩하기
    {
        stackCount++;
        OnStack();
    }

    public void Refresh(System.Action onExpireCallback = null) // 버프 갱신
    {
        if (durationTimer != null && timeManager != null)
            timeManager.StopTimer(durationTimer); // 이전 타이머 중지하고 새로 작성

        durationTimer = timeManager.StartTimer(Duration, false, () =>
        {
            OnExpire();
            onExpireCallback?.Invoke();
        });
    }

    public void Cancel() // 버프 강제 제거
    {
        if (durationTimer != null && timeManager != null)
        {
            timeManager.StopTimer(durationTimer);
            durationTimer = null;
        }
    }

    protected virtual void OnApply() { } // 버프가 적용될 때 실행될 함수
    protected virtual void OnStack() { } // 버프가 중첩될 때 실행될 함수
    protected virtual void OnExpire() { } // 버프가 만료될 때 실행될 함수
    public virtual void OnRemove() { } // 버프가 제거될 때 실행될 함수
}

// 예시 구현: 특정 Stat에 StatModifier를 추가/제거하는 StatBuff
public class StatBuff : Buff
{
    private readonly StatType statType;
    private readonly StatOperation operation;
    private readonly float value;

    public StatBuff(string buffId, float duration, BuffStackType stackType, BuffCategory category,
                    StatType statType, StatOperation operation, float value)
        : base(buffId, duration, stackType, category)
    {
        this.statType = statType;
        this.operation = operation;
        this.value = value;
    }

    protected override void OnApply()
    {
        var stat = target.GetStat(statType);
        if (stat == null) return;

        var modifier = new StatModifier(BuffId, operation, value);
        stat.AddModifier(modifier);
    }

    public override void OnRemove()
    {
        var stat = target.GetStat(statType);
        if (stat == null) return;

        stat.RemoveModifierById(BuffId);
    }
}
