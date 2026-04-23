using System.Collections.Generic;
using UnityEngine;

public class TimeManager : ManagerBase
{
    public float DefaultTime { get; private set; } // 기본
    public float DeltaTime { get; private set; } // 델타 타임
    public float UnScaleDeltaTime { get; private set; } // 언스케일 델타 타임

    private readonly List<GameTimer> timers = new(); // 진행중인 타이머 리스트

    public override void Init()
    {
        
    }

    public override void ManagerUpdate() // 타이머들 업데이트
    {
        DefaultTime = Time.time;
        DeltaTime = Time.deltaTime;
        UnScaleDeltaTime = Time.unscaledDeltaTime;

        var finished = new List<GameTimer>();

        for (int i = timers.Count - 1; i >= 0; i--)
        {
            var t = timers[i];
            float dt = t.IsUnscaled ? UnScaleDeltaTime : DeltaTime; // bool로 델타타임 지정

            t.TimerUpdate(dt);

            if (t.IsFinished)
                finished.Add(t);
        }

        foreach (var t in finished)
        {
            StopTimer(t);
        }
    }

    public GameTimer StartTimer(float duration, bool unscaled, System.Action onComplete)
    {
        var timer = new GameTimer(duration, unscaled, onComplete);
        timers.Add(timer);
        return timer;
    }

    public void StopTimer(GameTimer timer)
    {
        if (timer == null) return;
        timer.Cancel();
        timers.Remove(timer);
    }
}
