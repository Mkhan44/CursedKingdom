//Code written by Mohamed Riaz Khan of BukuGames.
//All code is written by me (Above name) unless otherwise stated via comments below.
//Not authorized for use outside of the Github repository of this game developed by BukuGames.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BarricadeSpace", menuName = "Space Effect Data/Barricade Space", order = 0)]
public class BarricadeSpace : SpaceEffectData, ISpaceEffect
{
    [Range(1, 5)] [SerializeField] private int levelNeededToPass;

    public int LevelNeededToPass { get => levelNeededToPass; set => levelNeededToPass = value; }

    public void EffectOfSpace(Player playerReference)
    {
        //If player's level is too low: Force them to the other path. Otherwise, check if it's the first time passing it and play an animation.
        //Finally, if it's not the 1st time or after the animation plays: Player can choose to move to this space.
        Debug.Log($"Landed on: {this.name} space and the level required to pass is: {LevelNeededToPass}");
    }
}
