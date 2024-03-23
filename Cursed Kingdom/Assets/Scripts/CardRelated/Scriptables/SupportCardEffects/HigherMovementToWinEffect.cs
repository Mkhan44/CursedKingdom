//Code written by Mohamed Riaz Khan of BukuGames.
//All code is written by me (Above name) unless otherwise stated via comments below.
//Not authorized for use outside of the Github repository of this game developed by BukuGames.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This support card effect is used when the opponent(s) in a duel must have a higher support card value than the player's to win. [ex: I play 5 , you need 4 more to win. So you'd need a 9 minimum.]
/// </summary>

[CreateAssetMenu(fileName = "HigherMovementToWinEffect", menuName = "Card Data/Support Card Effect Data/Higher Movement To Win Effect", order = 0)]
public class HigherMovementToWinEffect : SupportCardEffectData, ISupportEffect
{
    [Tooltip("Whatever this value is: The opponent needs their movement value to be THAT MUCH higher to win. [ex: Player played 5, this value is '4' thus opponent needs a 9 to win minimum.")]
    [SerializeField] [Range(1, 10)] private int movementCardValueExcessAmountToWinBy = 1;

    public int MovementCardValueExcessAmountToWinBy { get => movementCardValueExcessAmountToWinBy; set => movementCardValueExcessAmountToWinBy = value; }

    public override void EffectOfCard(Player playerReference, Card cardPlayed = null)
    {
        SupportCard cardUsed = (SupportCard)cardPlayed;
        if (cardUsed != null)
        {
            supportCardThatWasJustUsed = cardUsed;
        }
    }
}

