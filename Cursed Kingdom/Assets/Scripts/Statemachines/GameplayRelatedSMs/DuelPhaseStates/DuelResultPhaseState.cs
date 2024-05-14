//Code written by Mohamed Riaz Khan of BukuGames.
//All code is written by me (Above name) unless otherwise stated via comments below.
//Not authorized for use outside of the Github repository of this game developed by BukuGames.
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DuelResultPhaseState : BaseState
{
    DuelPhaseSM duelPhaseSM;
    private const string stateName = "DuelResultPhaseState";
    public DuelResultPhaseState(DuelPhaseSM stateMachine) : base(stateName, stateMachine)
    {
        duelPhaseSM = stateMachine as DuelPhaseSM;
    }

    public override void Enter()
    {
        base.Enter();
        PhaseDisplay.instance.TurnOnDisplay($"Result phase", 1.5f);
        PhaseDisplay.instance.displayTimeCompleted += Logic;
    }

    private void Logic()
    {
        PhaseDisplay.instance.displayTimeCompleted -= Logic;

        foreach(DuelPlayerInformation playerInfo in duelPhaseSM.PlayersInCurrentDuel)
        {
            if(duelPhaseSM.CurrentWinners.Count > 1)
            {
                bool foundMatch = false;
                foreach (DuelPlayerInformation winnerInfo in duelPhaseSM.CurrentWinners)
                {
                    if (playerInfo == winnerInfo)
                    {
                        foundMatch = true;
                    }
                    else
                    {

                    }
                }
                //Means the player we are looking at was not in the list of Players that won. They lost the duel.
                if (!foundMatch)
                {
                    //Will be more dynamic based on things like if players deal more damage this duel or if that class takes more damage when losing a duel etc.
                    playerInfo.PlayerInDuel.TakeDamage(1);
                }
            }
            //It's a tie. Everyone takes 1 damage.
            else if(duelPhaseSM.CurrentWinners.Count == duelPhaseSM.PlayersInCurrentDuel.Count)
            {
                playerInfo.PlayerInDuel.TakeDamage(1);
            }
            //Count should only be 1 for winners list in this case.
            else
            {
                if(playerInfo != duelPhaseSM.CurrentWinners[0])
                {
                    playerInfo.PlayerInDuel.TakeDamage(1);
                }
            }
            for(int i = 0; i < playerInfo.SelectedSupportCards.Count; i++)
            {
                int index = i;
                playerInfo.PlayerInDuel.DiscardFromHand(playerInfo.SelectedSupportCards[index].ThisCardType, playerInfo.SelectedSupportCards[index]);
            }

            for (int j = 0; j < playerInfo.SelectedMovementCards.Count; j++)
            {
                int index = j;
                playerInfo.PlayerInDuel.DiscardFromHand(playerInfo.SelectedMovementCards[index].ThisCardType, playerInfo.SelectedMovementCards[index]);
            }

        }

        duelPhaseSM.ChangeState(duelPhaseSM.duelNotDuelingPhaseState);
        duelPhaseSM.gameplayManager.GameplayPhaseStatemachineRef.ChangeState(duelPhaseSM.gameplayManager.GameplayPhaseStatemachineRef.gameplayResolveSpacePhaseState);
    }

    public override void UpdateLogic()
    {
        base.UpdateLogic();
    }

    public override void Exit()
    {
        base.Exit();
        duelPhaseSM.ResetDuelParameters();
    }
}
