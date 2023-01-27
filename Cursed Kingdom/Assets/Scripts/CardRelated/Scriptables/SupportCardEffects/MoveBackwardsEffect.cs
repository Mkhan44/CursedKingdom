//Code written by Mohamed Riaz Khan of BukuGames.
//All code is written by me (Above name) unless otherwise stated via comments below.
//Not authorized for use outside of the Github repository of this game developed by BukuGames.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This support card effect is used to move a target player backwards a certain amount of spaces.
/// </summary>

[CreateAssetMenu(fileName = "MoveBackwardsEffect", menuName = "Card Data/Support Card Effect Data/Move Backwards Effect", order = 0)]
public class MoveBackwardsEffect : SupportCardEffectData, ISupportEffect
{
    [SerializeField] [Range(1, 10)] private int numSpacesToMoveBack = 1;
    [SerializeField] private bool needToWinDuel;

    public int NumSpacesToMoveBack { get => numSpacesToMoveBack; set => numSpacesToMoveBack = value; }
    public bool NeedToWinDuel { get => needToWinDuel; set => needToWinDuel = value; }

    public override void EffectOfCard(Player playerReference, Card cardPlayed = null)
    {
        //If needToWinDuel == true ...Must win the duel for this effect to activate it. Maybe add it to a queue?
    }

}



