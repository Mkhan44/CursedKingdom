//Code written by Mohamed Riaz Khan of BukuGames.
//All code is written by me (Above name) unless otherwise stated via comments below.
//Not authorized for use outside of the Github repository of this game developed by BukuGames.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/*
(You must have at least 2 movement cards in your hand before the duel began to use this card.) 
Return the movement card you selected for this duel to your hand. 
Immediately draw and use the next movement card in the movement deck as your movement card for this duel. 
If you lose this duel: Discard all movement cards in your hand.
*/
/// </summary>
[CreateAssetMenu(fileName = "DuelDrawFromDeckAndUseMovementCardEffect", menuName = "Card Data/Support Card Effect Data/Duel Draw From Deck AndUseMovementCard Effect", order = 0)]
public class DuelDrawFromDeckAndUseMovementCardEffect : SupportCardEffectData, ISupportEffect
{
	[SerializeField] [Range(0,10)] int minimumMovementCardsRequiredInHand;
	[SerializeField] bool discardAllMovementCardsIfYouLoseTheDuel;

    public int MinimumMovementCardsRequiredInHand { get => minimumMovementCardsRequiredInHand; set => minimumMovementCardsRequiredInHand = value; }
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
		//Remove a 'selected movement card' from the player putting it back in their hand and then draw the next movement card in the deck. Use that value for the duel and add that to the list of selected movement cards instead.
		duelPlayerInformation.SelectedMovementCards[0].gameObject.SetActive(true);
		//duelPlayerInformation.SelectedMovementCards[0].DeselectCard();
		duelPlayerInformation.SelectedMovementCards.RemoveAt(0);

		List<MovementCard> movementCardsBeforeDraw = duelPlayerInformation.PlayerInDuel.GetMovementCardsInHand();
		duelPlayerInformation.PlayerInDuel.GameplayManagerRef.ThisDeckManager.DrawCard(Card.CardType.Movement, duelPlayerInformation.PlayerInDuel);

        //Might hafta turn this into another method and wait till an event is triggered to signify the draw being completed.
        foreach (MovementCard card in duelPlayerInformation.PlayerInDuel.GetMovementCardsInHand())
        {
			if(!movementCardsBeforeDraw.Contains(card))
			{
				//Dunno if we should curse this card if the Player is cursed since technically it's not a card from their hand...
				duelPlayerInformation.SelectedMovementCards.Add(card);
				foreach(Transform child in duelPlayerInformation.CardDuelResolveHolderObject.transform.GetChild(0))
				{
					MovementCardDuel movementCardDuel = child.GetComponent<MovementCardDuel>();
					movementCardDuel.SetupCard(card);
				}

				break;
			}
        }

		base.EffectOfCard(duelPlayerInformation, cardPlayed);
	}

    public override bool CanCostBePaid(DuelPlayerInformation playerDuelInfo, Card cardPlayer = null, bool justChecking = false)
    {
		bool canCostBePaid = false;

		if(playerDuelInfo.PlayerInDuel.MovementCardsInHandCount >= MinimumMovementCardsRequiredInHand)
		{
			canCostBePaid = true;
		}
		
        return canCostBePaid;
    }
}
