//Code written by Mohamed Riaz Khan of BukuGames.
//All code is written by me (Above name) unless otherwise stated via comments below.
//Not authorized for use outside of the Github repository of this game developed by BukuGames.
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameplayStartResolveSpacePhaseState : BaseState
{
	private GameplayPhaseSM gameplayPhaseSM;
	private Player currentPlayer;
	private const string stateName = "GameplayStartResolveSpacePhase";

	private bool startedHandlingPlayerSpaceStartEffects;
	public GameplayStartResolveSpacePhaseState(GameplayPhaseSM stateMachine) : base(stateName, stateMachine)
	{
		gameplayPhaseSM = stateMachine as GameplayPhaseSM;
	}


	public override void Enter()
	{
		base.Enter();
		currentPlayer = gameplayPhaseSM.gameplayManager.GetCurrentPlayer();
		startedHandlingPlayerSpaceStartEffects = false;
        PhaseDisplay.instance.TurnOnDisplay("Resolve start space effects phase!", 1.5f);
        PhaseDisplay.instance.displayTimeCompleted += AttemptToActivateSpaceEffects;
        SubscribeToPlayerEvents();
		gameplayPhaseSM.gameplayManager.HandDisplayPanel.ShrinkHand();
		currentPlayer.HideHand();
	}

	public override void UpdateLogic()
	{
		base.UpdateLogic();
	}

	public void AttemptToActivateSpaceEffects()
	{
        if (DoesSpaceHaveStartSpaceEffects())
        {
            currentPlayer.ShowHand();
            HandleStartOfTurnSpaceEffects();
            startedHandlingPlayerSpaceStartEffects = true;
        }
        else
        {
            gameplayPhaseSM.ChangeState(gameplayPhaseSM.gameplayMovementPhaseState);
        }
    }

	public override void Exit()
	{
		base.Exit();
		UnsubscribeToPlayerEvents();
		startedHandlingPlayerSpaceStartEffects = false;
        PhaseDisplay.instance.displayTimeCompleted -= AttemptToActivateSpaceEffects;
    }

	private void SubscribeToPlayerEvents()
	{
		currentPlayer.FinishedHandlingCurrentSpaceEffects += DoneHandlingStartSpaceEffects;
	}

	private void UnsubscribeToPlayerEvents()
	{
		currentPlayer.FinishedHandlingCurrentSpaceEffects -= DoneHandlingStartSpaceEffects;
	}

	private void DoneHandlingStartSpaceEffects(Player player)
	{
		gameplayPhaseSM.ChangeState(gameplayPhaseSM.gameplayMovementPhaseState);
	}

	private bool DoesSpaceHaveStartSpaceEffects()
	{
		bool hasStartSpaceEffects = false;
		Player currentPlayer = gameplayPhaseSM.gameplayManager.GetCurrentPlayer();
		Space spaceToCheck;

		spaceToCheck = currentPlayer.CurrentSpacePlayerIsOn;
		if (DebugModeSingleton.instance.IsDebugActive)
		{
			Space tempSpace = DebugModeSingleton.instance.OverrideSpaceLandEffect();

			if(tempSpace != null)
			{
				spaceToCheck = tempSpace;
			}
		}

		foreach (SpaceData.SpaceEffect spaceEffect in spaceToCheck.spaceData.spaceEffects)
		{
			if(spaceEffect.spaceEffectData.OnSpaceTurnStartEffect)
			{
				hasStartSpaceEffects = true;
				break;
			}
		}

		return hasStartSpaceEffects;
	}

	private void HandleStartOfTurnSpaceEffects()
	{
		Player currentPlayer = gameplayPhaseSM.gameplayManager.GetCurrentPlayer();
		Space currentSpace = currentPlayer.CurrentSpacePlayerIsOn;

		if (DebugModeSingleton.instance.IsDebugActive)
		{
			Space tempSpace = DebugModeSingleton.instance.OverrideSpaceLandEffect();
			
			if (tempSpace != null)
			{
				gameplayPhaseSM.gameplayManager.SpaceArtworkPopupDisplay.TurnOnDisplay(tempSpace, currentPlayer);
			}
			else
			{
				gameplayPhaseSM.gameplayManager.SpaceArtworkPopupDisplay.TurnOnDisplay(currentSpace, currentPlayer);
			}
		}
		else
		{
			gameplayPhaseSM.gameplayManager.SpaceArtworkPopupDisplay.TurnOnDisplay(currentSpace, currentPlayer);
		}
	}

	//Prolly move this logic into start turn resolve space effect????
	public void ActivateStartTurnPopupWithStartTurnSpaceEffects(Player nextPlayer, Space spaceStartEffectToTrigger)
	{
		List<Tuple<string, string, object, List<object>>> insertedParams = new();

		List<object> paramsList = new();
		paramsList.Add(nextPlayer);
		paramsList.Add(spaceStartEffectToTrigger);

		insertedParams.Add(Tuple.Create<string, string, object, List<object>>("Start!", nameof(StartTurnConfirmation), this, paramsList));

		DialogueBoxPopup.instance.ActivatePopupWithButtonChoices($"Player {nextPlayer.playerIDIntVal}'s turn!", insertedParams, 1, "Turn start!");
	}

	/// <summary>
	/// Takes in a list of objects in this order: Player nextPlayer, Space spaceStartEffectToTrigger.
	/// </summary>
	/// <returns></returns>
	public IEnumerator StartTurnConfirmation(List<object> objects)
	{
		yield return null;
		Player player = (Player)objects[0];
		Space spaceEffectToTrigger = (Space)objects[1];

		if (DebugModeSingleton.instance.IsDebugActive)
		{
			Space tempSpace = DebugModeSingleton.instance.OverrideSpaceLandEffect();
			if (tempSpace != null)
			{
				gameplayPhaseSM.gameplayManager.SpaceArtworkPopupDisplay.TurnOnDisplay(tempSpace, player);
			}
			else
			{
				gameplayPhaseSM.gameplayManager.SpaceArtworkPopupDisplay.TurnOnDisplay(spaceEffectToTrigger, player);
			}
		}
		else
		{
			gameplayPhaseSM.gameplayManager.SpaceArtworkPopupDisplay.TurnOnDisplay(spaceEffectToTrigger, player);
		}
	}
}
