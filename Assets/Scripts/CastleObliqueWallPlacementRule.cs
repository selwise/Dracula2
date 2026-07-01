using System;
using UnityEngine;

[Serializable]
public sealed class CastleObliqueWallPlacementRule
{
    public string ruleName = "Castle Proper East Gallery Left Return";
    public Vector3 perspectiveScale = new Vector3(1.8093f, 1.0877f, 1.1326f);
    public float yawDegrees = -66.0494f;
    public float z = 1.12f;
    public float backWallCenterX = -4.95f;
    public float backWallOffsetX = -0.01f;
    public float frontFloorLipCenterY = -4.17f;
    public float frontFloorLipHeight = 0.38f;
    public float bottomOverscan = 0.167f;

    public static CastleObliqueWallPlacementRule CastleProperEastGalleryLeftReturn()
    {
        return new CastleObliqueWallPlacementRule();
    }

    public CastleObliqueWallPlacementRule Clone()
    {
        CastleObliqueWallPlacementRule clone = new CastleObliqueWallPlacementRule();
        clone.ruleName = ruleName;
        clone.perspectiveScale = perspectiveScale;
        clone.yawDegrees = yawDegrees;
        clone.z = z;
        clone.backWallCenterX = backWallCenterX;
        clone.backWallOffsetX = backWallOffsetX;
        clone.frontFloorLipCenterY = frontFloorLipCenterY;
        clone.frontFloorLipHeight = frontFloorLipHeight;
        clone.bottomOverscan = bottomOverscan;
        return clone;
    }

    public Vector3 CalculatePosition(Sprite sprite)
    {
        float floorLipBottomY = frontFloorLipCenterY - frontFloorLipHeight * 0.5f;
        float spriteHeight = sprite != null ? sprite.bounds.size.y : 0f;
        float projectedHeight = spriteHeight * perspectiveScale.y;
        float centerY = floorLipBottomY + projectedHeight * 0.5f - bottomOverscan;
        float centerX = backWallCenterX + backWallOffsetX;

        return new Vector3(centerX, centerY, z);
    }

    public void ApplyTo(Transform target, Sprite sprite)
    {
        if (target == null)
        {
            return;
        }

        target.position = CalculatePosition(sprite);
        target.rotation = Quaternion.Euler(0f, yawDegrees, 0f);
        target.localScale = perspectiveScale;
    }

    public void CaptureFrom(Transform target, Sprite sprite)
    {
        if (target == null)
        {
            return;
        }

        perspectiveScale = target.localScale;
        yawDegrees = NormalizeSignedAngle(target.eulerAngles.y);
        z = target.position.z;
        backWallOffsetX = target.position.x - backWallCenterX;

        float spriteHeight = sprite != null ? sprite.bounds.size.y : 0f;
        float projectedHeight = spriteHeight * perspectiveScale.y;
        float floorLipBottomY = frontFloorLipCenterY - frontFloorLipHeight * 0.5f;
        bottomOverscan = floorLipBottomY + projectedHeight * 0.5f - target.position.y;
    }

    private static float NormalizeSignedAngle(float angle)
    {
        while (angle > 180f)
        {
            angle -= 360f;
        }

        while (angle < -180f)
        {
            angle += 360f;
        }

        return angle;
    }
}
