//Code written by Mohamed Riaz Khan of BukuGames.
//All code is written by me (Above name) unless otherwise stated via comments below.
//Not authorized for use outside of the Github repository of this game developed by BukuGames.

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
