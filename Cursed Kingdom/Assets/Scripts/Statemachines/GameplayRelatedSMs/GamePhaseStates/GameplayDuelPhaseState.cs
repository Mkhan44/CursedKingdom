//Code written by Mohamed Riaz Khan of BukuGames.
//All code is written by me (Above name) unless otherwise stated via comments below.
//Not authorized for use outside of the Github repository of this game developed by BukuGames.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameplayDuelPhaseState : BaseState
{
    GameplayPhaseSM gameplayPhaseSM;
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
        gameplayPhaseSM.gameplayManager.HandDisplayPanel.ShrinkHand(false);
        gameplayPhaseSM.gameplayManager.GetCurrentPlayer().HideHand();
    }

    public override void UpdateLogic()
    {
        base.UpdateLogic();
        
    }

    public override void Exit()
    {
        base.Exit();
        PhaseDisplay.instance.displayTimeCompleted -= Logic;
    }

    public void Logic()
    {
        gameplayPhaseSM.ChangeState(gameplayPhaseSM.gameplayEndPhaseState);
    }
}
