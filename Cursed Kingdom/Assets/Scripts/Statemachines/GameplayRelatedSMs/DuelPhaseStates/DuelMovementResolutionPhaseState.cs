//Code written by Mohamed Riaz Khan of BukuGames.
//All code is written by me (Above name) unless otherwise stated via comments below.
//Not authorized for use outside of the Github repository of this game developed by BukuGames.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class DuelMovementResolutionPhaseState : BaseState
{
	private DuelPhaseSM duelPhaseSM;
	private const string stateName = "DuelMovementResolutionPhaseState";
	private int currentHighestCardValue;

    public DuelMovementResolutionPhaseState(DuelPhaseSM stateMachine) : base(stateName, stateMachine)
	{
		duelPhaseSM = stateMachine as DuelPhaseSM;
		currentHighestCardValue = 0;
    }

	public override void Enter()
	{
		//Check which player we are in the duelPhaseSM Players list.
		base.Enter();
		PhaseDisplay.instance.TurnOnDisplay($"Resolve movement cards", 1.5f);
		PhaseDisplay.instance.displayTimeCompleted += Logic;
	}

	public override void UpdateLogic()
	{
		base.UpdateLogic();
	}

	public override void Exit()
	{
		base.Exit();
		currentHighestCardValue = 0;
    }

	public void Logic()
	{
        PhaseDisplay.instance.displayTimeCompleted -= Logic;
       // DialogueBoxPopup.instance.ActivatePopupWithJustText($"Player {duelPhaseSM.CurrentPlayerBeingHandled.PlayerInDuel.playerIDIntVal}'s movement card value is: {duelPhaseSM.CurrentPlayerBeingHandled.SelectedMovementCards[0].MovementCardValue}", 0, "Movement card resolution");

		if(duelPhaseSM.CurrentPlayerBeingHandled.CardDuelResolveHolderObject != null)
		{
			string animToPlay = "";
			if(duelPhaseSM.PlayersInCurrentDuel.IndexOf(duelPhaseSM.CurrentPlayerBeingHandled) % 2 == 0)
			{
				animToPlay = "FlipRight";
			}
			else
			{
				animToPlay = "FlipLeft";
			}
			duelPhaseSM.CurrentPlayerBeingHandled.CardDuelResolveHolderObject.transform.GetChild(0).GetChild(0).GetComponent<Animator>().Play(animToPlay);
		}

		if(duelPhaseSM.CurrentWinners.Count == 0)
		{
            duelPhaseSM.CurrentWinners.Add(duelPhaseSM.CurrentPlayerBeingHandled);
			currentHighestCardValue = duelPhaseSM.CurrentPlayerBeingHandled.SelectedMovementCards[0].GetCurrentCardValue();
        }
		else
		{
            if (duelPhaseSM.CurrentPlayerBeingHandled.SelectedMovementCards[0].GetCurrentCardValue() > currentHighestCardValue)
            {
                duelPhaseSM.CurrentWinners.Clear();
                duelPhaseSM.CurrentWinners.Add(duelPhaseSM.CurrentPlayerBeingHandled);
				currentHighestCardValue = duelPhaseSM.CurrentPlayerBeingHandled.SelectedMovementCards[0].GetCurrentCardValue();
            }
            else if (duelPhaseSM.CurrentPlayerBeingHandled.SelectedMovementCards[0].GetCurrentCardValue() == currentHighestCardValue)
            {
                duelPhaseSM.CurrentWinners.Add(duelPhaseSM.CurrentPlayerBeingHandled);
            }
		}

		duelPhaseSM.StartCoroutine(duelPhaseSM.TestingTimeBetweenPopupsMovementCardResolution());
	}

	public async void EnterResultPhase()
	{
		foreach (DuelPlayerInformation duelPlayerInformation in duelPhaseSM.PlayersInCurrentDuel)
		{
			
			if(duelPlayerInformation.CardDuelResolveHolderObject != null)
			{
				foreach(Transform child in duelPlayerInformation.CardDuelResolveHolderObject.transform.GetChild(0))
				{
					child.GetComponent<Animator>().Play("ComeUp");
				}
				foreach(Transform child in duelPlayerInformation.CardDuelResolveHolderObject.transform.GetChild(1))
				{
					child.GetComponent<Animator>().Play("ComeUp");
				}
			}
		}

		await Task.Delay(1000);
		
        duelPhaseSM.ChangeState(duelPhaseSM.DuelResultPhaseState);
    }
}
