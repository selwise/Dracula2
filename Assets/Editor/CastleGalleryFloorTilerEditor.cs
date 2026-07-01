using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[CustomEditor(typeof(CastleGalleryFloorTiler))]
public sealed class CastleGalleryFloorTilerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        CastleGalleryFloorTiler tiler = (CastleGalleryFloorTiler)target;
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Effective Step", tiler.EffectiveTileStep.ToString());

        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Rebuild Tiles"))
            {
                Undo.RecordObject(tiler, "Rebuild Castle Gallery Floor Tiles");
                tiler.RebuildTiles();
                EditorUtility.SetDirty(tiler);
                EditorSceneManager.MarkSceneDirty(tiler.gameObject.scene);
            }

            if (GUILayout.Button("Delete Tiles"))
            {
                Undo.RecordObject(tiler, "Delete Castle Gallery Floor Tiles");
                tiler.DeleteGeneratedTiles();
                EditorUtility.SetDirty(tiler);
                EditorSceneManager.MarkSceneDirty(tiler.gameObject.scene);
            }
        }

        Transform firstRightTile = tiler.FindTile(1);
        using (new EditorGUI.DisabledScope(firstRightTile == null))
        {
            if (GUILayout.Button("Capture Step From R01"))
            {
                Undo.RecordObject(tiler, "Capture Castle Gallery Floor Tile Step");
                tiler.CaptureStepFromTile(firstRightTile);
                EditorUtility.SetDirty(tiler);
                EditorSceneManager.MarkSceneDirty(tiler.gameObject.scene);
            }
        }
    }
}
