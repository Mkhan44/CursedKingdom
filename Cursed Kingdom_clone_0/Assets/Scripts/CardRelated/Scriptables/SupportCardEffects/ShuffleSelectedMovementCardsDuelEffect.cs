//Code written by Mohamed Riaz Khan of BukuGames.
//All code is written by me (Above name) unless otherwise stated via comments below.
//Not authorized for use outside of the Github repository of this game developed by BukuGames.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/*
When using this support card during a duel; it is executed first before any other support cards. 
(If multiple are used, it executes each in turn order during the duel). 
Before any other supports are used: Shuffles all movement cards around and potentially gives them to new owners. 
So let’s say player 1 ,2 , and 3 are in a duel: Player 1 uses a 1 card, player 2 uses a 9 card, player 3 uses a 5 card. 
If Player 1 uses Magical manipulation then the 1, 9 and 5 will be shuffled around and possibly can now end up in player’s movement usages for that duel. 
It IS possible to get the same card you started with just so 1v1s aren’t a guaranteed shuffle.
*/
[CreateAssetMenu(fileName = "ShuffleSelectedMovementCardsDuelEffect", menuName = "Card Data/Support Card Effect Data/Shuffle Selected Movement Cards Duel Effect", order = 0)]
public class ShuffleSelectedMovementCardsDuelEffect : SupportCardEffectData, ISupportEffect
{
	[SerializeField] private bool canObtainSameCardsAfterShuffle;

    public bool CanObtainSameCardsAfterShuffle { get => canObtainSameCardsAfterShuffle; set => canObtainSameCardsAfterShuffle = value; }

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
		SupportCard cardUsed = (SupportCard)cardPlayed;
		if (cardUsed != null)
		{
			supportCardThatWasJustUsed = cardUsed;
		}
		
		List<MovementCard> listOfSelectedMovementCardsThatWillBeSwapped = new();

		foreach(DuelPlayerInformation playerInformation in duelPlayerInformation.PlayerInDuel.GameplayManagerRef.DuelPhaseSMRef.PlayersInCurrentDuel)
		{
			//If the player only has 1 card selected, we use that card specifically, otherwise we randomly choose an index of their selected cards.
			if(playerInformation.SelectedMovementCards.Count < 2)
			{
				listOfSelectedMovementCardsThatWillBeSwapped.Add(playerInformation.SelectedMovementCards[0]);
			}
			else
			{
				//We need a way to keep track of which card is being swapped so when we give them a new card it's in the same spot of their selected list...
				int randomIndexToSelect = Random.Range(0, playerInformation.SelectedMovementCards.Count);
				listOfSelectedMovementCardsThatWillBeSwapped.Add(playerInformation.SelectedMovementCards[randomIndexToSelect]);
			}
		}

		int newCardValueIndex = 0;
		int newCardValue = 0;
		List<int> valuesToSwapOnMovementCards = new List<int>();

		foreach(MovementCard movementCard in listOfSelectedMovementCardsThatWillBeSwapped)
		{
			if(movementCard.TempCardValue > 0)
			{
				valuesToSwapOnMovementCards.Add(movementCard.TempCardValue);
			}
			else
			{
				valuesToSwapOnMovementCards.Add(movementCard.MovementCardValue);
			}

		}
		foreach(DuelPlayerInformation playerInformation in duelPlayerInformation.PlayerInDuel.GameplayManagerRef.DuelPhaseSMRef.PlayersInCurrentDuel)
		{
			//For now assume everyone has 1 card, we'll do the 2nd + cards later...
			if(valuesToSwapOnMovementCards.Count > 1)
			{
				newCardValueIndex = Random.Range(0,valuesToSwapOnMovementCards.Count);
				newCardValue = valuesToSwapOnMovementCards[newCardValueIndex];
			}
			else
			{
				newCardValueIndex = 0;
				newCardValue = valuesToSwapOnMovementCards[0];
			}

			Debug.Log($"Player: {playerInformation.PlayerInDuel.playerIDIntVal}'s card went from value of: {playerInformation.SelectedMovementCards[0].MovementCardValue} to {newCardValue}");

			playerInformation.SelectedMovementCards[0].TempCardValue = newCardValue;

			valuesToSwapOnMovementCards.RemoveAt(newCardValueIndex);
		}
		

		AnimateMovementCards(duelPlayerInformation, supportCardThatWasJustUsed, this);
	}

	public void AnimateMovementCards(DuelPlayerInformation duelPlayerInformation, SupportCard cardPlayed, SupportCardEffectData effectData)
	{
		duelPlayerInformation.PlayerInDuel.GameplayManagerRef.DuelPhaseSMRef.StartCoroutine(duelPlayerInformation.PlayerInDuel.GameplayManagerRef.DuelPhaseSMRef.ShuffleSelectedMovementCardsAnim(effectData));
	}
}
