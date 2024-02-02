//Code written by Mohamed Riaz Khan of BukuGames.
//All code is written by me (Above name) unless otherwise stated via comments below.
//Not authorized for use outside of the Github repository of this game developed by BukuGames.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameplayPhaseSM : BukuStateMachine
{

	//States
	public GameplayStartPhaseState gameplayStartPhase;
	public GameplayStartResolveSpacePhaseState gameplayStartResolveSpacePhaseState;
	public GameplayMovementPhaseState gameplayMovementPhaseState;
	public GameplayDuelPhaseState gameplayDuelPhaseState;
	public GameplayResolveSpacePhaseState gameplayResolveSpacePhaseState;
	public GameplayEndPhaseState gameplayEndPhaseState;

	//Victory has happened
	public GameplayVictoryPhaseState gameplayVictoryPhaseState;

    public GameplayManager gameplayManager;

	private void Awake()
	{
		gameplayStartPhase = new GameplayStartPhaseState(this);
		gameplayStartResolveSpacePhaseState = new GameplayStartResolveSpacePhaseState(this);
		gameplayMovementPhaseState = new GameplayMovementPhaseState(this);
		gameplayDuelPhaseState = new GameplayDuelPhaseState(this);
		gameplayResolveSpacePhaseState = new GameplayResolveSpacePhaseState(this);
		gameplayEndPhaseState = new GameplayEndPhaseState(this);
        gameplayVictoryPhaseState = new GameplayVictoryPhaseState(this);

        gameplayManager = GetComponent<GameplayManager>();

	}

	protected override BaseState GetInitialState()
	{
		return gameplayStartPhase;
	}

    /// <summary>
    /// Takes in a list of objects in this order: BaseState stateToChangeTo
    /// </summary>
    /// <returns></returns>
    public IEnumerator StartTurnConfirmation(List<object> objects)
    {
        yield return null;

        BaseState stateToChangeTo = (BaseState)objects[0];

        gameplayManager.HandDisplayPanel.ShrinkHand(false);
        ChangeState(stateToChangeTo);
    }

}
