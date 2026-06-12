using UnityEngine;
using UnityEngine.InputSystem;

public sealed class CryptPrototypeInteraction : MonoBehaviour
{
    public Vector3 coffinPosition = new Vector3(2.55f, 0.18f, 0f);
    public float interactRadius = 1.35f;
    public SpriteRenderer coffinGlowRenderer;
    public SpriteRenderer coffinEffectRenderer;
    public Sprite[] coffinEffectFrames;
    public Texture2D promptPanelTexture;
    public float promptBottomOffset = 94f;
    public float glowPulseSeconds = 1.45f;
    public float effectFrameSeconds = 0.13f;

    private bool coffinSealed;
    private float messageTimer;
    private float glowTimer;
    private float effectFrameTimer;
    private int effectFrameIndex;

    private bool IsNearCoffin
    {
        get
        {
            Vector2 delta = transform.position - coffinPosition;
            return delta.sqrMagnitude <= interactRadius * interactRadius;
        }
    }

    private void Update()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard != null && IsNearCoffin && keyboard.eKey.wasPressedThisFrame)
        {
            coffinSealed = !coffinSealed;
            messageTimer = 2.0f;
            glowTimer = glowPulseSeconds;
        }

        if (messageTimer > 0f)
        {
            messageTimer -= Time.deltaTime;
        }

        if (glowTimer > 0f)
        {
            glowTimer -= Time.deltaTime;
        }

        UpdateGlow();
        UpdateEffect();
    }

    private void UpdateGlow()
    {
        if (coffinGlowRenderer == null)
        {
            return;
        }

        float pulse = glowTimer > 0f ? Mathf.Clamp01(glowTimer / glowPulseSeconds) : 0f;
        float alpha = pulse * (coffinSealed ? 0.62f : 0.34f);
        coffinGlowRenderer.color = new Color(1f, 1f, 1f, alpha);
    }

    private void UpdateEffect()
    {
        if (coffinEffectRenderer == null)
        {
            return;
        }

        bool visible = IsNearCoffin || coffinSealed || glowTimer > 0f;
        coffinEffectRenderer.enabled = visible;
        if (!visible)
        {
            return;
        }

        if (coffinEffectFrames != null && coffinEffectFrames.Length > 0)
        {
            effectFrameTimer += Time.deltaTime;
            if (effectFrameTimer >= effectFrameSeconds)
            {
                effectFrameTimer = 0f;
                effectFrameIndex = (effectFrameIndex + 1) % coffinEffectFrames.Length;
            }

            coffinEffectRenderer.sprite = coffinEffectFrames[Mathf.Clamp(effectFrameIndex, 0, coffinEffectFrames.Length - 1)];
        }

        float pulse = glowTimer > 0f ? Mathf.Clamp01(glowTimer / glowPulseSeconds) : 0f;
        float alpha = coffinSealed ? 0.72f : IsNearCoffin ? 0.34f : 0f;
        alpha = Mathf.Max(alpha, pulse * 0.82f);
        coffinEffectRenderer.color = new Color(1f, 1f, 1f, alpha);
    }

    private void OnGUI()
    {
        string text = null;
        string key = null;
        if (messageTimer > 0f)
        {
            text = coffinSealed ? "COFFIN SEALED" : "COFFIN REVEALED";
        }
        else if (IsNearCoffin)
        {
            key = "E";
            text = coffinSealed ? "REVEAL COFFIN" : "SEAL COFFIN";
        }

        if (string.IsNullOrEmpty(text))
        {
            return;
        }

        float uiScale = Mathf.Clamp(Screen.height / 1080f, 1f, 2f);
        float width = 384f * uiScale;
        float height = 72f * uiScale;
        float promptY = Screen.height - promptBottomOffset * uiScale;
        Rect rect = new Rect((Screen.width - width) * 0.5f, promptY, width, height);
        Color previousColor = GUI.color;
        int previousDepth = GUI.depth;

        GUI.depth = -50;
        GUI.color = Color.white;
        if (promptPanelTexture != null)
        {
            GUI.DrawTexture(rect, promptPanelTexture, ScaleMode.StretchToFill, true);
        }
        else
        {
            GUI.color = new Color(0f, 0f, 0f, 0.78f);
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
        }

        if (!string.IsNullOrEmpty(key))
        {
            GUI.color = new Color(0.88f, 0.78f, 0.28f, 1f);
            GUI.Label(new Rect(rect.x + 24f * uiScale, rect.y + 15f * uiScale, 50f * uiScale, 42f * uiScale), key, GetKeyStyle(uiScale));
        }

        GUI.color = new Color(0.95f, 0.9f, 0.45f, 1f);
        GUI.Label(new Rect(rect.x + 78f * uiScale, rect.y + 12f * uiScale, rect.width - 156f * uiScale, rect.height - 24f * uiScale), text, GetPromptStyle(uiScale));
        GUI.color = previousColor;
        GUI.depth = previousDepth;
    }

    private static GUIStyle GetPromptStyle(float uiScale)
    {
        GUIStyle style = new GUIStyle(GUI.skin.label);
        style.alignment = TextAnchor.MiddleCenter;
        style.fontSize = Mathf.RoundToInt(20f * uiScale);
        style.fontStyle = FontStyle.Bold;
        style.normal.textColor = new Color(0.95f, 0.9f, 0.45f, 1f);
        return style;
    }

    private static GUIStyle GetKeyStyle(float uiScale)
    {
        GUIStyle style = new GUIStyle(GUI.skin.label);
        style.alignment = TextAnchor.MiddleCenter;
        style.fontSize = Mathf.RoundToInt(24f * uiScale);
        style.fontStyle = FontStyle.Bold;
        style.normal.textColor = new Color(0.88f, 0.78f, 0.28f, 1f);
        return style;
    }
}
