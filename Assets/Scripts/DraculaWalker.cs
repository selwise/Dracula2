using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(SpriteRenderer))]
public sealed class DraculaWalker : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 2.45f;
    public Vector2 minBounds = new Vector2(-3.75f, -2.05f);
    public Vector2 maxBounds = new Vector2(5.2f, 1.35f);

    [Header("Animation")]
    public SpriteRenderer spriteRenderer;
    public Transform visualRoot;
    public Sprite[] walkDown;
    public Sprite[] walkUp;
    public Sprite[] walkSide;
    public float frameTime = 0.16f;

    private enum Facing
    {
        Down,
        Up,
        Side
    }

    private Facing facing = Facing.Down;
    private Vector2 moveInput;
    private Vector3 visualStartLocalPosition;
    private int currentFrame;
    private float frameTimer;
    private int sideSign = 1;

    private void Awake()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        if (visualRoot != null && visualRoot != transform)
        {
            visualStartLocalPosition = visualRoot.localPosition;
        }

        ApplyFrame(0);
    }

    private void Update()
    {
        ReadKeyboard();
        Move();
        UpdateFacing();
        Animate();
        UpdateSortOrder();
    }

    private void ReadKeyboard()
    {
        moveInput = Vector2.zero;
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
        {
            return;
        }

        if (keyboard.aKey.isPressed)
        {
            moveInput.x -= 1f;
        }

        if (keyboard.dKey.isPressed)
        {
            moveInput.x += 1f;
        }

        if (keyboard.sKey.isPressed)
        {
            moveInput.y -= 1f;
        }

        if (keyboard.wKey.isPressed)
        {
            moveInput.y += 1f;
        }

        moveInput = Vector2.ClampMagnitude(moveInput, 1f);
    }

    private void Move()
    {
        if (moveInput.sqrMagnitude <= 0.0001f)
        {
            return;
        }

        Vector3 position = transform.position;
        position += new Vector3(moveInput.x, moveInput.y, 0f) * (moveSpeed * Time.deltaTime);
        position.x = Mathf.Clamp(position.x, minBounds.x, maxBounds.x);
        position.y = Mathf.Clamp(position.y, minBounds.y, maxBounds.y);
        transform.position = position;
    }

    private void UpdateFacing()
    {
        if (moveInput.sqrMagnitude <= 0.0001f)
        {
            return;
        }

        if (Mathf.Abs(moveInput.x) > Mathf.Abs(moveInput.y))
        {
            facing = Facing.Side;
            sideSign = moveInput.x < 0f ? -1 : 1;
            return;
        }

        facing = moveInput.y > 0f ? Facing.Up : Facing.Down;
    }

    private void Animate()
    {
        bool moving = moveInput.sqrMagnitude > 0.0001f;
        Sprite[] frames = GetFrames();
        if (frames == null || frames.Length == 0)
        {
            return;
        }

        if (moving)
        {
            frameTimer += Time.deltaTime;
            if (frameTimer >= frameTime)
            {
                frameTimer = 0f;
                currentFrame = (currentFrame + 1) % frames.Length;
            }
        }
        else
        {
            frameTimer = 0f;
            currentFrame = 0;
        }

        ApplyFrame(Mathf.Clamp(currentFrame, 0, frames.Length - 1));

        if (visualRoot != null && visualRoot != transform)
        {
            float bob = moving ? (currentFrame == 0 ? 0.025f : -0.01f) : Mathf.Sin(Time.time * 2.2f) * 0.004f;
            visualRoot.localPosition = visualStartLocalPosition + new Vector3(0f, bob, 0f);
        }
    }

    private Sprite[] GetFrames()
    {
        switch (facing)
        {
            case Facing.Up:
                return walkUp;
            case Facing.Side:
                return walkSide;
            default:
                return walkDown;
        }
    }

    private void ApplyFrame(int frameIndex)
    {
        if (spriteRenderer == null)
        {
            return;
        }

        Sprite[] frames = GetFrames();
        if (frames != null && frames.Length > 0)
        {
            spriteRenderer.sprite = frames[Mathf.Clamp(frameIndex, 0, frames.Length - 1)];
        }

        spriteRenderer.flipX = facing == Facing.Side && sideSign < 0;
    }

    private void UpdateSortOrder()
    {
        if (spriteRenderer == null)
        {
            return;
        }

        spriteRenderer.sortingOrder = 200 - Mathf.RoundToInt(transform.position.y * 20f);
    }
}
