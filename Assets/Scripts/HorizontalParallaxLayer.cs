using UnityEngine;

[DisallowMultipleComponent]
public sealed class HorizontalParallaxLayer : MonoBehaviour
{
    public Transform cameraTransform;
    [Min(0f)]
    public float screenTravelFactor = 1.14f;
    [Min(0f)]
    public float verticalTravelFactor = 1f;
    public bool driveTransformAtRuntime;
    public bool captureAnchorsOnEnable = true;
    public bool respectExternalTransformChanges = true;

    private Vector3 layerAnchorPosition;
    private Vector3 cameraAnchorPosition;
    private Vector3 lastDrivenPosition;
    private bool hasDrivenPosition;

    private void OnEnable()
    {
        if (cameraTransform == null && Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }

        if (captureAnchorsOnEnable)
        {
            CaptureAnchors();
        }
    }

    private void LateUpdate()
    {
        if (!driveTransformAtRuntime)
        {
            return;
        }

        if (cameraTransform == null)
        {
            return;
        }

        Vector3 worldOffset = CalculateWorldOffset();

        if (respectExternalTransformChanges && hasDrivenPosition &&
            (transform.position - lastDrivenPosition).sqrMagnitude > 0.0001f)
        {
            layerAnchorPosition = transform.position - worldOffset;
        }

        lastDrivenPosition = layerAnchorPosition + worldOffset;
        transform.position = lastDrivenPosition;
    }

    [ContextMenu("Capture Anchors")]
    public void CaptureAnchors()
    {
        layerAnchorPosition = transform.position;
        cameraAnchorPosition = cameraTransform != null ? cameraTransform.position : Vector3.zero;
        lastDrivenPosition = transform.position;
        hasDrivenPosition = true;
    }

    private Vector3 CalculateWorldOffset()
    {
        Vector3 cameraDelta = cameraTransform.position - cameraAnchorPosition;
        return new Vector3(
            (1f - screenTravelFactor) * cameraDelta.x,
            (1f - verticalTravelFactor) * cameraDelta.y,
            0f);
    }
}
