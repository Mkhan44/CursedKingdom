//Code written by Mohamed Riaz Khan of BukuGames.
//All code is written by me (Above name) unless otherwise stated via comments below.
//Not authorized for use outside of the Github repository of this game developed by BukuGames.
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class GameplayResolveSpacePhaseState : BaseState
{
	private GameplayPhaseSM gameplayPhaseSM;
	private Player currentPlayer;
	private bool isResolvingSpaceEffect;
	private const string stateName = "GameplayResolveSpacePhaseState";
	public GameplayResolveSpacePhaseState(GameplayPhaseSM stateMachine) : base(stateName, stateMachine)
	{
		gameplayPhaseSM = stateMachine as GameplayPhaseSM;
	}


	public override void Enter()
	{
		base.Enter();
		currentPlayer = gameplayPhaseSM.gameplayManager.GetCurrentPlayer();
		SubscribeToPlayerEvents();
		isResolvingSpaceEffect = false;
        PhaseDisplay.instance.TurnOnDisplay("Resolve space effects phase!", 1.5f);
        PhaseDisplay.instance.displayTimeCompleted += StartResolvingSpaceEffects;
		gameplayPhaseSM.gameplayManager.HandDisplayPanel.gameObject.SetActive(true);
        gameplayPhaseSM.gameplayManager.HandDisplayPanel.ShrinkHand();
        currentPlayer.HideHand();
    }

	public override void UpdateLogic()
	{
		base.UpdateLogic();
	}

	public void StartResolvingSpaceEffects()
	{
        if (!isResolvingSpaceEffect)
        {
            isResolvingSpaceEffect = true;
            //currentPlayer.CurrentSpacePlayerIsOn.StartCoroutine(currentPlayer.CurrentSpacePlayerIsOn.PlaySpaceInfoDisplayAnimationUI(currentPlayer));
            gameplayPhaseSM.gameplayManager.SpaceArtworkPopupDisplay.TurnOnDisplay(currentPlayer.CurrentSpacePlayerIsOn, currentPlayer);
			//For AI players.
			gameplayPhaseSM.gameplayManager.SpaceArtworkPopupDisplay.SpaceArtworkDisplayTurnOff += SpaceArtworkDonePopupDone;
            currentPlayer.ShowHand();
        }
    }

	public override void Exit()
	{
		base.Exit();
		UnsubscribeToPlayerEvents();
		isResolvingSpaceEffect = false;
        PhaseDisplay.instance.displayTimeCompleted -= StartResolvingSpaceEffects;
    }

	private void SpaceArtworkDonePopupDone(Player player)
	{
		gameplayPhaseSM.StartCoroutine(gameplayPhaseSM.SpaceArtworkPopupDoneDelay(player));
	}
	
	
	private void SubscribeToPlayerEvents()
	{
		currentPlayer.FinishedHandlingCurrentSpaceEffects += FinishedResolvingSpaceEffects;
	}

	private void UnsubscribeToPlayerEvents()
	{
		currentPlayer.FinishedHandlingCurrentSpaceEffects -= FinishedResolvingSpaceEffects;
	}
	private void FinishedResolvingSpaceEffects(Player player)
	{
		gameplayPhaseSM.ChangeState(gameplayPhaseSM.gameplayEndPhaseState);
	}
}
