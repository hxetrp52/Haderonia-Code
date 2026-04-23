using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class GamePadHaptic : MonoBehaviour
{
    private Coroutine hapticRoutine;
    private Gamepad activeGamepad;
    public bool canHaptic = true;
    private float settingMultiplier = 1f;

    public void SetSettingMultiplier(float multiplier)
    {
        settingMultiplier = Mathf.Max(0f, multiplier);
    }

    public void PlayHaptic(HapticProfile profile)
    {
        if (profile == null || !canHaptic)
            return;

        Gamepad targetGamepad = Gamepad.current;
        if (!IsGamepadUsable(targetGamepad))
            return;

        StopHaptic();
        activeGamepad = targetGamepad;
        hapticRoutine = StartCoroutine(HapticRoutine(profile, targetGamepad));
    }

    public void StopHaptic()
    {
        if (hapticRoutine != null)
        {
            StopCoroutine(hapticRoutine);
            hapticRoutine = null;
        }

        StopHapticOnGamepad(activeGamepad);

        if (Gamepad.current != null && Gamepad.current != activeGamepad)
            StopHapticOnGamepad(Gamepad.current);

        activeGamepad = null;
    }

    private IEnumerator HapticRoutine(HapticProfile profile, Gamepad targetGamepad)
    {
        float time = 0f;

        while (time < profile.duration)
        {
            if (!canHaptic || !IsGamepadUsable(targetGamepad))
            {
                StopHapticOnGamepad(targetGamepad);
                if (activeGamepad == targetGamepad)
                    activeGamepad = null;
                hapticRoutine = null;
                yield break;
            }

            float normalizedTime = time / profile.duration;

            float low = profile.lowFrequency.Evaluate(normalizedTime)
                        * profile.intensity
                        * settingMultiplier;
            float high = profile.highFrequency.Evaluate(normalizedTime)
                        * profile.intensity
                        * settingMultiplier;

            low = Mathf.Clamp01(low);
            high = Mathf.Clamp01(high);

            targetGamepad.SetMotorSpeeds(low, high);

            time += Time.deltaTime;
            yield return null;
        }

        StopHapticOnGamepad(targetGamepad);
        if (activeGamepad == targetGamepad)
            activeGamepad = null;
        hapticRoutine = null;
    }

    private static bool IsGamepadUsable(Gamepad gamepad)
    {
        return gamepad != null && gamepad.added && gamepad.enabled;
    }

    private static void StopHapticOnGamepad(Gamepad gamepad)
    {
        if (gamepad == null)
            return;

        gamepad.SetMotorSpeeds(0f, 0f);
        gamepad.ResetHaptics();
    }

    private void OnEnable()
    {
        InputSystem.onDeviceChange += OnDeviceChange;
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
            StopHaptic();
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus)
            StopHaptic();
    }

    private void OnDeviceChange(InputDevice device, InputDeviceChange change)
    {
        if (device != activeGamepad)
            return;

        switch (change)
        {
            case InputDeviceChange.Removed:
            case InputDeviceChange.Disconnected:
            case InputDeviceChange.Disabled:
            case InputDeviceChange.SoftReset:
            case InputDeviceChange.HardReset:
                StopHaptic();
                break;
        }
    }

    private void OnDisable()
    {
        InputSystem.onDeviceChange -= OnDeviceChange;
        StopHaptic();
    }
}
