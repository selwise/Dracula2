using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(DraculaWalker))]
[RequireComponent(typeof(SpriteRenderer))]
public sealed class DraculaSpriteStyleSwitcher : MonoBehaviour
{
    [Header("Selected Style")]
    public bool useSpectrumInspired;

    [Header("References")]
    public DraculaWalker walker;
    public SpriteRenderer spriteRenderer;

    [Header("Classic")]
    public DraculaSpriteSet classic = new DraculaSpriteSet();

    [Header("Spectrum Inspired")]
    public DraculaSpriteSet spectrumInspired = new DraculaSpriteSet();

    private bool lastAppliedUseSpectrumInspired;

    [System.Serializable]
    public sealed class DraculaSpriteSet
    {
        public Sprite[] walkDown;
        public Sprite[] walkUp;
        public Sprite[] walkRight;
        public Sprite[] walkLeft;
        public Sprite[] idleDown;
        public Sprite[] idleUp;
        public Sprite[] idleSide;
        public Sprite[] idleLeft;

        public bool HasPlayableFrames()
        {
            return HasFrames(walkDown) && HasFrames(walkRight);
        }

        public Sprite GetStartupSprite()
        {
            if (HasFrames(idleDown))
            {
                return idleDown[0];
            }

            if (HasFrames(walkDown))
            {
                return walkDown[0];
            }

            return HasFrames(walkRight) ? walkRight[0] : null;
        }

        private static bool HasFrames(Sprite[] frames)
        {
            return frames != null && frames.Length > 0 && frames[0] != null;
        }
    }

    private void Reset()
    {
        CacheReferences();
    }

    private void Awake()
    {
        CacheReferences();
        ApplySelectedStyle();
    }

    private void Update()
    {
        if (lastAppliedUseSpectrumInspired != useSpectrumInspired)
        {
            ApplySelectedStyle();
        }
    }

    private void OnValidate()
    {
        CacheReferences();

        if (!Application.isPlaying)
        {
            ApplySelectedStyle();
        }
    }

    [ContextMenu("Apply Selected Style")]
    public void ApplySelectedStyle()
    {
        CacheReferences();

        DraculaSpriteSet selectedSet = useSpectrumInspired ? spectrumInspired : classic;
        if (walker == null || selectedSet == null || !selectedSet.HasPlayableFrames())
        {
            return;
        }

        walker.walkDown = selectedSet.walkDown;
        walker.walkUp = selectedSet.walkUp;
        walker.walkRight = selectedSet.walkRight;
        walker.walkLeft = selectedSet.walkLeft;
        walker.idleDown = selectedSet.idleDown;
        walker.idleUp = selectedSet.idleUp;
        walker.idleSide = selectedSet.idleSide;
        walker.idleLeft = selectedSet.idleLeft;

        if (spriteRenderer != null)
        {
            Sprite startupSprite = selectedSet.GetStartupSprite();
            if (startupSprite != null)
            {
                spriteRenderer.sprite = startupSprite;
            }

            spriteRenderer.flipX = false;
        }

        lastAppliedUseSpectrumInspired = useSpectrumInspired;
    }

    private void CacheReferences()
    {
        if (walker == null)
        {
            walker = GetComponent<DraculaWalker>();
        }

        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
    }
}
