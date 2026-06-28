// ============================================================
// File: Assets/Scripts/RoomDustParticles.cs
// Author: AI Assistant
// Description: Atmospheric dust particles for pixel art rooms
// ============================================================

using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Creates and manages atmospheric dust particles in a room.
/// Single-pixel particles with subtle shimmer for pixel art aesthetics.
/// Matches the visual style of CryptAmbientAnimator (Perlin noise-based).
/// </summary>
public sealed class RoomDustParticles : MonoBehaviour
{
    [Header("Particle Count")]
    [Range(50, 500)]
    public int particleCount = 150;
    
    [Header("Particle Appearance")]
    [Range(0.5f, 3f)]
    public float particleSize = 1f;
    [Range(0.05f, 0.3f)]
    public float baseAlpha = 0.12f;
    [Range(0.5f, 2f)]
    public float alphaVariation = 0.8f;
    
    [Header("Shimmer Effect")]
    [Range(0.2f, 2f)]
    public float shimmerSpeed = 0.6f;
    [Range(0.3f, 1.0f)]
    public float shimmerMinAlpha = 0.6f;  // ← NEW: Minimum brightness (0.6 = 60%)
    [Range(0.5f, 3f)]
    public float shimmerScale = 1.5f;
    
    [Header("Movement")]
    [Range(0f, 0.5f)]
    public float driftSpeed = 0.08f;
    [Range(0f, 0.3f)]
    public float driftAmplitude = 0.15f;
    [Range(0.01f, 0.1f)]
    public float verticalRiseSpeed = 0.02f;
    
    [Header("Room Bounds")]
    public Vector2 boundsMin = new Vector2(-10f, -5f);
    public Vector2 boundsMax = new Vector2(10f, 5f);
    
    [Header("References")]
    public Sprite dustSprite;
    
    private List<DustParticle> particles = new List<DustParticle>();
    private Transform particlesContainer;
    
    private struct DustParticle
    {
        public Vector3 position;
        public float baseAlpha;
        public float shimmerOffset;
        public float driftOffset;
        public float verticalOffset;
        public SpriteRenderer renderer;
    }
    
    private void Awake()
    {
        InitializeParticles();
    }
    
    private void InitializeParticles()
    {
        particlesContainer = new GameObject("DustParticles").transform;
        particlesContainer.SetParent(transform);
        particlesContainer.localPosition = Vector3.zero;
        
        if (dustSprite == null)
        {
            dustSprite = CreateDefaultDustSprite();
        }
        
        particles = new List<DustParticle>(particleCount);
        
        for (int i = 0; i < particleCount; i++)
        {
            DustParticle particle = CreateParticle(i);
            particles.Add(particle);
        }
    }
    
    private DustParticle CreateParticle(int index)
    {
        GameObject particleObj = new GameObject($"Dust_{index}");
        particleObj.transform.SetParent(particlesContainer);
    
        Vector3 position = new Vector3(
            Random.Range(boundsMin.x, boundsMax.x),
            Random.Range(boundsMin.y, boundsMax.y),
            Random.Range(-0.5f, 0.5f)
        );
        particleObj.transform.position = position;
    
        SpriteRenderer renderer = particleObj.AddComponent<SpriteRenderer>();
        renderer.sprite = dustSprite;
        renderer.sortingLayerName = "Effects";
        renderer.sortingOrder = 10;
    
        // FIX: Clamp alpha variation to prevent near-zero alpha
        float alphaRangeMin = Mathf.Max(0.5f, 1f - alphaVariation);
        float alphaRangeMax = 1f + alphaVariation;
        float alpha = baseAlpha * Random.Range(alphaRangeMin, alphaRangeMax);
    
        Color color = new Color(1f, 1f, 1f, alpha);
        renderer.color = color;
        renderer.transform.localScale = Vector3.one * particleSize * Random.Range(0.8f, 1.2f);
    
        return new DustParticle
        {
            position = position,
            baseAlpha = alpha,
            shimmerOffset = Random.Range(0f, 100f),
            driftOffset = Random.Range(0f, 100f),
            verticalOffset = Random.Range(0f, 100f),
            renderer = renderer
        };
    }
    
    private void Update()
    {
        float time = Time.time;
    
        for (int i = 0; i < particles.Count; i++)
        {
            DustParticle p = particles[i];
        
            // Shimmer using Perlin noise (matches CryptAmbientAnimator style)
            float shimmerNoise = Mathf.PerlinNoise(
                p.shimmerOffset, 
                time * shimmerSpeed * shimmerScale
            );
        
            // KEY FIX: Lerp between 60%-100% of base alpha (never disappears)
            float alphaMultiplier = Mathf.Lerp(shimmerMinAlpha, 1.0f, shimmerNoise);
            float currentAlpha = p.baseAlpha * alphaMultiplier;
        
            Color color = p.renderer.color;
            color.a = Mathf.Clamp01(currentAlpha);
            p.renderer.color = color;
        
            // Subtle drift movement
            float driftX = Mathf.Sin(time * driftSpeed + p.driftOffset) * driftAmplitude;
            float driftY = Mathf.Cos(time * driftSpeed * 0.7f + p.driftOffset) * driftAmplitude;
            float verticalRise = Mathf.Sin(time * verticalRiseSpeed + p.verticalOffset) * 0.01f;
        
            Vector3 newPos = p.position + new Vector3(driftX, driftY + verticalRise, 0f);
            p.renderer.transform.position = newPos;
        
            // Wrap around bounds (seamless looping)
            newPos = WrapPosition(newPos);
            p.position = newPos;
        }
    }
    
    private Vector3 WrapPosition(Vector3 position)
    {
        float width = boundsMax.x - boundsMin.x;
        float height = boundsMax.y - boundsMin.y;
        
        if (position.x > boundsMax.x) position.x = boundsMin.x;
        if (position.x < boundsMin.x) position.x = boundsMax.x;
        if (position.y > boundsMax.y) position.y = boundsMin.y;
        if (position.y < boundsMin.y) position.y = boundsMax.y;
        
        return position;
    }
    
    private Sprite CreateDefaultDustSprite()
    {
        // FIX: 2x2 pixels at 128 PPU = much smaller in world space
        Texture2D texture = new Texture2D(2, 2, TextureFormat.ARGB32, false);
    
        // Fill with white
        for (int x = 0; x < 2; x++)
        {
            for (int y = 0; y < 2; y++)
            {
                texture.SetPixel(x, y, Color.white);
            }
        }
        texture.Apply();
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;
    
        Sprite sprite = Sprite.Create(
            texture,
            new Rect(0, 0, 2, 2),
            new Vector2(0.5f, 0.5f),
            128f  // ← CHANGED: 128 PPU instead of 64 (makes sprite 2x smaller)
        );
        sprite.name = "DustPixel2x2";
    
        return sprite;
    }
    private void OnDestroy()
    {
        if (particlesContainer != null)
        {
            Destroy(particlesContainer.gameObject);
        }
    }
    
    public void SetBoundsFromRoom(Rect roomBounds)
    {
        boundsMin = new Vector2(roomBounds.xMin, roomBounds.yMin);
        boundsMax = new Vector2(roomBounds.xMax, roomBounds.yMax);
        
        if (particles.Count > 0)
        {
            ClearAndReinitialize();
        }
    }
    
    private void ClearAndReinitialize()
    {
        if (particlesContainer != null)
        {
            Destroy(particlesContainer.gameObject);
        }
        particles.Clear();
        InitializeParticles();
    }
    
    public void SetParticleCount(int count)
    {
        particleCount = Mathf.Clamp(count, 50, 500);
        ClearAndReinitialize();
    }
}