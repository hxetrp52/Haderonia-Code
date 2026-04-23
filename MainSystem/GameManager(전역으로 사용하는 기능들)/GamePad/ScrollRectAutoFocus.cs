using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// ScrollRect에 붙이면 Unity UI 네비게이션(키보드/게임패드)으로
/// 포커스가 이동했을 때 해당 항목이 보이도록 즉시 스크롤합니다.
/// 마우스 휠 스크롤은 기존 그대로 동작합니다.
/// 세팅 UI, ScrollRect를 쓰는 모든 UI에 공통으로 사용할 수 있습니다.
/// </summary>
[RequireComponent(typeof(ScrollRect))]
public class ScrollRectAutoFocus : MonoBehaviour
{
    [Tooltip("선택된 항목이 뷰포트 가장자리에서 얼마나 떨어져 있어야 스크롤을 시작할지 설정합니다. (0 = 뷰포트 끝에 닿을 때, 0.1 = 뷰포트의 10% 여백)")]
    [Range(0f, 0.5f)]
    [SerializeField] private float scrollPadding = 0.1f;

    private ScrollRect scrollRect;
    private RectTransform viewport;
    private RectTransform content;
    private GameObject previousSelected;

    private readonly Vector3[] viewportCorners = new Vector3[4];
    private readonly Vector3[] contentCorners = new Vector3[4];
    private readonly Vector3[] targetCorners = new Vector3[4];

    private void Awake()
    {
        scrollRect = GetComponent<ScrollRect>();
        viewport = scrollRect.viewport != null ? scrollRect.viewport : scrollRect.GetComponent<RectTransform>();
        content = scrollRect.content;
    }

    private void Update()
    {

        // Unity UI에는 선택 변경 이벤트가 없으므로 폴링으로 감지합니다.
        // 선택이 바뀌지 않으면 이른 반환으로 비용을 최소화합니다.
        GameObject selected = EventSystem.current.currentSelectedGameObject;
        if (selected == null || selected == previousSelected) return;

        previousSelected = selected;

        RectTransform selectedRect = selected.GetComponent<RectTransform>();
        if (selectedRect == null) return;

        if (!IsChildOfContent(selectedRect)) return;

        ScrollToShow(selectedRect);
    }

    private bool IsChildOfContent(RectTransform target)
    {
        Transform current = target.parent;
        while (current != null)
        {
            if (current == content) return true;
            current = current.parent;
        }
        return false;
    }

    private void ScrollToShow(RectTransform target)
    {
        // 레이아웃이 최신 상태인지 강제로 갱신합니다.
        // 세팅 그룹이 방금 활성화된 직후처럼 레이아웃이 아직 계산되지 않은 상태에서도
        // GetWorldCorners가 올바른 값을 반환하도록 합니다.
        Canvas.ForceUpdateCanvases();

        // 마우스 휠 등으로 남아 있는 관성 속도를 제거합니다.
        // ScrollRect는 m_Inertia=true일 때 같은 프레임의 LateUpdate에서 속도를 적용해
        // 직접 설정한 normalizedPosition 값을 덮어쓸 수 있습니다.
        scrollRect.StopMovement();

        // 월드 좌표 기준으로 뷰포트, 컨텐츠, 타겟의 경계를 구합니다.
        // GetWorldCorners 순서: [0] 좌하단, [1] 좌상단, [2] 우상단, [3] 우하단
        viewport.GetWorldCorners(viewportCorners);
        content.GetWorldCorners(contentCorners);
        target.GetWorldCorners(targetCorners);

        // 뷰포트 경계 (월드 공간)
        float viewportMinY = viewportCorners[0].y;
        float viewportMaxY = viewportCorners[1].y;
        float viewportMinX = viewportCorners[0].x;
        float viewportMaxX = viewportCorners[2].x;

        float viewportHeight = viewportMaxY - viewportMinY;
        float viewportWidth = viewportMaxX - viewportMinX;

        float paddingY = viewportHeight * scrollPadding;
        float paddingX = viewportWidth * scrollPadding;

        // 타겟 경계 (월드 공간)
        float targetMinY = targetCorners[0].y;
        float targetMaxY = targetCorners[1].y;
        float targetMinX = targetCorners[0].x;
        float targetMaxX = targetCorners[2].x;

        // 컨텐츠 전체 크기 (월드 공간 기준 — Canvas 스케일에 무관하게 동작)
        float contentHeight = contentCorners[1].y - contentCorners[0].y;
        float contentWidth = contentCorners[2].x - contentCorners[0].x;

        // 수직 스크롤 처리
        // verticalNormalizedPosition: 0 = 컨텐츠 하단, 1 = 컨텐츠 상단
        // 타겟이 뷰포트 하단 아래에 있으면 → 아래로 스크롤(position 감소) → 컨텐츠가 위로 이동 → 타겟이 위로 올라옴
        // 타겟이 뷰포트 상단 위에 있으면 → 위로 스크롤(position 증가) → 컨텐츠가 아래로 이동 → 타겟이 아래로 내려옴
        if (scrollRect.vertical)
        {
            float scrollableHeight = contentHeight - viewportHeight;
            if (scrollableHeight > 0f)
            {
                if (targetMinY < viewportMinY + paddingY)
                {
                    float delta = (viewportMinY + paddingY) - targetMinY;
                    float normalizedDelta = delta / scrollableHeight;
                    scrollRect.verticalNormalizedPosition = Mathf.Clamp01(scrollRect.verticalNormalizedPosition - normalizedDelta);
                }
                else if (targetMaxY > viewportMaxY - paddingY)
                {
                    float delta = targetMaxY - (viewportMaxY - paddingY);
                    float normalizedDelta = delta / scrollableHeight;
                    scrollRect.verticalNormalizedPosition = Mathf.Clamp01(scrollRect.verticalNormalizedPosition + normalizedDelta);
                }
            }
        }

        // 수평 스크롤 처리
        // horizontalNormalizedPosition: 0 = 컨텐츠 좌측, 1 = 컨텐츠 우측
        if (scrollRect.horizontal)
        {
            float scrollableWidth = contentWidth - viewportWidth;
            if (scrollableWidth > 0f)
            {
                if (targetMinX < viewportMinX + paddingX)
                {
                    float delta = (viewportMinX + paddingX) - targetMinX;
                    float normalizedDelta = delta / scrollableWidth;
                    scrollRect.horizontalNormalizedPosition = Mathf.Clamp01(scrollRect.horizontalNormalizedPosition - normalizedDelta);
                }
                else if (targetMaxX > viewportMaxX - paddingX)
                {
                    float delta = targetMaxX - (viewportMaxX - paddingX);
                    float normalizedDelta = delta / scrollableWidth;
                    scrollRect.horizontalNormalizedPosition = Mathf.Clamp01(scrollRect.horizontalNormalizedPosition + normalizedDelta);
                }
            }
        }
    }
}
