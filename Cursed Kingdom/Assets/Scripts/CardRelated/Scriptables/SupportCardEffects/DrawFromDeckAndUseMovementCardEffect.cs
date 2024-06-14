//Code written by Mohamed Riaz Khan of BukuGames.
//All code is written by me (Above name) unless otherwise stated via comments below.
//Not authorized for use outside of the Github repository of this game developed by BukuGames.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/*
(You must have at least 1 movement card in your hand before the duel began to use this card.) 
Return the movement card you selected for this duel to your hand. 
Immediately draw and use the next movement card in the movement deck as your movement card for this duel. 
If you win the duel: Deal 1 extra damage to the losers up to the number of movement cards you have in your hand (Max: 3 extra damage). 
If you lose this duel: Discard all movement cards in your hand.
*/
/// </summary>
[CreateAssetMenu(fileName = "DrawFromDeckAndUseMovementCardEffect", menuName = "Card Data/Support Card Effect Data/Draw From Deck AndUseMovementCard Effect", order = 0)]
public class DrawFromDeckAndUseMovementCardEffect : SupportCardEffectData, ISupportEffect
{
	[SerializeField] bool requiresMovementCardInHandFirst;
	[SerializeField] bool discardAllMovementCardsIfYouLoseTheDuel;

    public bool RequiresMovementCardInHandFirst { get => requiresMovementCardInHandFirst; set => requiresMovementCardInHandFirst = value; }
    public bool DiscardAllMovementCardsIfYouLoseTheDuel { get => discardAllMovementCardsIfYouLoseTheDuel; set => discardAllMovementCardsIfYouLoseTheDuel = value; }

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
		base.EffectOfCard(duelPlayerInformation, cardPlayed);
	}
}
