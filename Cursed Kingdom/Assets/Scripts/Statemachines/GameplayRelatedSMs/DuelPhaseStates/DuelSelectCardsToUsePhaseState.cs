//Code written by Mohamed Riaz Khan of BukuGames.
//All code is written by me (Above name) unless otherwise stated via comments below.
//Not authorized for use outside of the Github repository of this game developed by BukuGames.
using System;
using System.Collections;
using System.Collections.Generic;
using PurrNet;
using UnityEngine;
using UnityEngine.UI;

public class DuelSelectCardsToUsePhaseState : BaseState
{
	private DuelPhaseSM duelPhaseSM;
	private const string stateName = "DuelSelectCardsToUsePhaseState";

	private bool selectedMovementCard = false;
	private bool selectedSupportCard = false;
	private bool confirmCardsSelection = false;
	private Button deselectMovementCardsButton;
	private Button deselectSupportCardsButton;
	private Button confirmCardsSelectionButton;

	public Button DeselectMovementCardsButton { get => deselectMovementCardsButton; set => deselectMovementCardsButton = value; }
	public Button DeselectSupportCardsButton { get => deselectSupportCardsButton; set => deselectSupportCardsButton = value; }
	public Button ConfirmCardsSelectionButton { get => confirmCardsSelectionButton; set => confirmCardsSelectionButton = value; }
	public bool SelectedMovementCard { get => selectedMovementCard; set => selectedMovementCard = value; }
	public bool SelectedSupportCard { get => selectedSupportCard; set => selectedSupportCard = value; }
	public bool ConfirmCardsSelection { get => confirmCardsSelection; set => confirmCardsSelection = value; }

	public DuelSelectCardsToUsePhaseState(DuelPhaseSM stateMachine) : base(stateName, stateMachine)
	{
		duelPhaseSM = stateMachine as DuelPhaseSM;
		AssignButtonClickEvents(duelPhaseSM.movementCardsDeselectButton, duelPhaseSM.supportCardsDeselectButton, duelPhaseSM.confirmChoicesButton);
	}

	public override void Enter()
	{
		//Check which player we are in the duelPhaseSM Players list.
		base.Enter();
		if(duelPhaseSM.CurrentPlayerBeingHandled.PlayerInDuel.PlayerAIReference != null)
		{
			Logic();
		}
		else
		{
			PhaseDisplay.instance.TurnOnDisplay($"Select cards to duel with, Player {duelPhaseSM.CurrentPlayerBeingHandled.PlayerInDuel.playerIDIntVal}.", 1.5f);
			PhaseDisplay.instance.displayTimeCompleted += Logic;
		}
		
		//Camera move based on index of current player +1 since it's 0 indexed..
		
	}

	public override void UpdateLogic()
	{
		base.UpdateLogic();
		if(!ConfirmCardsSelectionButton.IsInteractable() && SelectedMovementCard)
		{
			ConfirmCardsSelectionButton.interactable = true;
		}
	}

	public override void Exit()
	{
		SelectedMovementCard = false;
		SelectedSupportCard = false;
		ConfirmCardsSelection = false;
		DeselectMovementCardsButton.interactable = false;
		DeselectSupportCardsButton.interactable = false;
		ConfirmCardsSelectionButton.interactable = false;
		duelPhaseSM.duelUIHolder.SetActive(false);
        duelPhaseSM.movementCardDuelHolderPrefab.SetActive(false);
        duelPhaseSM.supportCardDuelHolderPrefab.SetActive(false);
        base.Exit();
	}

	public void AssignButtonClickEvents(Button movementCards, Button supportCards, Button confirmCards)
	{
		DeselectMovementCardsButton = movementCards;
		DeselectSupportCardsButton = supportCards;
		ConfirmCardsSelectionButton = confirmCards;

		DeselectMovementCardsButton.onClick.AddListener(DeselectMovementCards);
		DeselectSupportCardsButton.onClick.AddListener(DeselectSupportCards);
		ConfirmCardsSelectionButton.onClick.AddListener(ConfirmSelection);
	}

	public void SelectMovementCard(List<MovementCard> movementCards, bool wasForcedDrawDueToNotHavingMovementCardsInHand = false)
	{
		DialogueBoxPopup.instance.DeactivatePopup();
		duelPhaseSM.gameplayManager.HandDisplayPanel.ShrinkHand();

		int index = duelPhaseSM.PlayersInCurrentDuel.IndexOf(duelPhaseSM.CurrentPlayerBeingHandled);
		duelPhaseSM.PlayersInCurrentDuel[index].SelectedMovementCards = movementCards;

		foreach(MovementCard card in movementCards)
		{
			card.gameObject.SetActive(false);
		}

		//Check if we have reached the maximum amount of movement cards before turning this on.
		SelectedMovementCard = true;
		if(!wasForcedDrawDueToNotHavingMovementCardsInHand)
		{
			DeselectMovementCardsButton.interactable = true;
		}

		GameObject spawnedMovementCard = GameObject.Instantiate(duelPhaseSM.movementCardDuelPrefab, duelPhaseSM.movementCardDuelHolderPrefab.transform);
		spawnedMovementCard.transform.SetParent(duelPhaseSM.movementCardDuelHolderPrefab.transform);
		MovementCardDuel spawnedMovementCardDuel = spawnedMovementCard.GetComponent<MovementCardDuel>();
		spawnedMovementCardDuel.DuelPhaseSMReference = duelPhaseSM;
		spawnedMovementCardDuel.SetupCard(movementCards[0]);
		//Debug.Log($"spawned card animator enabled: {spawnedMovementCardDuel.CardAnimator.isActiveAndEnabled}");
		spawnedMovementCardDuel.CardAnimator.enabled = true;
		//Debug.Log($"spawned card animator enabled after: {spawnedMovementCardDuel.CardAnimator.isActiveAndEnabled}");
		spawnedMovementCardDuel.CardAnimator.Play("Select");
		//A lot of this isn't needed if we aren't doing networking for some reason...Gotta figure out why networkmanager is borked.
        if (wasForcedDrawDueToNotHavingMovementCardsInHand)
        {
            spawnedMovementCardDuel.IsClickable = false;
			spawnedMovementCardDuel.GetComponent<LayoutElement>().enabled = false;
			spawnedMovementCard.GetComponent<RectTransform>().anchoredPosition = new Vector3(0,0,spawnedMovementCard.transform.position.z);
			spawnedMovementCardDuel.CardAnimator.Play("Select");
        }


        //Debug.Log($"Player {duelPhaseSM.PlayersInCurrentDuel[index].PlayerInDuel.playerIDIntVal} has selected at least 1 movement card for the duel.");
	}

	public void DeselectMovementCards()
	{
		foreach(Transform child in duelPhaseSM.movementCardDuelHolderPrefab.transform)
		{
			GameObject.Destroy(child.gameObject);
		}

		//Loop through all movement cards that the player has selected currently. Remove them from the list so they aren't counted as being selected.
		List<MovementCard> copiedMovementList = new();

		foreach(MovementCard selectedCard in duelPhaseSM.CurrentPlayerBeingHandled.SelectedMovementCards)
		{
			copiedMovementList.Add(selectedCard);
			selectedCard.gameObject.SetActive(true);
			selectedCard.DeselectCard();
		}


		foreach (MovementCard selectedCard in duelPhaseSM.CurrentPlayerBeingHandled.SelectedMovementCards)
		{
			copiedMovementList.Remove(selectedCard);
		}

		duelPhaseSM.CurrentPlayerBeingHandled.SelectedMovementCards = copiedMovementList;

		SelectedMovementCard = false;
		DeselectMovementCardsButton.interactable = false;
		ConfirmCardsSelectionButton.interactable = false;
	}
	
	public void DeselectMovementCard(MovementCard movementCardReference)
	{
        foreach (Transform child in duelPhaseSM.movementCardDuelHolderPrefab.transform)
        {
            MovementCardDuel movementCardDuelRef = child.GetComponent<MovementCardDuel>();
			if(movementCardDuelRef != null )
			{
				if(movementCardDuelRef.MovementCardReference == movementCardReference)
				{
					GameObject.Destroy(child.gameObject);
				}
			}
        }

        movementCardReference.gameObject.SetActive(true);
		movementCardReference.DeselectCard();
		
		duelPhaseSM.CurrentPlayerBeingHandled.SelectedMovementCards.Remove(movementCardReference);
		if(duelPhaseSM.CurrentPlayerBeingHandled.SelectedMovementCards.Count == 0)
		{
			SelectedMovementCard = false;
			DeselectMovementCardsButton.interactable = false;
            ConfirmCardsSelectionButton.interactable = false;
        }
	}


	public void SelectSupportCard(List<SupportCard> supportCards)
	{
		DialogueBoxPopup.instance.DeactivatePopup();
		duelPhaseSM.gameplayManager.HandDisplayPanel.ShrinkHand();

		int index = duelPhaseSM.PlayersInCurrentDuel.IndexOf(duelPhaseSM.CurrentPlayerBeingHandled);
		duelPhaseSM.PlayersInCurrentDuel[index].SelectedSupportCards = supportCards;

		foreach (SupportCard card in supportCards)
		{
			card.gameObject.SetActive(false);
		}

		//Check if we have reached the maximum amount of support cards before turning this on.
		SelectedSupportCard = true;
		DeselectSupportCardsButton.interactable = true;

        GameObject spawnedSupportCard = GameObject.Instantiate(duelPhaseSM.supportCardDuelPrefab, duelPhaseSM.supportCardDuelHolderPrefab.transform);
		spawnedSupportCard.transform.SetParent(duelPhaseSM.supportCardDuelHolderPrefab.transform);
        SupportCardDuel spawnedSupportCardDuel = spawnedSupportCard.GetComponent<SupportCardDuel>();
        spawnedSupportCardDuel.DuelPhaseSMReference = duelPhaseSM;
        spawnedSupportCardDuel.SetupCard(supportCards[0]);
		//Debug.Log($"spawned card animator enabled: {spawnedSupportCardDuel.CardAnimator.isActiveAndEnabled}");
		spawnedSupportCardDuel.CardAnimator.enabled = true;
		//Debug.Log($"spawned card animator enabled after: {spawnedSupportCardDuel.CardAnimator.isActiveAndEnabled}");
		spawnedSupportCardDuel.CardAnimator.Play("Select");

        //Debug.Log($"Player {duelPhaseSM.PlayersInCurrentDuel[index].PlayerInDuel.playerIDIntVal} has selected at least 1 support card for the duel.");
	}

	public void DeselectSupportCards()
	{
        foreach (Transform child in duelPhaseSM.supportCardDuelHolderPrefab.transform)
        {
            GameObject.Destroy(child.gameObject);
        }

        //Loop through all support cards that the player has selected currently. Remove them from the list so they aren't counted as being selected.

        List<SupportCard> copiedSupportList = new();

		foreach (SupportCard selectedCard in duelPhaseSM.CurrentPlayerBeingHandled.SelectedSupportCards)
		{
			selectedCard.gameObject.SetActive(true);
			selectedCard.DeselectCard();
			copiedSupportList.Add(selectedCard);
		}

		foreach (SupportCard selectedCard in duelPhaseSM.CurrentPlayerBeingHandled.SelectedSupportCards)
		{
			copiedSupportList.Remove(selectedCard);
		}

		duelPhaseSM.CurrentPlayerBeingHandled.SelectedSupportCards = copiedSupportList;

		SelectedSupportCard = false;
		DeselectSupportCardsButton.interactable = false;
	}
	
	public void DeselectSupportCard(SupportCard supportCardReference)
	{
        foreach (Transform child in duelPhaseSM.supportCardDuelHolderPrefab.transform)
        {
            SupportCardDuel supportCardDuelRef = child.GetComponent<SupportCardDuel>();
            if (supportCardDuelRef != null)
            {
                if (supportCardDuelRef.SupportCardReference == supportCardReference)
                {
                    GameObject.Destroy(child.gameObject);
                }
            }
        }

        supportCardReference.gameObject.SetActive(true);
		supportCardReference.DeselectCard();
		
		duelPhaseSM.CurrentPlayerBeingHandled.SelectedSupportCards.Remove(supportCardReference);
		if(duelPhaseSM.CurrentPlayerBeingHandled.SelectedSupportCards.Count == 0)
		{
			SelectedSupportCard = false;
			DeselectSupportCardsButton.interactable = false;
		}
	}

	public void ConfirmSelection()
	{
		ConfirmCardsSelection = true;
		SwitchToNextPlayerInDuel();
	}

	private void SwitchToNextPlayerInDuel()
	{
        foreach (Transform child in duelPhaseSM.movementCardDuelHolderPrefab.transform)
        {
            GameObject.Destroy(child.gameObject);
        }

        foreach (Transform child in duelPhaseSM.supportCardDuelHolderPrefab.transform)
        {
            GameObject.Destroy(child.gameObject);
        }

        DialogueBoxPopup.instance.DeactivatePopup();
		duelPhaseSM.gameplayManager.HandDisplayPanel.ShrinkHand();
		duelPhaseSM.CurrentPlayerBeingHandled.PlayerInDuel.HideHand();

		int indexOfCurrentPlayer = duelPhaseSM.PlayersInCurrentDuel.IndexOf(duelPhaseSM.CurrentPlayerBeingHandled);

		//We're at the last player. Move to the resolution phase.
		if (duelPhaseSM.PlayersInCurrentDuel[duelPhaseSM.PlayersInCurrentDuel.Count - 1] == duelPhaseSM.CurrentPlayerBeingHandled)
		{
			duelPhaseSM.gameplayManager.HandDisplayPanel.SetCurrentActiveHandUI(duelPhaseSM.gameplayManager.Players.IndexOf(duelPhaseSM.PlayersInCurrentDuel[0].PlayerInDuel));
			duelPhaseSM.CurrentPlayerBeingHandled = duelPhaseSM.PlayersInCurrentDuel[0];
			duelPhaseSM.ChangeState(duelPhaseSM.duelSupportResolutionPhaseState);
		}
		else
		{
			duelPhaseSM.CurrentPlayerBeingHandled = duelPhaseSM.PlayersInCurrentDuel[indexOfCurrentPlayer + 1];
			duelPhaseSM.gameplayManager.HandDisplayPanel.SetCurrentActiveHandUI(duelPhaseSM.gameplayManager.Players.IndexOf(duelPhaseSM.CurrentPlayerBeingHandled.PlayerInDuel));
			duelPhaseSM.ChangeState(duelPhaseSM.duelSelectCardsToUsePhaseState);
		}

		duelPhaseSM.gameplayManager.GameplayCameraManagerRef.DuelVirtualCameraAnimator.SetInteger(GameplayCameraManager.ZOOMTOSPOTNUM, 0);
		duelPhaseSM.gameplayManager.GameplayCameraManagerRef.DuelVirtualCameraAnimator.SetBool(GameplayCameraManager.ISGOINGBACKTODEFAULT, true);
	}

	private void SelectCardsForAIPlayers()
	{
		//If it's an AI or not the current player (When multiplayer) then we just have the player auto-pick their cards or if it's a human we skip the camera move to go to only us.
		foreach(DuelPlayerInformation playerInformation in  duelPhaseSM.PlayersInCurrentDuel)
		{
			//Select the AI player's cards randomly from their hand (Add to the lists of movement and support cards)
			if(playerInformation.PlayerInDuel.PlayerAIReference != null)
			{


			}

		}
	}


	private void Logic()
	{
		//This is specifically checking for if we  selected cards for AI opponents at the end of the start phase. May want to change how we're doing this.
		if(duelPhaseSM.CurrentPlayerBeingHandled.SelectedMovementCards.Count > 0)
		{
			PhaseDisplay.instance.displayTimeCompleted -= Logic;
			SwitchToNextPlayerInDuel();
			return;
		}
        duelPhaseSM.gameplayManager.GameplayCameraManagerRef.DuelVirtualCameraAnimator.SetBool(GameplayCameraManager.ISGOINGBACKTODEFAULT, false);
		duelPhaseSM.gameplayManager.GameplayCameraManagerRef.DuelVirtualCameraAnimator.SetInteger(GameplayCameraManager.ZOOMTOSPOTNUM, (duelPhaseSM.PlayersInCurrentDuel.IndexOf(duelPhaseSM.CurrentPlayerBeingHandled) + 1));

		PhaseDisplay.instance.displayTimeCompleted -= Logic;
		if(duelPhaseSM.CurrentPlayerBeingHandled.PlayerInDuel.MovementCardsInHandCount == 0)
		{
			//Give them the next on in the deck. Do a popup. After popup is gone then move to support card phase.
			duelPhaseSM.CurrentPlayerBeingHandled.PlayerInDuel.DrawThenUseMovementCardImmediatelyDuel();
		}
		else
		{
			//DialogueBoxPopup.instance.ActivatePopupWithJustText($"Player {duelPhaseSM.CurrentPlayerBeingHandled.PlayerInDuel.playerIDIntVal}: Please select cards to use for the duel.", 0, "Card selection");
			
		}

		duelPhaseSM.duelUIHolder.SetActive(true);
        duelPhaseSM.movementCardDuelHolderPrefab.SetActive(true);
        duelPhaseSM.supportCardDuelHolderPrefab.SetActive(true);
        duelPhaseSM.CurrentPlayerBeingHandled.PlayerInDuel.ShowHand();
        int currentIndex = duelPhaseSM.PlayersInCurrentDuel.IndexOf(duelPhaseSM.CurrentPlayerBeingHandled);
        string animToPlay = "";
        switch (currentIndex)
        {
            case 0:
                {
                    animToPlay = "Spot1";
                    break;
                }
            case 1:
                {
                    animToPlay = "Spot2";
                    break;
                }
            case 2:
                {
                    animToPlay = "Spot3";
                    break;
                }
            case 3:
                {
                    animToPlay = "Spot4";
                    break;
                }
        }
        duelPhaseSM.duelSelectCardsUIAnimator.Play(animToPlay);
    }
}
