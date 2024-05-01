//Code written by Mohamed Riaz Khan of BukuGames.
//All code is written by me (Above name) unless otherwise stated via comments below.
//Not authorized for use outside of the Github repository of this game developed by BukuGames.
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DuelSupportCardPhaseState : BaseState
{
	DuelPhaseSM duelPhaseSM;
	private const string stateName = "DuelSupportCardPhaseState";
	public DuelSupportCardPhaseState(DuelPhaseSM stateMachine) : base(stateName, stateMachine)
	{
		duelPhaseSM = stateMachine as DuelPhaseSM;
	}

	public override void Enter()
	{
		//Check which player we are in the duelPhaseSM Players list.
		base.Enter();
	}

	public override void UpdateLogic()
	{
		base.UpdateLogic();
	}

	public override void Exit()
	{
		//If we are at the last Player in the list (Check against duelPhaseSM Players list and current player)
		//Then move onto the next phase. Otherwise move onto the next Player and move back to the Movement card placement state.
		base.Exit();
	}
}
