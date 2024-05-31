//Code written by Mohamed Riaz Khan of BukuGames.
//All code is written by me (Above name) unless otherwise stated via comments below.
//Not authorized for use outside of the Github repository of this game developed by BukuGames.
using System;
using System.Collections;
using System.Collections.Generic;
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
		PhaseDisplay.instance.TurnOnDisplay($"Select cards to duel with, Player {duelPhaseSM.CurrentPlayerBeingHandled.PlayerInDuel.playerIDIntVal}.", 1.5f);
		PhaseDisplay.instance.displayTimeCompleted += Logic;
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
		
        Debug.Log($"Player {duelPhaseSM.PlayersInCurrentDuel[index].PlayerInDuel.playerIDIntVal} has selected at least 1 movement card for the duel.");
        /**
		
		duelPhaseSM.CurrentPlayerBeingHandled = duelPhaseSM.PlayersInCurrentDuel[index];
		Debug.Log($"{duelPhaseSM.PlayersInCurrentDuel[index].SelectedMovementCards.Count} Are the amount of movement cards that Player {duelPhaseSM.PlayersInCurrentDuel[index].PlayerInDuel.playerIDIntVal} has selected for the duel.");
		duelPhaseSM.ChangeState(duelPhaseSM.duelSupportCardPhaseState);
		*/
    }

	public void DeselectMovementCards()
	{
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
        Debug.Log($"Player {duelPhaseSM.PlayersInCurrentDuel[index].PlayerInDuel.playerIDIntVal} has selected at least 1 support card for the duel.");
    }

	public void DeselectSupportCards()
	{
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

	public void ConfirmSelection()
	{
		ConfirmCardsSelection = true;
		SwitchToNextPlayerInDuel();
	}

    private void SwitchToNextPlayerInDuel()
    {
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
    }


    private void Logic()
	{
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
        duelPhaseSM.CurrentPlayerBeingHandled.PlayerInDuel.ShowHand();
    }
}
