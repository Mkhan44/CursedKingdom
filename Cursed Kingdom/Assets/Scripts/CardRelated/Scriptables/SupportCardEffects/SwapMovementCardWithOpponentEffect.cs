//Code written by Mohamed Riaz Khan of BukuGames.
//All code is written by me (Above name) unless otherwise stated via comments below.
//Not authorized for use outside of the Github repository of this game developed by BukuGames.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This support card effect is used when there is a 1v1 duel only. 
/// You can only use this card if you have 1 opponent in the duel. Swap 1 movement card's current value that was used (RNG if multiple) with your opponent and that becomes your movement card's current value for this duel.
/// If you win the duel you deal extra damage to your opponent.
/// If at resolution: more than 1 player is an opponent (EX: Unwillful warp was used) then this card's effect fizzles.
/// </summary>
[CreateAssetMenu(fileName = "SwapMovementCardValueWithOpponentEffect", menuName = "Card Data/Support Card Effect Data/Swap Movement Card Value With Opponent Effect", order = 0)]
public class SwapMovementCardValueWithOpponentEffect : SupportCardEffectData, ISupportEffect
{
	[SerializeField] [Range(1, 10)] private int extraDamageToDeal = 1;

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
		//Only 1 movement card. So yeah.
		if(duelPlayerInformation.SelectedMovementCards.Count < 2)
		{
			foreach(DuelPlayerInformation playerInformation in duelPlayerInformation.PlayerInDuel.GameplayManagerRef.DuelPhaseSMRef.PlayersInCurrentDuel)
			{
				if(playerInformation != duelPlayerInformation)
				{
					int currentPlayerMovementValue;
					int opponentPlayerMovementValue;

					if(duelPlayerInformation.SelectedMovementCards[0].TempCardValue != 0)
					{
						currentPlayerMovementValue = duelPlayerInformation.SelectedMovementCards[0].TempCardValue;
					}
					else
					{
						currentPlayerMovementValue = duelPlayerInformation.SelectedMovementCards[0].MovementCardValue;
					}

					if(playerInformation.SelectedMovementCards[0].TempCardValue != 0)
					{
						opponentPlayerMovementValue = playerInformation.SelectedMovementCards[0].TempCardValue;
					}
					else
					{
						opponentPlayerMovementValue = playerInformation.SelectedMovementCards[0].MovementCardValue;
					}

					duelPlayerInformation.SelectedMovementCards[0].TempCardValue = opponentPlayerMovementValue;
					playerInformation.SelectedMovementCards[0].TempCardValue = currentPlayerMovementValue;
					break;
				}
			}

		}

		base.EffectOfCard(duelPlayerInformation, cardPlayed);
	}

	public override bool CanCostBePaid(DuelPlayerInformation duelPlayerInformation, Card cardPlayed = null, bool justChecking = false)
    {
        bool canCostBePaid = false;
		if(duelPlayerInformation.PlayerInDuel.GameplayManagerRef.DuelPhaseSMRef.PlayersInCurrentDuel.Count < 3)
		{
			canCostBePaid = true;
		}

        if(!canCostBePaid && !justChecking)
        {
            DialogueBoxPopup.instance.ActivatePopupWithJustText("You have more than 1 opponent in the duel currently!", 1.5f);
        }

        return canCostBePaid;
    }
}
