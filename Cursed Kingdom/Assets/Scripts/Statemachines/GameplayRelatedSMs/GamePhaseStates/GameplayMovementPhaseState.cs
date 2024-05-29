//Code written by Mohamed Riaz Khan of BukuGames.
//All code is written by me (Above name) unless otherwise stated via comments below.
//Not authorized for use outside of the Github repository of this game developed by BukuGames.
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class GameplayMovementPhaseState : BaseState
{
	private GameplayPhaseSM gameplayPhaseSM;
	private Player currentPlayer;
	private bool playerStartedMoving;
	private bool playerWentToZeroCards;
    private bool checkedForDuelOpponents;
	private const string stateName = "GameplayMovementPhaseState";
	public GameplayMovementPhaseState(GameplayPhaseSM stateMachine) : base(stateName, stateMachine)
	{
		gameplayPhaseSM = stateMachine as GameplayPhaseSM;
	}


	public override void Enter()
	{
		base.Enter();
        currentPlayer = gameplayPhaseSM.gameplayManager.GetCurrentPlayer();
		playerStartedMoving = false;
		playerWentToZeroCards = false;
        checkedForDuelOpponents = false;
        PhaseDisplay.instance.TurnOnDisplay("Movement phase!", 1.5f);
        currentPlayer.ShowHand();
    }

	public override void UpdateLogic()
	{
		base.UpdateLogic();
		if(!playerWentToZeroCards && currentPlayer.MovementCardsInHandCount == 0 && currentPlayer.NumMovementCardsUsedThisTurn == 0)
		{
			currentPlayer.GameplayManagerRef.UseMovementCardNoCardsInHandButton.gameObject.SetActive(true);
			currentPlayer.GameplayManagerRef.UseMovementCardNoCardsInHandButton.onClick.RemoveAllListeners();
			currentPlayer.GameplayManagerRef.UseMovementCardNoCardsInHandButton.onClick.AddListener(currentPlayer.DrawThenUseMovementCardImmediatelyMovement);
			playerWentToZeroCards = true;
        }

        
		if(currentPlayer.NumMovementCardsUsedThisTurn != 0 && currentPlayer.SpacesLeftToMove == 0 && !checkedForDuelOpponents)
		{
            checkedForDuelOpponents = true;
            //Check if there are any players within range of the Player who's turn it is. If there is at least 1 player, popup a box asking them if they want to duel. Yes = duel phase No = resolve space phase
            List<Player> playersToDuel = new List<Player>();
            if (!currentPlayer.CurrentSpacePlayerIsOn.spaceData.IsNonDuelSpace)
            {
                playersToDuel.Add(currentPlayer);
                playersToDuel.AddRange(FindPlayersInDuelRange(currentPlayer, currentPlayer.RangeOfSpacesToLookForDuelOpponents));
            }

            if(playersToDuel.Count > 1)
            {
                //Popup
                ActivatePopupForDuelNotDuel(playersToDuel);

                //Debug.Log($"We found {playersToDuel.Count - 1} players to duel with!");
            }
            //No opponents in range: Skip duel phase.
            else
            {
                gameplayPhaseSM.ChangeState(gameplayPhaseSM.gameplayResolveSpacePhaseState);
            }
		}
	}

	public override void Exit()
	{
		base.Exit();
		playerStartedMoving = false;
        checkedForDuelOpponents = false;

    }

    public void ActivatePopupForDuelNotDuel(List<Player> playersToDuelAgainst)
    {
        List<Tuple<string,string, object, List<object>>> insertedParams = new();
        List<object> paramsList = new();
        insertedParams.Add(Tuple.Create<string,string, object, List<object>>("Yes", nameof(gameplayPhaseSM.GoToDuelState), gameplayPhaseSM, paramsList));

        paramsList.Add(playersToDuelAgainst);
        insertedParams.Add(Tuple.Create<string, string, object, List<object>>("No", nameof(gameplayPhaseSM.GoToSpaceResolutionState), gameplayPhaseSM, paramsList));

        DialogueBoxPopup.instance.ActivatePopupWithButtonChoices($"Do you wish to duel? There are {playersToDuelAgainst.Count - 1} opponents to duel against.", insertedParams, 1, "Duel selection");
    }

    

    public List<Player> FindPlayersInDuelRange(Player playerReference, int rangeOfSpacesToLook = 3)
    {
        List<Player> playersInDuelRange = new();
        List<Space> spacesToCheckNext = new ();
        List<Space> spacesJustChecked = new();

        List<Space> spacesAlreadyChecked = new();
        

        for (int i = 0; i < rangeOfSpacesToLook; i++)
        {
            if (i == 0)
            {
                spacesToCheckNext = GetSpacesToCheckFromCurrentSpaceDuel(playerReference.CurrentSpacePlayerIsOn, ref spacesAlreadyChecked, ref spacesJustChecked);
            }

            //This temporary list needs to be used because we can't change 'spacesToCheckNext' while in the foreach. We will change it after the foreach ends.
            List<Space> tempSpacesToCheckNext = new();
            spacesJustChecked.Clear();
            foreach (Space space in spacesToCheckNext)
            {
                if (!space.spaceData.IsNonDuelSpace)
                {
                    foreach (Player playerOnSpace in space.playersOnThisSpace)
                    {
                        if (playerOnSpace != currentPlayer && !playersInDuelRange.Contains(playerOnSpace))
                        {
                            if (playersInDuelRange.Count == 0)
                            {
                                playersInDuelRange.Add(playerOnSpace);
                                continue;
                            }

                            //If the Player we want to add goes first in the turn order; insert them before the final player.
                            if (gameplayPhaseSM.gameplayManager.Players.IndexOf(playerOnSpace) < gameplayPhaseSM.gameplayManager.Players.IndexOf(playersInDuelRange[playersInDuelRange.Count - 1]))
                            {
                                if (playersInDuelRange.Count == 1)
                                {
                                    playersInDuelRange.Insert(0, playerOnSpace);
                                }
                                else
                                {
                                    playersInDuelRange.Insert(playersInDuelRange.Count - 1, playerOnSpace);
                                }
                            }
                            else
                            {
                                playersInDuelRange.Add(playerOnSpace);
                            }
                        }
                    }
                }

                if (!spacesAlreadyChecked.Contains(space))
                {
                    spacesAlreadyChecked.Add(space);
                }

                List<Space> spacesAlreadyCheckedBefore = spacesAlreadyChecked;
                List<Space> superTempSpacesToCheckNext = GetSpacesToCheckFromCurrentSpaceDuel(space, ref spacesAlreadyChecked, ref spacesJustChecked);

                //If we get more than 1 space to check next...we need to have a list of previous spaces checked for that new spaces as a new 'origin' of sorts.
                //Ex: if superTempSpacesToCheckNext.count > 1 



                //To ensure that we get a list of the ones that were JUST checked. This is for spaces that the player can go multiple directions on.
                foreach (Space spaceChecked in spacesAlreadyChecked)
                {
                    if (!spacesAlreadyCheckedBefore.Contains(spaceChecked))
                    {
                        spacesJustChecked.Add(spaceChecked);
                    }
                }

                foreach (Space superSpace in superTempSpacesToCheckNext)
                {
                    if (!tempSpacesToCheckNext.Contains(superSpace))
                    {
                        tempSpacesToCheckNext.Add(superSpace);
                    }
                }
            }

            spacesToCheckNext = tempSpacesToCheckNext;
        }

        return playersInDuelRange;
    }

    public List<Space> FindValidSpaces(Player playerReference, int spacesLeftToMove)
    {
        List<Space> validSpaces = new();

        //Loop through all spaces up to the range in every direction and see if there is a Player on that space.
        List<Space> spacesToCheckNext = new();
        List<Space> spacesJustChecked = new();

        //This list needs to only keep track of the immediate spaces we just checked. If we keep adding more to it, if you can get to the same space multiple ways then it won't keep track of that.
        List<Space> spacesAlreadyChecked = new();
        for (int i = 0; i < spacesLeftToMove; i++)
        {
            if (i == 0)
            {
                spacesToCheckNext = GetSpacesToCheckFromCurrentSpace(playerReference.CurrentSpacePlayerIsOn, ref spacesAlreadyChecked, ref spacesJustChecked);   
            }

            //This temporary list needs to be used because we can't change 'spacesToCheckNext' while in the foreach. We will change it after the foreach ends.
            List<Space> tempSpacesToCheckNext = new();
            spacesJustChecked.Clear();
            foreach (Space space in spacesToCheckNext)
            {
                if(i == spacesLeftToMove -1)
                {
                    //If it's a space that doesn't decrease your movement: Don't add it.
                    if(space.spaceData.DecreasesSpacesToMove && !validSpaces.Contains(space))
                    {
                        validSpaces.Add(space);
                    }
                }
                
                if (!spacesAlreadyChecked.Contains(space))
                {
                    spacesAlreadyChecked.Add(space);
                }

                List<Space> spacesAlreadyCheckedBefore = spacesAlreadyChecked;
                List<Space> superTempSpacesToCheckNext = GetSpacesToCheckFromCurrentSpace(space, ref spacesAlreadyChecked, ref spacesJustChecked);

                //If we get more than 1 space to check next...we need to have a list of previous spaces checked for that new spaces as a new 'origin' of sorts.
                //Ex: if superTempSpacesToCheckNext.count > 1 

                

                //To ensure that we get a list of the ones that were JUST checked. This is for spaces that the player can go multiple directions on.
                foreach(Space spaceChecked in spacesAlreadyChecked)
                {
                    if(!spacesAlreadyCheckedBefore.Contains(spaceChecked))
                    {
                        spacesJustChecked.Add(spaceChecked);
                    }
                }

                foreach (Space superSpace in superTempSpacesToCheckNext)
                {
                    if (!tempSpacesToCheckNext.Contains(superSpace))
                    {
                        tempSpacesToCheckNext.Add(superSpace);
                    }
                }
            }

            spacesToCheckNext = tempSpacesToCheckNext;
        }
        
        return validSpaces;
    }

    private List<Space> GetSpacesToCheckFromCurrentSpace(Space spaceToGetNeighborsFrom, ref List<Space> spacesAlreadyChecked, ref List<Space> spacesJustChecked)
    {
        List<Space> spacesToCheckNext = new();

        
        //Check all 4 neighbors.
        if (spaceToGetNeighborsFrom.NorthNeighbor != null && !spacesAlreadyChecked.Contains(spaceToGetNeighborsFrom.NorthNeighbor) && spaceToGetNeighborsFrom.ValidDirectionsFromThisSpace.Contains(Space.Direction.North) && !spacesJustChecked.Contains(spaceToGetNeighborsFrom.NorthNeighbor))
        {
            if (!spaceToGetNeighborsFrom.NorthNeighbor.spaceData.DecreasesSpacesToMove)
            {
                if (spaceToGetNeighborsFrom.NorthNeighbor.spaceData.spaceEffects[0].spaceEffectData.GetType() == typeof(BarricadeSpace))
                {
                    BarricadeSpace barricadeSpace = spaceToGetNeighborsFrom.NorthNeighbor.spaceData.spaceEffects[0].spaceEffectData as BarricadeSpace;
                    if (currentPlayer.CurrentLevel >= barricadeSpace.LevelNeededToPass)
                    {
                        spacesAlreadyChecked.Add(spaceToGetNeighborsFrom.NorthNeighbor);
                        spacesJustChecked.Add(spaceToGetNeighborsFrom.NorthNeighbor);
                        spacesToCheckNext.AddRange(GetSpacesToCheckFromCurrentSpace(spaceToGetNeighborsFrom.NorthNeighbor, ref spacesAlreadyChecked, ref spacesJustChecked));
                    }
                }
                else
                {
                    spacesAlreadyChecked.Add(spaceToGetNeighborsFrom.NorthNeighbor);
                    spacesJustChecked.Add(spaceToGetNeighborsFrom.NorthNeighbor);
                    spacesToCheckNext.AddRange(GetSpacesToCheckFromCurrentSpace(spaceToGetNeighborsFrom.NorthNeighbor, ref spacesAlreadyChecked, ref spacesJustChecked));
                }
            }
            else
            {
                spacesToCheckNext.Add(spaceToGetNeighborsFrom.NorthNeighbor);
            }
        }

        if (spaceToGetNeighborsFrom.SouthNeighbor != null && !spacesAlreadyChecked.Contains(spaceToGetNeighborsFrom.SouthNeighbor) && spaceToGetNeighborsFrom.ValidDirectionsFromThisSpace.Contains(Space.Direction.South) && !spacesJustChecked.Contains(spaceToGetNeighborsFrom.SouthNeighbor))
        {
            if (!spaceToGetNeighborsFrom.SouthNeighbor.spaceData.DecreasesSpacesToMove)
            {
                if (spaceToGetNeighborsFrom.SouthNeighbor.spaceData.spaceEffects[0].spaceEffectData.GetType() == typeof(BarricadeSpace))
                {
                    BarricadeSpace barricadeSpace = spaceToGetNeighborsFrom.SouthNeighbor.spaceData.spaceEffects[0].spaceEffectData as BarricadeSpace;
                    if (currentPlayer.CurrentLevel >= barricadeSpace.LevelNeededToPass)
                    {
                        spacesAlreadyChecked.Add(spaceToGetNeighborsFrom.SouthNeighbor);
                        spacesJustChecked.Add(spaceToGetNeighborsFrom.SouthNeighbor);
                        spacesToCheckNext.AddRange(GetSpacesToCheckFromCurrentSpace(spaceToGetNeighborsFrom.SouthNeighbor, ref spacesAlreadyChecked, ref spacesJustChecked));
                    }
                }
                else
                {
                    spacesAlreadyChecked.Add(spaceToGetNeighborsFrom.SouthNeighbor);
                    spacesJustChecked.Add(spaceToGetNeighborsFrom.SouthNeighbor);
                    spacesToCheckNext.AddRange(GetSpacesToCheckFromCurrentSpace(spaceToGetNeighborsFrom.SouthNeighbor, ref spacesAlreadyChecked, ref spacesJustChecked));
                }
            }
            else
            {
                spacesToCheckNext.Add(spaceToGetNeighborsFrom.SouthNeighbor);
            }
        }

        if (spaceToGetNeighborsFrom.EastNeighbor != null && !spacesAlreadyChecked.Contains(spaceToGetNeighborsFrom.EastNeighbor) && spaceToGetNeighborsFrom.ValidDirectionsFromThisSpace.Contains(Space.Direction.East) && !spacesJustChecked.Contains(spaceToGetNeighborsFrom.EastNeighbor))
        {
            if (!spaceToGetNeighborsFrom.EastNeighbor.spaceData.DecreasesSpacesToMove)
            {
                if (spaceToGetNeighborsFrom.EastNeighbor.spaceData.spaceEffects[0].spaceEffectData.GetType() == typeof(BarricadeSpace))
                {
                    BarricadeSpace barricadeSpace = spaceToGetNeighborsFrom.EastNeighbor.spaceData.spaceEffects[0].spaceEffectData as BarricadeSpace;
                    if (currentPlayer.CurrentLevel >= barricadeSpace.LevelNeededToPass)
                    {
                        spacesAlreadyChecked.Add(spaceToGetNeighborsFrom.EastNeighbor);
                        spacesJustChecked.Add(spaceToGetNeighborsFrom.EastNeighbor);
                        spacesToCheckNext.AddRange(GetSpacesToCheckFromCurrentSpace(spaceToGetNeighborsFrom.EastNeighbor, ref spacesAlreadyChecked, ref spacesJustChecked));
                    }
                }
                else
                {
                    spacesAlreadyChecked.Add(spaceToGetNeighborsFrom.EastNeighbor);
                    spacesJustChecked.Add(spaceToGetNeighborsFrom.EastNeighbor);
                    spacesToCheckNext.AddRange(GetSpacesToCheckFromCurrentSpace(spaceToGetNeighborsFrom.EastNeighbor, ref spacesAlreadyChecked, ref spacesJustChecked));
                }
            }
            else
            {
                spacesToCheckNext.Add(spaceToGetNeighborsFrom.EastNeighbor);
            }
        }

        if (spaceToGetNeighborsFrom.WestNeighbor != null && !spacesAlreadyChecked.Contains(spaceToGetNeighborsFrom.WestNeighbor) && spaceToGetNeighborsFrom.ValidDirectionsFromThisSpace.Contains(Space.Direction.West) && !spacesJustChecked.Contains(spaceToGetNeighborsFrom.WestNeighbor))
        {
            if (!spaceToGetNeighborsFrom.WestNeighbor.spaceData.DecreasesSpacesToMove)
            {
                if (spaceToGetNeighborsFrom.WestNeighbor.spaceData.spaceEffects[0].spaceEffectData.GetType() == typeof(BarricadeSpace))
                {
                    BarricadeSpace barricadeSpace = spaceToGetNeighborsFrom.WestNeighbor.spaceData.spaceEffects[0].spaceEffectData as BarricadeSpace;
                    if (currentPlayer.CurrentLevel >= barricadeSpace.LevelNeededToPass)
                    {
                        spacesAlreadyChecked.Add(spaceToGetNeighborsFrom.WestNeighbor);
                        spacesJustChecked.Add(spaceToGetNeighborsFrom.WestNeighbor);
                        spacesToCheckNext.AddRange(GetSpacesToCheckFromCurrentSpace(spaceToGetNeighborsFrom.WestNeighbor, ref spacesAlreadyChecked, ref spacesJustChecked));
                    }
                }
                else
                {
                    spacesAlreadyChecked.Add(spaceToGetNeighborsFrom.WestNeighbor);
                    spacesJustChecked.Add(spaceToGetNeighborsFrom.WestNeighbor);
                    spacesToCheckNext.AddRange(GetSpacesToCheckFromCurrentSpace(spaceToGetNeighborsFrom.WestNeighbor, ref spacesAlreadyChecked, ref spacesJustChecked));
                }
            }
            else
            {
                spacesToCheckNext.Add(spaceToGetNeighborsFrom.WestNeighbor);
            }
        }

        return spacesToCheckNext;
    }

    private List<Space> GetSpacesToCheckFromCurrentSpaceDuel(Space spaceToGetNeighborsFrom, ref List<Space> spacesAlreadyChecked, ref List<Space> spacesJustChecked)
    {
        List<Space> spacesToCheckNext = new();

        
        //Check all 4 neighbors.
        if (spaceToGetNeighborsFrom.NorthNeighbor != null && !spacesAlreadyChecked.Contains(spaceToGetNeighborsFrom.NorthNeighbor) && !spacesJustChecked.Contains(spaceToGetNeighborsFrom.NorthNeighbor))
        {
            if (!spaceToGetNeighborsFrom.NorthNeighbor.spaceData.DecreasesSpacesToMove)
            {
                if (spaceToGetNeighborsFrom.NorthNeighbor.spaceData.spaceEffects[0].spaceEffectData.GetType() == typeof(BarricadeSpace))
                {
                    BarricadeSpace barricadeSpace = spaceToGetNeighborsFrom.NorthNeighbor.spaceData.spaceEffects[0].spaceEffectData as BarricadeSpace;
                    spacesAlreadyChecked.Add(spaceToGetNeighborsFrom.NorthNeighbor);
                    spacesJustChecked.Add(spaceToGetNeighborsFrom.NorthNeighbor);
                    spacesToCheckNext.AddRange(GetSpacesToCheckFromCurrentSpaceDuel(spaceToGetNeighborsFrom.NorthNeighbor, ref spacesAlreadyChecked, ref spacesJustChecked));
                }
                else
                {
                    spacesAlreadyChecked.Add(spaceToGetNeighborsFrom.NorthNeighbor);
                    spacesJustChecked.Add(spaceToGetNeighborsFrom.NorthNeighbor);
                    spacesToCheckNext.AddRange(GetSpacesToCheckFromCurrentSpaceDuel(spaceToGetNeighborsFrom.NorthNeighbor, ref spacesAlreadyChecked, ref spacesJustChecked));
                }
            }
            else
            {
                spacesToCheckNext.Add(spaceToGetNeighborsFrom.NorthNeighbor);
            }
        }


        if (spaceToGetNeighborsFrom.SouthNeighbor != null && !spacesAlreadyChecked.Contains(spaceToGetNeighborsFrom.SouthNeighbor) && !spacesJustChecked.Contains(spaceToGetNeighborsFrom.SouthNeighbor))
        {
            if (!spaceToGetNeighborsFrom.SouthNeighbor.spaceData.DecreasesSpacesToMove)
            {
                if (spaceToGetNeighborsFrom.SouthNeighbor.spaceData.spaceEffects[0].spaceEffectData.GetType() == typeof(BarricadeSpace))
                {
                    BarricadeSpace barricadeSpace = spaceToGetNeighborsFrom.SouthNeighbor.spaceData.spaceEffects[0].spaceEffectData as BarricadeSpace;
                    spacesAlreadyChecked.Add(spaceToGetNeighborsFrom.SouthNeighbor);
                    spacesJustChecked.Add(spaceToGetNeighborsFrom.SouthNeighbor);
                    spacesToCheckNext.AddRange(GetSpacesToCheckFromCurrentSpaceDuel(spaceToGetNeighborsFrom.SouthNeighbor, ref spacesAlreadyChecked, ref spacesJustChecked));
                }
                else
                {
                    spacesAlreadyChecked.Add(spaceToGetNeighborsFrom.SouthNeighbor);
                    spacesJustChecked.Add(spaceToGetNeighborsFrom.SouthNeighbor);
                    spacesToCheckNext.AddRange(GetSpacesToCheckFromCurrentSpaceDuel(spaceToGetNeighborsFrom.SouthNeighbor, ref spacesAlreadyChecked, ref spacesJustChecked));
                }
            }
            else
            {
                spacesToCheckNext.Add(spaceToGetNeighborsFrom.SouthNeighbor);
            }
        }

        if (spaceToGetNeighborsFrom.EastNeighbor != null && !spacesAlreadyChecked.Contains(spaceToGetNeighborsFrom.EastNeighbor) && !spacesJustChecked.Contains(spaceToGetNeighborsFrom.EastNeighbor))
        {
            if (!spaceToGetNeighborsFrom.EastNeighbor.spaceData.DecreasesSpacesToMove)
            {
                if (spaceToGetNeighborsFrom.EastNeighbor.spaceData.spaceEffects[0].spaceEffectData.GetType() == typeof(BarricadeSpace))
                {
                    BarricadeSpace barricadeSpace = spaceToGetNeighborsFrom.EastNeighbor.spaceData.spaceEffects[0].spaceEffectData as BarricadeSpace;
                    spacesAlreadyChecked.Add(spaceToGetNeighborsFrom.EastNeighbor);
                    spacesJustChecked.Add(spaceToGetNeighborsFrom.EastNeighbor);
                    spacesToCheckNext.AddRange(GetSpacesToCheckFromCurrentSpaceDuel(spaceToGetNeighborsFrom.EastNeighbor, ref spacesAlreadyChecked, ref spacesJustChecked));
                }
                else
                {
                    spacesAlreadyChecked.Add(spaceToGetNeighborsFrom.EastNeighbor);
                    spacesJustChecked.Add(spaceToGetNeighborsFrom.EastNeighbor);
                    spacesToCheckNext.AddRange(GetSpacesToCheckFromCurrentSpaceDuel(spaceToGetNeighborsFrom.EastNeighbor, ref spacesAlreadyChecked, ref spacesJustChecked));
                }
            }
            else
            {
                spacesToCheckNext.Add(spaceToGetNeighborsFrom.EastNeighbor);
            }
        }


        if (spaceToGetNeighborsFrom.WestNeighbor != null && !spacesAlreadyChecked.Contains(spaceToGetNeighborsFrom.WestNeighbor) && !spacesJustChecked.Contains(spaceToGetNeighborsFrom.WestNeighbor))
        {
            if (!spaceToGetNeighborsFrom.WestNeighbor.spaceData.DecreasesSpacesToMove)
            {
                if (spaceToGetNeighborsFrom.WestNeighbor.spaceData.spaceEffects[0].spaceEffectData.GetType() == typeof(BarricadeSpace))
                {
                    BarricadeSpace barricadeSpace = spaceToGetNeighborsFrom.WestNeighbor.spaceData.spaceEffects[0].spaceEffectData as BarricadeSpace;
                    spacesAlreadyChecked.Add(spaceToGetNeighborsFrom.WestNeighbor);
                    spacesJustChecked.Add(spaceToGetNeighborsFrom.WestNeighbor);
                    spacesToCheckNext.AddRange(GetSpacesToCheckFromCurrentSpaceDuel(spaceToGetNeighborsFrom.WestNeighbor, ref spacesAlreadyChecked, ref spacesJustChecked));
                }
                else
                {
                    spacesAlreadyChecked.Add(spaceToGetNeighborsFrom.WestNeighbor);
                    spacesJustChecked.Add(spaceToGetNeighborsFrom.WestNeighbor);
                    spacesToCheckNext.AddRange(GetSpacesToCheckFromCurrentSpaceDuel(spaceToGetNeighborsFrom.WestNeighbor, ref spacesAlreadyChecked, ref spacesJustChecked));
                }
            }
            else
            {
                spacesToCheckNext.Add(spaceToGetNeighborsFrom.WestNeighbor);
            }
        }


        return spacesToCheckNext;
    }
}
