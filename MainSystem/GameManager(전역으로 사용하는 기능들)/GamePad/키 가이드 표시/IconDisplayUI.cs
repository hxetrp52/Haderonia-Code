using UnityEngine;
using UnityEngine.UI;

public class IconDisplayUI : IconDisplayBase
{
    [SerializeField] private Image targetImage;

    protected override void ApplySprite(Sprite sprite)
    {
        if (targetImage != null)
            targetImage.sprite = sprite;
    }
}
