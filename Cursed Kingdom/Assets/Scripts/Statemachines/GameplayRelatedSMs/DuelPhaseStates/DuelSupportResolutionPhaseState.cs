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
		//duelPhaseSM.gameplayManager.GameplayCameraManagerRef.DuelVirtualCameraAnimator.SetBool(GameplayCameraManager.ISGOINGBACKTODEFAULT, false);
		//JUST A TEST MAKE SURE TO ACTUALLY PLAY AN ANIMATION HERE.
		GameObject.Find("Duel Resolve Cam").GetComponent<Cinemachine.CinemachineVirtualCamera>().Priority = 12;
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
		
        if (duelPhaseSM.CurrentPlayerBeingHandled.SelectedSupportCards.Count > 0)
		{
			//DialogueBoxPopup.instance.ActivatePopupWithJustText($"Player {duelPhaseSM.CurrentPlayerBeingHandled.PlayerInDuel.playerIDIntVal} used {duelPhaseSM.CurrentPlayerBeingHandled.SelectedSupportCards[0].SupportCardData.name}", 0, "Support card resolution");
			currentSupportCardWeAreHandling = duelPhaseSM.CurrentPlayerBeingHandled.SelectedSupportCards[0];
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

	//Spawn the holders a certain distance away from where the player is. Spawn it the way the player is facing (Right = spawn to the right etc)
	public void SpawnInCardResolveHolders()
	{
		//Get transform of current player we are handling:
		foreach(DuelPlayerInformation duelPlayerInformation in duelPhaseSM.PlayersInCurrentDuel)
		{
			int indexOfCurrentPlayer = duelPhaseSM.PlayersInCurrentDuel.IndexOf(duelPlayerInformation);
			duelPlayerInformation.CardDuelResolveHolderObject = duelPhaseSM.duelResolveCardsHolder.transform.GetChild(indexOfCurrentPlayer).gameObject;

			/*
			Transform currentPlayerTransform = duelPlayerInformation.PlayerDuelTransform;
			//newPlayerDuelPrefabObj.transform.position = gameplayPhaseSM.gameplayManager.duelPlaneSpawnPointsParent.transform.GetChild(currentIndex).position;
			GameObject newResolveCardHolder = GameObject.Instantiate(duelPhaseSM.cardResolveHolderPrefab, duelPhaseSM.duelResolveCardsHolder.transform);
			duelPlayerInformation.CardDuelResolveHolderObject = newResolveCardHolder;
			RectTransform currentHolderRect = newResolveCardHolder.GetComponent<RectTransform>();
			RectTransform currentSpotRect = duelPhaseSM.gameplayManager.duelPlaneSpawnPointsParent.transform.GetChild(indexOfCurrentPlayer).GetComponent<RectTransform>();

			currentHolderRect.localPosition = currentSpotRect.anchoredPosition;

			//Need a way to have this 'starting position' be based on the player right now it's always based on player 1.

			//If it's even number that means we move this to the left. If it's an odd number then we move it to the right.
			if(indexOfCurrentPlayer % 2 == 0)
			{
				//newResolveCardHolder.transform.position = new Vector3(newResolveCardHolder.transform.position.x + 1, newResolveCardHolder.transform.position.y, 0);
			}
			else
			{
				//newResolveCardHolder.transform.position = new Vector3(newResolveCardHolder.transform.position.x - 1, newResolveCardHolder.transform.position.y, 0);
			}
			*/
		}

		SetupSupportCardsToResolve();
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
				}

				foreach(Transform child in duelPlayerInformation.CardDuelResolveHolderObject.transform.GetChild(1))
				{
					child.GetComponent<Animator>().Play("ComeDown");
				}
			}
		}
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
