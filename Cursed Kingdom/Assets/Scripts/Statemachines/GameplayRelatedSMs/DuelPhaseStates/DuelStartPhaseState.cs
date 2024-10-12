//Code written by Mohamed Riaz Khan of BukuGames.
//All code is written by me (Above name) unless otherwise stated via comments below.
//Not authorized for use outside of the Github repository of this game developed by BukuGames.
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class DuelStartPhaseState : BaseState
{
    DuelPhaseSM duelPhaseSM;
    private const string stateName = "DuelStartPhase";
    public DuelStartPhaseState(DuelPhaseSM stateMachine) : base(stateName, stateMachine)
    {
        duelPhaseSM = stateMachine as DuelPhaseSM;
    }

    public override void Enter()
    {
        base.Enter();
        Audio_Manager.Instance.TransitionBetweenDuelAndBoardMusic();
        PhaseDisplay.instance.TurnOnDisplay("Duel!", 1.5f);
        PhaseDisplay.instance.displayTimeCompleted += Logic;
        //Play animation to enter into the duel state.

    }

    public override void UpdateLogic()
    {
        base.UpdateLogic();
    }

    public override void Exit()
    {
        base.Exit();
    }

    private void Logic()
    {
        PhaseDisplay.instance.displayTimeCompleted -= Logic;
        duelPhaseSM.CurrentPlayerBeingHandled = duelPhaseSM.PlayersInCurrentDuel[0];
        //Select all AI player's cards automatically here.
        foreach(DuelPlayerInformation playerInformation in  duelPhaseSM.PlayersInCurrentDuel)
		{
			//Select the AI player's cards randomly from their hand (Add to the lists of movement and support cards)
			if(playerInformation.PlayerInDuel.PlayerAIReference != null)
			{
                int randomChanceToUseSupportCard = 0;
                //Randomize if they use a support card or not.
                if(playerInformation.PlayerInDuel.MaxSupportCardsToUse >= 1 && playerInformation.PlayerInDuel.NumSupportCardsUsedThisTurn < playerInformation.PlayerInDuel.MaxSupportCardsToUse && playerInformation.PlayerInDuel.SupportCardsInHandCount > 0)
                {
                    randomChanceToUseSupportCard = Random.Range(0, 2);
                }

                if(randomChanceToUseSupportCard > 0)
                {
                    playerInformation.PlayerInDuel.PlayerAIReference.SelectSupportCardForDuel(playerInformation);
                    continue;
                }

                playerInformation.PlayerInDuel.PlayerAIReference.SelectMovementCardForDuel(playerInformation);
			}
		}

        duelPhaseSM.ChangeState(duelPhaseSM.duelSelectCardsToUsePhaseState);
    }

}
