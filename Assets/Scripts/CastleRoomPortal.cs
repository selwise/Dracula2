using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public sealed class CastleRoomPortal : MonoBehaviour
{
    public AdventureLoopController loopController;
    public Transform destination;
    public string destinationRoomName = "Unknown Room";
    public Vector2 destinationMinBounds;
    public Vector2 destinationMaxBounds;
    public bool requireActiveActor = true;
    public bool snapCamera = true;
    public float portalCooldown = 0.45f;

    private void Awake()
    {
        Collider2D trigger = GetComponent<Collider2D>();
        trigger.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        AdventureActor actor = other.GetComponentInParent<AdventureActor>();
        if (actor == null)
        {
            return;
        }

        if (!actor.CanUsePortal)
        {
            return;
        }

        if (requireActiveActor && loopController != null && !loopController.IsActiveActor(actor))
        {
            return;
        }

        Travel(actor);
    }

    public void Travel(AdventureActor actor)
    {
        if (actor == null || destination == null)
        {
            return;
        }

        Vector3 target = destination.position;
        target.z = actor.transform.position.z;

        Rigidbody2D body = actor.GetComponent<Rigidbody2D>();
        if (body != null)
        {
            body.linearVelocity = Vector2.zero;
            body.position = new Vector2(target.x, target.y);
        }

        actor.transform.position = target;
        actor.roomName = destinationRoomName;
        actor.LockPortals(portalCooldown);

        if (actor.walker != null)
        {
            actor.walker.minBounds = destinationMinBounds;
            actor.walker.maxBounds = destinationMaxBounds;
        }

        if (snapCamera && loopController != null)
        {
            loopController.SnapCameraToActiveActor();
        }
    }
}
