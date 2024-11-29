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

    private List<DuelPlayerInformation> playersLeftToHandleEndOfDuelSupportEffects;

    private List<DuelPlayerInformation> playersLeftToHandleEndOfDuelSpaceEffects;

    private DuelPlayerInformation duelPlayerInformationWhosEndOfSpaceEffectsWeAreHandling;

    private DuelPlayerInformation duelPlayerInformationWhosSupportCardsWeAreHandling;

    private List<SpaceData.SpaceEffect> spaceEffectsToHandle;

    private SpaceData.SpaceEffect currentSpaceEffectWeAreHandling;

    private List<SupportCardData.SupportCardEffect> supportCardEffectsTohandle;

    private SupportCardData.SupportCardEffect currentSupportCardEffectWeAreHandling;

    public DuelPlayerInformation DuelPlayerInformationWhosEndOfSpaceEffectsWeAreHandling { get => duelPlayerInformationWhosEndOfSpaceEffectsWeAreHandling; set => duelPlayerInformationWhosEndOfSpaceEffectsWeAreHandling = value; }
    public DuelPlayerInformation DuelPlayerInformationWhosSupportCardsWeAreHandling { get => duelPlayerInformationWhosSupportCardsWeAreHandling; set => duelPlayerInformationWhosSupportCardsWeAreHandling = value; }
    public List<DuelPlayerInformation> PlayersLeftToHandleEndOfDuelSupportEffects { get => playersLeftToHandleEndOfDuelSupportEffects; set => playersLeftToHandleEndOfDuelSupportEffects = value; }
    public List<DuelPlayerInformation> PlayersLeftToHandleEndOfDuelSpaceEffects { get => playersLeftToHandleEndOfDuelSpaceEffects; set => playersLeftToHandleEndOfDuelSpaceEffects = value; }
    public List<SpaceData.SpaceEffect> SpaceEffectsToHandle { get => spaceEffectsToHandle; set => spaceEffectsToHandle = value; }
    public SpaceData.SpaceEffect CurrentSpaceEffectWeAreHandling { get => currentSpaceEffectWeAreHandling; set => currentSpaceEffectWeAreHandling = value; }
    public List<SupportCardData.SupportCardEffect> SupportCardEffectsTohandle { get => supportCardEffectsTohandle; set => supportCardEffectsTohandle = value; }
    public SupportCardData.SupportCardEffect CurrentSupportCardEffectWeAreHandling { get => currentSupportCardEffectWeAreHandling; set => currentSupportCardEffectWeAreHandling = value; }

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
        DuelPlayerInformationWhosSupportCardsWeAreHandling = null;
        DuelPlayerInformationWhosEndOfSpaceEffectsWeAreHandling = null;
        PlayersLeftToHandleEndOfDuelSupportEffects = new();
        PlayersLeftToHandleEndOfDuelSpaceEffects = new();
        SpaceEffectsToHandle = new();
        CurrentSupportCardEffectWeAreHandling = null;
        SupportCardEffectsTohandle = new();
        CurrentSupportCardEffectWeAreHandling = null;
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

        //Setup the players to handle support cards of.
        foreach(DuelPlayerInformation duelPlayerInformation in duelPhaseSM.PlayersInCurrentDuel)
        {
            if(duelPlayerInformation.PlayerInDuel.IsDefeated)
            {
                //If this player died due to damage calc we'll probably still need to discard everything they have? ...I think death might auto discard but double check this.
                continue;
            }
            else
            {
                PlayersLeftToHandleEndOfDuelSupportEffects.Add(duelPlayerInformation);
            }
        }

        StartHandlingSupportCardEffects();
    }

    private void StartHandlingSupportCardEffects()
    {
        if(PlayersLeftToHandleEndOfDuelSupportEffects.Count != 0)
        {
            ActivateEndOfDuelSupportCardEffects(PlayersLeftToHandleEndOfDuelSupportEffects[0]);
        }
        else
        {
            FinishedHandlingAllSupportCardEffects();
        }
    }

    private void FinishedHandlingAllSupportCardEffects()
    {
        //Discard the cards. We're done.
        foreach(DuelPlayerInformation duelPlayerInformation in duelPhaseSM.PlayersInCurrentDuel)
        {
            DiscardSelectedCardsAfterDuel(duelPlayerInformation);
        }

         //Setup the players to handle space effects of.
        foreach(DuelPlayerInformation duelPlayerInformation in duelPhaseSM.PlayersInCurrentDuel)
        {
            if(duelPlayerInformation.PlayerInDuel.IsDefeated)
            {
                continue;
            }
            else
            {
                PlayersLeftToHandleEndOfDuelSpaceEffects.Add(duelPlayerInformation);
            }
        }

        StartHandlingEndOfDuelSpaceEffects();
    }

    private void StartHandlingEndOfDuelSpaceEffects()
    {
        // currentPlayer.CurrentSpacePlayerIsOn.StartCoroutine(currentPlayer.CurrentSpacePlayerIsOn.PlaySpaceInfoDisplayAnimationUI(currentPlayer));
        // gameplayPhaseSM.gameplayManager.SpaceArtworkPopupDisplay.TurnOnDisplay(currentPlayer.CurrentSpacePlayerIsOn, currentPlayer);
        // gameplayPhaseSM.gameplayManager.SpaceArtworkPopupDisplay.SpaceArtworkDisplayTurnOff += SpaceArtworkDonePopupDone;
        CurrentSpaceEffectWeAreHandling = null;
        if(PlayersLeftToHandleEndOfDuelSpaceEffects.Count != 0)
        {
            ActivateEndOfDuelSpaceEffects(PlayersLeftToHandleEndOfDuelSpaceEffects[0]);
        }
        else
        {
            FinishedHandlingEndOfSpaceDuelEffects();
        }
    }

    /// <summary>
    /// Unsubscribe from the current space effect we are handling as it is done.
    /// </summary>
    public void HandleCompletionOfEndOfDuelSpaceEffects(Player player = null)
    {
        DuelPlayerInformationWhosEndOfSpaceEffectsWeAreHandling.PlayerInDuel.FinishedHandlingCurrentSpaceEffects -= HandleCompletionOfEndOfDuelSpaceEffects;
        SpaceEffectsToHandle.Clear();
        StartHandlingEndOfDuelSpaceEffects();
    }

    public void FinishedHandlingEndOfSpaceDuelEffects()
    {
        duelPhaseSM.ChangeState(duelPhaseSM.duelNotDuelingPhaseState);
        duelPhaseSM.gameplayManager.GameplayPhaseStatemachineRef.ChangeState(duelPhaseSM.gameplayManager.GameplayPhaseStatemachineRef.gameplayResolveSpacePhaseState);
    }

    private void ActivateEndOfDuelSpaceEffects(DuelPlayerInformation playerWhoWeAreHandling)
    {
        if(PlayersLeftToHandleEndOfDuelSpaceEffects.Contains(playerWhoWeAreHandling))
        {
            PlayersLeftToHandleEndOfDuelSpaceEffects.Remove(playerWhoWeAreHandling);
        }

        DuelPlayerInformationWhosEndOfSpaceEffectsWeAreHandling = playerWhoWeAreHandling;
        bool hasAnEndEffect = false;
        SpaceData spaceData = playerWhoWeAreHandling.PlayerInDuel.CurrentSpacePlayerIsOn.spaceData;
        Space spaceWeAreUsing = playerWhoWeAreHandling.PlayerInDuel.CurrentSpacePlayerIsOn; 

        //For debug mode.
        SpaceData cachedSpaceData = spaceData;

        if (DebugModeSingleton.instance.IsDebugActive)
        {
            Space tempSpace = DebugModeSingleton.instance.OverrideSpaceLandEffect();
            spaceWeAreUsing = tempSpace;

            if (tempSpace != null)
            {
                spaceData = tempSpace.spaceData;
            }

        }

        foreach(SpaceData.SpaceEffect spaceEffect in spaceData.spaceEffects)
        {
            if(spaceEffect.spaceEffectData.AfterDuelEffect)
            {
                hasAnEndEffect = true;
                CurrentSpaceEffectWeAreHandling = spaceEffect;
                break;
            }
            else if(spaceEffect.spaceEffectData.IsAfterDuelEffectAndMustWin)
            {
                if(duelPhaseSM.CurrentWinners.Count == 1 && duelPhaseSM.CurrentWinners.Contains(DuelPlayerInformationWhosEndOfSpaceEffectsWeAreHandling))
                {
                    hasAnEndEffect = true;
                    CurrentSpaceEffectWeAreHandling = spaceEffect;
                    break;
                }
            }
        }

        //Revert the space data back if we used debug to change it.
        if (DebugModeSingleton.instance.IsDebugActive)
        {
            spaceData = cachedSpaceData;
        }

        //Do we just skip to calling this activate effect again if this is not true?
        if(hasAnEndEffect)
        {
            if(CurrentSpaceEffectWeAreHandling.spaceEffectData.IsAfterDuelEffectAndMustWin && !duelPhaseSM.CurrentWinners.Contains(DuelPlayerInformationWhosEndOfSpaceEffectsWeAreHandling))
            {
                StartHandlingEndOfDuelSpaceEffects();
            }
            else
            {
                duelPhaseSM.gameplayManager.SpaceArtworkPopupDisplay.TurnOnDisplay(spaceWeAreUsing, DuelPlayerInformationWhosEndOfSpaceEffectsWeAreHandling.PlayerInDuel);
			    duelPhaseSM.gameplayManager.SpaceArtworkPopupDisplay.SpaceArtworkDisplayTurnOff += BeginExecutingEndOfSpaceEffectsOfCurrentPlayer;
            }
        }
        else
        {
            StartHandlingEndOfDuelSpaceEffects();
        }
    }

    private void BeginExecutingEndOfSpaceEffectsOfCurrentPlayer(Player player = null)
    {
        //Turning off the display will make it go through the space effects.
        duelPhaseSM.gameplayManager.SpaceArtworkPopupDisplay.SpaceArtworkDisplayTurnOff -= BeginExecutingEndOfSpaceEffectsOfCurrentPlayer;
        DuelPlayerInformationWhosEndOfSpaceEffectsWeAreHandling.PlayerInDuel.FinishedHandlingCurrentSpaceEffects += HandleCompletionOfEndOfDuelSpaceEffects;
    }

    //Method for applying after duel support card effects.
    private void ActivateEndOfDuelSupportCardEffects(DuelPlayerInformation playerWhoWeAreHandling)
    {
        if(PlayersLeftToHandleEndOfDuelSupportEffects.Contains(playerWhoWeAreHandling))
        {
            PlayersLeftToHandleEndOfDuelSupportEffects.Remove(playerWhoWeAreHandling);
        }

        //Right now this logic will only work for 1 support card. If the player selects 2+ support cards we will need to find a way to
        //loop back through here and catch the other support cards that we have not handled yet.
        foreach(SupportCard supportCard in playerWhoWeAreHandling.SelectedSupportCards)
        {
            foreach(SupportCardData.SupportCardEffect supportCardEffect in supportCard.SupportCardData.supportCardEffects)
			{
                if((supportCardEffect.supportCardEffectData.IsACost && supportCardEffect.supportCardEffectData.CanCostBePaid(playerWhoWeAreHandling, supportCard, true)) || !supportCardEffect.supportCardEffectData.IsACost)
                {
                    if(supportCardEffect.supportCardEffectData.IsAfterDuelEffectAndNeedsToWin && duelPhaseSM.CurrentWinners.Count == 1 &&  duelPhaseSM.CurrentWinners.Contains(playerWhoWeAreHandling))
                    {
                        isHandlingSupportCardEffect = true;
                        DuelPlayerInformationWhosSupportCardsWeAreHandling = playerWhoWeAreHandling;
                        DuelPlayerInformationWhosSupportCardsWeAreHandling.PlayerInDuel.IsHandlingSupportCardEffects = true;
                        SupportCardEffectsTohandle.Add(supportCardEffect);
                        // supportCardEffect.supportCardEffectData.SupportCardEffectCompleted += MoveOnToNextSupportCardEffect;
                        // supportCardEffect.supportCardEffectData.EffectOfCard(playerWhoWeAreHandling);
                    }
                    else if(supportCardEffect.supportCardEffectData.IsAfterDuelEffect)
                    {
                        isHandlingSupportCardEffect = true;
                        DuelPlayerInformationWhosSupportCardsWeAreHandling = playerWhoWeAreHandling;
                        DuelPlayerInformationWhosSupportCardsWeAreHandling.PlayerInDuel.IsHandlingSupportCardEffects = true;
                        SupportCardEffectsTohandle.Add(supportCardEffect);
                        // supportCardEffect.supportCardEffectData.SupportCardEffectCompleted += MoveOnToNextSupportCardEffect;
                        // supportCardEffect.supportCardEffectData.EffectOfCard(playerWhoWeAreHandling);
                    } 
                    else
                    {
                        Debug.Log($"This is not a end of duel effect.");
                    }
                }
            }

            if(supportCardEffectsTohandle.Count > 0)
            {
                BeginExecutingSupportCardEffectsOfCurrentPlayer();
                return;
            }
        }

        StartHandlingSupportCardEffects();
    }

    private void BeginExecutingSupportCardEffectsOfCurrentPlayer()
    {
        if(SupportCardEffectsTohandle.Count == 0)
        {
            isHandlingSupportCardEffect = false;
            DuelPlayerInformationWhosSupportCardsWeAreHandling.PlayerInDuel.IsHandlingSupportCardEffects = false;
            StartHandlingSupportCardEffects();
            return;
        }

        if(CurrentSupportCardEffectWeAreHandling is null)
        {
            CurrentSupportCardEffectWeAreHandling = SupportCardEffectsTohandle[0];
            CurrentSupportCardEffectWeAreHandling.supportCardEffectData.SupportCardEffectCompleted += MoveOnToNextSupportCardEffect;
            //If the player info is null there's an issue. It shouldn't be.
            CurrentSupportCardEffectWeAreHandling.supportCardEffectData.EffectOfCard(DuelPlayerInformationWhosSupportCardsWeAreHandling);
        }
    }

    public void MoveOnToNextSupportCardEffect(SupportCard supportCard = null)
    {
        SupportCardEffectsTohandle.RemoveAt(0);
        CurrentSupportCardEffectWeAreHandling.supportCardEffectData.SupportCardEffectCompleted -= MoveOnToNextSupportCardEffect;
        CurrentSupportCardEffectWeAreHandling = null;
        BeginExecutingSupportCardEffectsOfCurrentPlayer();
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
        CurrentSpaceEffectWeAreHandling = null;
        PlayersLeftToHandleEndOfDuelSupportEffects.Clear();
        PlayersLeftToHandleEndOfDuelSpaceEffects.Clear();
        SupportCardEffectsTohandle.Clear();
        SpaceEffectsToHandle.Clear();
        duelPhaseSM.EnableAbilityButtons();
        duelPhaseSM.ResetDuelParameters();
    }
}
