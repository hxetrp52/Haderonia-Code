using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class AddressableLoadManager : ManagerBase
{

    private readonly Dictionary<string, GameObject> prefabCache = new();
    private readonly Dictionary<string, AsyncOperationHandle<GameObject>> loadingHandles = new();

    public override void Init()
    {
        
    }

    public override void ManagerUpdate()
    {
        
    }

    public void LoadPrefab(string key, Action<GameObject> onComplete)
    {
        // 이미 캐시에 있음
        if (prefabCache.TryGetValue(key, out var cachedPrefab))
        {
            onComplete?.Invoke(cachedPrefab);
            return;
        }

        // 이미 로딩 중
        if (loadingHandles.TryGetValue(key, out var loadingHandle))
        {
            loadingHandle.Completed += handle =>
            {
                onComplete?.Invoke(handle.Result);
            };
            return;
        }

        var handle = Addressables.LoadAssetAsync<GameObject>(key);
        loadingHandles.Add(key, handle);

        handle.Completed += op =>
        {
            loadingHandles.Remove(key);

            if (op.Status == AsyncOperationStatus.Succeeded)
            {
                prefabCache[key] = op.Result;
                onComplete?.Invoke(op.Result);
            }
            else
            {
                Debug.LogError($"[AddressableManager] Load Failed : {key}");
                onComplete?.Invoke(null);
            }
        };
    }

    // 프리팹 인스턴스 생성
    public void InstantiatePrefab(
        string key,
        Transform parent,
        Action<GameObject> onComplete)
    {
        LoadPrefab(key, prefab =>
        {
            if (prefab == null)
            {
                onComplete?.Invoke(null);
                return;
            }

            var instance = Instantiate(prefab, parent);
            onComplete?.Invoke(instance);
        });
    }

    // 캐시된 프리팹 해제
    public void ReleasePrefab(string key)
    {
        if (!prefabCache.TryGetValue(key, out var prefab))
            return;

        Addressables.Release(prefab);
        prefabCache.Remove(key);
    }

    // 전체 해제
    public void ClearAll()
    {
        foreach (var prefab in prefabCache.Values)
        {
            Addressables.Release(prefab);
        }

        prefabCache.Clear();
        loadingHandles.Clear();
    }
}
