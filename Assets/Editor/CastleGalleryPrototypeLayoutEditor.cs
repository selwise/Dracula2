using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CastleGalleryPrototypeLayout))]
public sealed class CastleGalleryPrototypeLayoutEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        CastleGalleryPrototypeLayout layout = (CastleGalleryPrototypeLayout)target;
        EditorGUILayout.Space();

        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Capture Current Transforms"))
            {
                Undo.RecordObject(layout, "Capture Castle Gallery Layout");
                layout.CaptureCurrent();
                EditorUtility.SetDirty(layout);
            }

            if (GUILayout.Button("Apply Values"))
            {
                Undo.RecordObjects(GetUndoTargets(layout), "Apply Castle Gallery Layout");
                layout.ApplyToTargets();
                EditorUtility.SetDirty(layout);
            }
        }
    }

    private static Object[] GetUndoTargets(CastleGalleryPrototypeLayout layout)
    {
        List<Object> targets = new List<Object>();
        targets.Add(layout);

        CastleGalleryPrototypeLayout.Placement[] placements = layout.Placements;
        for (int i = 0; i < placements.Length; i++)
        {
            if (placements[i] != null && placements[i].target != null)
            {
                targets.Add(placements[i].target);
            }
        }

        return targets.ToArray();
    }
}
