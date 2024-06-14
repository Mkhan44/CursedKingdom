//Code written by Mohamed Riaz Khan of BukuGames.
//All code is written by me (Above name) unless otherwise stated via comments below.
//Not authorized for use outside of the Github repository of this game developed by BukuGames.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This support card effect is used when there is a 1v1 duel only. 
/// You can only use this card if you have 1 opponent in the duel. Swap 1 movement card that was used (RNG if multiple) with your opponent and that becomes your movement card for this duel.
/// If you win the duel you deal extra damage to your opponent.
/// If at resolution: more than 1 player is an opponent (EX: Unwillful warp was used) then this card's effect fizzles.
/// </summary>
[CreateAssetMenu(fileName = "SwapMovementCardWithOpponentEffect", menuName = "Card Data/Support Card Effect Data/Swap Movement Card With Opponent Effect", order = 0)]
public class SwapMovementCardWithOpponentEffect : SupportCardEffectData, ISupportEffect
{
	[SerializeField] [Range(1, 10)] private int extraDamageToDeal;

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
		base.EffectOfCard(duelPlayerInformation, cardPlayed);
	}
}
