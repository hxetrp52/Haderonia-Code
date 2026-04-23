using UnityEngine;
using FMODUnity;

[CreateAssetMenu(fileName = "SFXContainer", menuName = "SFXContainer/SFXContainer")]
public class SFXContainerData : ScriptableObject
{
    public EventReference[] sfxEvents;

    public EventReference GetSFXEvent(int index)
    {
        if (index < 0 || sfxEvents == null || index >= sfxEvents.Length)
        {
            Debug.LogWarning($"[SFXContainerData] invalid index: {index}");
            return default;
        }
        return sfxEvents[index];
    }
}