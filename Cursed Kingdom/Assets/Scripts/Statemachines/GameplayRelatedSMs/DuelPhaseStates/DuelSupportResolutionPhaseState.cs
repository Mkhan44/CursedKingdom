//Code written by Mohamed Riaz Khan of BukuGames.
//All code is written by me (Above name) unless otherwise stated via comments below.
//Not authorized for use outside of the Github repository of this game developed by BukuGames.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class DuelSupportResolutionPhaseState : BaseState
{
	DuelPhaseSM duelPhaseSM;
	private const string stateName = "DuelSupportResolutionPhaseState";

	private int indexOfCurrentSupportCardWeAreHandling;
	public DuelSupportResolutionPhaseState(DuelPhaseSM stateMachine) : base(stateName, stateMachine)
	{
		duelPhaseSM = stateMachine as DuelPhaseSM;
	}

	public override void Enter()
	{
		//Check which player we are in the duelPhaseSM Players list.
		base.Enter();
        PhaseDisplay.instance.TurnOnDisplay($"Resolve support cards", 1.5f);
		duelPhaseSM.gameplayManager.GameplayCameraManagerRef.DuelVirtualCameraAnimator.SetBool(GameplayCameraManager.ISGOINGBACKTODEFAULT, false);
		duelPhaseSM.gameplayManager.GameplayCameraManagerRef.DuelVirtualCameraAnimator.SetBool(GameplayCameraManager.ISRESOLVINGCAM, true);
		indexOfCurrentSupportCardWeAreHandling = 0;
        PhaseDisplay.instance.displayTimeCompleted += Logic;
    }

    public void Logic()
    {
        PhaseDisplay.instance.displayTimeCompleted -= Logic;

		if(!duelPhaseSM.duelResolveCardsHolder.activeSelf)
		{
			duelPhaseSM.duelResolveCardsHolder.SetActive(true);
			SpawnInCardResolveHolders();
		}
		else
		{
			FlipAndUseSupportCards();
		}

    }

	public void FlipAndUseSupportCards()
	{
		
		if (duelPhaseSM.CurrentPlayerBeingHandled.SelectedSupportCards.Count > 0)
		{
			//DialogueBoxPopup.instance.ActivatePopupWithJustText($"Player {duelPhaseSM.CurrentPlayerBeingHandled.PlayerInDuel.playerIDIntVal} used {duelPhaseSM.CurrentPlayerBeingHandled.SelectedSupportCards[0].SupportCardData.name}", 0, "Support card resolution");
			//duelPhaseSM.CurrentPlayerBeingHandled.SelectedSupportCards[0];
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
				duelPhaseSM.CurrentPlayerBeingHandled.CardDuelResolveHolderObject.transform.GetChild(1).GetChild(0).GetComponent<Animator>().Play(animToPlay);
			}

			//Will need to make this work for all effects. Maybe have something on this that kicks off after all effects are completed?

			foreach(SupportCardData.SupportCardEffect supportCardEffect in duelPhaseSM.CurrentPlayerBeingHandled.SelectedSupportCards[indexOfCurrentSupportCardWeAreHandling].SupportCardData.supportCardEffects)
			{

				if(supportCardEffect.supportCardEffectData.IsAfterDuelEffectAndNeedsToWin || supportCardEffect.supportCardEffectData.IsAfterDuelEffect || supportCardEffect.supportCardEffectData.IsDuringDuelDamageCalc)
				{
					//Do nothing we don't want to handle these until after the duel is over.
					if(duelPhaseSM.CurrentPlayerBeingHandled.SelectedSupportCards[0].SupportCardData.supportCardEffects.Count == 1)
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

			//duelPhaseSM.StartCoroutine(duelPhaseSM.TestingTimeBetweenPopupsMovementCardResolution());
            AfterSupportCardEffectIsDone();
		}

		//Will need to loop through each support card: Use each.
	}

	//Spawn the holders a certain distance away from where the player is. Spawn it the way the player is facing (Right = spawn to the right etc)
	public void SpawnInCardResolveHolders()
	{
		//Get transform of current player we are handling:
		foreach(DuelPlayerInformation duelPlayerInformation in duelPhaseSM.PlayersInCurrentDuel)
		{
			int indexOfCurrentPlayer = duelPhaseSM.PlayersInCurrentDuel.IndexOf(duelPlayerInformation);
			duelPlayerInformation.CardDuelResolveHolderObject = duelPhaseSM.duelResolveCardsHolder.transform.GetChild(indexOfCurrentPlayer).gameObject;
		}

		duelPhaseSM.StartCoroutine(duelPhaseSM.SetupCardsToResolve());
	}

	

	public void SetupSupportCardsToResolve()
	{
		foreach(DuelPlayerInformation duelPlayerInformation in duelPhaseSM.PlayersInCurrentDuel)
		{
			if(duelPlayerInformation.CardDuelResolveHolderObject != null)
			{
				foreach(SupportCard supportCard in duelPlayerInformation.SelectedSupportCards)
				{
					GameObject spawnedSupportCard = GameObject.Instantiate(duelPhaseSM.supportCardDuelPrefab, duelPlayerInformation.CardDuelResolveHolderObject.transform.GetChild(1));
					SupportCardDuel spawnedSupportCardDuel = spawnedSupportCard.GetComponent<SupportCardDuel>();
					spawnedSupportCardDuel.DuelPhaseSMReference = duelPhaseSM;
					spawnedSupportCardDuel.SetupCard(supportCard);
					spawnedSupportCardDuel.IsClickable = false;
				}

				foreach(Transform child in duelPlayerInformation.CardDuelResolveHolderObject.transform.GetChild(1))
				{
					child.GetComponent<Animator>().Play("ComeDown");
				}

				//After final card animation begins playing, we need to wait until it's done before moving on by getting the clip length.
			}
		}
	}

	public void SetupMovementCardsToResolve()
	{
		foreach(DuelPlayerInformation duelPlayerInformation in duelPhaseSM.PlayersInCurrentDuel)
		{
			if(duelPlayerInformation.CardDuelResolveHolderObject != null)
			{
				foreach(MovementCard movementCard in duelPlayerInformation.SelectedMovementCards)
				{
					GameObject spawnedMovementCard = GameObject.Instantiate(duelPhaseSM.movementCardDuelPrefab, duelPlayerInformation.CardDuelResolveHolderObject.transform.GetChild(0));
					MovementCardDuel spawnedMovementCardDuel = spawnedMovementCard.GetComponent<MovementCardDuel>();
					spawnedMovementCardDuel.DuelPhaseSMReference = duelPhaseSM;
					spawnedMovementCardDuel.SetupCard(movementCard);
					spawnedMovementCardDuel.IsClickable = false;
				}

				foreach(Transform child in duelPlayerInformation.CardDuelResolveHolderObject.transform.GetChild(0))
				{
					child.GetComponent<Animator>().Play("ComeDown");
				}
			}
		}
	}

	public void AfterSupportCardEffectIsDone(SupportCard supportCardUsed = null)
	{
		if(duelPhaseSM.CurrentPlayerBeingHandled.SelectedSupportCards.Count != 0 && duelPhaseSM.CurrentPlayerBeingHandled.SelectedSupportCards[indexOfCurrentSupportCardWeAreHandling] != null)
		{
            foreach (SupportCardData.SupportCardEffect supportCardEffect in duelPhaseSM.CurrentPlayerBeingHandled.SelectedSupportCards[indexOfCurrentSupportCardWeAreHandling].SupportCardData.supportCardEffects)
            {
                supportCardEffect.supportCardEffectData.SupportCardEffectCompleted -= AfterSupportCardEffectIsDone;
            }
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
