using UnityEngine;

[ExecuteAlways]
public sealed class CryptHudOverlay : MonoBehaviour
{
    public Texture2D frameTexture;
    public bool visible = true;
    public bool visibleInEditMode = true;
    [Range(0f, 1f)]
    public float editModeOpacity = 1f;

    private void OnGUI()
    {
        bool shouldDraw = Application.isPlaying ? visible : visibleInEditMode;
        if (!shouldDraw || frameTexture == null)
        {
            return;
        }

        int previousDepth = GUI.depth;
        Color previousColor = GUI.color;
        GUI.depth = 1000;
        GUI.color = Application.isPlaying ? Color.white : new Color(1f, 1f, 1f, editModeOpacity);
        GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), frameTexture, ScaleMode.StretchToFill, true);
        GUI.color = previousColor;
        GUI.depth = previousDepth;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        UnityEditor.EditorApplication.QueuePlayerLoopUpdate();
        UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
    }
#endif
}
