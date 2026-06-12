using UnityEngine;

public sealed class CryptAmbientAnimator : MonoBehaviour
{
    public SpriteRenderer[] flickerRenderers;
    public SpriteRenderer sigilRenderer;
    public float flickerSpeed = 3.4f;
    public float sigilPulseSpeed = 1.35f;
    public float scalePulse = 0.012f;

    private Color[] flickerBaseColors;
    private Vector3[] flickerBaseScales;
    private Color sigilBaseColor;
    private Vector3 sigilBaseScale;

    private void Awake()
    {
        int count = flickerRenderers == null ? 0 : flickerRenderers.Length;
        flickerBaseColors = new Color[count];
        flickerBaseScales = new Vector3[count];

        for (int i = 0; i < count; i++)
        {
            SpriteRenderer renderer = flickerRenderers[i];
            if (renderer == null)
            {
                continue;
            }

            flickerBaseColors[i] = renderer.color;
            flickerBaseScales[i] = renderer.transform.localScale;
        }

        if (sigilRenderer != null)
        {
            sigilBaseColor = sigilRenderer.color;
            sigilBaseScale = sigilRenderer.transform.localScale;
        }
    }

    private void Update()
    {
        AnimateFlicker();
        AnimateSigil();
    }

    private void AnimateFlicker()
    {
        if (flickerRenderers == null)
        {
            return;
        }

        float time = Time.time * flickerSpeed;
        for (int i = 0; i < flickerRenderers.Length; i++)
        {
            SpriteRenderer renderer = flickerRenderers[i];
            if (renderer == null)
            {
                continue;
            }

            float noise = Mathf.PerlinNoise(i * 17.3f, time);
            float wave = Mathf.Sin(Time.time * (2.6f + i * 0.41f) + i) * 0.5f + 0.5f;
            float intensity = Mathf.Lerp(0.68f, 1.18f, noise * 0.7f + wave * 0.3f);
            Color color = flickerBaseColors[i];
            color.a *= intensity;
            renderer.color = color;
            renderer.transform.localScale = flickerBaseScales[i] * (1f + (intensity - 1f) * scalePulse);
        }
    }

    private void AnimateSigil()
    {
        if (sigilRenderer == null)
        {
            return;
        }

        float pulse = Mathf.Sin(Time.time * sigilPulseSpeed) * 0.5f + 0.5f;
        float intensity = Mathf.Lerp(0.45f, 0.95f, pulse);
        Color color = sigilBaseColor;
        color.a *= intensity;
        sigilRenderer.color = color;
        sigilRenderer.transform.localScale = sigilBaseScale * Mathf.Lerp(0.992f, 1.008f, pulse);
    }
}
