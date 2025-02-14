//Code written by Mohamed Riaz Khan of BukuGames.
//All code is written by me (Above name) unless otherwise stated via comments below.
//Not authorized for use outside of the Github repository of this game developed by BukuGames.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/*
If you win the duel: Deal x extra damage to the losers up to the number of movement/support cards you have in your hand (Max: x extra damage). 
*/
/// </summary>
[CreateAssetMenu(fileName = "DuelExtraDamageBasedOnCardsInHand", menuName = "Card Data/Support Card Effect Data/Duel Extra Damage Based On Cards In Hand Effect", order = 0)]
public class DuelExtraDamageBasedOnCardsInHand : SupportCardEffectData, ISupportEffect
{
	[SerializeField] private Card.CardType cardTypeToCheckFor;
    [SerializeField] [Range(0,10)] private int extraDamageToDeal = 0; 

    public Card.CardType CardTypeToCheckFor { get => cardTypeToCheckFor; set => cardTypeToCheckFor = value; }
    public int ExtraDamageToDeal { get => extraDamageToDeal; set => extraDamageToDeal = value; }

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
        DuelPlayerInformation currentDuelWinner = duelPlayerInformation.PlayerInDuel.GameplayManagerRef.DuelPhaseSMRef.CurrentWinners[0];

        if(CardTypeToCheckFor == Card.CardType.Movement)
        {
            ExtraDamageToDeal = currentDuelWinner.PlayerInDuel.MovementCardsInHandCount - currentDuelWinner.SelectedMovementCards.Count;
        }
        else if(CardTypeToCheckFor == Card.CardType.Support)
        {
            ExtraDamageToDeal = currentDuelWinner.PlayerInDuel.SupportCardsInHandCount - currentDuelWinner.SelectedSupportCards.Count;
        }
        else
        {
            ExtraDamageToDeal = (currentDuelWinner.PlayerInDuel.MovementCardsInHandCount + currentDuelWinner.PlayerInDuel.SupportCardsInHandCount) 
                                - (currentDuelWinner.SelectedMovementCards.Count + currentDuelWinner.SelectedSupportCards.Count);
        }

        if(ExtraDamageToDeal < 0)
        {
            ExtraDamageToDeal = 0;
        }

        duelPlayerInformation.DamageToTake += ExtraDamageToDeal;

		base.EffectOfCard(duelPlayerInformation, cardPlayed);
	}

    public override void CompletedEffect(Player playerReference)
    {
        ExtraDamageToDeal = 0;
        base.CompletedEffect(playerReference);
    }

}

