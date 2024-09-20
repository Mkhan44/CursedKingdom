//Code written by Mohamed Riaz Khan of BukuGames.
//All code is written by me (Above name) unless otherwise stated via comments below.
//Not authorized for use outside of the Github repository of this game developed by BukuGames.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/*
After a duel: The player discards x amount of cards of a certain type or their entire hand potentially.
*/
/// </summary>
[CreateAssetMenu(fileName = "DuelDiscardAfterDuelEffect", menuName = "Card Data/Support Card Effect Data/Duel Discard After Duel Effect", order = 0)]
public class DuelDiscardAfterDuelEffect : SupportCardEffectData, ISupportEffect
{
	[SerializeField] private Card.CardType cardTypeToDiscard;
    [SerializeField] [Range(0,10)] private int numCardsToDiscard = 0;
    [SerializeField] private bool discardAllCards;

    public Card.CardType CardTypeToDiscard { get => cardTypeToDiscard; set => cardTypeToDiscard = value; }
    public int NumCardsToDiscard { get => numCardsToDiscard; set => numCardsToDiscard = value; }
    public bool DiscardAllCards { get => discardAllCards; set => discardAllCards = value; }

    public override void EffectOfCard(Player playerReference, Card cardPlayed = null)
	{
		SupportCard cardUsed = (SupportCard)cardPlayed;
		if (cardUsed != null)
		{
			supportCardThatWasJustUsed = cardUsed;
		}
	}

	public override void EffectOfCard(DuelPlayerInformation duelPlayerInformation, Card cardPlayed = null)
	{
        //We won so don't need to discard anything.
        if(duelPlayerInformation.PlayerInDuel.GameplayManagerRef.DuelPhaseSMRef.CurrentWinners.Contains(duelPlayerInformation))
        {
            base.EffectOfCard(duelPlayerInformation, cardPlayed);
            return;
        }

        //Ignore the number of cards to discard and just discard everything.
        if(DiscardAllCards)
        {
            if(CardTypeToDiscard == Card.CardType.Movement)
            {
                while(duelPlayerInformation.PlayerInDuel.MovementCardsInHandCount > 0)
                {
                    Card movementCard = duelPlayerInformation.PlayerInDuel.GetMovementCardsInHand()[0];
                    duelPlayerInformation.PlayerInDuel.DiscardFromHand(movementCard.ThisCardType, movementCard);
                }
            }
            else if(CardTypeToDiscard == Card.CardType.Support)
            {
                while(duelPlayerInformation.PlayerInDuel.SupportCardsInHandCount > 0)
                {
                    Card supportCard = duelPlayerInformation.PlayerInDuel.GetSupportCardsInHand()[0];
                    duelPlayerInformation.PlayerInDuel.DiscardFromHand(supportCard.ThisCardType, supportCard);
                }
            }
            else
            {
                duelPlayerInformation.PlayerInDuel.DiscardAllCardsInHand();
            }
            base.EffectOfCard(duelPlayerInformation, cardPlayed);
            return;
        }

        duelPlayerInformation.PlayerInDuel.DoneDiscardingForEffect += CompletedEffect;
        if(CardTypeToDiscard == Card.CardType.Movement)
        {
            duelPlayerInformation.PlayerInDuel.SetCardsToDiscard(Card.CardType.Movement, NumCardsToDiscard);
        }
        else if(CardTypeToDiscard == Card.CardType.Support)
        {
            duelPlayerInformation.PlayerInDuel.SetCardsToDiscard(Card.CardType.Support, NumCardsToDiscard);
        }
        else
        {
            duelPlayerInformation.PlayerInDuel.SetCardsToDiscard(Card.CardType.Both, NumCardsToDiscard);
        }
	}

    public override void CompletedEffect(Player playerReference)
    {
        playerReference.DoneDiscardingForEffect -= CompletedEffect;
        base.CompletedEffect(playerReference);
    }

}

