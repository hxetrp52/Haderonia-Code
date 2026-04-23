using UnityEngine;

[CreateAssetMenu(fileName = "HapticProfile", menuName = "Input/Haptic Profile")]
public class HapticProfile : ScriptableObject
{
    [Header("Duration")]
    public float duration = 0.5f;

    [Header("Motor Curves (0 ~ 1)")]
    public AnimationCurve lowFrequency;   // 왼쪽 모터
    public AnimationCurve highFrequency;  // 오른쪽 모터

    [Header("Strength")]
    [Range(0f, 1f)] public float intensity = 1f;
}
