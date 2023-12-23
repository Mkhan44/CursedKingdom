//Code written by Mohamed Riaz Khan of BukuGames.
//All code is written by me (Above name) unless otherwise stated via comments below.
//Not authorized for use outside of the Github repository of this game developed by BukuGames.
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameplayStartPhaseState : BaseState
{
	GameplayPhaseSM gameplayPhaseSM;
	private const string stateName = "GameplayStartPhase";
    private bool popupAppeared = false;
	public GameplayStartPhaseState(GameplayPhaseSM stateMachine) : base(stateName, stateMachine)
	{
		gameplayPhaseSM = stateMachine as GameplayPhaseSM;
	}


	public override void Enter()
	{
		base.Enter();
        PhaseDisplay.instance.TurnOnDisplay("Start phase!", 1.5f);
        PhaseDisplay.instance.displayTimeCompleted += ActivateStartTurnPopup;
	}

	public override void UpdateLogic()
	{
		base.UpdateLogic();
	}

    public override void Exit()
    {
        base.Exit();
        PhaseDisplay.instance.displayTimeCompleted -= ActivateStartTurnPopup;
        popupAppeared = false;
    }

    public void ActivateStartTurnPopup()
    {
        List<Tuple<string, string, object, List<object>>> insertedParams = new();

        List<object> paramsList = new List<object>();
        paramsList.Add(gameplayPhaseSM.gameplayStartResolveSpacePhaseState);

        insertedParams.Add(Tuple.Create<string, string, object, List<object>>("Start!", nameof(gameplayPhaseSM.StartTurnConfirmation), gameplayPhaseSM, paramsList));

        DialogueBoxPopup.instance.ActivatePopupWithButtonChoices($"Player {gameplayPhaseSM.gameplayManager.GetCurrentPlayer().playerIDIntVal}'s turn!", insertedParams, 1, "Turn start!");
    }
}
