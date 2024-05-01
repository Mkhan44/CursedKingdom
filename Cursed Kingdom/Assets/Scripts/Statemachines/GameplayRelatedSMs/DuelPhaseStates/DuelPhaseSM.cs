//Code written by Mohamed Riaz Khan of BukuGames.
//All code is written by me (Above name) unless otherwise stated via comments below.
//Not authorized for use outside of the Github repository of this game developed by BukuGames.
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DuelPhaseSM : BukuStateMachine
{
	//States
	
	//Default
	public DuelNotDuelingPhaseState duelNotDuelingPhaseState;

	public DuelStartPhaseState duelStartPhase;

	public GameplayManager gameplayManager;

	//Players in duel
	
	//THIS LIST should be in the order of: Player who initiated the duel followed by turn order of other players. If someone comes in via unwillful warp, 
	//they become the last player in the list.
	private List<Tuple<Player, List<MovementCard>, List<SupportCard>>> playersInCurrentDuel;
	private Player currentPlayerBeingHandled;

	public List<Tuple<Player, List<MovementCard>, List<SupportCard>>> PlayersInCurrentDuel { get => playersInCurrentDuel; set => playersInCurrentDuel = value; }
	
	public Player CurrentPlayerBeingHandled { get => currentPlayerBeingHandled; set => currentPlayerBeingHandled = value; }
	private void Awake()
	{
		gameplayManager = GetComponent<GameplayManager>();
	}

	protected override BaseState GetInitialState()
	{
		return duelNotDuelingPhaseState;
	}

}
