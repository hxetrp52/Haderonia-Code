using UnityEngine;

public class GamePadManager : ManagerBase
{
    public GamePadHaptic gamepadHaptic;
    public UIHapticWatcher UIHapticWatcher;
    public override void Init()
    {
        UIHapticWatcher.SetEventSystem();
    }

    public override void ManagerUpdate()
    {
        UIHapticWatcher.UIWatcherUpdate();
    }

}
