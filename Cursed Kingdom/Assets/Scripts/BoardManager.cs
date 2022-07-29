//Code written by Mohamed Riaz Khan of BukuGames.
//All code is written by me (Above name) unless otherwise stated via comments below.
//Not authorized for use outside of the Github repository of this game developed by BukuGames.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Responsible for setting up the board.
public class BoardManager : MonoBehaviour
{
    public BoardLayoutData boardLayoutData;
    public GameObject boardHolder;

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
        int spaceRotation = 0;
        float spaceIncreaseValue = 2.20f;
        Vector3 nextSpawnPoint = new Vector3(boardLayoutData.firstSpaceSpawnPoint.x, boardLayoutData.firstSpaceSpawnPoint.y, boardLayoutData.firstSpaceSpawnPoint.z);
        for (int i = 0; i < 4; i++)
        {
            //Bad, refactor this....Actually the entire for loop lol. Need to account for the inside spaces too.
            switch(i)
            {
                //Spawning to the left.
                case 0:
                    {
                        spaceRotation = 0;
                        break;
                    }
                //Spawning going up the left side.
                case 1:
                    {
                        spaceRotation = 90;
                        break;
                    }
                //Spawning going right at the top.
                case 2:
                    {
                        spaceRotation = 180;
                        break;
                    }
                //Spawning going down the right side.
                case 3:
                    {
                        spaceRotation = 270;
                        break;
                    }
            }
            for (int k = 0; k < boardLayoutData.numSpacesPerRow; k++)
            {
                GameObject newSpace = Instantiate(boardLayoutData.regularSpacePrefab, boardHolder.transform);

                //Rotate based on where we are on the board.
                Quaternion tempRot = newSpace.transform.rotation;
                tempRot.y = spaceRotation;
                newSpace.transform.rotation = tempRot;

                //Position in the next applicable position based on where the previous one was spawned.
                newSpace.transform.localPosition = nextSpawnPoint;
                Vector3 currentPos = newSpace.transform.localPosition;

                //Bad, refactor this....Actually the entire for loop lol. Need to account for the inside spaces too.
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

}
