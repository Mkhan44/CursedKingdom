using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class Tween_ArtworkPopup : MonoBehaviour
{
    public GameObject artwork;
    public GameObject parent;
    public CanvasGroup parentCanvas;
    public float fadeDuration = 1f;
    public float startY = -31.35f;
    public float endY = 31.35f;
    public float moveDuration = 1.0f;

    private void OnEnable()
    {
        // Cancel any ongoing animation
        LeanTween.cancel(artwork);
        LeanTween.cancel(parent);

        // Fade in the Canvas Group
        parentCanvas = parent.GetComponent<CanvasGroup>();
        parentCanvas.alpha = 0f;
        LeanTween.alphaCanvas(parentCanvas, 1f, fadeDuration);



        // Calculate the target position based on startY and endY values
        Vector3 targetPosition = artwork.transform.localPosition;
        targetPosition.y = endY;

        // Set the starting position
        Vector3 startPosition = artwork.transform.localPosition;
        startPosition.y = startY;
        artwork.transform.localPosition = startPosition;

        // Move the artwork from the starting position to the target position
        LeanTween.moveLocal(artwork, targetPosition, moveDuration);
    }
}
