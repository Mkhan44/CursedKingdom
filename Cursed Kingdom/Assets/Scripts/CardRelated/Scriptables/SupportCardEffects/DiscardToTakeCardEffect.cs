//Code written by Mohamed Riaz Khan of BukuGames.
//All code is written by me (Above name) unless otherwise stated via comments below.
//Not authorized for use outside of the Github repository of this game developed by BukuGames.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This support card effect is used to discard as a cost in order to take a card from an opponent.
/// </summary>

[CreateAssetMenu(fileName = "DiscardToTakeCardEffect", menuName = "Card Data/Support Card Effect Data/Discard To Take Card Effect", order = 0)]
public class DiscardToTakeCardEffect : SupportCardEffectData, ISupportEffect
{
    [SerializeField] private Card.CardType cardTypeToDiscard;
    [SerializeField] [Range(1, 10)] private int numCardsToDiscard = 1;
    [SerializeField] private Card.CardType cardTypeToTakeFromOpponent;
    [SerializeField] [Range(1, 10)] private int numCardsToTake = 1;

    public Card.CardType CardTypeToDiscard { get => cardTypeToDiscard; set => cardTypeToDiscard = value; }
    public int NumCardsToDiscard { get => numCardsToDiscard; set => numCardsToDiscard = value; }
    public Card.CardType CardTypeToTakeFromOpponent { get => cardTypeToTakeFromOpponent; set => cardTypeToTakeFromOpponent = value; }
    public int NumCardsToTake { get => numCardsToTake; set => numCardsToTake = value; }

    public override void EffectOfCard(Player playerReference, Card cardPlayed = null)
    {
        playerReference.DoneAttackingForEffect += CompletedEffect;
        playerReference.ActivateChoosePlayerToTakeCardsFromSupportSelectionPopup(NumCardsToDiscard, CardTypeToDiscard, NumCardsToTake, CardTypeToTakeFromOpponent);
    }

    public override bool CanCostBePaid(Player playerReference)
    {
        bool canCostBePaid = false;

        if (CardTypeToDiscard == Card.CardType.Movement)
        {
            if ((playerReference.MovementCardsInHandCount >= NumCardsToDiscard))
            {
                canCostBePaid = true;
            }
        }
        else if (CardTypeToDiscard == Card.CardType.Support)
        {
            //Has to be greater than 1 here because we don't count this card.
            if (((playerReference.SupportCardsInHandCount - 1) >= NumCardsToDiscard))
            {
                canCostBePaid = true;
            }
        }
        else
        {
            //Do a -1 because we need to account for this card not having been used yet.
            if (((playerReference.CardsInhand.Count - 1) >= NumCardsToDiscard))
            {
                canCostBePaid = true;
            }
        }


        //Early return since if even 1 of the conditions are not met we can't do it.
        if (!canCostBePaid)
        {
            DialogueBoxPopup.instance.ActivatePopupWithJustText("You need at least 1 other support card to discard to use this card!", 2.5f);
            return canCostBePaid;
        }

        //Doing another check for the other players' condition of cards in hand.
        canCostBePaid = false;

        foreach (Player player in playerReference.GameplayManagerRef.Players)
        {
            if(player != playerReference)
            {
                if(CardTypeToTakeFromOpponent == Card.CardType.Movement)
                {
                    if (player.MovementCardsInHandCount >= NumCardsToTake)
                    {
                        canCostBePaid = true;
                    }
                }
                else if(CardTypeToTakeFromOpponent == Card.CardType.Support)
                {
                    if (player.SupportCardsInHandCount >= NumCardsToTake)
                    {
                        canCostBePaid = true;
                    }
                }
                else
                {
                    if (player.CardsInhand.Count >= NumCardsToTake)
                    {
                        canCostBePaid = true;
                    }
                }
            }
        }

        if (!canCostBePaid)
        {
            DialogueBoxPopup.instance.ActivatePopupWithJustText("There are no valid targets to use this card against currently.", 2.5f);
            return canCostBePaid;
        }
        return canCostBePaid;
    }

    public override void CompletedEffect(Player playerReference)
    {
        base.CompletedEffect(playerReference);
        playerReference.DoneAttackingForEffect -= CompletedEffect;
    }
}

