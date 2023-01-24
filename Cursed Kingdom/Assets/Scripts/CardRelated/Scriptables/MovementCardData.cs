//Code written by Mohamed Riaz Khan of BukuGames.
//All code is written by me (Above name) unless otherwise stated via comments below.
//Not authorized for use outside of the Github repository of this game developed by BukuGames.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "Movement Card Data", menuName = "Card Data/Movement Card Data", order = 0)]
public class MovementCardData : CardData
{
    [SerializeField] [Range(1,10)]private int movementValue = 1;


    public int MovementValue { get => movementValue; set => movementValue = value; }
}
