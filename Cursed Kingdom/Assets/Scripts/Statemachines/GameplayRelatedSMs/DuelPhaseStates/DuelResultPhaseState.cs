//Code written by Mohamed Riaz Khan of BukuGames.
//All code is written by me (Above name) unless otherwise stated via comments below.
//Not authorized for use outside of the Github repository of this game developed by BukuGames.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Analytics;

public class DuelResultPhaseState : BaseState
{
    private DuelPhaseSM duelPhaseSM;
    private const string stateName = "DuelResultPhaseState";

    private bool isHandlingSupportCardEffect;
    private int indexOfCurrentSupportCardWeAreHandling;

    private DuelPlayerInformation duelPlayerInformationWhosSupportCardsWeAreHandling;
    public DuelResultPhaseState(DuelPhaseSM stateMachine) : base(stateName, stateMachine)
    {
        duelPhaseSM = stateMachine as DuelPhaseSM;
    }

    public override void Enter()
    {
        base.Enter();
        PhaseDisplay.instance.TurnOnDisplay($"Result phase", 1.5f);
        isHandlingSupportCardEffect = false;
        indexOfCurrentSupportCardWeAreHandling = 0;
        duelPlayerInformationWhosSupportCardsWeAreHandling = null;
        duelPhaseSM.FadePanelCompletedFadingDuel += TurnOffCameraAfterDuel;
        duelPhaseSM.StartCoroutine(duelPhaseSM.FadePanelActivate(0.5f));
    }

    public void TurnOffCameraAfterDuel()
    {
        Audio_Manager.Instance.TransitionBetweenDuelAndBoardMusic();
        duelPhaseSM.FadePanelCompletedFadingDuel -= TurnOffCameraAfterDuel;
        duelPhaseSM.gameplayManager.GameplayCameraManagerRef.TurnOffVirtualDuelCamera();

        foreach (DuelPlayerInformation duelPlayerInformation in duelPhaseSM.PlayersInCurrentDuel)
        {
            duelPhaseSM.StartCoroutine(duelPhaseSM.EndOfDuelResultAnimation(duelPlayerInformation));
        }
    }

    public void DamageResults()
    {
        bool firstTimeThroughLoop = false;
        foreach (DuelPlayerInformation playerInfo in duelPhaseSM.PlayersInCurrentDuel)
        {
            playerInfo.DamageToTake += 1;
            //It's a tie. Everyone takes 1 damage.
            if (duelPhaseSM.CurrentWinners.Count > 1)
            {
                playerInfo.PlayerInDuel.TakeDamage(playerInfo.DamageToTake);
               // DiscardSelectedCardsAfterDuel(playerInfo);
                continue;

                //if (duelPhaseSM.CurrentWinners.Count == duelPhaseSM.PlayersInCurrentDuel.Count)
                //{
                    
                //}

                //bool foundMatch = false;
                //foreach (DuelPlayerInformation winnerInfo in duelPhaseSM.CurrentWinners)
                //{
                //    if (playerInfo == winnerInfo)
                //    {
                //        foundMatch = true;
                //    }
                //}
                ////Means the player we are looking at was not in the list of Players that won. They lost the duel.
                //if (!foundMatch)
                //{
                //    //Will be more dynamic based on things like if players deal more damage this duel or if that class takes more damage when losing a duel etc.
                //    playerInfo.PlayerInDuel.TakeDamage(damageToDeal);
                //}
            }
            //Count should only be 1 for winners list in this case.
            else
            {
                //Add damage modifiers for anything that happens during damage calculation....Also too many foreach loops here >.> Might just do this before we even go through any characters? Idk yet.
                if(!firstTimeThroughLoop)
                {
                    foreach(SupportCard supportCard in duelPhaseSM.CurrentWinners[0].SelectedSupportCards)
                    {
                        foreach(SupportCardData.SupportCardEffect supportCardEffect in supportCard.SupportCardData.supportCardEffects)
                        {
                            if (supportCardEffect.supportCardEffectData.IsDuringDuelDamageCalc)
                            {
                                foreach (DuelPlayerInformation duelPlayerInfo in duelPhaseSM.PlayersInCurrentDuel)
                                {
                                    supportCardEffect.supportCardEffectData.EffectOfCard(duelPlayerInfo, supportCard);
                                }
                            }
                        }
                    }

                    firstTimeThroughLoop = true;
                }

                if (playerInfo != duelPhaseSM.CurrentWinners[0])
                {
                    playerInfo.PlayerInDuel.TakeDamage(playerInfo.DamageToTake);
                }
            }

            //DiscardSelectedCardsAfterDuel(playerInfo);
        }

        StartHandlingSupportCardEffects();
    }

    private void StartHandlingSupportCardEffects()
    {
        foreach(DuelPlayerInformation duelPlayerInformation in duelPhaseSM.PlayersInCurrentDuel)
        {
            if(duelPlayerInformation.PlayerInDuel.IsDefeated)
            {
                continue;
            }
            else
            {
                ActivateEndOfDuelSupportCardEffects(duelPlayerInformation);
                break;
            }
        }
    }

    //Method for applying after duel support card effects.
    private void ActivateEndOfDuelSupportCardEffects(DuelPlayerInformation playerWhoWeAreHandling)
    {
        foreach(SupportCard supportCard in playerWhoWeAreHandling.SelectedSupportCards)
        {
            foreach(SupportCardData.SupportCardEffect supportCardEffect in supportCard.SupportCardData.supportCardEffects)
			{
				if(supportCardEffect.supportCardEffectData.IsAfterDuelEffectAndNeedsToWin && duelPhaseSM.CurrentWinners.Count == 1 &&  duelPhaseSM.CurrentWinners.Contains(playerWhoWeAreHandling))
				{
                    isHandlingSupportCardEffect = true;
                    duelPlayerInformationWhosSupportCardsWeAreHandling = playerWhoWeAreHandling;
                    supportCardEffect.supportCardEffectData.SupportCardEffectCompleted += MoveOnToNextSupportCardEffect;
                    supportCardEffect.supportCardEffectData.EffectOfCard(playerWhoWeAreHandling);
				}
                else if(supportCardEffect.supportCardEffectData.IsAfterDuelEffect)
                {
                    isHandlingSupportCardEffect = true;
                    duelPlayerInformationWhosSupportCardsWeAreHandling = playerWhoWeAreHandling;
                    supportCardEffect.supportCardEffectData.SupportCardEffectCompleted += MoveOnToNextSupportCardEffect;
                    supportCardEffect.supportCardEffectData.EffectOfCard(playerWhoWeAreHandling);
                } 
                else
                {
                    Debug.Log($"This is not a end of duel effect.");
                }
            }

        }

        //Only call this part if we have completed all after duel effects...Diff method?
        int indexOfCurrentPlayer = duelPhaseSM.PlayersInCurrentDuel.IndexOf(playerWhoWeAreHandling) + 1;
        if(indexOfCurrentPlayer == duelPhaseSM.PlayersInCurrentDuel.Count)
        {
            //Discard the cards. We're done.
            foreach(DuelPlayerInformation duelPlayerInformation in duelPhaseSM.PlayersInCurrentDuel)
            {
                DiscardSelectedCardsAfterDuel(duelPlayerInformation);
            }

            duelPhaseSM.ChangeState(duelPhaseSM.duelNotDuelingPhaseState);
            duelPhaseSM.gameplayManager.GameplayPhaseStatemachineRef.ChangeState(duelPhaseSM.gameplayManager.GameplayPhaseStatemachineRef.gameplayResolveSpacePhaseState);
        }
        else
        {
            //Calling with index instead of index + 1 since we're already doing the +1 earlier so we want the next player.
            //Also should do a 'death' check here to see if the next player we are attempting to get is already defeated. If they are just skip them.
            ActivateEndOfDuelSupportCardEffects(duelPhaseSM.PlayersInCurrentDuel[indexOfCurrentPlayer]);
        }
    }

    public void MoveOnToNextSupportCardEffect(SupportCard supportCard = null)
    {
        foreach(SupportCard supportCardWeAreHandling in duelPlayerInformationWhosSupportCardsWeAreHandling.SelectedSupportCards)
        {
            //Unsubbing from all but this is fine. If we want to we can also keep track globally of the effect we're handling and just unsub from that one via checking if it matches...
            foreach(SupportCardData.SupportCardEffect supportCardEffect in supportCardWeAreHandling.SupportCardData.supportCardEffects)
            {
                supportCardEffect.supportCardEffectData.SupportCardEffectCompleted -= MoveOnToNextSupportCardEffect;
            }
        }

        isHandlingSupportCardEffect = false;
    }

    private void DiscardSelectedCardsAfterDuel(DuelPlayerInformation playerInfo)
    {
        //We'll need to resolve end of duel movement cards before discarding them here.
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

    public override void UpdateLogic()
    {
        base.UpdateLogic();
    }

    public override void Exit()
    {
        base.Exit();
        isHandlingSupportCardEffect = false;
        duelPhaseSM.EnableAbilityButtons();
        duelPhaseSM.ResetDuelParameters();
    }
}
