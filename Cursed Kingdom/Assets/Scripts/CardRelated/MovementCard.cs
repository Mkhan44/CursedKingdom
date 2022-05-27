//Code written by Mohamed Riaz Khan of BukuGames.
//All code is written by me (Above name) unless otherwise stated via comments below.
//Not authorized for use outside of the Github repository of this game developed by BukuGames.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MovementCard : Card
{
    [SerializeField] private MovementCardData movementCardData; 
    [SerializeField] private int defaultMovementNumber;

    public int DefaultMovementNumber { get => defaultMovementNumber; set => defaultMovementNumber = value; }
    public MovementCardData MovementCardData { get => movementCardData; set => movementCardData = value; }

    private void Start()
    {
        InitializeCard();
    }
    protected override void InitializeCard()
    {
        //Test data. Will grab this from scriptable.
        defaultMovementNumber = MovementCardData.DefaultCardValue;
    }
}
