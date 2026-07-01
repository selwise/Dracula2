using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
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

    public Vector3 EffectiveTileStep
    {
        get
        {
            if (tileStepOverride != Vector3.zero)
            {
                return tileStepOverride;
            }

            EnsureSourceRenderer();
            if (sourceRenderer == null)
            {
                return Vector3.zero;
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

            return new Vector3(localWidth * sourceRenderer.transform.localScale.x, 0f, 0f);
        }
    }

    public void RebuildTiles()
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

        RemoveManagedTilesOutsideRange(parent);

        for (int i = 1; i <= tilesRight; i++)
        {
            PlaceTile(parent, sourceTransform, step, i, GetTileName(i));
        }

        for (int i = 1; i <= tilesLeft; i++)
        {
            PlaceTile(parent, sourceTransform, -step, i, GetTileName(-i));
        }
    }

    public void DeleteGeneratedTiles()
    {
        Transform sourceTransform = sourceRenderer != null ? sourceRenderer.transform : transform;
        Transform parent = sourceTransform.parent;
        List<GameObject> tiles = FindManagedTiles(parent);
        for (int i = tiles.Count - 1; i >= 0; i--)
        {
            DestroyTile(tiles[i]);
        }
    }

    public void CaptureStepFromTile(Transform tile)
    {
        EnsureSourceRenderer();
        if (sourceRenderer == null || tile == null)
        {
            return;
        }

        tileStepOverride = tile.localPosition - sourceRenderer.transform.localPosition;
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

    private void OnValidate()
    {
        EnsureSourceRenderer();
        tilesLeft = Mathf.Max(0, tilesLeft);
        tilesRight = Mathf.Max(0, tilesRight);
        if (string.IsNullOrWhiteSpace(tileNamePrefix) && sourceRenderer != null)
        {
            tileNamePrefix = sourceRenderer.gameObject.name + " Tile";
        }
    }

    private void EnsureSourceRenderer()
    {
        if (sourceRenderer == null)
        {
            sourceRenderer = GetComponent<SpriteRenderer>();
        }
    }

    private void PlaceTile(Transform parent, Transform sourceTransform, Vector3 step, int distance, string tileName)
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
            if (!Application.isPlaying)
            {
                Undo.RegisterCreatedObjectUndo(tileObject, "Create Castle Gallery Floor Tile");
            }
#endif
        }

        SpriteRenderer tileRenderer = tileObject.GetComponent<SpriteRenderer>();
        CopySourceToTile(tileRenderer);

#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            Undo.RecordObject(tileObject.transform, "Place Castle Gallery Floor Tile");
            if (tileRenderer != null)
            {
                Undo.RecordObject(tileRenderer, "Match Castle Gallery Floor Tile Renderer");
            }
        }
#endif

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

    private void RemoveManagedTilesOutsideRange(Transform parent)
    {
        List<GameObject> tiles = FindManagedTiles(parent);
        for (int i = tiles.Count - 1; i >= 0; i--)
        {
            GameObject tile = tiles[i];
            if (IsExpectedTileName(tile.name))
            {
                continue;
            }

            DestroyTile(tile);
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

    private static void DestroyTile(GameObject tile)
    {
        if (tile == null)
        {
            return;
        }

#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            Undo.DestroyObjectImmediate(tile);
            return;
        }
#endif

        Destroy(tile);
    }
}
