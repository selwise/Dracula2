using UnityEngine;

[ExecuteAlways]
[DisallowMultipleComponent]
[RequireComponent(typeof(SpriteRenderer))]
public sealed class CastleObliqueWallPlacement : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private CastleObliqueWallPlacementRule rule = CastleObliqueWallPlacementRule.CastleProperEastGalleryLeftReturn();
    [SerializeField] private bool applyRuleOnValidate;

    public SpriteRenderer SpriteRenderer
    {
        get { return spriteRenderer; }
    }

    public CastleObliqueWallPlacementRule Rule
    {
        get { return rule; }
    }

    public void Configure(SpriteRenderer renderer, CastleObliqueWallPlacementRule sourceRule)
    {
        spriteRenderer = renderer != null ? renderer : GetComponent<SpriteRenderer>();
        rule = sourceRule != null ? sourceRule.Clone() : CastleObliqueWallPlacementRule.CastleProperEastGalleryLeftReturn();
    }

    public void ApplyRule()
    {
        EnsureRenderer();
        Sprite sprite = spriteRenderer != null ? spriteRenderer.sprite : null;
        rule.ApplyTo(transform, sprite);
    }

    public void CaptureCurrentTransformAsRule()
    {
        EnsureRenderer();
        Sprite sprite = spriteRenderer != null ? spriteRenderer.sprite : null;
        rule.CaptureFrom(transform, sprite);
    }

    private void Reset()
    {
        EnsureRenderer();
    }

    private void OnValidate()
    {
        EnsureRenderer();
        if (!Application.isPlaying && applyRuleOnValidate)
        {
            ApplyRule();
        }
    }

    private void EnsureRenderer()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
    }
}
