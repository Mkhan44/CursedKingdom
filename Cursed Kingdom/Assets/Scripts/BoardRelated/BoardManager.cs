using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    public BoardSpacesData boardSpacesData;

    // Start is called before the first frame update
    private void Start()
    {
       
    }
    private void OnValidate()
    {
       // StartupSetupSpaces();
       
    }
    public void StartupSetupSpaces()
    {
        if(boardSpacesData == null)
        {
            Debug.LogError("Hey, you don't have any data to use for generating the board!");
            return;
        }
        int currentBoardSpacesDataPerimeterIndex = 0;
        int currentBoardSpacesDataInsideIndex = 0;
        foreach (Transform row in this.transform)
        {
            foreach (Transform space in row.transform)
            {
                if (space.gameObject.activeInHierarchy)
                {
                    Space theSpace = space.GetComponent<Space>();
                    if (theSpace != null)
                    {
                        if (currentBoardSpacesDataPerimeterIndex < boardSpacesData.perimeterSpaces.Count)
                        {
                            theSpace.spaceData = boardSpacesData.perimeterSpaces[currentBoardSpacesDataPerimeterIndex];
                            currentBoardSpacesDataPerimeterIndex += 1;
                        }
                        else if (currentBoardSpacesDataInsideIndex < boardSpacesData.insideSpaces.Count)
                        {
                            theSpace.spaceData = boardSpacesData.insideSpaces[currentBoardSpacesDataInsideIndex];
                            currentBoardSpacesDataInsideIndex += 1;
                        }
                        theSpace.SetupSpace();

                    }
                }
            }
            currentBoardSpacesDataPerimeterIndex = 0;
            currentBoardSpacesDataInsideIndex = 0;
        }
    }

}
