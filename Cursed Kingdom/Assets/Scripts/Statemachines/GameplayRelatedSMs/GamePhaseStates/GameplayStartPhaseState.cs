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
		Debug.Log($"Start phase entered!");
	}

	public override void UpdateLogic()
	{
		base.UpdateLogic();
        if(!popupAppeared)
        {
            ActivateStartTurnPopup(gameplayPhaseSM.gameplayStartResolveSpacePhaseState);
            popupAppeared = true;
        }
	}

    public override void Exit()
    {
        base.Exit();
        popupAppeared = false;
    }

    public void ActivateStartTurnPopup(BaseState stateToChangeTo)
    {
        List<Tuple<string, string, object, List<object>>> insertedParams = new();

        List<object> paramsList = new List<object>();
        paramsList.Add(stateToChangeTo);

        insertedParams.Add(Tuple.Create<string, string, object, List<object>>("Start!", nameof(gameplayPhaseSM.StartTurnConfirmation), gameplayPhaseSM, paramsList));

        DialogueBoxPopup.instance.ActivatePopupWithButtonChoices($"Player {gameplayPhaseSM.gameplayManager.GetCurrentPlayer().playerIDIntVal}'s turn!", insertedParams, 1, "Turn start!");
    }

   


    
}
