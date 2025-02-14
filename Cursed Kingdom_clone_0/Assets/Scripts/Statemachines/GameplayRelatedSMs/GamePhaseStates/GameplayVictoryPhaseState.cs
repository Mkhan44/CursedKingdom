//Code written by Mohamed Riaz Khan of BukuGames.
//All code is written by me (Above name) unless otherwise stated via comments below.
//Not authorized for use outside of the Github repository of this game developed by BukuGames.
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameplayVictoryPhaseState : BaseState
{
    private GameplayPhaseSM gameplayPhaseSM;
    private Player winningPlayer;
    private const string stateName = "GameplayVictoryPhaseState";

    public GameplayVictoryPhaseState(GameplayPhaseSM stateMachine) : base(stateName, stateMachine)
    {
        gameplayPhaseSM = stateMachine as GameplayPhaseSM;
    }

    public override void Enter()
    {
        base.Enter();
        Victory();
    }

    public override void UpdateLogic()
    {
        base.UpdateLogic();
    }

    public override void Exit()
    {
        base.Exit();
    }

    public void Victory()
    {
        gameplayPhaseSM.gameplayManager.Victory(gameplayPhaseSM.gameplayManager.playerCharacter.GetComponent<Player>());
    }



}
