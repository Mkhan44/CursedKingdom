//Code written by Mohamed Riaz Khan of BukuGames.
//All code is written by me (Above name) unless otherwise stated via comments below.
//Not authorized for use outside of the Github repository of this game developed by BukuGames.
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DuelSupportResolutionPhaseState : BaseState
{
	DuelPhaseSM duelPhaseSM;
	private const string stateName = "DuelSupportResolutionPhaseState";
	public DuelSupportResolutionPhaseState(DuelPhaseSM stateMachine) : base(stateName, stateMachine)
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
		base.Exit();
	}
}
