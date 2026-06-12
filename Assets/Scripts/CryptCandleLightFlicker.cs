using UnityEngine;

[ExecuteAlways]
public sealed class CryptCandleLightFlicker : MonoBehaviour
{
    public Light[] candleLights;
    public SpriteRenderer[] glowRenderers;
    public SpriteRenderer shadowGradeRenderer;

    [Header("Flicker")]
    public float flickerSpeed = 3.8f;
    public float lightIntensityPulse = 0.28f;
    public float lightRangePulse = 0.16f;
    public float glowAlphaPulse = 0.22f;
    public float glowScalePulse = 0.018f;
    public float shadowBreath = 0.055f;

    private float[] baseIntensities;
    private float[] baseRanges;
    private Color[] baseGlowColors;
    private Vector3[] baseGlowScales;
    private Color baseShadowColor;

    private void OnEnable()
    {
        CacheBaseValues();
        ApplyFlicker(GetTime());
    }

    private void OnValidate()
    {
        CacheBaseValues();
        ApplyFlicker(GetTime());
    }

    private void Update()
    {
        ApplyFlicker(GetTime());

#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            UnityEditor.EditorApplication.QueuePlayerLoopUpdate();
        }
#endif
    }

    private void CacheBaseValues()
    {
        int lightCount = candleLights == null ? 0 : candleLights.Length;
        baseIntensities = new float[lightCount];
        baseRanges = new float[lightCount];

        for (int i = 0; i < lightCount; i++)
        {
            Light candleLight = candleLights[i];
            if (candleLight == null)
            {
                continue;
            }

            baseIntensities[i] = candleLight.intensity;
            baseRanges[i] = candleLight.range;
        }

        int glowCount = glowRenderers == null ? 0 : glowRenderers.Length;
        baseGlowColors = new Color[glowCount];
        baseGlowScales = new Vector3[glowCount];

        for (int i = 0; i < glowCount; i++)
        {
            SpriteRenderer renderer = glowRenderers[i];
            if (renderer == null)
            {
                continue;
            }

            baseGlowColors[i] = renderer.color;
            baseGlowScales[i] = renderer.transform.localScale;
        }

        if (shadowGradeRenderer != null)
        {
            baseShadowColor = shadowGradeRenderer.color;
        }
    }

    private void ApplyFlicker(float time)
    {
        if ((candleLights != null && (baseIntensities == null || baseRanges == null || baseIntensities.Length != candleLights.Length || baseRanges.Length != candleLights.Length)) ||
            (glowRenderers != null && (baseGlowColors == null || baseGlowScales == null || baseGlowColors.Length != glowRenderers.Length || baseGlowScales.Length != glowRenderers.Length)))
        {
            CacheBaseValues();
        }

        float roomPulse = Mathf.Sin(time * 0.72f) * 0.5f + 0.5f;

        if (candleLights != null)
        {
            for (int i = 0; i < candleLights.Length; i++)
            {
                Light candleLight = candleLights[i];
                if (candleLight == null)
                {
                    continue;
                }

                float flicker = CandleNoise(i, time);
                float intensityScale = 1f + (flicker - 0.5f) * lightIntensityPulse;
                float rangeScale = 1f + (flicker - 0.5f) * lightRangePulse;
                candleLight.intensity = baseIntensities[i] * intensityScale;
                candleLight.range = baseRanges[i] * rangeScale;
            }
        }

        if (glowRenderers != null)
        {
            for (int i = 0; i < glowRenderers.Length; i++)
            {
                SpriteRenderer renderer = glowRenderers[i];
                if (renderer == null)
                {
                    continue;
                }

                float flicker = CandleNoise(i + 11, time);
                Color color = baseGlowColors[i];
                color.a *= 1f + (flicker - 0.5f) * glowAlphaPulse;
                renderer.color = color;
                renderer.transform.localScale = baseGlowScales[i] * (1f + (flicker - 0.5f) * glowScalePulse);
            }
        }

        if (shadowGradeRenderer != null)
        {
            Color color = baseShadowColor;
            color.a *= 1f + Mathf.Lerp(-shadowBreath, shadowBreath, roomPulse);
            shadowGradeRenderer.color = color;
        }
    }

    private float CandleNoise(int index, float time)
    {
        float noise = Mathf.PerlinNoise(index * 19.37f, time * flickerSpeed);
        float wave = Mathf.Sin(time * (2.2f + index * 0.23f) + index * 1.71f) * 0.5f + 0.5f;
        return Mathf.Clamp01(noise * 0.72f + wave * 0.28f);
    }

    private float GetTime()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            return (float)UnityEditor.EditorApplication.timeSinceStartup;
        }
#endif
        return Time.time;
    }
}
