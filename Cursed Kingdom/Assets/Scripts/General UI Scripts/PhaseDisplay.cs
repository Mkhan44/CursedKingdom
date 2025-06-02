//Code written by Mohamed Riaz Khan of BukuGames.
//All code is written by me (Above name) unless otherwise stated via comments below.
//Not authorized for use outside of the Github repository of this game developed by BukuGames.
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PhaseDisplay : MonoBehaviour
{
    public static PhaseDisplay instance;
    public event Action displayTimeCompleted;

    [SerializeField] private GameObject blockerPanel;
    [SerializeField] private CanvasGroup textPanelDisplay;
    [SerializeField] private TextMeshProUGUI phaseText;
    private Coroutine fadeCoroutine;

    private void Awake()
    {
        instance = this;
        textPanelDisplay.alpha = 0.0f;
        phaseText.text = "Test";
        blockerPanel.SetActive(false);
    }

    private void Start()
    {
       // Invoke("Started" , 0.1f);
    }

    private void Started()
    {
        instance = this;
        textPanelDisplay.alpha = 0.0f;
        phaseText.text = "Test";
        blockerPanel.SetActive(false);
    }


    public void TurnOnDisplay(string textToDisplay, float timeToKeepDisplayOn = 1.0f)
    {
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }
        phaseText.text = textToDisplay;
        blockerPanel.SetActive(true);
        fadeCoroutine = StartCoroutine(FadeIn(0.5f, timeToKeepDisplayOn));
    }

    public void TurnOffDisplay()
    {
        fadeCoroutine = null;
        blockerPanel.SetActive(false);
        displayTimeCompleted?.Invoke();
    }

    public IEnumerator FadeIn(float fadeInTime, float timeToKeepDisplayOn)
    {
        float elapsedTime = 0f;
        float startValue = 0f;
        while(elapsedTime < fadeInTime)
        {
            elapsedTime += Time.deltaTime;
            float newAlpha = Mathf.Lerp(startValue, 1f, elapsedTime/ fadeInTime);
            textPanelDisplay.alpha = newAlpha;
            yield return null;
        }

        yield return new WaitForSeconds(timeToKeepDisplayOn);
        StartCoroutine(FadeOut(elapsedTime));
    }

    public void FadeEarly()
    {
        if(fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
            TurnOffDisplay();
        }
    }

    public IEnumerator FadeOut(float fadeOutTime)
    {
        float elapsedTime = 0f;
        float startValue = textPanelDisplay.alpha;
        while (elapsedTime < fadeOutTime)
        {
            elapsedTime += Time.deltaTime;
            float newAlpha = Mathf.Lerp(startValue, 0f, elapsedTime / fadeOutTime);
            textPanelDisplay.alpha = newAlpha;
            yield return null;
        }
        TurnOffDisplay();
    }


}
