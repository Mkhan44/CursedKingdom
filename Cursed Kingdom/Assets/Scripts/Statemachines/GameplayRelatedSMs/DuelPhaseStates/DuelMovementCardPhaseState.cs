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
		duelPhaseSM.CurrentPlayerBeingHandled.Item1.HideHand();

        Tuple<Player, List<MovementCard>, List<SupportCard>> movementCardTuple = new Tuple<Player, List<MovementCard>, List<SupportCard>>(duelPhaseSM.CurrentPlayerBeingHandled.Item1, movementCards, duelPhaseSM.CurrentPlayerBeingHandled.Item3);
		int index = duelPhaseSM.PlayersInCurrentDuel.IndexOf(duelPhaseSM.CurrentPlayerBeingHandled);

		duelPhaseSM.PlayersInCurrentDuel[index] = movementCardTuple;
		Debug.Log($"{duelPhaseSM.PlayersInCurrentDuel[index].Item2.Count} Are the amount of movement cards that Player {duelPhaseSM.PlayersInCurrentDuel[index].Item1.playerIDIntVal} has selected for the duel.");
	}
	private void Logic()
	{
        PhaseDisplay.instance.displayTimeCompleted -= Logic;
		if(duelPhaseSM.CurrentPlayerBeingHandled.Item1.MovementCardsInHandCount == 0)
		{
			//Give them the next on in the deck. Do a popup. After popup is gone then move to support card phase.
		}
		else
		{
            DialogueBoxPopup.instance.ActivatePopupWithJustText($"Please select a movement card to use for the duel.", 0, "Card selection");
            duelPhaseSM.CurrentPlayerBeingHandled.Item1.ShowHand();
        }
		
    }
}
