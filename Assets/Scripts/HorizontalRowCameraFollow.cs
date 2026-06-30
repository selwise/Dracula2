using UnityEngine;

[ExecuteAlways]
[DisallowMultipleComponent]
[RequireComponent(typeof(Camera))]
public sealed class HorizontalRowCameraFollow : MonoBehaviour
{
    public Transform target;
    public Vector2 cameraMin = new Vector2(-10f, 0f);
    public Vector2 cameraMax = new Vector2(10f, 0f);
    public Vector3 offset = new Vector3(0f, 0.82f, -10f);
    [Min(0f)]
    public float followSpeed = 12f;
    public bool followInEditMode = true;
    public bool snapInEditMode = true;

    private void LateUpdate()
    {
        if (target == null)
        {
            return;
        }

        if (!Application.isPlaying && !followInEditMode)
        {
            return;
        }

        Vector3 desiredPosition = new Vector3(
            Mathf.Clamp(target.position.x + offset.x, cameraMin.x, cameraMax.x),
            Mathf.Clamp(target.position.y + offset.y, cameraMin.y, cameraMax.y),
            offset.z);

        if (!Application.isPlaying || snapInEditMode || followSpeed <= 0f)
        {
            transform.position = desiredPosition;
            return;
        }

        float t = 1f - Mathf.Exp(-followSpeed * Time.deltaTime);
        transform.position = Vector3.Lerp(transform.position, desiredPosition, t);
    }

    public void SnapToTarget()
    {
        if (target == null)
        {
            return;
        }

        transform.position = new Vector3(
            Mathf.Clamp(target.position.x + offset.x, cameraMin.x, cameraMax.x),
            Mathf.Clamp(target.position.y + offset.y, cameraMin.y, cameraMax.y),
            offset.z);
    }
}
