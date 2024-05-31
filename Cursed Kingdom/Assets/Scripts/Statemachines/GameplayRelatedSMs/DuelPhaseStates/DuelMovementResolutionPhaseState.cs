//Code written by Mohamed Riaz Khan of BukuGames.
//All code is written by me (Above name) unless otherwise stated via comments below.
//Not authorized for use outside of the Github repository of this game developed by BukuGames.
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DuelMovementResolutionPhaseState : BaseState
{
	private DuelPhaseSM duelPhaseSM;
	private const string stateName = "DuelMovementResolutionPhaseState";
	private int currentHighestCardValue;

    public DuelMovementResolutionPhaseState(DuelPhaseSM stateMachine) : base(stateName, stateMachine)
	{
		duelPhaseSM = stateMachine as DuelPhaseSM;
		currentHighestCardValue = 0;
    }

	public override void Enter()
	{
		//Check which player we are in the duelPhaseSM Players list.
		base.Enter();
		PhaseDisplay.instance.TurnOnDisplay($"Resolve movement cards", 1.5f);
		PhaseDisplay.instance.displayTimeCompleted += Logic;
	}

	public override void UpdateLogic()
	{
		base.UpdateLogic();
	}

	public override void Exit()
	{
		base.Exit();
		currentHighestCardValue = 0;
    }

	public void Logic()
	{
        PhaseDisplay.instance.displayTimeCompleted -= Logic;
        DialogueBoxPopup.instance.ActivatePopupWithJustText($"Player {duelPhaseSM.CurrentPlayerBeingHandled.PlayerInDuel.playerIDIntVal}'s movement card value is: {duelPhaseSM.CurrentPlayerBeingHandled.SelectedMovementCards[0].MovementCardValue}", 0, "Movement card resolution");
		if(duelPhaseSM.CurrentWinners.Count == 0)
		{
            duelPhaseSM.CurrentWinners.Add(duelPhaseSM.CurrentPlayerBeingHandled);
			currentHighestCardValue = duelPhaseSM.CurrentPlayerBeingHandled.SelectedMovementCards[0].GetCurrentCardValue();
        }
		else
		{
            if (duelPhaseSM.CurrentPlayerBeingHandled.SelectedMovementCards[0].GetCurrentCardValue() > currentHighestCardValue)
            {
                duelPhaseSM.CurrentWinners.Clear();
                duelPhaseSM.CurrentWinners.Add(duelPhaseSM.CurrentPlayerBeingHandled);
				currentHighestCardValue = duelPhaseSM.CurrentPlayerBeingHandled.SelectedMovementCards[0].GetCurrentCardValue();
            }
            else if (duelPhaseSM.CurrentPlayerBeingHandled.SelectedMovementCards[0].GetCurrentCardValue() == currentHighestCardValue)
            {
                duelPhaseSM.CurrentWinners.Add(duelPhaseSM.CurrentPlayerBeingHandled);
            }
		}

		duelPhaseSM.StartCoroutine(duelPhaseSM.TestingTimeBetweenPopupsMovementCardResolution());
	}
}
