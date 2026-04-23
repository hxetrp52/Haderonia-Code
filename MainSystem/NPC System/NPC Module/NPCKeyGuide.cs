using UnityEngine;
using DG.Tweening;

public class NPCKeyGuide : NPCModuleBase
{
    [SerializeField] private GameObject iconDisplay;
    private SpriteRenderer keySpriteRender;
    private Tween currentTween;

    [SerializeField] private float floatDistance = 0.5f;
    [SerializeField] private float duration = 0.25f;

    private Vector3 basePosition;
    public override void Initialize(NPCBase npcBase)
    {
        base.Initialize(npcBase);

        keySpriteRender = iconDisplay.GetComponent<SpriteRenderer>();
        basePosition = iconDisplay.transform.position;

        SetAlpha(0f);
        iconDisplay.SetActive(false);

        npcBase.Onfocus += ShowKeyGuide;
        npcBase.Onunfocus += HideKeyGuide;
    }

    public void ShowKeyGuide()
    {
        currentTween?.Kill();
        iconDisplay.SetActive(true);

        Sequence seq = DOTween.Sequence();

        seq.Join(keySpriteRender.DOFade(1f, duration))
           .Join(iconDisplay.transform.DOMoveY(basePosition.y + floatDistance, duration)
           .SetEase(Ease.OutBack));

        currentTween = seq;
    }

    public void HideKeyGuide()
    {
        currentTween?.Kill();

        Sequence seq = DOTween.Sequence();

        seq.Join(keySpriteRender.DOFade(0f, duration))
           .Join(iconDisplay.transform.DOMoveY(basePosition.y - floatDistance, duration)
           .SetEase(Ease.InBack))
           .OnComplete(() =>
           {
               iconDisplay.SetActive(false);
           });

        currentTween = seq;
    }

    private void SetAlpha(float value)
    {
        Color c = keySpriteRender.color;
        c.a = value;
        keySpriteRender.color = c;
    }

}
