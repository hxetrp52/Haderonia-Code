using UnityEngine;
using FMODUnity;
using FMOD.Studio;
using System.Collections.Generic;
using AudioKey;

public class AudioManager : ManagerBase
{
    // Loop 사운드 관리 딕셔너리 (키 + 오브젝트ID 조합)
    private Dictionary<(string, int), EventInstance> loopingInstances = new();

    private EventInstance currentBgm;
    private string currentBgmKey = "";
    private Bus masterBus;

    public override void Init()
    {
        masterBus = RuntimeManager.GetBus("bus:/");

        SceneLifecycleEvent.OnSceneExit += (SceneId sceneId) => StopBGM(); // SceneId를 받긴 하는데 안 씀

        PlayBGM(AudioKey.Key.BGM_Lobby_Haderonia);
    }

    public override void ManagerUpdate()
    {

    }

    #region BGM

    // immediate = flase 음향 페이드 효과 | true 음향 바로 전환
    public void PlayBGM(string path, bool immediate = false) 
    {
        // 경로가 비어있으면 리턴
        if (string.IsNullOrEmpty(path)) return;

        // 이미 재생 중인 곡이면 리턴
        if (currentBgmKey == path && IsPlaying(currentBgm)) return;

        StopBGM(immediate);

        try
        {
            currentBgm = RuntimeManager.CreateInstance(path);
            currentBgm.start();
            currentBgmKey = path;
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[AudioManager] BGM 재생 실패 (경로 확인 필요): {path} \n{e.Message}");
        }
    }

    public void StopBGM(bool immediate = false)
    {
        if (currentBgm.isValid())
        {
            currentBgm.stop(immediate ? FMOD.Studio.STOP_MODE.IMMEDIATE : FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            currentBgm.release();
            currentBgmKey = "";
        }
    }

    #endregion

    #region SFX - OneShot

    // 2D 효과음
    public void PlaySFX(string path)
    {
        if (string.IsNullOrEmpty(path)) return;

        RuntimeManager.PlayOneShot(path);
    }

    // 2D 효과음 (EventReference)
    public void PlaySFX(EventReference eventReference)
    {
        if (eventReference.IsNull) return;

        RuntimeManager.PlayOneShot(eventReference);
    }

    // 3D효과음
    public void PlaySFX(string path, Vector3 worldPos)
    {
        if (string.IsNullOrEmpty(path)) return;

        RuntimeManager.PlayOneShot(path, worldPos);
    }

    // 3D효과음 (EventReference)
    public void PlaySFX(EventReference eventReference, Vector3 worldPos)
    {
        if (eventReference.IsNull) return;

        RuntimeManager.PlayOneShot(eventReference, worldPos);
    }
    #endregion

    #region SFX - Loop

    // 그 게임오브젝트에 반복 효과음 넣을 시 호출
    public void PlayLoopSFX(string path, GameObject targetObj) 
    {
        if (targetObj == null || string.IsNullOrEmpty(path)) return;

        var uniqueKey = (path, targetObj.GetInstanceID());

        if (loopingInstances.ContainsKey(uniqueKey)) return;

        try
        {
            // 경로로 인스턴스 생성
            EventInstance instance = RuntimeManager.CreateInstance(path);
            RuntimeManager.AttachInstanceToGameObject(instance, targetObj);
            instance.start();

            loopingInstances.Add(uniqueKey, instance);
        }
        catch
        {
            Debug.LogWarning($"[AudioManager] Loop SFX 재생 실패: {path}");
        }
    }

    // 그 게임오브젝트에 반복 효과음 넣을 시 호출 (EventReference)
    public void PlayLoopSFX(EventReference eventReference, GameObject targetObj)
    {
        if (targetObj == null || eventReference.IsNull) return;

        string eventGuidKey = eventReference.Guid.ToString();
        var uniqueKey = (eventGuidKey, targetObj.GetInstanceID());

        if (loopingInstances.ContainsKey(uniqueKey)) return;

        try
        {
            EventInstance instance = RuntimeManager.CreateInstance(eventReference);
            RuntimeManager.AttachInstanceToGameObject(instance, targetObj);
            instance.start();

            loopingInstances.Add(uniqueKey, instance);
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[AudioManager] Loop SFX 재생 실패: {eventReference}\n{e.Message}");
        }
    }

    // 특정 루프 효과음 제거 시 호출
    public void StopLoopSFX(string path, GameObject targetObj, bool immediate = false) 
    {
        if (targetObj == null || string.IsNullOrEmpty(path)) return;

        var uniqueKey = (path, targetObj.GetInstanceID());

        if (loopingInstances.TryGetValue(uniqueKey, out var instance))
        {
            instance.stop(immediate ? FMOD.Studio.STOP_MODE.IMMEDIATE : FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            instance.release();
            loopingInstances.Remove(uniqueKey);
        }
    }

    // 특정 루프 효과음 제거 시 호출 (EventReference)
    public void StopLoopSFX(EventReference eventReference, GameObject targetObj, bool immediate = false)
    {
        if (targetObj == null || eventReference.IsNull) return;

        string eventGuidKey = eventReference.Guid.ToString();
        var uniqueKey = (eventGuidKey, targetObj.GetInstanceID());

        if (loopingInstances.TryGetValue(uniqueKey, out var instance))
        {
            instance.stop(immediate ? FMOD.Studio.STOP_MODE.IMMEDIATE : FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            instance.release();
            loopingInstances.Remove(uniqueKey);
        }
    }

    // 모든 루프 효과음 제거 시 호출(보통 오브젝트 파괴 시 호출되도록)
    public void StopAllLoopOnObject(GameObject targetObj)
    {
        if (targetObj == null) return;
        int targetID = targetObj.GetInstanceID();

        // 삭제할 키들을 담을 리스트
        List<(string, int)> keysToRemove = new List<(string, int)>();

        foreach (var kvp in loopingInstances)
        {
            if (kvp.Key.Item2 == targetID)
            {
                kvp.Value.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
                kvp.Value.release();
                keysToRemove.Add(kvp.Key);
            }
        }

        foreach (var key in keysToRemove)
        {
            loopingInstances.Remove(key);
        }
    }

    #endregion

    private bool IsPlaying(EventInstance instance) 
    {
        if (!instance.isValid()) return false;
        instance.getPlaybackState(out var state);
        return state != PLAYBACK_STATE.STOPPED;
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        SetApplicationAudioPaused(pauseStatus);
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        SetApplicationAudioPaused(!hasFocus);
    }

    private void SetApplicationAudioPaused(bool shouldPause)
    {
        if (!masterBus.isValid()) return;

        bool isPlayInBackground = SettingsManager.Instance?.CurrentSettings.audio.playInBackground ?? false;
        if (isPlayInBackground) return;

        masterBus.setPaused(shouldPause);
    }
}
