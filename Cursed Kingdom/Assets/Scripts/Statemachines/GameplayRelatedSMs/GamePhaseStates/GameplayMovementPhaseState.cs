//Code written by Mohamed Riaz Khan of BukuGames.
//All code is written by me (Above name) unless otherwise stated via comments below.
//Not authorized for use outside of the Github repository of this game developed by BukuGames.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameplayMovementPhaseState : BaseState
{
	private GameplayPhaseSM gameplayPhaseSM;
	private Player currentPlayer;
	private bool playerStartedMoving;
	private bool playerWentToZeroCards;
	private const string stateName = "GameplayMovementPhaseState";
	public GameplayMovementPhaseState(GameplayPhaseSM stateMachine) : base(stateName, stateMachine)
	{
		gameplayPhaseSM = stateMachine as GameplayPhaseSM;
	}


	public override void Enter()
	{
		base.Enter();
		currentPlayer = gameplayPhaseSM.gameplayManager.GetCurrentPlayer();
		playerStartedMoving = false;
		playerWentToZeroCards = false;
        PhaseDisplay.instance.TurnOnDisplay("Movement phase!", 1.5f);
        currentPlayer.ShowHand();
    }

	public override void UpdateLogic()
	{
		base.UpdateLogic();
		if(!playerWentToZeroCards && currentPlayer.MovementCardsInHand == 0 && currentPlayer.NumMovementCardsUsedThisTurn == 0)
		{
			currentPlayer.GameplayManagerRef.UseMovementCardNoCardsInHandButton.gameObject.SetActive(true);
			currentPlayer.GameplayManagerRef.UseMovementCardNoCardsInHandButton.onClick.RemoveAllListeners();
			currentPlayer.GameplayManagerRef.UseMovementCardNoCardsInHandButton.onClick.AddListener(currentPlayer.DrawThenUseMovementCardImmediately);
			playerWentToZeroCards = true;
        }
		if(currentPlayer.NumMovementCardsUsedThisTurn != 0 && currentPlayer.SpacesLeftToMove == 0)
		{
			gameplayPhaseSM.ChangeState(gameplayPhaseSM.gameplayResolveSpacePhaseState);
		}
	}

	public override void Exit()
	{
		base.Exit();
		playerStartedMoving = false;
	}
}
