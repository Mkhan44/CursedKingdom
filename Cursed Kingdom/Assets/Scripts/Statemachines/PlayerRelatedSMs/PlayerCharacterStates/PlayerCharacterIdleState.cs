//Code written by Mohamed Riaz Khan of BukuGames.
//All code is written by me (Above name) unless otherwise stated via comments below.
//Not authorized for use outside of the Github repository of this game developed by BukuGames.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCharacterIdleState : BaseState
{
	PlayerCharacterSM playerCharacterSM;
	private const string stateName = "PlayerCharacterIdleState";
	public PlayerCharacterIdleState(PlayerCharacterSM stateMachine) : base(stateName, stateMachine)
	{
		playerCharacterSM = stateMachine as PlayerCharacterSM;
	}


	public override void Enter()
	{
		base.Enter();
		playerCharacterSM.playerAnimator.SetBool(playerCharacterSM.ISMOVINGPARAMETER, false);
		//Debug.Log($"Player number {playerCharacterSM.GetComponent<Player>().playerIDIntVal}'s entry of Idle state has been successful!");
	}

	public override void UpdateLogic()
	{
		base.UpdateLogic();
		//Do idle stuffs.
	}
}
