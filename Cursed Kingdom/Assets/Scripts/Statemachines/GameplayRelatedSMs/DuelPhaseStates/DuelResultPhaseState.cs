//Code written by Mohamed Riaz Khan of BukuGames.
//All code is written by me (Above name) unless otherwise stated via comments below.
//Not authorized for use outside of the Github repository of this game developed by BukuGames.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
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
        duelPhaseSM.FadePanelCompletedFadingDuel += TurnOffCameraAfterDuel;
        duelPhaseSM.StartCoroutine(duelPhaseSM.FadePanelActivate(0.5f));
    }

    public void TurnOffCameraAfterDuel()
    {
        duelPhaseSM.FadePanelCompletedFadingDuel -= TurnOffCameraAfterDuel;
        duelPhaseSM.gameplayManager.GameplayCameraManagerRef.TurnOffVirtualDuelCamera();

        foreach (DuelPlayerInformation duelPlayerInformation in duelPhaseSM.PlayersInCurrentDuel)
        {
            duelPhaseSM.StartCoroutine(duelPhaseSM.EndOfDuelResultAnimation(duelPlayerInformation));
        }
    }

    public void DamageResults()
    {
        foreach (DuelPlayerInformation playerInfo in duelPhaseSM.PlayersInCurrentDuel)
        {
            if (duelPhaseSM.CurrentWinners.Count > 1)
            {
                //It's a tie. Everyone takes 1 damage.
                if (duelPhaseSM.CurrentWinners.Count == duelPhaseSM.PlayersInCurrentDuel.Count)
                {
                    playerInfo.PlayerInDuel.TakeDamage(1);
                    continue;
                }

                bool foundMatch = false;
                foreach (DuelPlayerInformation winnerInfo in duelPhaseSM.CurrentWinners)
                {
                    if (playerInfo == winnerInfo)
                    {
                        foundMatch = true;
                    }
                }
                //Means the player we are looking at was not in the list of Players that won. They lost the duel.
                if (!foundMatch)
                {
                    //Will be more dynamic based on things like if players deal more damage this duel or if that class takes more damage when losing a duel etc.
                    playerInfo.PlayerInDuel.TakeDamage(1);
                }
            }
            //Count should only be 1 for winners list in this case.
            else
            {
                if (playerInfo != duelPhaseSM.CurrentWinners[0])
                {
                    playerInfo.PlayerInDuel.TakeDamage(1);
                }
            }
            for (int i = 0; i < playerInfo.SelectedSupportCards.Count; i++)
            {
                int index = i;
                playerInfo.SelectedSupportCards[index].gameObject.SetActive(true);
                playerInfo.SelectedSupportCards[index].transform.localScale = playerInfo.SelectedSupportCards[index].OriginalSize;
                playerInfo.PlayerInDuel.DiscardFromHand(playerInfo.SelectedSupportCards[index].ThisCardType, playerInfo.SelectedSupportCards[index]);
            }

            for (int j = 0; j < playerInfo.SelectedMovementCards.Count; j++)
            {
                int index = j;
                playerInfo.SelectedMovementCards[index].gameObject.SetActive(true);
                playerInfo.SelectedMovementCards[index].transform.localScale = playerInfo.SelectedMovementCards[index].OriginalSize;
                playerInfo.SelectedMovementCards[index].ResetMovementValue();
                playerInfo.PlayerInDuel.DiscardFromHand(playerInfo.SelectedMovementCards[index].ThisCardType, playerInfo.SelectedMovementCards[index]);
            }
        }
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
