using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tween_Victory : MonoBehaviour
{
    [SerializeField] GameObject panel, victory;


    // Start is called before the first frame update
    void Start()
    {
  
    }
    void OnEnable()
    {
        LeanTween.cancel(panel);
        LeanTween.cancel(victory);

        LeanTween.moveLocal(panel, new Vector3(-1000f, 248f, 0f), 1f).setEase(LeanTweenType.easeOutQuad);
        LeanTween.scale(victory, new Vector3(1f, 1f, 1f), 2f).setDelay(1f).setEase(LeanTweenType.easeOutQuad);
    }

     void OnDisable()
    {
        LeanTween.moveLocal(panel, new Vector3(-2500f, 248f, 0f), .1f);
        LeanTween.scale(victory, new Vector3(100f, 100f, 100f), .1f);
    }
}
