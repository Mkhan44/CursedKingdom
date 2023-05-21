using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tween_Victory : MonoBehaviour
{
    public GameObject panel;
    public GameObject victory;

    public float startX = -2500f;
    public float endX = -1000f;
    public Vector3 startScale = new Vector3(100f, 100f, 100f);
    public float moveDuration = 0.5f;
    private float scaleDuration = 1f;

    private void OnEnable()
    {
        // Cancel any ongoing animation
        LeanTween.cancel(panel);
        LeanTween.cancel(victory);


        // Calculate the target position based on startX and endX values
        Vector3 targetPosition = panel.transform.localPosition;
        targetPosition.x = endX;

        // Set the starting position
        Vector3 startPosition = panel.transform.localPosition;
        startPosition.x = startX;
        panel.transform.localPosition = startPosition;

        // Move the artwork from the starting position to the target position
        LeanTween.moveLocal(panel, targetPosition, moveDuration).setEase(LeanTweenType.easeOutQuad).setOnComplete(ScaleVictory);

        victory.transform.localScale = startScale;
    }

    private void ScaleVictory()
    {
        
        Vector3 targetScale = new Vector3(1f, 1f, 1f);

        LeanTween.scale(victory, targetScale, scaleDuration)
            .setEase(LeanTweenType.easeOutExpo)
            .setFrom(startScale);
    }
}
