//Code written by Mohamed Riaz Khan of BukuGames.
//All code is written by me (Above name) unless otherwise stated via comments below.
//Not authorized for use outside of the Github repository of this game developed by BukuGames.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "HalveMovementCardSpace", menuName = "Space Effect Data/Halve Movement Card Space", order = 0)]
public class HalveMovementCardSpace : SpaceEffectData, ISpaceEffect
{

    //As of right now: This DOES stack with Curse halving. Round the numbers up.
    public void EffectOfSpace(Player playerReference)
    {
        Debug.Log($"Landed on: {this.name} space and all movement cards in their hand have their values halved.");
    }
}
