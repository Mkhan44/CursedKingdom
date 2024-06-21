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

			foreach(SupportCardData.SupportCardEffect supportCardEffect in currentSupportCardWeAreHandling.SupportCardData.supportCardEffects)
			{

				if(supportCardEffect.supportCardEffectData.IsAfterDuelEffect || supportCardEffect.supportCardEffectData.IsDuringDuelDamageCalc)
				{
					//Do nothing we don't want to handle these until after the duel is over.
					if(currentSupportCardWeAreHandling.SupportCardData.supportCardEffects.Count == 1)
					{
						AfterSupportCardEffectIsDone();
					}
				}
				else
				{
                    supportCardEffect.supportCardEffectData.SupportCardEffectCompleted += AfterSupportCardEffectIsDone;
                    supportCardEffect.supportCardEffectData.EffectOfCard(duelPhaseSM.CurrentPlayerBeingHandled);
                }
            }
			
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
            foreach (SupportCardData.SupportCardEffect supportCardEffect in currentSupportCardWeAreHandling.SupportCardData.supportCardEffects)
            {
                supportCardEffect.supportCardEffectData.SupportCardEffectCompleted -= AfterSupportCardEffectIsDone;
            }
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
