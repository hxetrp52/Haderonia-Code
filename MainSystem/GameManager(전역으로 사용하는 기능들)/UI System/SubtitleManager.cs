using Febucci.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SubtitleManager : ManagerBase
{
    public GameObject subtitleCanvas;
    public bool isSubtitleActive = true;

    [SerializeField] private GameObject subtitleObject;
    [SerializeField] private TMP_Text subtitleText;
    [SerializeField] private TypewriterByCharacter typewriter;
    [SerializeField] private float defaultHoldTime = 1.5f;

    private struct SubtitleData
    {
        public string text;
        public float holdTime;
        public float preDelay;
        public Action onBeforeStarted; // 이 대사 출력 전 (포커싱/카메라 이동 등)
        public Action onStarted; // 이 대사 시작 시
        public Action onEnded;   // 이 대사 종료 시

        public SubtitleData(string text, float holdTime, float preDelay, Action onBeforeStarted, Action onStarted, Action onEnded)
        {
            this.text = text;
            this.holdTime = holdTime;
            this.preDelay = preDelay;
            this.onBeforeStarted = onBeforeStarted;
            this.onStarted = onStarted;
            this.onEnded = onEnded;
        }
    }

    private readonly Queue<SubtitleData> subtitleQueue = new Queue<SubtitleData>();
    private SubtitleData? currentSubtitleData = null;

    private Action onAllSubtitlesFinished; // 모든 자막이 끝났을 때 호출되는 콜백

    private bool isPlayingSubtitle = false;
    private Coroutine holdCoroutine;
    private Coroutine preDelayCoroutine;

    private void OnEnable()
    {
        typewriter.onTextShowed.AddListener(OnTextShowed);
        typewriter.onTextDisappeared.AddListener(OnTextDisappeared);
    }

    private void OnDisable()
    {
        typewriter.onTextShowed.RemoveListener(OnTextShowed);
        typewriter.onTextDisappeared.RemoveListener(OnTextDisappeared);

        if (holdCoroutine != null)
        {
            StopCoroutine(holdCoroutine);
            holdCoroutine = null;
        }
        if (preDelayCoroutine != null)
        {
            StopCoroutine(preDelayCoroutine);
            preDelayCoroutine = null;
        }
        onAllSubtitlesFinished = null;
    }

    public override void Init() { }
    public override void ManagerUpdate() { }

    // 자막별 개별 콜백 지정
    public void ShowSubtitle(string text, float holdTime, Action onStarted = null, Action onEnded = null)
    {
        ShowSubtitle(text, holdTime, 0f, null, onStarted, onEnded);
    }

    public void ShowSubtitle(string text)
    {
        ShowSubtitle(text, defaultHoldTime, null, null);
    }
                
    // 자막 출력 전 지연/콜백 포함
    public void ShowSubtitle(
        string text,
        float holdTime,
        float preDelay,
        Action onBeforeStarted,
        Action onStarted = null,
        Action onEnded = null)
    {
        if (!isSubtitleActive) return;

        if (holdTime < 0f) holdTime = 0f;
        if (preDelay < 0f) preDelay = 0f;
        subtitleQueue.Enqueue(new SubtitleData(text, holdTime, preDelay, onBeforeStarted, onStarted, onEnded));

        if (!isPlayingSubtitle)
            PlayNextSubtitle();
    }

    

    private void PlayNextSubtitle()
    {
        if (subtitleQueue.Count == 0)
        {
            isPlayingSubtitle = false;
            currentSubtitleData = null;
            if (subtitleObject != null) subtitleObject.SetActive(false);

            onAllSubtitlesFinished?.Invoke();
            onAllSubtitlesFinished = null; // 1회성
            return;
        }

        isPlayingSubtitle = true;
        if (subtitleObject != null) subtitleObject.SetActive(true);

        var next = subtitleQueue.Dequeue();
        currentSubtitleData = next;

        if (preDelayCoroutine != null)
        {
            StopCoroutine(preDelayCoroutine);
            preDelayCoroutine = null;
        }
        preDelayCoroutine = StartCoroutine(PlayWithPreDelay(next));
    }

    private IEnumerator PlayWithPreDelay(SubtitleData data)
    {
        data.onBeforeStarted?.Invoke();

        if (data.preDelay > 0f)
        {
            subtitleText.text = string.Empty;
            yield return new WaitForSeconds(data.preDelay);
        }

        // 개별 시작 콜백
        data.onStarted?.Invoke();
        typewriter.ShowText(data.text);
        preDelayCoroutine = null;
    }

    // 타이핑 완료 -> holdTime 후 사라짐 시작
    private void OnTextShowed()
    {
        if (!currentSubtitleData.HasValue) return;

        if (holdCoroutine != null)
            StopCoroutine(holdCoroutine);

        holdCoroutine = StartCoroutine(HoldThenDisappear(currentSubtitleData.Value.holdTime));
    }

    private IEnumerator HoldThenDisappear(float time)
    {
        yield return new WaitForSeconds(time);
        typewriter.StartDisappearingText();
        holdCoroutine = null;
    }

    // 사라짐 완료 -> 개별 종료 콜백 -> 다음 자막
    private void OnTextDisappeared()
    {
        if (currentSubtitleData.HasValue)
        {
            currentSubtitleData.Value.onEnded?.Invoke();
        }

        PlayNextSubtitle();
    }

    #region 자막 전체 종료 콜백 추가 / 제거
    /// <summary>
    /// "현재 큐까지 전부 끝났을 때" 1회 실행될 콜백 등록
    /// </summary>
    public void SetAllFinishedCallback(Action callback)
    {
        onAllSubtitlesFinished = callback;
    }

    /// <summary>
    /// 등록된 전체 종료 콜백 제거
    /// </summary>
    public void ClearAllFinishedCallback()
    {
        onAllSubtitlesFinished = null;
    }
    #endregion 
}
