//Code written by Mohamed Riaz Khan of BukuGames.
//All code is written by me (Above name) unless otherwise stated via comments below.
//Not authorized for use outside of the Github repository of this game developed by BukuGames.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This support card effect is used to cure the 'cursed' status from a Player.
/// </summary>

[CreateAssetMenu(fileName = "CureCursedEffect", menuName = "Card Data/Support Card Effect Data/Cure Cursed Effect", order = 0)]
public class CureCursedEffect : SupportCardEffectData, ISupportEffect
{
    public override void EffectOfCard(Player playerReference, Card cardPlayed = null)
    {
        SupportCard cardUsed = (SupportCard)cardPlayed;
        if (cardUsed != null)
        {
            supportCardThatWasJustUsed = cardUsed;
        }
        playerReference.StatusEffectUpdateCompleted += CompletedEffect;
        playerReference.CureCurse();
    }

    public override void CompletedEffect(Player playerReference)
    {
        playerReference.StatusEffectUpdateCompleted -= CompletedEffect;
        base.CompletedEffect(playerReference);
    }

    public override bool CanCostBePaid(Player playerReference, bool justChecking = false)
    {
        bool canCostBePaid = playerReference.IsCursed;

        if (!canCostBePaid && !justChecking)
        {
            DialogueBoxPopup.instance.ActivatePopupWithJustText("You're not cursed currently.", 1.5f);
        }
        return canCostBePaid;
    }

}


