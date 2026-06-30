using UnityEngine;

[ExecuteAlways]
[DisallowMultipleComponent]
[RequireComponent(typeof(SpriteRenderer))]
public sealed class CharacterReadabilityOverlay : MonoBehaviour
{
    public SpriteRenderer sourceRenderer;
    public SpriteRenderer overlayRenderer;
    public Material overlayMaterial;

    [Header("Grade")]
    public bool overlayEnabled = true;
    [Range(0f, 0.35f)]
    public float alpha = 0.12f;
    public Color tint = new Color(0.78f, 0.83f, 0.92f, 1f);

    [Header("Draw Order")]
    public int sortingOrderOffset = 1;
    public Vector3 localOffset = Vector3.zero;

    private const string OverlayName = "Readability Overlay";
#if UNITY_EDITOR
    private bool editorSyncQueued;
#endif

    private void Reset()
    {
        CacheReferences();
        SyncOverlay();
    }

    private void OnEnable()
    {
        CacheReferences();
        SyncOverlay();
    }

    private void OnValidate()
    {
        CacheReferences();

#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            QueueEditorSync();
            return;
        }
#endif

        SyncOverlay();
    }

    private void LateUpdate()
    {
        SyncOverlay();
    }

    private void CacheReferences()
    {
        if (sourceRenderer == null)
        {
            sourceRenderer = GetComponent<SpriteRenderer>();
        }

        if (overlayRenderer == null)
        {
            Transform overlayTransform = transform.Find(OverlayName);
            if (overlayTransform != null)
            {
                overlayRenderer = overlayTransform.GetComponent<SpriteRenderer>();
            }
        }

        if (overlayRenderer == null)
        {
            GameObject overlayObject = new GameObject(OverlayName);
            overlayObject.transform.SetParent(transform, false);
            overlayRenderer = overlayObject.AddComponent<SpriteRenderer>();
        }
    }

    private void SyncOverlay()
    {
        if (sourceRenderer == null || overlayRenderer == null)
        {
            return;
        }

        overlayRenderer.enabled = overlayEnabled && sourceRenderer.enabled && sourceRenderer.sprite != null && alpha > 0f;
        overlayRenderer.sprite = sourceRenderer.sprite;
        overlayRenderer.flipX = sourceRenderer.flipX;
        overlayRenderer.flipY = sourceRenderer.flipY;
        overlayRenderer.spriteSortPoint = sourceRenderer.spriteSortPoint;
        overlayRenderer.maskInteraction = sourceRenderer.maskInteraction;
        overlayRenderer.sortingLayerID = sourceRenderer.sortingLayerID;
        overlayRenderer.sortingOrder = sourceRenderer.sortingOrder + sortingOrderOffset;

        Color color = tint;
        color.a = alpha;
        overlayRenderer.color = color;

        if (overlayMaterial != null)
        {
            overlayRenderer.sharedMaterial = overlayMaterial;
        }

        Transform overlayTransform = overlayRenderer.transform;
        overlayTransform.localPosition = localOffset;
        overlayTransform.localRotation = Quaternion.identity;
        overlayTransform.localScale = Vector3.one;
    }

#if UNITY_EDITOR
    private void QueueEditorSync()
    {
        if (editorSyncQueued)
        {
            return;
        }

        editorSyncQueued = true;
        UnityEditor.EditorApplication.delayCall += DelayedEditorSync;
    }

    private void DelayedEditorSync()
    {
        editorSyncQueued = false;
        if (this == null)
        {
            return;
        }

        CacheReferences();
        SyncOverlay();
        UnityEditor.EditorApplication.QueuePlayerLoopUpdate();
    }
#endif
}
