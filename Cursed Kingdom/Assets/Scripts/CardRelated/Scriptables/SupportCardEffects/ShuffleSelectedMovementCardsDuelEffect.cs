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
		base.EffectOfCard(duelPlayerInformation, cardPlayed);
	}
}
