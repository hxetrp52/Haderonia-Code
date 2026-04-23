using UnityEngine;
public class GameTimer
{
    public float Duration { get; private set; }
    public float Elapsed { get; private set; }
    public bool IsUnscaled { get; private set; }

    public bool IsFinished => Elapsed >= Duration;

    private System.Action onComplete;
    private bool isCanceled = false;

    public GameTimer(float duration, bool unscaled, System.Action onComplete)
    {
        Duration = duration;
        IsUnscaled = unscaled;
        this.onComplete = onComplete;
    }

    public void TimerUpdate(float dt)
    {
        if (isCanceled) return;
        Elapsed += dt;

        if (IsFinished)
            onComplete?.Invoke();
    }

    public void Cancel()
    {
        isCanceled = true;
    }

    /// <summary> 남은 시간 (초) </summary>
    public float GetRemainingTime()
    {
        return Mathf.Max(0f, Duration - Elapsed);
    }

    /// <summary> 0~1 진행도 </summary>
    public float GetNormalized()
    {
        return Mathf.Clamp01(Elapsed / Duration);
    }

    /// <summary> 분:초 형태 문자열 (00:00) </summary>
    public string GetRemainingTimeMMSS()
    {
        int totalSec = Mathf.CeilToInt(GetRemainingTime());
        int min = totalSec / 60;
        int sec = totalSec % 60;
        return $"{min:00}:{sec:00}";
    }

    /// <summary> 초 문자열 (0.0 같은 형식 가능) </summary>
    public string GetRemainingTimeText(string format = "0.0")
    {
        return GetRemainingTime().ToString(format);
    }
}
