//Code written by Mohamed Riaz Khan of BukuGames.
//All code is written by me (Above name) unless otherwise stated via comments below.
//Not authorized for use outside of the Github repository of this game developed by BukuGames.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameplayEndPhaseState : BaseState
{
    private GameplayPhaseSM gameplayPhaseSM;
    private Player currentPlayer;
    private bool endingTurn;
    private const string stateName = "GameplayEndPhaseState";
    public GameplayEndPhaseState(GameplayPhaseSM stateMachine) : base(stateName, stateMachine)
    {
        gameplayPhaseSM = stateMachine as GameplayPhaseSM;
    }


    public override void Enter()
    {
        base.Enter();
        currentPlayer = gameplayPhaseSM.gameplayManager.GetCurrentPlayer();
        SubscribeToPlayerEvents();
        endingTurn = false;
        PhaseDisplay.instance.TurnOnDisplay("End phase!", 1.5f);
        PhaseDisplay.instance.displayTimeCompleted += Logic;
        gameplayPhaseSM.gameplayManager.HandDisplayPanel.ShrinkHand(false);
        currentPlayer.HideHand();
    }

    public override void UpdateLogic()
    {
        base.UpdateLogic();
    }

    public void Logic()
    {
        if (!endingTurn)
        {
            endingTurn = true;
            HandleEndOfTurnEffects();
        }
    }

    public override void Exit()
    {
        base.Exit();
        UnsubscribeToPlayerEvents();
        endingTurn = false;
        PhaseDisplay.instance.displayTimeCompleted -= Logic;
    }

    private void SubscribeToPlayerEvents()
    {
        currentPlayer.TurnHasEnded += EndTurn;
    }

    private void UnsubscribeToPlayerEvents()
    {
        currentPlayer.TurnHasEnded -= EndTurn;
    }

    private void HandleEndOfTurnEffects()
    {
        if (!currentPlayer.IsDefeated)
        {
            gameplayPhaseSM.gameplayManager.ThisDeckManager.DrawCard(Card.CardType.Movement, currentPlayer);
            //THIS DOESN'T WAIT FOR ALL END PHASE EFFS TO HAPPEN BEFORE ENDING THE TURN...MIGHT BE AN ISSUE.
            currentPlayer.ApplyCurrentSpaceEffects(currentPlayer);
        }

        if (!currentPlayer.IsMoving && !currentPlayer.MaxHandSizeExceeded())
        {
            //Reset all player usage counts.
            foreach (Player player in gameplayPhaseSM.gameplayManager.Players)
            {
                if (!player.IsDefeated)
                {
                    player.ResetSupportCardUsageCount();
                    player.ResetMovementCardUsageCount();
                }
            }

            //Idk if we wanna do this here because if the player can view their hand on another player's turn it still needs to reflect.
            currentPlayer.ResetMovementCardsInHandValues();

            if (currentPlayer.IsCursed)
            {
                currentPlayer.CurseEffect();
            }
            currentPlayer.TurnIsCompleted();
        }
    }

    private void EndTurn(Player theCurrentPlayer)
    {
        int numPlayersDefeated = 0;
        Player currentNonDefeatedPlayer = currentPlayer;

        currentPlayer.UpdateStatusEffectCount();
        currentPlayer.UpdateCooldownStatus();


        foreach (Player player in gameplayPhaseSM.gameplayManager.Players)
        {
            if (player == currentPlayer)
            {
                int indexOfCurrentPlayer = gameplayPhaseSM.gameplayManager.Players.IndexOf(player);
                if (indexOfCurrentPlayer == gameplayPhaseSM.gameplayManager.Players.Count - 1)
                {
                    foreach (Player thePlayer in gameplayPhaseSM.gameplayManager.Players)
                    {
                        if (!gameplayPhaseSM.gameplayManager.IsPlayerDefeated(thePlayer))
                        {
                            gameplayPhaseSM.gameplayManager.playerCharacter = gameplayPhaseSM.gameplayManager.Players[gameplayPhaseSM.gameplayManager.Players.IndexOf(thePlayer)].GetComponent<Transform>();
                            break;
                        }
                    }
                }
                else
                {
                    int numPlayersAfterToCheck = 0;
                    bool startingOver = false;
                    numPlayersAfterToCheck = indexOfCurrentPlayer + 1;

                    for (int i = numPlayersAfterToCheck; i < gameplayPhaseSM.gameplayManager.Players.Count; i++)
                    {
                        if (!gameplayPhaseSM.gameplayManager.Players[i].IsDefeated)
                        {
                            gameplayPhaseSM.gameplayManager.playerCharacter = gameplayPhaseSM.gameplayManager.Players[i].GetComponent<Transform>();
                            break;
                        }

                        if (i == gameplayPhaseSM.gameplayManager.Players.Count - 1 && !startingOver)
                        {
                            startingOver = true;
                            i = -1;
                        }
                    }

                    // playerCharacter = Players[indexOfCurrentPlayer + 1].GetComponent<Transform>();
                }
                gameplayPhaseSM.gameplayManager.cinemachineVirtualCameras[0].LookAt = gameplayPhaseSM.gameplayManager.playerCharacter;
                gameplayPhaseSM.gameplayManager.cinemachineVirtualCameras[0].Follow = gameplayPhaseSM.gameplayManager.playerCharacter;
                Player nextPlayer = gameplayPhaseSM.gameplayManager.playerCharacter.GetComponent<Player>();
                gameplayPhaseSM.gameplayManager.HandDisplayPanel.SetCurrentActiveHandUI(gameplayPhaseSM.gameplayManager.Players.IndexOf(nextPlayer));
                gameplayPhaseSM.gameplayManager.HandDisplayPanel.ShrinkHand();
                currentPlayer.HideHand();
                nextPlayer.ShowHand();

                if (!nextPlayer.IsOnCooldown && nextPlayer.ClassData.abilityData.CanBeManuallyActivated)
                {
                    gameplayPhaseSM.gameplayManager.UseAbilityButton.transform.parent.gameObject.SetActive(true);
                    gameplayPhaseSM.gameplayManager.UseAbilityButton.onClick.RemoveAllListeners();
                    gameplayPhaseSM.gameplayManager.UseAbilityButton.onClick.AddListener(nextPlayer.UseAbility);
                }
                else
                {
                    gameplayPhaseSM.gameplayManager.UseAbilityButton.transform.parent.gameObject.SetActive(false);
                    gameplayPhaseSM.gameplayManager.UseAbilityButton.onClick.RemoveAllListeners();
                }

                if (nextPlayer.CanUseEliteAbility && nextPlayer.ClassData.eliteAbilityData.CanBeManuallyActivated)
                {
                    gameplayPhaseSM.gameplayManager.UseEliteAbilityButton.transform.parent.gameObject.SetActive(true);
                    gameplayPhaseSM.gameplayManager.UseEliteAbilityButton.onClick.RemoveAllListeners();
                    gameplayPhaseSM.gameplayManager.UseEliteAbilityButton.onClick.AddListener(nextPlayer.UseEliteAbility);
                }
                else
                {
                    gameplayPhaseSM.gameplayManager.UseEliteAbilityButton.transform.parent.gameObject.SetActive(false);
                    gameplayPhaseSM.gameplayManager.UseEliteAbilityButton.onClick.RemoveAllListeners();
                }

                if (DebugModeSingleton.instance.IsDebugActive && DebugModeSingleton.instance.OverrideSpaceLandEffect() != null)
                {
                    Space tempSpace = DebugModeSingleton.instance.OverrideSpaceLandEffect();
                    Space currentSpace = nextPlayer.CurrentSpacePlayerIsOn;
                    if (tempSpace != null)
                    {
                        nextPlayer.CurrentSpacePlayerIsOn = tempSpace;
                    }


                    bool hasStartTurnEffect = false;
                    foreach (SpaceData.SpaceEffect spaceEffectData in tempSpace.spaceData.spaceEffects)
                    {
                        if (spaceEffectData.spaceEffectData.OnSpaceTurnStartEffect)
                        {
                            hasStartTurnEffect = true;
                            break;
                        }
                    }
                    if (hasStartTurnEffect)
                    {
                        //nextPlayer.startOfTurnEffect = true;
                        gameplayPhaseSM.ChangeState(gameplayPhaseSM.gameplayStartPhase);
                        //ActivateStartTurnPopup(nextPlayer, tempSpace);
                    }
                    else
                    {
                        gameplayPhaseSM.ChangeState(gameplayPhaseSM.gameplayStartPhase);
                        // DialogueBoxPopup.instance.ActivatePopupWithConfirmation($"Player {nextPlayer.playerIDIntVal}'s turn!", "Start!", "Turn start");
                    }

                    nextPlayer.CurrentSpacePlayerIsOn = currentSpace;
                    break;
                }
                else
                {
                    bool hasStartTurnEffect = false;
                    foreach (SpaceData.SpaceEffect spaceEffectData in nextPlayer.CurrentSpacePlayerIsOn.spaceData.spaceEffects)
                    {
                        if (spaceEffectData.spaceEffectData.OnSpaceTurnStartEffect)
                        {
                            hasStartTurnEffect = true;
                            break;
                        }
                    }
                    if (hasStartTurnEffect)
                    {
                        //nextPlayer.startOfTurnEffect = true;
                        gameplayPhaseSM.ChangeState(gameplayPhaseSM.gameplayStartPhase);
                        // ActivateStartTurnPopup(nextPlayer, nextPlayer.CurrentSpacePlayerIsOn);
                    }
                    else
                    {
                        gameplayPhaseSM.ChangeState(gameplayPhaseSM.gameplayStartPhase);
                        //DialogueBoxPopup.instance.ActivatePopupWithConfirmation($"Player {nextPlayer.playerIDIntVal}'s turn!", "Start!", "Turn start");
                    }
                    break;
                }
            }
        }
    }

    private Player FindNextPlayer(int indexOfCurrentPlayer)
    {
        Player nextPlayer = null;

        int numPlayersAfterToCheck = 0;
        bool startingOver = false;
        numPlayersAfterToCheck = indexOfCurrentPlayer + 1;

        for (int i = numPlayersAfterToCheck; i < gameplayPhaseSM.gameplayManager.Players.Count; i++)
        {
            if (!gameplayPhaseSM.gameplayManager.Players[i].IsDefeated)
            {
                gameplayPhaseSM.gameplayManager.playerCharacter = gameplayPhaseSM.gameplayManager.Players[i].GetComponent<Transform>();
                nextPlayer = gameplayPhaseSM.gameplayManager.Players[i];
                break;
            }

            //Loop doesn't start over and instead we just keep the current player...Need to reloop it again starting from 0.
            if (i == gameplayPhaseSM.gameplayManager.Players.Count && !startingOver)
            {
                startingOver = true;
                FindNextPlayer(0);
            }
        }

        return nextPlayer;

    }
}
