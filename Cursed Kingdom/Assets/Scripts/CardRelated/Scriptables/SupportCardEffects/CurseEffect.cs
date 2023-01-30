//Code written by Mohamed Riaz Khan of BukuGames.
//All code is written by me (Above name) unless otherwise stated via comments below.
//Not authorized for use outside of the Github repository of this game developed by BukuGames.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This support card effect is used to curse a target. If the target is already cursed then this cannot be used.
/// </summary>

[CreateAssetMenu(fileName = "CurseEffect", menuName = "Card Data/Support Card Effect Data/Curse Effect", order = 0)]
public class CurseEffect : SupportCardEffectData, ISupportEffect
{
    [SerializeField] [Range(1, 10)] private int numTurnsToCurse = 1;
    [SerializeField] private bool curseImmediately;

    public int NumTurnsToCurse { get => numTurnsToCurse; set => numTurnsToCurse = value; }
    public bool CurseImmediately { get => curseImmediately; set => curseImmediately = value; }

    public override void EffectOfCard(Player playerReference, Card cardPlayed = null)
    {
        //If player is already cursed do nothing. Otherwise, add to their curse turn count.
        //If 'CurseImmediately' is true, curse as soon as the card is used. Otherwise don't curse until after the current turn ends.
    }

}
