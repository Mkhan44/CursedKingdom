//Code written by Mohamed Riaz Khan of BukuGames.
//All code is written by me (Above name) unless otherwise stated via comments below.
//Not authorized for use outside of the Github repository of this game developed by BukuGames.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class CanvasResolution : MonoBehaviour
{
    public float resoX;
    public float resoY;

    private CanvasScaler can;

    public static CanvasResolution instance;

    private void Awake()
    {
        instance = this;
    }
    // Start is called before the first frame update
    private void Start()
    {
        can = GetComponent<CanvasScaler>();
        setInfo();
    }

    private void LateUpdate()
    {

        if (GetCurrentScreenReso().x != resoX || GetCurrentScreenReso().y != resoY)
        {
            setInfo();
        }
        
    }


    private void setInfo()
    {
        /*
#if UNITY_EDITOR
        resoX = 1440f;
        resoY = 2560f;
        can.referenceResolution = new Vector2(resoX, resoY);
        return;
#endif
        */
        Vector2 currentScreenReso = GetCurrentScreenReso();
        resoX = currentScreenReso.x;
        resoY = currentScreenReso.y;

        can.referenceResolution = new Vector2(resoX, resoY);
    }

    private void ApplySafeZone()
    {

    }

    public Vector2 getReferenceReso()
    {
        return can.referenceResolution;
    }

    public Vector2 GetCurrentScreenReso()
    {
        return new Vector2(Screen.currentResolution.width, Screen.currentResolution.height);
    }

}