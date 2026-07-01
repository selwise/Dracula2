using UnityEngine;

[ExecuteAlways]
[DisallowMultipleComponent]
[RequireComponent(typeof(SpriteRenderer))]
public sealed class SpriteFrameAnimator : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;
    public Sprite[] frames = new Sprite[0];
    [Range(1f, 60f)] public float framesPerSecond = 12f;
    public bool playOnEnable = true;
    public bool loop = true;
    public bool animateInEditMode;

    private bool isPlaying;
    private int frameIndex;
    private float frameTimer;

#if UNITY_EDITOR
    private double lastEditorTime;
#endif

    public void Configure(SpriteRenderer renderer, Sprite[] animationFrames, float framerate)
    {
        spriteRenderer = renderer != null ? renderer : GetComponent<SpriteRenderer>();
        frames = animationFrames ?? new Sprite[0];
        framesPerSecond = Mathf.Max(1f, framerate);
        ApplyFrame(0);
    }

    public void Play()
    {
        isPlaying = true;
    }

    public void Stop()
    {
        isPlaying = false;
    }

    private void Reset()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnEnable()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        isPlaying = playOnEnable;
        frameTimer = 0f;
#if UNITY_EDITOR
        lastEditorTime = UnityEditor.EditorApplication.timeSinceStartup;
#endif
        ApplyFrame(frameIndex);
    }

    private void Update()
    {
        if (!isPlaying || frames == null || frames.Length <= 1 || spriteRenderer == null)
        {
            return;
        }

        float deltaTime = Time.deltaTime;
        if (!Application.isPlaying)
        {
            if (!animateInEditMode)
            {
                return;
            }

#if UNITY_EDITOR
            double editorTime = UnityEditor.EditorApplication.timeSinceStartup;
            deltaTime = (float)(editorTime - lastEditorTime);
            lastEditorTime = editorTime;
            UnityEditor.SceneView.RepaintAll();
#else
            return;
#endif
        }

        float frameDuration = 1f / Mathf.Max(1f, framesPerSecond);
        frameTimer += deltaTime;
        while (frameTimer >= frameDuration)
        {
            frameTimer -= frameDuration;
            frameIndex++;

            if (frameIndex >= frames.Length)
            {
                if (!loop)
                {
                    frameIndex = frames.Length - 1;
                    isPlaying = false;
                    break;
                }

                frameIndex = 0;
            }

            ApplyFrame(frameIndex);
        }
    }

    private void ApplyFrame(int index)
    {
        if (frames == null || frames.Length == 0 || spriteRenderer == null)
        {
            return;
        }

        frameIndex = Mathf.Clamp(index, 0, frames.Length - 1);
        spriteRenderer.sprite = frames[frameIndex];
    }
}
