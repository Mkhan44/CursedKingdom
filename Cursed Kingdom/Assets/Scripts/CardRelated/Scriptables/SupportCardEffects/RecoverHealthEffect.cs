//Code written by Mohamed Riaz Khan of BukuGames.
//All code is written by me (Above name) unless otherwise stated via comments below.
//Not authorized for use outside of the Github repository of this game developed by BukuGames.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This support card effect is used when there is a suppport card that recovers the player's health.
/// </summary>

[CreateAssetMenu(fileName = "RecoverHealthEffect", menuName = "Card Data/Support Card Effect Data/Recover Health Effect", order = 0)]
public class RecoverHealthEffect : SupportCardEffectData, ISupportEffect
{
    [SerializeField] [Range(1, 10)] private int healthToRecover = 1;

    public int HealthToRecover { get => healthToRecover; set => healthToRecover = value; }

    public override void EffectOfCard(Player playerReference, Card cardPlayed = null)
    {
        SupportCard cardUsed = (SupportCard)cardPlayed;
        if (cardUsed != null)
        {
            supportCardThatWasJustUsed = cardUsed;
        }
        playerReference.DoneRecoveringHealthEffect += CompletedEffect;
        playerReference.RecoverHealth(HealthToRecover);
    }

    public override bool CanCostBePaid(Player playerReference)
    {
        bool canCostBePaid = false;

        if(!(playerReference.CurrentHealth == playerReference.MaxHealth))
        {
            canCostBePaid = true;
        }

        if (!canCostBePaid)
        {
            DialogueBoxPopup.instance.ActivatePopupWithJustText("You're already at max health!", 1.5f);
        }
        return canCostBePaid;
    }

    public override void CompletedEffect(Player playerReference)
    {
        playerReference.DoneRecoveringHealthEffect -= CompletedEffect;
        base.CompletedEffect(playerReference);
    }
}

