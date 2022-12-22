//Code written by Mohamed Riaz Khan of BukuGames.
//All code is written by me (Above name) unless otherwise stated via comments below.
//Not authorized for use outside of the Github repository of this game developed by BukuGames.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CurseSpaceEffect", menuName = "Space Effect Data/Curse Space", order = 0)]
public class CurseSpace : SpaceEffectData, ISpaceEffect
{
    [Range(1, 10)] [SerializeField] private int numTurnsToBeCursed;

    public int NumTurnsToBeCursed { get => numTurnsToBeCursed; set => numTurnsToBeCursed = value; }

    public void EffectOfSpace(Player playerReference)
    {
        Debug.Log($"Landed on: {this.name} space and should be cursed for: {NumTurnsToBeCursed} turn(s).");
        //playerReference.IsPoisoned = true;
    }
}
