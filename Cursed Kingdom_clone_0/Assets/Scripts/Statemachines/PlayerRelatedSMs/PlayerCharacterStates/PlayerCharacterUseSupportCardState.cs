//Code written by Mohamed Riaz Khan of BukuGames.
//All code is written by me (Above name) unless otherwise stated via comments below.
//Not authorized for use outside of the Github repository of this game developed by BukuGames.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCharacterUseSupportCardState : BaseState
{
	PlayerCharacterSM playerCharacterSM;
	private const string stateName = "PlayerCharacterUseSupportCardState";
	public PlayerCharacterUseSupportCardState(PlayerCharacterSM stateMachine) : base(stateName, stateMachine)
	{
		playerCharacterSM = stateMachine as PlayerCharacterSM;
	}


	public override void Enter()
	{
		base.Enter();
	}

	public override void UpdateLogic()
	{
		base.UpdateLogic();
	}
}
