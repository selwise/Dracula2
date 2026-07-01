using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[CustomEditor(typeof(CastleObliqueWallPlacement))]
public sealed class CastleObliqueWallPlacementEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        CastleObliqueWallPlacement placement = (CastleObliqueWallPlacement)target;
        EditorGUILayout.Space();

        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Apply Rule"))
            {
                Undo.RecordObjects(new Object[] { placement, placement.transform }, "Apply Oblique Wall Rule");
                placement.ApplyRule();
                EditorUtility.SetDirty(placement);
                EditorUtility.SetDirty(placement.transform);
                EditorSceneManager.MarkSceneDirty(placement.gameObject.scene);
            }

            if (GUILayout.Button("Capture As Rule"))
            {
                Undo.RecordObject(placement, "Capture Oblique Wall Rule");
                placement.CaptureCurrentTransformAsRule();
                EditorUtility.SetDirty(placement);
                EditorSceneManager.MarkSceneDirty(placement.gameObject.scene);
            }
        }
    }
}
