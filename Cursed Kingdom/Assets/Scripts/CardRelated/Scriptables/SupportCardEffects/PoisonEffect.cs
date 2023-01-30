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
    [SerializeField] [Range(1, 10)] private int numTurnsToPoison = 1;
    [SerializeField] private bool poisonImmediately;
    public int NumTurnsToPoison { get => numTurnsToPoison; set => numTurnsToPoison = value; }
    public bool PoisonImmediately { get => poisonImmediately; set => poisonImmediately = value; }

    public override void EffectOfCard(Player playerReference, Card cardPlayed = null)
    {
        //If player is already poisoned do nothing. Otherwise, add to their poison turn count.
        //if 'PoisonImmediately' is true, Poison the player immediately. Otherwise, wait until the turn is over to poison them.
    }

}
