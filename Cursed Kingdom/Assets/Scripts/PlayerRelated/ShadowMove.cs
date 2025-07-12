using UnityEngine;

public class ShadowMove : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform playerSprite;  // The sprite that rotates on X
    [SerializeField] private Transform shadow;        // The shadow parent you control

    [Header("Settings")]
    [SerializeField] private float maxYOffset = 0.1f; // Maximum Y distance
    [SerializeField] private float maxXRotation = 40f; // Max positive or negative rotation
    [SerializeField] private float smoothTime = 0.1f;  // SmoothDamp time

    private float initialShadowY;
    private float yVelocity = 0f;

    void Start()
    {
        if (playerSprite == null || shadow == null)
        {
            Debug.LogError("Missing references!");
            enabled = false;
            return;
        }

        initialShadowY = shadow.localPosition.y;
    }

    void LateUpdate()
    {
        float xRot = playerSprite.localEulerAngles.x;

        // Convert 0–360 to -180..180
        if (xRot > 180f) xRot -= 360f;

        float absRot = Mathf.Abs(xRot);

        float t = Mathf.InverseLerp(0f, maxXRotation, absRot);
        t = Mathf.Clamp01(t);

        float targetY = initialShadowY + maxYOffset * t;

        Vector3 localPos = shadow.localPosition;
        localPos.y = Mathf.SmoothDamp(localPos.y, targetY, ref yVelocity, smoothTime);
        shadow.localPosition = localPos;
    }
}