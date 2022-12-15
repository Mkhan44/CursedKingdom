//Code written by Mohamed Riaz Khan of BukuGames.
//All code is written by me (Above name) unless otherwise stated via comments below.
//Not authorized for use outside of the Github repository of this game developed by BukuGames.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Recover Health", menuName = "Space Effect Data/Recover Health", order = 0)]
public class RecoverHealthSpace : SpaceEffectData, ISpaceEffect
{
    public int healthToRecover;
    public void EffectOfSpace(Player playerReference, bool afterDuel = false, bool startOfTurn = false)
    {
        playerReference.CurrentHealth += healthToRecover;
    }
}
