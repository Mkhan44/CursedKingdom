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
        PhaseDisplay.instance.TurnOnDisplay("Duel!", 1.5f);
        PhaseDisplay.instance.displayTimeCompleted += Logic;
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

    private void Logic()
    {
        PhaseDisplay.instance.displayTimeCompleted -= Logic;
        duelPhaseSM.CurrentPlayerBeingHandled = duelPhaseSM.PlayersInCurrentDuel[0];
        duelPhaseSM.ChangeState(duelPhaseSM.duelSelectCardsToUsePhaseState);
    }

}
