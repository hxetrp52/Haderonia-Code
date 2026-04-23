using UnityEngine;

public abstract class PoolableObject : MonoBehaviour, IPoolObject
{
    private RuntimeObjectPool ownerPool;

    public void SetOwnerPool(RuntimeObjectPool pool)
    {
        ownerPool = pool;
    }

    public virtual void OnSpawn() { }
    public virtual void OnDespawn() { }

    public void ReturnToPool()
    {
        ownerPool?.Despawn(this);
    }
}
public interface IPoolObject
{
    void OnSpawn();
    void OnDespawn();
    void ReturnToPool();
}