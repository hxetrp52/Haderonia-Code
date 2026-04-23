using UnityEngine;

public class IconDisplaySprite : IconDisplayBase
{
    [SerializeField] private SpriteRenderer targetRenderer;

    protected override void ApplySprite(Sprite sprite)
    {
        if (targetRenderer != null)
            targetRenderer.sprite = sprite;
    }
}