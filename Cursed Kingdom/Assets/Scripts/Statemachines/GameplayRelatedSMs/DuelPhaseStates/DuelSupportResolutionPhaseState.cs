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
	private SupportCard currentSupportCardWeAreHandling = new();
	public DuelSupportResolutionPhaseState(DuelPhaseSM stateMachine) : base(stateName, stateMachine)
	{
		duelPhaseSM = stateMachine as DuelPhaseSM;
	}

	public override void Enter()
	{
		//Check which player we are in the duelPhaseSM Players list.
		base.Enter();
        PhaseDisplay.instance.TurnOnDisplay($"Resolve support cards", 1.5f);
        PhaseDisplay.instance.displayTimeCompleted += Logic;
    }

    public void Logic()
    {
        PhaseDisplay.instance.displayTimeCompleted -= Logic;
        if (duelPhaseSM.CurrentPlayerBeingHandled.SelectedSupportCards.Count > 0)
		{
			DialogueBoxPopup.instance.ActivatePopupWithJustText($"Player {duelPhaseSM.CurrentPlayerBeingHandled.PlayerInDuel.playerIDIntVal} used {duelPhaseSM.CurrentPlayerBeingHandled.SelectedSupportCards[0].SupportCardData.name}", 0, "Support card resolution");
			currentSupportCardWeAreHandling = duelPhaseSM.CurrentPlayerBeingHandled.SelectedSupportCards[0];

			//Will need to make this work for all effects. Maybe have something on this that kicks off after all effects are completed?
			currentSupportCardWeAreHandling.SupportCardData.supportCardEffects[0].supportCardEffectData.SupportCardEffectCompleted += AfterSupportCardEffectIsDone;
			currentSupportCardWeAreHandling.SupportCardData.supportCardEffects[0].supportCardEffectData.EffectOfCard(duelPhaseSM.CurrentPlayerBeingHandled);
        }
		else
		{
            DialogueBoxPopup.instance.ActivatePopupWithJustText($"Player {duelPhaseSM.CurrentPlayerBeingHandled.PlayerInDuel.playerIDIntVal} did not use a support card", 0, "Support card resolution");

            AfterSupportCardEffectIsDone();
		}

		//Will need to loop through each support card: Use each.

    }

	public void AfterSupportCardEffectIsDone(SupportCard supportCardUsed = null)
	{
		if(currentSupportCardWeAreHandling != null)
		{
            currentSupportCardWeAreHandling.SupportCardData.supportCardEffects[0].supportCardEffectData.SupportCardEffectCompleted -= AfterSupportCardEffectIsDone;
            currentSupportCardWeAreHandling = null;
        }

		duelPhaseSM.StartCoroutine(duelPhaseSM.TestingTimeBetweenPopupsSupportCardResolution());

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
