using UnityEngine;

[DisallowMultipleComponent]
public sealed class HorizontalParallaxLayer : MonoBehaviour
{
    public Transform cameraTransform;
    [Min(0f)]
    public float screenTravelFactor = 1.14f;
    [Min(0f)]
    public float verticalTravelFactor = 1f;
    public bool captureAnchorsOnEnable = true;

    private Vector3 layerAnchorPosition;
    private Vector3 cameraAnchorPosition;

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
        if (cameraTransform == null)
        {
            return;
        }

        Vector3 cameraDelta = cameraTransform.position - cameraAnchorPosition;
        Vector3 worldOffset = new Vector3(
            (1f - screenTravelFactor) * cameraDelta.x,
            (1f - verticalTravelFactor) * cameraDelta.y,
            0f);

        transform.position = layerAnchorPosition + worldOffset;
    }

    [ContextMenu("Capture Anchors")]
    public void CaptureAnchors()
    {
        layerAnchorPosition = transform.position;
        cameraAnchorPosition = cameraTransform != null ? cameraTransform.position : Vector3.zero;
    }
}
