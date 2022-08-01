//Code written by Mohamed Riaz Khan of BukuGames.
//All code is written by me (Above name) unless otherwise stated via comments below.
//Not authorized for use outside of the Github repository of this game developed by BukuGames.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Responsible for setting up the board.
public class BoardManager : MonoBehaviour
{
    [Header("Data")]
    public BoardLayoutData boardLayoutData;
    public GameObject boardHolder;

    [Header("Space gaps")]
    public float spaceIncreaseValue = 2.20f;
    public float smallSpaceAboveBigSpace = 1.6f;
    public float smallSpaceVerticalGap = 1.09f;
    public float smallSpaceHorizontalGap = 2.184f;


    public void SetBoardLayoutData(BoardLayoutData boardLayoutData)
    {
        this.boardLayoutData = boardLayoutData;
    }

    public void SetupBoard()
    {
        if(boardLayoutData == null)
        {
            Debug.LogWarning("There is no board layout!!!");
            return;
        }

        //(4 rows)
        float spaceRotation = 0;

        Vector3 nextSpawnPoint = new Vector3(boardLayoutData.firstSpaceSpawnPoint.x, boardLayoutData.firstSpaceSpawnPoint.y, boardLayoutData.firstSpaceSpawnPoint.z);

        
        for (int i = 0; i < 4; i++)
        {
            spaceRotation = SetSpaceRotation(spaceRotation, i);
            for (int j = 0; j < boardLayoutData.numSpacesPerRow; j++)
            {
                //Hardcoded, refactor this.
                //If it's the final space of the board, don't spawn because we don't want to overlap with the start space.
                if (j == boardLayoutData.numSpacesPerRow - 1 && i == 3)
                {
                    continue;
                }

                Vector3 nextSpawnPointSmall = nextSpawnPoint;

                //MIDDLE PATHS

                //We're 1 away from the halfway point, so start spawning in the middle path.
                if (j * 2 == boardLayoutData.numSpacesPerRow - 1)
                {
                    GameObject newSmallSpace;
                    Vector3 tempRotSmall;
                    Vector3 currentPosSmall;

                    //First column of small spaces.
                    for (int k = 0; k < boardLayoutData.numMiddleSpacesPerColumn; k++)
                    {
                        newSmallSpace = Instantiate(boardLayoutData.smallSpacePrefab, boardHolder.transform);

                        //Rotate based on where we are on the board.
                        tempRotSmall = newSmallSpace.transform.localEulerAngles;
                        tempRotSmall.y = spaceRotation;
                        newSmallSpace.transform.localEulerAngles = tempRotSmall;

                        //Position in the next applicable position based on where the previous one was spawned.

                        newSmallSpace.transform.localPosition = nextSpawnPointSmall;
                        currentPosSmall = newSmallSpace.transform.localPosition;

                        if (k == 0)
                        {
                            currentPosSmall = SetFirstSmallSpaceGap(i, currentPosSmall);

                        }
                        else
                        {
                            currentPosSmall = SetSmallSpaceVerticalGapDirection(i, currentPosSmall, true);
                        }


                        nextSpawnPointSmall = currentPosSmall;
                        newSmallSpace.transform.localPosition = nextSpawnPointSmall;
                    }

                    //Spawn 2 spaces horizontally to the left.
                    nextSpawnPointSmall = SpawnSmallHorizontalSpace(spaceRotation, i, nextSpawnPointSmall, out newSmallSpace, out tempRotSmall, out currentPosSmall);

                    //Spawn the top barricade space. If i = 3 , spawn the final space in the middle of the board.


                    nextSpawnPointSmall = SpawnSmallHorizontalSpace(spaceRotation, i, nextSpawnPointSmall, out newSmallSpace, out tempRotSmall, out currentPosSmall);



                    //Second column of small spaces going downward.
                    for (int l = 0; l < boardLayoutData.numMiddleSpacesPerColumn - 1; l++)
                    {
                        newSmallSpace = Instantiate(boardLayoutData.smallSpacePrefab, boardHolder.transform);

                        //Rotate based on where we are on the board.
                        tempRotSmall = newSmallSpace.transform.localEulerAngles;
                        tempRotSmall.y = spaceRotation;
                        newSmallSpace.transform.localEulerAngles = tempRotSmall;

                        //Position in the next applicable position based on where the previous one was spawned.

                        newSmallSpace.transform.localPosition = nextSpawnPointSmall;
                        currentPosSmall = newSmallSpace.transform.localPosition;

                        currentPosSmall = SetSmallSpaceVerticalGapDirection(i, currentPosSmall, false);

                        nextSpawnPointSmall = currentPosSmall;
                        newSmallSpace.transform.localPosition = nextSpawnPointSmall;
                    }
                }

                //MIDDLE PATHS END

                GameObject newSpace = Instantiate(boardLayoutData.regularSpacePrefab, boardHolder.transform);

                //Rotate based on where we are on the board.
                Vector3 tempRot = newSpace.transform.localEulerAngles;
                tempRot.y = spaceRotation;
                newSpace.transform.localEulerAngles = tempRot;

                //Position in the next applicable position based on where the previous one was spawned.
                newSpace.transform.localPosition = nextSpawnPoint;
                Vector3 currentPos = newSpace.transform.localPosition;

                //Hardcoded, refactor this.
                //Checks to see this is the first spawn. If so, just spawn at origin point and continue the loop.
                if (j == 0 && i == 0)
                {
                    continue;
                }

                //Hardcoded, refactor this.
                //Checks to see if we're not on the 1st or last row and continues spawning so there is no overlap.
                if (j == 0 && i > 0)
                {
                    j += 1;
                }

                switch (i)
                {
                    //Spawning to the left.
                    case 0:
                        {
                            currentPos.x -= spaceIncreaseValue;
                            break;
                        }
                    //Spawning going up the left side.
                    case 1:
                        {
                            currentPos.z += spaceIncreaseValue;
                            break;
                        }
                    //Spawning going right at the top.
                    case 2:
                        {
                            currentPos.x += spaceIncreaseValue;
                            break;
                        }
                    //Spawning going down the right side.
                    case 3:
                        {
                            currentPos.z -= spaceIncreaseValue;
                            break;
                        }
                }
                nextSpawnPoint = currentPos;
               
                newSpace.transform.localPosition = nextSpawnPoint;
            }
        }


    }

    private Vector3 SpawnSmallHorizontalSpace(float spaceRotation, int i, Vector3 nextSpawnPointSmall, out GameObject newSmallSpace, out Vector3 tempRotSmall, out Vector3 currentPosSmall)
    {
        newSmallSpace = Instantiate(boardLayoutData.smallSpacePrefab, boardHolder.transform);

        //Rotate based on where we are on the board.
        tempRotSmall = newSmallSpace.transform.localEulerAngles;
        tempRotSmall.y = spaceRotation;
        newSmallSpace.transform.localEulerAngles = tempRotSmall;

        //Position in the next applicable position based on where the previous one was spawned.

        newSmallSpace.transform.localPosition = nextSpawnPointSmall;
        currentPosSmall = newSmallSpace.transform.localPosition;

        currentPosSmall = SetSmallSpaceHorizontalGapDirection(i, currentPosSmall);

        nextSpawnPointSmall = currentPosSmall;
        newSmallSpace.transform.localPosition = nextSpawnPointSmall;
        return nextSpawnPointSmall;
    }

    private Vector3 SetFirstSmallSpaceGap(int i, Vector3 currentPosSmall)
    {
        switch (i)
        {
            //Spawning to the left.
            case 0:
                {
                    currentPosSmall.z += smallSpaceAboveBigSpace;
                    break;
                }
            //Spawning going up the left side.
            case 1:
                {
                    currentPosSmall.x += smallSpaceAboveBigSpace;
                    break;
                }
            //Spawning going right at the top.
            case 2:
                {
                    currentPosSmall.z -= smallSpaceAboveBigSpace;
                    break;
                }
            //Spawning going down the right side.
            case 3:
                {
                    currentPosSmall.x -= smallSpaceAboveBigSpace;
                    break;
                }
        }

        return currentPosSmall;
    }

    private Vector3 SetSmallSpaceVerticalGapDirection(int i, Vector3 currentPosSmall, bool isAscending)
    {
        if(isAscending)
        {
            //Bad, refactor this....Actually the entire for loop lol. Need to account for the inside spaces too.
            switch (i)
            {
                //Spawning to the left.
                case 0:
                    {
                        currentPosSmall.z += smallSpaceVerticalGap;
                        break;
                    }
                //Spawning going up the left side.
                case 1:
                    {
                        currentPosSmall.x += smallSpaceVerticalGap;
                        break;
                    }
                //Spawning going right at the top.
                case 2:
                    {
                        currentPosSmall.z -= smallSpaceVerticalGap;
                        break;
                    }
                //Spawning going down the right side.
                case 3:
                    {
                        currentPosSmall.x -= smallSpaceVerticalGap;
                        break;
                    }
            }
        }
        else
        {
            switch (i)
            {
                //Spawning to the left.
                case 0:
                    {
                        currentPosSmall.z -= smallSpaceVerticalGap;
                        break;
                    }
                //Spawning going up the left side.
                case 1:
                    {
                        currentPosSmall.x -= smallSpaceVerticalGap;
                        break;
                    }
                //Spawning going right at the top.
                case 2:
                    {
                        currentPosSmall.z += smallSpaceVerticalGap;
                        break;
                    }
                //Spawning going down the right side.
                case 3:
                    {
                        currentPosSmall.x += smallSpaceVerticalGap;
                        break;
                    }
            }

        }

        return currentPosSmall;
    }

    private Vector3 SetSmallSpaceHorizontalGapDirection(int i, Vector3 currentPosSmall)
    {
        switch (i)
        {
            //Spawning to the left.
            case 0:
                {
                    currentPosSmall.x -= smallSpaceHorizontalGap;
                    break;
                }
            //Spawning going up the left side.
            case 1:
                {
                    currentPosSmall.z += smallSpaceHorizontalGap;
                    break;
                }
            //Spawning going right at the top.
            case 2:
                {
                    currentPosSmall.x += smallSpaceHorizontalGap;
                    break;
                }
            //Spawning going down the right side.
            case 3:
                {
                    currentPosSmall.z -= smallSpaceHorizontalGap;
                    break;
                }
        }

        return currentPosSmall;
    }

    private float SetSpaceRotation(float spaceRotation, int i)
    {
        //Bad, refactor this....Actually the entire for loop lol. Need to account for the inside spaces too.
        switch (i)
        {
            //Spawning to the left.
            case 0:
                {
                    spaceRotation = 0f;
                    break;
                }
            //Spawning going up the left side.
            case 1:
                {
                    spaceRotation = 90f;
                    break;
                }
            //Spawning going right at the top.
            case 2:
                {
                    spaceRotation = 180f;
                    break;
                }
            //Spawning going down the right side.
            case 3:
                {
                    spaceRotation = 270f;
                    break;
                }
        }

        return spaceRotation;
    }
}
