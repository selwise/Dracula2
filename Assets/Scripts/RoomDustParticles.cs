// ============================================================
// File: Assets/Scripts/RoomDustParticles.cs
// Description: Atmospheric dust particles using pre-sliced spritesheet
// ============================================================

using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Creates and manages atmospheric dust particles using a pre-sliced spritesheet.
/// Supports animated frames with controllable framerate.
/// </summary>
public sealed class RoomDustParticles : MonoBehaviour
{
    [Header("Particle Count")]
    [Range(50, 500)]
    public int particleCount = 200;
    
    [Header("Particle Appearance")]
    [Range(0.1f, 2f)]
    public float particleSize = 0.8f;
    [Range(0.2f, 0.6f)]
    public float baseBrightness = 0.4f;
    [Range(0.1f, 0.4f)]
    public float brightnessVariation = 0.2f;
    
    [Header("Animation")]
    [Range(1, 24)]
    public float animationFramerate = 8f;
    public bool randomizeFrameOffset = true;
    
    [Header("Movement")]
    [Range(0f, 2f)]           // ← Was [0f, 0.3f] - 6x more range!
    public float driftSpeed = 0.15f;
    [Range(0f, 1f)]           // ← Was [0f, 0.2f] - 5x more range!
    public float driftAmplitude = 0.2f;
    [Range(0f, 0.5f)]         // ← Was [0.01f, 0.1f] - 5x more range!
    public float verticalRiseSpeed = 0.05f;
    
    [Header("Room Bounds")]
    public Vector2 boundsMin = new Vector2(-10f, -5f);
    public Vector2 boundsMax = new Vector2(10f, 5f);
    
    [Header("References")]
    public Sprite[] dustFrames;  // ← Assign all 10 sliced frames here
    
    private List<DustParticle> particles = new List<DustParticle>();
    private Transform particlesContainer;
    
    private struct DustParticle
    {
        public Vector3 position;
        public float baseBrightness;
        public float driftOffset;
        public float verticalOffset;
        public float frameOffset;
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
        
        // Fallback if no frames assigned
        if (dustFrames == null || dustFrames.Length == 0)
        {
            Debug.LogWarning("DustFrames not assigned! Using fallback sprite.");
            dustFrames = new Sprite[] { CreateDefaultDustSprite() };
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
        renderer.sprite = dustFrames[0];
        renderer.sortingLayerName = "Effects";
        renderer.sortingOrder = 10;
        
        // Brightness controls visibility (not alpha) - works on any background
        float brightness = baseBrightness * Random.Range(1f - brightnessVariation, 1f + brightnessVariation);
        Color color = new Color(brightness, brightness, brightness, 1f);
        renderer.color = color;
        renderer.transform.localScale = Vector3.one * particleSize * Random.Range(0.8f, 1.2f);
        
        return new DustParticle
        {
            position = position,
            baseBrightness = brightness,
            driftOffset = Random.Range(0f, 100f),
            verticalOffset = Random.Range(0f, 100f),
            frameOffset = randomizeFrameOffset ? Random.Range(0f, dustFrames.Length) : 0f,
            renderer = renderer
        };
    }
    
    private void Update()
    {
        float time = Time.time;
        int frameCount = dustFrames.Length;
        
        for (int i = 0; i < particles.Count; i++)
        {
            DustParticle p = particles[i];
            
            // Animate spritesheet frames
            if (frameCount > 1)
            {
                float animTime = time * animationFramerate + p.frameOffset;
                int currentFrame = Mathf.FloorToInt(animTime) % frameCount;
                currentFrame = Mathf.Abs(currentFrame); // Ensure positive
                p.renderer.sprite = dustFrames[currentFrame];
            }
            
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
        Texture2D texture = new Texture2D(2, 2, TextureFormat.ARGB32, false);
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
            128f
        );
        sprite.name = "DustDefault";
        
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
}