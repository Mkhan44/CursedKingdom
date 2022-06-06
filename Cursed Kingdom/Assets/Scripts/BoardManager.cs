using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Responsible for setting up the board.
public class BoardManager : MonoBehaviour
{
    private BoardLayoutData boardLayout;

    public BoardLayoutData BoardLayout { get => boardLayout; set => boardLayout = value; }

    public void SetBoardLayoutData(BoardLayoutData boardLayoutData)
    {
        BoardLayout = boardLayoutData;
    }

    public void SetupBoard()
    {
        if(boardLayout == null)
        {
            Debug.LogWarning("There is no board layout!!!");
            return;
        }


    }

}
