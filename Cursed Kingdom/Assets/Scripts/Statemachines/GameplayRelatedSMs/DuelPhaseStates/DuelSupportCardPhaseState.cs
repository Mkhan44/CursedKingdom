//Code written by Mohamed Riaz Khan of BukuGames.
//All code is written by me (Above name) unless otherwise stated via comments below.
//Not authorized for use outside of the Github repository of this game developed by BukuGames.
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DuelSupportCardPhaseState : BaseState
{
	DuelPhaseSM duelPhaseSM;
	private const string stateName = "DuelSupportCardPhaseState";
	public DuelSupportCardPhaseState(DuelPhaseSM stateMachine) : base(stateName, stateMachine)
	{
		duelPhaseSM = stateMachine as DuelPhaseSM;
	}

	public override void Enter()
	{
		//Check which player we are in the duelPhaseSM Players list.
		base.Enter();
        PhaseDisplay.instance.TurnOnDisplay($"Choose support card", 1.5f);
        PhaseDisplay.instance.displayTimeCompleted += Logic;
    }

    public override void UpdateLogic()
	{
		base.UpdateLogic();
	}

	public override void Exit()
	{
		//If we are at the last Player in the list (Check against duelPhaseSM Players list and current player)
		//Then move onto the next phase. Otherwise move onto the next Player and move back to the Movement card placement state.
		base.Exit();
	}

    public void SupportCardSelected(List<SupportCard> supportCards)
    {
        duelPhaseSM.gameplayManager.HandDisplayPanel.ShrinkHand();
        duelPhaseSM.CurrentPlayerBeingHandled.Item1.HideHand();

        Tuple<Player, List<MovementCard>, List<SupportCard>> supportCardTuple = new Tuple<Player, List<MovementCard>, List<SupportCard>>(duelPhaseSM.CurrentPlayerBeingHandled.Item1, duelPhaseSM.CurrentPlayerBeingHandled.Item2, supportCards);
        int index = duelPhaseSM.PlayersInCurrentDuel.IndexOf(duelPhaseSM.CurrentPlayerBeingHandled);

        duelPhaseSM.PlayersInCurrentDuel[index] = supportCardTuple;
        duelPhaseSM.CurrentPlayerBeingHandled = duelPhaseSM.PlayersInCurrentDuel[index];
        Debug.Log($"{duelPhaseSM.PlayersInCurrentDuel[index].Item3.Count} Are the amount of support cards that Player {duelPhaseSM.PlayersInCurrentDuel[index].Item1.playerIDIntVal} has selected for the duel.");

        //Change to next player. if final player then move onto resolution.
        SwitchToNextPlayerInDuel();
    }

    /// <summary>
    /// Player did not select a support card to use. Leave list empty and move onto next player.
    /// </summary>
    public void SupportCardNotSelected()
    {
        duelPhaseSM.gameplayManager.HandDisplayPanel.ShrinkHand();
        duelPhaseSM.CurrentPlayerBeingHandled.Item1.HideHand();
        SwitchToNextPlayerInDuel();
    }

    private void SwitchToNextPlayerInDuel()
    {
        DialogueBoxPopup.instance.DeactivatePopup();
        int indexOfCurrentPlayer = duelPhaseSM.PlayersInCurrentDuel.IndexOf(duelPhaseSM.CurrentPlayerBeingHandled);

        //We're at the last player. Move to the resolution phase.
        if (duelPhaseSM.PlayersInCurrentDuel[duelPhaseSM.PlayersInCurrentDuel.Count-1] == duelPhaseSM.CurrentPlayerBeingHandled)
        {
            duelPhaseSM.gameplayManager.HandDisplayPanel.SetCurrentActiveHandUI(duelPhaseSM.gameplayManager.Players.IndexOf(duelPhaseSM.PlayersInCurrentDuel[0].Item1));
            duelPhaseSM.ChangeState(duelPhaseSM.duelSupportResolutionPhaseState);
            Debug.Log("We reached the final player. Moving into resolution state for support cards.");
        }
        else
        {
            Debug.Log($"We finished handling player {duelPhaseSM.CurrentPlayerBeingHandled.Item1.playerIDIntVal}'s cards. Onto the next player.");
            duelPhaseSM.CurrentPlayerBeingHandled = duelPhaseSM.PlayersInCurrentDuel[indexOfCurrentPlayer + 1];

            duelPhaseSM.gameplayManager.HandDisplayPanel.SetCurrentActiveHandUI(duelPhaseSM.gameplayManager.Players.IndexOf(duelPhaseSM.CurrentPlayerBeingHandled.Item1));
            
            duelPhaseSM.ChangeState(duelPhaseSM.duelMovementCardPhaseState);
        }
    }

    private void Logic()
    {
        PhaseDisplay.instance.displayTimeCompleted -= Logic;
        bool hasAtLeastOneDuelSupportCard = false;

        foreach(SupportCard supportCard in duelPhaseSM.CurrentPlayerBeingHandled.Item1.GetSupportCardsInHand())
        {
            if(supportCard.SupportCardData.ThisSupportCardType == SupportCardData.SupportCardType.Duel)
            {
                hasAtLeastOneDuelSupportCard = true;
            }
        }

        if (duelPhaseSM.CurrentPlayerBeingHandled.Item1.SupportCardsInHandCount == 0 || duelPhaseSM.CurrentPlayerBeingHandled.Item1.NumSupportCardsUsedThisTurn >= duelPhaseSM.CurrentPlayerBeingHandled.Item1.MaxSupportCardsToUse || !hasAtLeastOneDuelSupportCard) 
        {
            //Skip this because they don't have any support cards to use or they've already used a support card this turn.
            SwitchToNextPlayerInDuel();
        }
        else
        {
            List<Tuple<string, string, object, List<object>>> insertedParams = new();
            insertedParams.Add(Tuple.Create<string, string, object, List<object>>("Don't use a support card", nameof(duelPhaseSM.ChooseNoSupportCardToUseInDuel), duelPhaseSM, new List<object>()));

            DialogueBoxPopup.instance.ActivatePopupWithButtonChoices($"Player {duelPhaseSM.CurrentPlayerBeingHandled.Item1.playerIDIntVal}: Please choose a support card if you wish to use one in the duel.",insertedParams, 1, "Card selection", false);

            duelPhaseSM.CurrentPlayerBeingHandled.Item1.ShowHand();
        }
    }
}
