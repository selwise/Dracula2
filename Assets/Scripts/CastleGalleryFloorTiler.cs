using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

[ExecuteAlways]
[DisallowMultipleComponent]
public sealed class CastleGalleryFloorTiler : MonoBehaviour
{
    [SerializeField] private SpriteRenderer sourceRenderer;
    [SerializeField, Min(0)] private int tilesLeft;
    [SerializeField, Min(0)] private int tilesRight = 2;
    [SerializeField] private string tileNamePrefix = "Hallway Deep Background Tile";
    [SerializeField] private Vector3 tileStepOverride;
    [SerializeField] private bool useTileStepOverride;
    [SerializeField] private bool stepFollowsSourceRotation;
    [SerializeField] private bool liveUpdateInEditMode = true;

    [System.NonSerialized] private bool hasSnapshot;
    [System.NonSerialized] private Vector3 lastSourceLocalPosition;
    [System.NonSerialized] private Quaternion lastSourceLocalRotation;
    [System.NonSerialized] private Vector3 lastSourceLocalScale;
    [System.NonSerialized] private Sprite lastSprite;
    [System.NonSerialized] private SpriteDrawMode lastDrawMode;
    [System.NonSerialized] private SpriteTileMode lastTileMode;
    [System.NonSerialized] private Vector2 lastRendererSize;
    [System.NonSerialized] private Color lastColor;
    [System.NonSerialized] private int lastSortingLayerId;
    [System.NonSerialized] private int lastSortingOrder;
    [System.NonSerialized] private int lastTilesLeft;
    [System.NonSerialized] private int lastTilesRight;
    [System.NonSerialized] private string lastTileNamePrefix;
    [System.NonSerialized] private Vector3 lastTileStepOverride;
    [System.NonSerialized] private bool lastUseTileStepOverride;
    [System.NonSerialized] private bool lastStepFollowsSourceRotation;

    public SpriteRenderer SourceRenderer
    {
        get { return sourceRenderer; }
    }

    public int TilesLeft
    {
        get { return tilesLeft; }
    }

    public int TilesRight
    {
        get { return tilesRight; }
    }

    public string TileNamePrefix
    {
        get { return tileNamePrefix; }
    }

    public bool LiveUpdateInEditMode
    {
        get { return liveUpdateInEditMode; }
    }

    public bool StepFollowsSourceRotation
    {
        get { return stepFollowsSourceRotation; }
    }

    public bool UseTileStepOverride
    {
        get { return useTileStepOverride; }
    }

    public Vector3 EffectiveTileStep
    {
        get
        {
            EnsureSourceRenderer();
            if (sourceRenderer == null)
            {
                return Vector3.zero;
            }

            if (useTileStepOverride && tileStepOverride != Vector3.zero)
            {
                return stepFollowsSourceRotation
                    ? sourceRenderer.transform.localRotation * tileStepOverride
                    : tileStepOverride;
            }

            float localWidth = 0f;
            if (sourceRenderer.drawMode == SpriteDrawMode.Simple && sourceRenderer.sprite != null)
            {
                localWidth = sourceRenderer.sprite.bounds.size.x;
            }
            else
            {
                localWidth = sourceRenderer.size.x;
            }

            Vector3 step = new Vector3(localWidth * sourceRenderer.transform.localScale.x, 0f, 0f);
            return stepFollowsSourceRotation
                ? sourceRenderer.transform.localRotation * step
                : step;
        }
    }

    public void RebuildTiles()
    {
        RebuildTiles(true);
    }

    public void DeleteGeneratedTiles()
    {
        EnsureSourceRenderer();
        Transform sourceTransform = sourceRenderer != null ? sourceRenderer.transform : transform;
        Transform parent = sourceTransform.parent;
        List<GameObject> tiles = FindManagedTiles(parent);
        for (int i = tiles.Count - 1; i >= 0; i--)
        {
            DestroyTile(tiles[i], true);
        }

        CaptureSnapshot();
        MarkSceneDirty();
    }

    public void CaptureStepFromTile(Transform tile)
    {
        EnsureSourceRenderer();
        if (sourceRenderer == null || tile == null)
        {
            return;
        }

        Vector3 step = tile.localPosition - sourceRenderer.transform.localPosition;
        tileStepOverride = stepFollowsSourceRotation
            ? Quaternion.Inverse(sourceRenderer.transform.localRotation) * step
            : step;
        useTileStepOverride = true;
        hasSnapshot = false;
    }

    public Transform FindTile(int index)
    {
        EnsureSourceRenderer();
        if (sourceRenderer == null)
        {
            return null;
        }

        Transform parent = sourceRenderer.transform.parent;
        if (parent == null)
        {
            return null;
        }

        return parent.Find(GetTileName(index));
    }

    private void Reset()
    {
        sourceRenderer = GetComponent<SpriteRenderer>();
        if (sourceRenderer != null)
        {
            tileNamePrefix = sourceRenderer.gameObject.name + " Tile";
        }
    }

    private void OnEnable()
    {
        EnsureSourceRenderer();
        hasSnapshot = false;
    }

    private void OnValidate()
    {
        EnsureSourceRenderer();
        tilesLeft = Mathf.Max(0, tilesLeft);
        tilesRight = Mathf.Max(0, tilesRight);
        if (string.IsNullOrWhiteSpace(tileNamePrefix) && sourceRenderer != null)
        {
            tileNamePrefix = sourceRenderer.gameObject.name + " Tile";
        }

        hasSnapshot = false;
    }

    private void Update()
    {
        if (Application.isPlaying || !liveUpdateInEditMode)
        {
            return;
        }

        EnsureSourceRenderer();
        if (sourceRenderer == null)
        {
            return;
        }

        if (NeedsSync())
        {
            RebuildTiles(false);
        }
    }

    private void RebuildTiles(bool recordUndo)
    {
        EnsureSourceRenderer();
        if (sourceRenderer == null)
        {
            return;
        }

        Transform sourceTransform = sourceRenderer.transform;
        Transform parent = sourceTransform.parent;
        Vector3 step = EffectiveTileStep;
        if (step == Vector3.zero)
        {
            return;
        }

        RemoveManagedTilesOutsideRange(parent, recordUndo);

        for (int i = 1; i <= tilesRight; i++)
        {
            PlaceTile(parent, sourceTransform, step, i, GetTileName(i), recordUndo);
        }

        for (int i = 1; i <= tilesLeft; i++)
        {
            PlaceTile(parent, sourceTransform, -step, i, GetTileName(-i), recordUndo);
        }

        CaptureSnapshot();
        MarkSceneDirty();
    }

    private bool NeedsSync()
    {
        if (sourceRenderer == null)
        {
            return false;
        }

        Transform sourceTransform = sourceRenderer.transform;
        if (!hasSnapshot)
        {
            return true;
        }

        if (lastSourceLocalPosition != sourceTransform.localPosition ||
            lastSourceLocalRotation != sourceTransform.localRotation ||
            lastSourceLocalScale != sourceTransform.localScale ||
            lastSprite != sourceRenderer.sprite ||
            lastDrawMode != sourceRenderer.drawMode ||
            lastTileMode != sourceRenderer.tileMode ||
            lastRendererSize != sourceRenderer.size ||
            lastColor != sourceRenderer.color ||
            lastSortingLayerId != sourceRenderer.sortingLayerID ||
            lastSortingOrder != sourceRenderer.sortingOrder ||
            lastTilesLeft != tilesLeft ||
            lastTilesRight != tilesRight ||
            lastTileNamePrefix != tileNamePrefix ||
            lastTileStepOverride != tileStepOverride ||
            lastUseTileStepOverride != useTileStepOverride ||
            lastStepFollowsSourceRotation != stepFollowsSourceRotation)
        {
            return true;
        }

        for (int i = 1; i <= tilesRight; i++)
        {
            if (FindTile(i) == null)
            {
                return true;
            }
        }

        for (int i = 1; i <= tilesLeft; i++)
        {
            if (FindTile(-i) == null)
            {
                return true;
            }
        }

        List<GameObject> tiles = FindManagedTiles(sourceTransform.parent);
        for (int i = 0; i < tiles.Count; i++)
        {
            if (!IsExpectedTileName(tiles[i].name))
            {
                return true;
            }
        }

        return false;
    }

    private void CaptureSnapshot()
    {
        if (sourceRenderer == null)
        {
            hasSnapshot = false;
            return;
        }

        Transform sourceTransform = sourceRenderer.transform;
        lastSourceLocalPosition = sourceTransform.localPosition;
        lastSourceLocalRotation = sourceTransform.localRotation;
        lastSourceLocalScale = sourceTransform.localScale;
        lastSprite = sourceRenderer.sprite;
        lastDrawMode = sourceRenderer.drawMode;
        lastTileMode = sourceRenderer.tileMode;
        lastRendererSize = sourceRenderer.size;
        lastColor = sourceRenderer.color;
        lastSortingLayerId = sourceRenderer.sortingLayerID;
        lastSortingOrder = sourceRenderer.sortingOrder;
        lastTilesLeft = tilesLeft;
        lastTilesRight = tilesRight;
        lastTileNamePrefix = tileNamePrefix;
        lastTileStepOverride = tileStepOverride;
        lastUseTileStepOverride = useTileStepOverride;
        lastStepFollowsSourceRotation = stepFollowsSourceRotation;
        hasSnapshot = true;
    }

    private void EnsureSourceRenderer()
    {
        if (sourceRenderer == null)
        {
            sourceRenderer = GetComponent<SpriteRenderer>();
        }
    }

    private void PlaceTile(Transform parent, Transform sourceTransform, Vector3 step, int distance, string tileName, bool recordUndo)
    {
        Transform existing = parent != null ? parent.Find(tileName) : null;
        GameObject tileObject;
        if (existing != null)
        {
            tileObject = existing.gameObject;
        }
        else
        {
            tileObject = Instantiate(sourceTransform.gameObject, parent);
            tileObject.name = tileName;
            RemoveTilerComponents(tileObject);
#if UNITY_EDITOR
            if (recordUndo && !Application.isPlaying)
            {
                Undo.RegisterCreatedObjectUndo(tileObject, "Create Castle Gallery Tile");
            }
#endif
        }

        SpriteRenderer tileRenderer = tileObject.GetComponent<SpriteRenderer>();

#if UNITY_EDITOR
        if (recordUndo && !Application.isPlaying)
        {
            Undo.RecordObject(tileObject.transform, "Place Castle Gallery Tile");
            if (tileRenderer != null)
            {
                Undo.RecordObject(tileRenderer, "Match Castle Gallery Tile Renderer");
            }
        }
#endif

        CopySourceToTile(tileRenderer);
        tileObject.transform.localPosition = sourceTransform.localPosition + step * distance;
        tileObject.transform.localRotation = sourceTransform.localRotation;
        tileObject.transform.localScale = sourceTransform.localScale;
        tileObject.SetActive(sourceTransform.gameObject.activeSelf);

#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            EditorUtility.SetDirty(tileObject);
            if (tileRenderer != null)
            {
                EditorUtility.SetDirty(tileRenderer);
            }
        }
#endif
    }

    private void CopySourceToTile(SpriteRenderer tileRenderer)
    {
        if (tileRenderer == null || sourceRenderer == null)
        {
            return;
        }

        tileRenderer.sprite = sourceRenderer.sprite;
        tileRenderer.drawMode = sourceRenderer.drawMode;
        tileRenderer.tileMode = sourceRenderer.tileMode;
        tileRenderer.size = sourceRenderer.size;
        tileRenderer.color = sourceRenderer.color;
        tileRenderer.flipX = sourceRenderer.flipX;
        tileRenderer.flipY = sourceRenderer.flipY;
        tileRenderer.maskInteraction = sourceRenderer.maskInteraction;
        tileRenderer.sortingLayerID = sourceRenderer.sortingLayerID;
        tileRenderer.sortingOrder = sourceRenderer.sortingOrder;
        tileRenderer.sharedMaterial = sourceRenderer.sharedMaterial;
    }

    private void RemoveManagedTilesOutsideRange(Transform parent, bool recordUndo)
    {
        List<GameObject> tiles = FindManagedTiles(parent);
        for (int i = tiles.Count - 1; i >= 0; i--)
        {
            GameObject tile = tiles[i];
            if (IsExpectedTileName(tile.name))
            {
                continue;
            }

            DestroyTile(tile, recordUndo);
        }
    }

    private List<GameObject> FindManagedTiles(Transform parent)
    {
        List<GameObject> tiles = new List<GameObject>();
        if (parent == null || string.IsNullOrEmpty(tileNamePrefix))
        {
            return tiles;
        }

        for (int i = 0; i < parent.childCount; i++)
        {
            Transform child = parent.GetChild(i);
            if (child.gameObject != gameObject && child.name.StartsWith(tileNamePrefix))
            {
                tiles.Add(child.gameObject);
            }
        }

        return tiles;
    }

    private bool IsExpectedTileName(string objectName)
    {
        for (int i = 1; i <= tilesRight; i++)
        {
            if (objectName == GetTileName(i))
            {
                return true;
            }
        }

        for (int i = 1; i <= tilesLeft; i++)
        {
            if (objectName == GetTileName(-i))
            {
                return true;
            }
        }

        return false;
    }

    private string GetTileName(int index)
    {
        string side = index < 0 ? "L" : "R";
        return tileNamePrefix + " " + side + Mathf.Abs(index).ToString("00");
    }

    private static void RemoveTilerComponents(GameObject tileObject)
    {
        CastleGalleryFloorTiler[] tilers = tileObject.GetComponents<CastleGalleryFloorTiler>();
        for (int i = tilers.Length - 1; i >= 0; i--)
        {
            if (Application.isPlaying)
            {
                Destroy(tilers[i]);
            }
            else
            {
                DestroyImmediate(tilers[i]);
            }
        }
    }

    private static void DestroyTile(GameObject tile, bool recordUndo)
    {
        if (tile == null)
        {
            return;
        }

#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            if (recordUndo)
            {
                Undo.DestroyObjectImmediate(tile);
            }
            else
            {
                DestroyImmediate(tile);
            }

            return;
        }
#endif

        Destroy(tile);
    }

    private void MarkSceneDirty()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying && gameObject.scene.IsValid())
        {
            EditorUtility.SetDirty(this);
            EditorSceneManager.MarkSceneDirty(gameObject.scene);
        }
#endif
    }
}
