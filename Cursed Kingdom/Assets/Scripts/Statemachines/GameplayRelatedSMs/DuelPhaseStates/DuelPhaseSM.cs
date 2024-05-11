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

	public DuelStartPhaseState duelStartPhaseState;
	public DuelMovementCardPhaseState duelMovementCardPhaseState;
	public DuelSupportCardPhaseState duelSupportCardPhaseState;
	public DuelSupportResolutionPhaseState duelSupportResolutionPhaseState;
	public DuelMovementResolutionPhaseState duelMovementResolutionPhaseState;
	public DuelResultPhaseState DuelResultPhaseState;

	public GameplayManager gameplayManager;

	//Players in duel
	
	//THIS LIST should be in the order of: Player who initiated the duel followed by turn order of other players. If someone comes in via unwillful warp, 
	//they become the last player in the list.
	private List<Tuple<Player, List<MovementCard>, List<SupportCard>>> playersInCurrentDuel;
	private Tuple<Player, List<MovementCard>, List<SupportCard>> currentPlayerBeingHandled;

	public List<Tuple<Player, List<MovementCard>, List<SupportCard>>> PlayersInCurrentDuel { get => playersInCurrentDuel; set => playersInCurrentDuel = value; }
	
	public Tuple<Player, List<MovementCard>, List<SupportCard>> CurrentPlayerBeingHandled { get => currentPlayerBeingHandled; set => currentPlayerBeingHandled = value; }
	private void Awake()
	{
		duelNotDuelingPhaseState = new DuelNotDuelingPhaseState(this);
        duelStartPhaseState = new DuelStartPhaseState(this);
        duelMovementCardPhaseState = new DuelMovementCardPhaseState(this);
        duelSupportCardPhaseState = new DuelSupportCardPhaseState(this);
        duelSupportResolutionPhaseState = new DuelSupportResolutionPhaseState(this);
        duelMovementResolutionPhaseState = new DuelMovementResolutionPhaseState(this);
        DuelResultPhaseState = new DuelResultPhaseState(this);

        gameplayManager = GetComponent<GameplayManager>();
	}

	protected override BaseState GetInitialState()
	{
		PlayersInCurrentDuel = new();
        return duelNotDuelingPhaseState;
	}

	public IEnumerator ChooseNoSupportCardToUseInDuel()
	{
		yield return null;
		duelSupportCardPhaseState.SupportCardNotSelected();
    }

    public IEnumerator TestingTimeBetweenPopups()
    {
        yield return new WaitForSeconds(2.5f);

        DialogueBoxPopup.instance.DeactivatePopup();

        int indexOfCurrentPlayerBeingHandled = PlayersInCurrentDuel.IndexOf(CurrentPlayerBeingHandled);


        if (indexOfCurrentPlayerBeingHandled != PlayersInCurrentDuel.Count - 1)
        {
            //Shouldn't ever go out of bounds.
            CurrentPlayerBeingHandled = PlayersInCurrentDuel[indexOfCurrentPlayerBeingHandled + 1];
            duelSupportResolutionPhaseState.Logic();
        }
        //We're at the final player. Reset back to the first player who initiated the duel and move onto Movementcard resolution phase.
        else
        {
            CurrentPlayerBeingHandled = PlayersInCurrentDuel[0];
            ChangeState(duelMovementResolutionPhaseState);
            Debug.Log("We are entering movement resolution phase.");
        }
    }

}
