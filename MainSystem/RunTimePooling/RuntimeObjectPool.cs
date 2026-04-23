using System.Collections.Generic;
using UnityEngine;

public class RuntimeObjectPool : MonoBehaviour
{
    class Pool
    {
        public GameObject prefab;
        public Transform root;
        public Queue<PoolableObject> inactiveQueue = new();
        public HashSet<PoolableObject> activeSet = new();
    }

    private Dictionary<GameObject, Pool> pools = new();
    private Dictionary<PoolableObject, Pool> reverseMap = new();

    // 풀 생성 (생성할 프리팹, 부모 트랜스폼, 초기 개수)
    public void CreatePool(GameObject prefab, Transform root, int initialCount)
    {
        if (pools.ContainsKey(prefab))
            return;

        Pool pool = new Pool
        {
            prefab = prefab,
            root = root
        };

        for (int i = 0; i < initialCount; i++)
        {
            PoolableObject obj = CreateNew(pool);
            obj.gameObject.SetActive(false);
            pool.inactiveQueue.Enqueue(obj);
        }

        pools.Add(prefab, pool);
    }

    // 풀에서 객체 생성
    public PoolableObject Spawn(GameObject prefab, Vector3 pos, Quaternion rot)
    {
        if (!pools.TryGetValue(prefab, out Pool pool))
        {
            Debug.LogError($"[RuntimeObjectPool] Pool not found: {prefab.name}");
            return null;
        }

        PoolableObject obj = pool.inactiveQueue.Count > 0
            ? pool.inactiveQueue.Dequeue()
            : CreateNew(pool);

        obj.transform.SetPositionAndRotation(pos, rot);
        obj.gameObject.SetActive(true);

        pool.activeSet.Add(obj);
        obj.OnSpawn();

        return obj;
    }

    // 풀에 객체 반환
    public void Despawn(PoolableObject obj)
    {
        if (!reverseMap.TryGetValue(obj, out Pool pool))
        {
            Destroy(obj.gameObject);
            return;
        }

        if (!pool.activeSet.Remove(obj))
            return;

        obj.OnDespawn();
        obj.gameObject.SetActive(false);
        obj.transform.SetParent(pool.root);

        pool.inactiveQueue.Enqueue(obj);
    }

    // 특정 풀 전체 반환
    public void DespawnAll(GameObject prefab)
    {
        if (!pools.TryGetValue(prefab, out Pool pool))
            return;

        // activeSet은 순회 중 수정하면 안 되므로 복사
        var activeList = new List<PoolableObject>(pool.activeSet);

        foreach (var obj in activeList)
        {
            Despawn(obj);
        }
    }

    // 모든 풀 전체 반환
    public void DespawnAll()
    {
        foreach (var pool in pools.Values)
        {
            var activeList = new List<PoolableObject>(pool.activeSet);
            foreach (var obj in activeList)
            {
                Despawn(obj);
            }
        }
    }

    //특정 풀 제거
    public void ClearPool(GameObject prefab)
    {
        if (!pools.TryGetValue(prefab, out Pool pool))
            return;

        // 활성 객체 제거
        foreach (var obj in pool.activeSet)
        {
            Destroy(obj.gameObject);
            reverseMap.Remove(obj);
        }

        // 비활성 객체 제거
        foreach (var obj in pool.inactiveQueue)
        {
            Destroy(obj.gameObject);
            reverseMap.Remove(obj);
        }

        pool.activeSet.Clear();
        pool.inactiveQueue.Clear();

        pools.Remove(prefab);
    }

    // 모든 풀 제거
    public void ClearAllPools()
    {
        var prefabList = new List<GameObject>(pools.Keys);

        foreach (var prefab in prefabList)
        {
            ClearPool(prefab);
        }

        pools.Clear();
        reverseMap.Clear();
    }

    // 새로운 풀 객체 생성
    PoolableObject CreateNew(Pool pool)
    {
        GameObject go = Instantiate(pool.prefab, pool.root);
        PoolableObject poolObj = go.GetComponent<PoolableObject>();

        if (poolObj == null)
        {
            Debug.LogError($"{pool.prefab.name} → PoolableObject 상속 필요");
            Destroy(go);
            return null;
        }

        poolObj.SetOwnerPool(this);
        reverseMap.Add(poolObj, pool);

        return poolObj;
    }
}
