using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestManager : MonoBehaviour
{
    public GameObject cardToSpawn;
    public Transform cardParentCanvas;


    private void Start()
    {
        if(cardParentCanvas != null)
        {
            Instantiate(cardToSpawn, cardParentCanvas);
        }
    }
}
