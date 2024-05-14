//Code written by Mohamed Riaz Khan of BukuGames.
//All code is written by me (Above name) unless otherwise stated via comments below.
//Not authorized for use outside of the Github repository of this game developed by BukuGames.
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DuelMovementCardPhaseState : BaseState
{
	DuelPhaseSM duelPhaseSM;
	private const string stateName = "DuelMovementCardPhaseState";
	public DuelMovementCardPhaseState(DuelPhaseSM stateMachine) : base(stateName, stateMachine)
	{
		duelPhaseSM = stateMachine as DuelPhaseSM;
	}

	public override void Enter()
	{
		//Check which player we are in the duelPhaseSM Players list.
		base.Enter();
        PhaseDisplay.instance.TurnOnDisplay($"Choose movement card", 1.5f);
        PhaseDisplay.instance.displayTimeCompleted += Logic;
    }

	public override void UpdateLogic()
	{
		base.UpdateLogic();
	}

	public override void Exit()
	{
		//Moving to support card placement...
		base.Exit();
	}

	public void MovementCardSelected(List<MovementCard> movementCards)
	{
		DialogueBoxPopup.instance.DeactivatePopup();
		duelPhaseSM.gameplayManager.HandDisplayPanel.ShrinkHand();
		duelPhaseSM.CurrentPlayerBeingHandled.PlayerInDuel.HideHand();

		int index = duelPhaseSM.PlayersInCurrentDuel.IndexOf(duelPhaseSM.CurrentPlayerBeingHandled);

		duelPhaseSM.PlayersInCurrentDuel[index].SelectedMovementCards = movementCards;
		duelPhaseSM.CurrentPlayerBeingHandled = duelPhaseSM.PlayersInCurrentDuel[index];
        Debug.Log($"{duelPhaseSM.PlayersInCurrentDuel[index].SelectedMovementCards.Count} Are the amount of movement cards that Player {duelPhaseSM.PlayersInCurrentDuel[index].PlayerInDuel.playerIDIntVal} has selected for the duel.");
		duelPhaseSM.ChangeState(duelPhaseSM.duelSupportCardPhaseState);
	}
	private void Logic()
	{
        PhaseDisplay.instance.displayTimeCompleted -= Logic;
		if(duelPhaseSM.CurrentPlayerBeingHandled.PlayerInDuel.MovementCardsInHandCount == 0)
		{
			//Give them the next on in the deck. Do a popup. After popup is gone then move to support card phase.
		}
		else
		{
            DialogueBoxPopup.instance.ActivatePopupWithJustText($"Player {duelPhaseSM.CurrentPlayerBeingHandled.PlayerInDuel.playerIDIntVal}: Please select a movement card to use for the duel.", 0, "Card selection");
            duelPhaseSM.CurrentPlayerBeingHandled.PlayerInDuel.ShowHand();
        }
		
    }
}
