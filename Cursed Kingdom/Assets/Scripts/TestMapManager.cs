//Code written by Mohamed Riaz Khan of BukuGames.
//All code is written by me (Above name) unless otherwise stated via comments below.
//Not authorized for use outside of the Github repository of this game developed by BukuGames.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestMapManager : MonoBehaviour
{
    public GameObject currentHighlightedSpace;
    public TestManager testManager;
    public Space currentSpaceInfo;

    private void Start()
    {
        testManager = this.GetComponent<TestManager>();
    }

    public void ActivateHighlight(int playerIntVal = 0)
    {
        if(testManager.players[playerIntVal] is null)
        {
            Debug.LogWarning("Invalid index.");
            return;
        }
        currentSpaceInfo = testManager.players[playerIntVal].CurrentSpacePlayerIsOn;
        currentHighlightedSpace = currentSpaceInfo.gameObject;
        currentSpaceInfo.selectedBorder.gameObject.SetActive(true);
    }

    //Need to find a way to tell player how far they are from current highlighted space.
    public void ChangeCurrentHighlightedSpace()
    {
        

    }
}
