// ============================================================
// File: Assets/Scripts/CandleFlame.cs
// ============================================================

using UnityEngine;
using System.Reflection;

/// <summary>
/// Attach to candle prefabs for animated flame + dynamic 2D lighting.
/// Unity 6 compatible - uses reflection for Light2D to avoid namespace issues.
/// </summary>
public class CandleFlame : MonoBehaviour
{
    [Header("Flame Animation")]
    public Sprite[] flameFrames;
    [Range(1f, 24f)]
    public float animationFramerate = 12f;
    [Range(0f, 0.3f)]
    public float framerateVariation = 0.1f;
    
    [Header("Flame Appearance")]
    [Range(0.1f, 2f)]
    public float flameSize = 1f;
    [Range(0f, 0.3f)]
    public float positionJitter = 0f;  // ← Changed from 0.05f to 0f
    [Range(0f, 0.3f)]
    public float jitterSpeedVariation = 0.2f;
    
    [Header("Lighting")]
    public Color lightColor = new Color(1f, 0.85f, 0.6f);
    [Range(1f, 10f)]
    public float lightIntensity = 3f;
    [Range(0f, 0.5f)]
    public float intensityVariation = 0.1f;
    [Range(1f, 20f)]
    public float lightRange = 5f;
    [Range(0f, 1f)]
    public float lightFlickerIntensity = 0.3f;
    [Range(0.5f, 5f)]
    public float lightFlickerSpeed = 2f;
    [Range(0f, 0.5f)]
    public float flickerSpeedVariation = 0.15f;
    
    [Header("Offset")]
    public Vector2 flameOffset = new Vector2(0f, 0.3f);
    
    [Header("Randomization")]
    public bool randomizeFrameStart = true;
    public bool randomizeFlickerPhase = true;
    public bool randomizeAllOnStart = true;
    
    [Header("Debug")]
    public bool hideBaseSpriteAtRuntime = true;
    public bool showDebugLogs = false;

    private SpriteRenderer baseSpriteRenderer;  // Add with other 
    
    // Internal state
    private SpriteRenderer flameRenderer;
    private Component candleLight;
    private System.Type light2DType;
    private PropertyInfo intensityProperty;
    private PropertyInfo colorProperty;
    private PropertyInfo rangeProperty;
    
    private int frameCount;
    private float frameTimer;
    private int currentFrame;
    private float flickerPhase;
    private float jitterPhase;
    
    private float runtimeFramerate;
    private float runtimeFlickerSpeed;
    private float runtimeIntensity;
    private float runtimeJitterSpeed;
    
    private void Awake()
    {
        
        SpriteRenderer baseSr = GetComponent<SpriteRenderer>();
        if (baseSr != null) baseSr.enabled = false;
        else Debug.LogWarning("CandleFlame: No SpriteRenderer found on this GameObject!");
        
        if (randomizeAllOnStart)
        {
            ApplyRandomVariation();
        }
        
        SetupFlame();
        SetupLight();
    }
    
    public void ApplyRandomVariation()
    {
        float rateMult = UnityEngine.Random.Range(1f - framerateVariation, 1f + framerateVariation);
        runtimeFramerate = animationFramerate * rateMult;
        
        float flickerMult = UnityEngine.Random.Range(1f - flickerSpeedVariation, 1f + flickerSpeedVariation);
        runtimeFlickerSpeed = lightFlickerSpeed * flickerMult;
        
        float intensityMult = UnityEngine.Random.Range(1f - intensityVariation, 1f + intensityVariation);
        runtimeIntensity = lightIntensity * intensityMult;
        
        runtimeJitterSpeed = 10f * UnityEngine.Random.Range(1f - jitterSpeedVariation, 1f + jitterSpeedVariation);
        
        flickerPhase = UnityEngine.Random.Range(0f, 100f);
        jitterPhase = UnityEngine.Random.Range(0f, 100f);
    }
    
    private void SetupFlame()
    {
        GameObject flameObj = new GameObject("Flame");
        flameObj.transform.SetParent(transform);
        flameObj.transform.localPosition = flameOffset;
        
        flameRenderer = flameObj.AddComponent<SpriteRenderer>();
        
        if (flameFrames != null && flameFrames.Length > 0)
        {
            flameRenderer.sprite = flameFrames[0];
            frameCount = flameFrames.Length;
            
            if (randomizeFrameStart)
            {
                currentFrame = UnityEngine.Random.Range(0, frameCount);
                flameRenderer.sprite = flameFrames[currentFrame];
            }
        }
        
        flameRenderer.sortingLayerName = "Effects";
        flameRenderer.sortingOrder = 20;
        flameRenderer.transform.localScale = Vector3.one * flameSize;
    }
    
    private void SetupLight()
    {
        GameObject lightObj = new GameObject("Light");
        lightObj.transform.SetParent(transform);
        lightObj.transform.localPosition = flameOffset;
        
        // Find Light2D type via reflection (Unity 6 compatible)
        light2DType = System.Type.GetType("UnityEngine.Rendering.Universal.Light2D, Unity.RenderPipelines.Universal.Runtime");
        
        if (light2DType == null)
        {
            light2DType = System.Type.GetType("UnityEngine.Rendering.Light2D, Unity.RenderPipelines.Universal.Runtime");
        }
        
        if (light2DType != null)
        {
            candleLight = lightObj.AddComponent(light2DType);
            
            // Cache property references
            intensityProperty = light2DType.GetProperty("intensity");
            colorProperty = light2DType.GetProperty("color");
            rangeProperty = light2DType.GetProperty("range");
            
            // Set initial values
            if (intensityProperty != null)
                intensityProperty.SetValue(candleLight, runtimeIntensity > 0 ? runtimeIntensity : lightIntensity);
            if (colorProperty != null)
                colorProperty.SetValue(candleLight, lightColor);
            if (rangeProperty != null)
                rangeProperty.SetValue(candleLight, lightRange);
        }
        else
        {
            Debug.LogWarning("CandleFlame: Light2D not found. URP 2D Lighting may not be installed or enabled.");
        }
    }
    
    private void Update()
    {
        // Animate flame - added flameRenderer != null check
        if (frameCount > 1 && flameRenderer != null)
        {
            frameTimer += Time.deltaTime;
            float frameDuration = 1f / (runtimeFramerate > 0 ? runtimeFramerate : animationFramerate);
        
            if (frameTimer >= frameDuration)
            {
                frameTimer -= frameDuration;
                currentFrame = (currentFrame + 1) % frameCount;
                flameRenderer.sprite = flameFrames[currentFrame];
            }
        }
    
        // Position jitter - added flameRenderer != null check
        if (positionJitter > 0 && flameRenderer != null)
        {
            float jitterX = Mathf.PerlinNoise(Time.time * runtimeJitterSpeed, jitterPhase) * 2f - 1f;
            float jitterY = Mathf.PerlinNoise(jitterPhase, Time.time * runtimeJitterSpeed * 0.7f) * 2f - 1f;
            flameRenderer.transform.localPosition = flameOffset + new Vector2(jitterX, jitterY) * positionJitter;
        }
        
        // Light flicker
        if (lightFlickerIntensity > 0 && candleLight != null && intensityProperty != null)
        {
            float flicker = Mathf.PerlinNoise(Time.time * (runtimeFlickerSpeed > 0 ? runtimeFlickerSpeed : lightFlickerSpeed) + flickerPhase, 100f);
            float baseIntensity = runtimeIntensity > 0 ? runtimeIntensity : lightIntensity;
            float newIntensity = baseIntensity * (1f - lightFlickerIntensity + flicker * lightFlickerIntensity);
            intensityProperty.SetValue(candleLight, newIntensity);
        }
    }
    
    public void SetFlameActive(bool active)
    {
        flameRenderer.enabled = active;
        if (candleLight != null) candleLight.gameObject.SetActive(active);
    }
    
    public void Extinguish() => SetFlameActive(false);
    public void Ignite() => SetFlameActive(true);
    
    [ContextMenu("Refresh Variation")]
    public void EditorRefreshVariation()
    {
        ApplyRandomVariation();
    }
    
    [ContextMenu("Show Base Sprite")]
    public void EditorShowBaseSprite()
    {
        if (baseSpriteRenderer != null) baseSpriteRenderer.enabled = true;
    }

    [ContextMenu("Hide Base Sprite")]
    public void EditorHideBaseSprite()
    {
        if (baseSpriteRenderer != null) baseSpriteRenderer.enabled = false;
    }
    
}