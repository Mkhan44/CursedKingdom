//Code written by Mohamed Riaz Khan of BukuGames.
//All code is written by me (Above name) unless otherwise stated via comments below.
//Not authorized for use outside of the Github repository of this game developed by BukuGames.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameplayDuelPhaseState : BaseState
{
    private GameplayPhaseSM gameplayPhaseSM;
    private const string stateName = "GameplayDuelPhase";
    public GameplayDuelPhaseState(GameplayPhaseSM stateMachine) : base(stateName, stateMachine)
    {
        gameplayPhaseSM = stateMachine as GameplayPhaseSM;
    }


    public override void Enter()
    {
        base.Enter();
        PhaseDisplay.instance.TurnOnDisplay("Duel phase!", 1.5f);
        PhaseDisplay.instance.displayTimeCompleted += Logic;
        gameplayPhaseSM.gameplayManager.DuelPhaseSMRef.FadePanelCompletedFadingDuel += TurnOnCameraAfterFade;
        SpawnInPlayerDuelPrefabs();
        gameplayPhaseSM.gameplayManager.HandDisplayPanel.ShrinkHand(false);
        gameplayPhaseSM.gameplayManager.GetCurrentPlayer().HideHand();
        foreach(DuelPlayerInformation duelPlayerInformation in gameplayPhaseSM.gameplayManager.DuelPhaseSMRef.PlayersInCurrentDuel)
        {
            gameplayPhaseSM.gameplayManager.DuelPhaseSMRef.StartCoroutine(gameplayPhaseSM.gameplayManager.DuelPhaseSMRef.CharacterDuelAnimationTransition(duelPlayerInformation));
        }
    }

    public override void UpdateLogic()
    {
        base.UpdateLogic();
    }

    public override void Exit()
    {
        base.Exit();
    }

    public void SpawnInPlayerDuelPrefabs()
    {
        if(gameplayPhaseSM.gameplayManager.duelPlaneSpawnPointsParent != null)
        {
            int currentIndex = 0;
            foreach(DuelPlayerInformation duelPlayerInformation in gameplayPhaseSM.gameplayManager.DuelPhaseSMRef.PlayersInCurrentDuel)
            {
                GameObject newPlayerDuelPrefabObj;
                //Don't spawn under the parent. Just use the parents' transform to spawn it into the world. Look at how we're spawning in the Players onto the board.
                newPlayerDuelPrefabObj = Object.Instantiate(gameplayPhaseSM.gameplayManager.PlayerDuelPrefab, gameplayPhaseSM.gameplayManager.duelPlaneSpawnPointsParent.transform.GetChild(currentIndex), false);
                newPlayerDuelPrefabObj.transform.parent = null;
                newPlayerDuelPrefabObj.transform.localScale = gameplayPhaseSM.gameplayManager.PlayerDuelPrefab.transform.localScale;
                newPlayerDuelPrefabObj.transform.position = gameplayPhaseSM.gameplayManager.duelPlaneSpawnPointsParent.transform.GetChild(currentIndex).position;

                duelPlayerInformation.SetupPlayerDuelPrefabInstance(newPlayerDuelPrefabObj);
                duelPlayerInformation.PlayerDuelAnimator.SetBool(Player.ISDUELINGIDLE, true);
                currentIndex++;
            }
        }
    }

    public void Logic()
    {
        gameplayPhaseSM.gameplayManager.DuelPhaseSMRef.ChangeState(gameplayPhaseSM.gameplayManager.DuelPhaseSMRef.duelStartPhaseState);
        PhaseDisplay.instance.displayTimeCompleted -= Logic;
    }

    public void TurnOnCameraAfterFade()
    {
        gameplayPhaseSM.gameplayManager.DuelPhaseSMRef.FadePanelCompletedFadingDuel -= TurnOnCameraAfterFade;
        gameplayPhaseSM.gameplayManager.GameplayCameraManagerRef.TurnOnVirtualDuelCamera();
    }
}
