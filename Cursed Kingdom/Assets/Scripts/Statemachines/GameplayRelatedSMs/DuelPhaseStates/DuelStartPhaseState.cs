//Code written by Mohamed Riaz Khan of BukuGames.
//All code is written by me (Above name) unless otherwise stated via comments below.
//Not authorized for use outside of the Github repository of this game developed by BukuGames.
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DuelStartPhaseState : BaseState
{
    DuelPhaseSM duelPhaseSM;
    private const string stateName = "DuelStartPhase";
    public DuelStartPhaseState(DuelPhaseSM stateMachine) : base(stateName, stateMachine)
    {
        duelPhaseSM = stateMachine as DuelPhaseSM;
    }

    public override void Enter()
    {
        base.Enter();
        //Play animation to enter into the duel state.

    }

    public override void UpdateLogic()
    {
        base.UpdateLogic();
    }

    public override void Exit()
    {
        base.Exit();
    }


    /// <summary>
    /// Sorts the players in the duel to be the current player followed by other players in the current turn order.
    /// </summary>
    private void SortPlayersInDuel()
    {

    }

}
