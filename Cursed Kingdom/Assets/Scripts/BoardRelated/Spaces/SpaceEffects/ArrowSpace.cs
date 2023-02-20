//Code written by Mohamed Riaz Khan of BukuGames.
//All code is written by me (Above name) unless otherwise stated via comments below.
//Not authorized for use outside of the Github repository of this game developed by BukuGames.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ArrowSpaceEffect", menuName = "Space Effect Data/ArrowSpace", order = 0)]
public class ArrowSpace : SpaceEffectData, ISpaceEffect
{
    [SerializeField] private List<DirectionToTravel> directionToTravel;

    public List<DirectionToTravel> DirectionToTravel1 { get => directionToTravel; set => directionToTravel = value; }


    //Should be something on the Player script we can increase for the turn to have max amount of cards able to be used.
    public override void EffectOfSpace(Player playerReference)
    {
        base.EffectOfSpace(playerReference);
        Debug.Log($"Landed on: {this.name} space and can travel one of {directionToTravel.Count} ways.");
    }
}
