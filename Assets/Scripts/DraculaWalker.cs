using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Rigidbody2D))]
public sealed class DraculaWalker : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 2.45f;
    public Vector2 minBounds = new Vector2(-3.75f, -1.45f);
    public Vector2 maxBounds = new Vector2(5.2f, 1.35f);
    public Vector2[] walkBoundary;

    [Header("Animation")]
    public SpriteRenderer spriteRenderer;
    public Rigidbody2D body;
    public Transform visualRoot;
    public Sprite[] walkDown;
    public Sprite[] walkUp;
    public Sprite[] walkSide;
    public Sprite[] idleDown;
    public Sprite[] idleUp;
    public Sprite[] idleSide;
    public Sprite[] toIdleDown;
    public float frameTime = 0.16f;
    public float sideFrameTime = 0.08f;
    public float upFrameTime = 0.108f;
    public float idleFrameTime = 0.18f;
    public float toIdleFrameTime = 0.10f;

    [Header("Depth Sorting")]
    public int baseSortingOrder = 280;
    public float ySortMultiplier = 28f;
    public int minSortingOrder = 180;
    public int maxSortingOrder = 340;

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
    private bool wasMoving;
    private bool playingToIdle;

    private void Awake()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        if (body == null)
        {
            body = GetComponent<Rigidbody2D>();
        }

        if (body != null)
        {
            body.gravityScale = 0f;
            body.freezeRotation = true;
        }

        if (visualRoot != null && visualRoot != transform)
        {
            visualStartLocalPosition = visualRoot.localPosition;
        }

        ApplyFrame(GetIdleFrames(), 0, false);
        UpdateSortOrder();
    }

    private void Update()
    {
        ReadKeyboard();
        UpdateFacing();
        Animate();
        UpdateSortOrder();
    }

    private void FixedUpdate()
    {
        Move(Time.fixedDeltaTime);
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

    private void Move(float deltaTime)
    {
        if (moveInput.sqrMagnitude <= 0.0001f)
        {
            return;
        }

        Vector3 previousPosition = transform.position;
        Vector3 delta = new Vector3(moveInput.x, moveInput.y, 0f) * (moveSpeed * deltaTime);
        Vector3 position = ClampToRectBounds(previousPosition + delta);
        position = ClampToWalkBoundary(previousPosition, position, delta);

        if (body != null && Application.isPlaying)
        {
            body.MovePosition(new Vector2(position.x, position.y));
        }
        else
        {
            transform.position = position;
        }
    }

    private Vector3 ClampToRectBounds(Vector3 position)
    {
        position.x = Mathf.Clamp(position.x, minBounds.x, maxBounds.x);
        position.y = Mathf.Clamp(position.y, minBounds.y, maxBounds.y);
        return position;
    }

    private Vector3 ClampToWalkBoundary(Vector3 previousPosition, Vector3 position, Vector3 delta)
    {
        if (walkBoundary == null || walkBoundary.Length < 3)
        {
            return position;
        }

        Vector2 point = new Vector2(position.x, position.y);
        if (IsPointInsideBoundary(point))
        {
            return position;
        }

        if (!IsPointInsideBoundary(new Vector2(previousPosition.x, previousPosition.y)))
        {
            Vector2 closest = ClosestPointOnBoundary(point);
            position.x = closest.x;
            position.y = closest.y;
            return position;
        }

        Vector3 xOnly = ClampToRectBounds(previousPosition + new Vector3(delta.x, 0f, 0f));
        if (IsPointInsideBoundary(new Vector2(xOnly.x, xOnly.y)))
        {
            return xOnly;
        }

        Vector3 yOnly = ClampToRectBounds(previousPosition + new Vector3(0f, delta.y, 0f));
        if (IsPointInsideBoundary(new Vector2(yOnly.x, yOnly.y)))
        {
            return yOnly;
        }

        return previousPosition;
    }

    private bool IsPointInsideBoundary(Vector2 point)
    {
        bool inside = false;
        int count = walkBoundary.Length;
        for (int i = 0, j = count - 1; i < count; j = i++)
        {
            Vector2 a = walkBoundary[i];
            Vector2 b = walkBoundary[j];
            bool crosses = (a.y > point.y) != (b.y > point.y);
            if (crosses)
            {
                float xAtY = (b.x - a.x) * (point.y - a.y) / (b.y - a.y) + a.x;
                if (point.x < xAtY)
                {
                    inside = !inside;
                }
            }
        }

        return inside;
    }

    private Vector2 ClosestPointOnBoundary(Vector2 point)
    {
        Vector2 closest = walkBoundary[0];
        float closestSqrDistance = float.MaxValue;
        for (int i = 0; i < walkBoundary.Length; i++)
        {
            Vector2 a = walkBoundary[i];
            Vector2 b = walkBoundary[(i + 1) % walkBoundary.Length];
            Vector2 candidate = ClosestPointOnSegment(point, a, b);
            float sqrDistance = (candidate - point).sqrMagnitude;
            if (sqrDistance < closestSqrDistance)
            {
                closestSqrDistance = sqrDistance;
                closest = candidate;
            }
        }

        return closest;
    }

    private static Vector2 ClosestPointOnSegment(Vector2 point, Vector2 a, Vector2 b)
    {
        Vector2 segment = b - a;
        float lengthSqr = segment.sqrMagnitude;
        if (lengthSqr <= 0.0001f)
        {
            return a;
        }

        float t = Vector2.Dot(point - a, segment) / lengthSqr;
        return a + segment * Mathf.Clamp01(t);
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
        if (moving)
        {
            if (!wasMoving)
            {
                playingToIdle = false;
                currentFrame = 0;
                frameTimer = 0f;
            }

            wasMoving = true;
            Sprite[] frames = GetMovementFrames();
            if (frames == null || frames.Length == 0)
            {
                return;
            }

            frameTimer += Time.deltaTime;
            float activeFrameTime = GetMovementFrameTime();
            if (frameTimer >= activeFrameTime)
            {
                frameTimer = 0f;
                currentFrame = (currentFrame + 1) % frames.Length;
            }

            ApplyFrame(frames, Mathf.Clamp(currentFrame, 0, frames.Length - 1), facing == Facing.Side && sideSign < 0);
        }
        else
        {
            if (wasMoving)
            {
                playingToIdle = facing == Facing.Down && toIdleDown != null && toIdleDown.Length > 0;
                currentFrame = 0;
                frameTimer = 0f;
                wasMoving = false;
            }

            Sprite[] frames = playingToIdle ? toIdleDown : GetIdleFrames();
            if (frames == null || frames.Length == 0)
            {
                frames = GetMovementFrames();
            }

            if (frames == null || frames.Length == 0)
            {
                return;
            }

            frameTimer += Time.deltaTime;
            float activeFrameTime = playingToIdle ? toIdleFrameTime : idleFrameTime;
            if (frameTimer >= activeFrameTime)
            {
                frameTimer = 0f;
                currentFrame++;
                if (playingToIdle && currentFrame >= frames.Length)
                {
                    playingToIdle = false;
                    currentFrame = 0;
                    frames = GetIdleFrames();
                }

                if (!playingToIdle)
                {
                    currentFrame %= frames.Length;
                }
            }

            ApplyFrame(frames, Mathf.Clamp(currentFrame, 0, frames.Length - 1), !playingToIdle && GetIdleFlipX());
        }

        if (visualRoot != null && visualRoot != transform)
        {
            bool usesExtractedDownCycle = moving && facing == Facing.Down && walkDown != null && walkDown.Length > 6;
            float bob = usesExtractedDownCycle || !moving ? 0f : currentFrame % 2 == 0 ? 0.018f : -0.01f;
            visualRoot.localPosition = visualStartLocalPosition + new Vector3(0f, bob, 0f);
        }
    }

    private Sprite[] GetMovementFrames()
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

    private float GetMovementFrameTime()
    {
        switch (facing)
        {
            case Facing.Up:
                return upFrameTime;
            case Facing.Side:
                return sideFrameTime;
            default:
                return frameTime;
        }
    }

    private Sprite[] GetIdleFrames()
    {
        switch (facing)
        {
            case Facing.Up:
                if (idleUp != null && idleUp.Length > 0)
                {
                    return idleUp;
                }

                return walkUp;
            case Facing.Side:
                if (idleSide != null && idleSide.Length > 0)
                {
                    return idleSide;
                }

                return walkSide;
            default:
                return idleDown != null && idleDown.Length > 0 ? idleDown : walkDown;
        }
    }

    private bool GetIdleFlipX()
    {
        return facing == Facing.Side && sideSign < 0;
    }

    private void ApplyFrame(Sprite[] frames, int frameIndex, bool flipX)
    {
        if (spriteRenderer == null)
        {
            return;
        }

        if (frames != null && frames.Length > 0)
        {
            spriteRenderer.sprite = frames[Mathf.Clamp(frameIndex, 0, frames.Length - 1)];
        }

        spriteRenderer.flipX = flipX;
    }

    private void UpdateSortOrder()
    {
        if (spriteRenderer == null)
        {
            return;
        }

        int order = baseSortingOrder - Mathf.RoundToInt(transform.position.y * ySortMultiplier);
        spriteRenderer.sortingOrder = Mathf.Clamp(order, minSortingOrder, maxSortingOrder);
    }
}
