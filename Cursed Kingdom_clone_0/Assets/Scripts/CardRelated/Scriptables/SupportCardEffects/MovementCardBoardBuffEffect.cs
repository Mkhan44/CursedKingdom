//Code written by Mohamed Riaz Khan of BukuGames.
//All code is written by me (Above name) unless otherwise stated via comments below.
//Not authorized for use outside of the Github repository of this game developed by BukuGames.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This support card effect is used when we want to buff the user's movement cards in their hand during movement phase.
/// </summary>

[CreateAssetMenu(fileName = "MovementCardBoardBuffEffect", menuName = "Card Data/Support Card Effect Data/Movement Card Board Buff Effect", order = 0)]
public class MovementCardBoardBuffEffect : SupportCardEffectData, ISupportEffect
{
    [SerializeField] [Range(1, 10)] private int numMovementIncrease = 1;
    [SerializeField] private bool buffAllCards = true;

    public int NumMovementIncrease { get => numMovementIncrease; set => numMovementIncrease = value; }
    public bool BuffAllCards { get => buffAllCards; set => buffAllCards = value; }

    //Might want an event to be triggered so we can do an animation. This would be something like "FinishedManipulatingCardValue..."
    public override void EffectOfCard(Player playerReference, Card cardPlayed = null)
    {
        SupportCard cardUsed = (SupportCard)cardPlayed;
        if (cardUsed != null)
        {
            supportCardThatWasJustUsed = cardUsed;
        }
        //If BuffAllCards != true ....Need to let player choose which card to apply the buff to.
        if (BuffAllCards)
        {
            playerReference.BoostAllMovementCardValuesInHand(numMovementIncrease);
            base.CompletedEffect(playerReference);
        }
    }
}

