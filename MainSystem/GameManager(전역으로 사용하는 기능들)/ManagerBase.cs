using UnityEngine;

public abstract class ManagerBase : MonoBehaviour
{
    public abstract void Init(); // Awake 대체
    public abstract void ManagerUpdate(); // Update 대체
}
