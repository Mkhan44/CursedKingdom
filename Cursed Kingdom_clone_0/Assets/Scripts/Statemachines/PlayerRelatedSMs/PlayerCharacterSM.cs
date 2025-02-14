//Code written by Mohamed Riaz Khan of BukuGames.
//All code is written by me (Above name) unless otherwise stated via comments below.
//Not authorized for use outside of the Github repository of this game developed by BukuGames.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCharacterSM : BukuStateMachine
{
	[SerializeField] public const string NEGATIVEEFFECT = "NegativeEffect";
	[SerializeField] public const string POSITIVEEFFECT = "PositiveEffect";
	public readonly string ISMOVINGPARAMETER = "IsMoving";
	
	//States
	public PlayerCharacterIdleState playerCharacterIdleState;
	public PlayerCharacterMoveState playerCharacterMoveState;
	public PlayerCharacterUseSupportCardState playerCharacterUseSupportCardState;

	public Animator playerAnimator;
	public Player player;
	
	private void Awake() 
	{
		playerCharacterIdleState = new PlayerCharacterIdleState(this);
		playerCharacterMoveState = new PlayerCharacterMoveState(this);
		playerCharacterUseSupportCardState = new PlayerCharacterUseSupportCardState(this);
		player = GetComponent<Player>();
		playerAnimator = GetComponent<Animator>();

	}

	protected override BaseState GetInitialState()
	{
		return playerCharacterIdleState;
	}


}
