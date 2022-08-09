using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    public List<BoardSpacesData> boardSpacesData;

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
        int currentRowIndex = 0;
        foreach (Transform row in this.transform)
        {

            if (currentRowIndex >= boardSpacesData.Count)
            {
                //Conference room code will go here.
                return;
            }
            foreach (Transform space in row.transform)
            {
                if (space.gameObject.activeInHierarchy)
                {
                    Space theSpace = space.GetComponent<Space>();
                    if (theSpace != null)
                    {
                        if (currentBoardSpacesDataPerimeterIndex < boardSpacesData[currentRowIndex].perimeterSpaces.Count)
                        {
                            theSpace.spaceData = boardSpacesData[currentRowIndex].perimeterSpaces[currentBoardSpacesDataPerimeterIndex];
                            currentBoardSpacesDataPerimeterIndex += 1;
                        }
                        else if (currentBoardSpacesDataInsideIndex < boardSpacesData[currentRowIndex].insideSpaces.Count)
                        {
                            theSpace.spaceData = boardSpacesData[currentRowIndex].insideSpaces[currentBoardSpacesDataInsideIndex];
                            currentBoardSpacesDataInsideIndex += 1;
                        }
                        theSpace.SetupSpace();

                    }
                }
            }
            currentRowIndex += 1;
            currentBoardSpacesDataPerimeterIndex = 0;
            currentBoardSpacesDataInsideIndex = 0;
        }
    }

}
