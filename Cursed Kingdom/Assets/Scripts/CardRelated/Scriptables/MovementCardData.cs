//Code written by Mohamed Riaz Khan of BukuGames.
//All code is written by me (Above name) unless otherwise stated via comments below.
//Not authorized for use outside of the Github repository of this game developed by BukuGames.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Movement Card Data", menuName = "Movement Card Data", order = 0)]
public class MovementCardData : ScriptableObject
{
    [SerializeField] int defaultMovementValue = 1;

    public int DefaultCardValue { get => defaultMovementValue; set => defaultMovementValue = value; }
}
