//Code written by Mohamed Riaz Khan of BukuGames.
//All code is written by me (Above name) unless otherwise stated via comments below.
//Not authorized for use outside of the Github repository of this game developed by BukuGames.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This support card effect is used when an opponent uses another support card. The effect still happens, but afterwards that card goes to the player who used this card's hand instead.
/// </summary>

[CreateAssetMenu(fileName = "GrapplingHookEffect", menuName = "Card Data/Support Card Effect Data/Grappling Hook Effect", order = 0)]
public class GrapplingHookEffect : SupportCardEffectData, ISupportEffect
{
    public override void EffectOfCard(Player playerReference, Card cardPlayed = null)
    {
        //Takes card that was played, adds to the player's hand.
    }

}

