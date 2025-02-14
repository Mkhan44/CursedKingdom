//Code written by Mohamed Riaz Khan of BukuGames.
//All code is written by me (Above name) unless otherwise stated via comments below.
//Not authorized for use outside of the Github repository of this game developed by BukuGames.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DuelNotDuelingPhaseState : BaseState
{
    DuelPhaseSM duelPhaseSM;
    private const string stateName = "DuelNotDuelingPhaseState";
    private bool popupAppeared = false;
    public DuelNotDuelingPhaseState(DuelPhaseSM stateMachine) : base(stateName, stateMachine)
    {
        duelPhaseSM = stateMachine as DuelPhaseSM;
    }


    public override void Enter()
    {
        base.Enter();
        duelPhaseSM.PlayersInCurrentDuel.Clear();
    }

    public override void UpdateLogic()
    {
        base.UpdateLogic();
    }

    public override void Exit()
    {
        base.Exit();
    }

}
