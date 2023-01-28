//Code written by Mohamed Riaz Khan of BukuGames.
//All code is written by me (Above name) unless otherwise stated via comments below.
//Not authorized for use outside of the Github repository of this game developed by BukuGames.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This support card effect is used to poison a target. If the target is already poisoned then this cannot be used.
/// </summary>

[CreateAssetMenu(fileName = "PoisonEffect", menuName = "Card Data/Support Card Effect Data/Poison Effect", order = 0)]
public class PoisonEffect : SupportCardEffectData, ISupportEffect
{
    [SerializeField] [Range(1, 10)] private int numTurnsToPoison;

    public int NumTurnsToPoison { get => numTurnsToPoison; set => numTurnsToPoison = value; }

    public override void EffectOfCard(Player playerReference, Card cardPlayed = null)
    {
        //If player is already poisoned do nothing. Otherwise, add to their poison turn count.
    }

}


