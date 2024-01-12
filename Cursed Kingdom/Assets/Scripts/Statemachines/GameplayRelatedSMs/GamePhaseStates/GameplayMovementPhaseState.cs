//Code written by Mohamed Riaz Khan of BukuGames.
//All code is written by me (Above name) unless otherwise stated via comments below.
//Not authorized for use outside of the Github repository of this game developed by BukuGames.
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
			currentPlayer.GameplayManagerRef.UseMovementCardNoCardsInHandButton.onClick.AddListener(currentPlayer.DrawThenUseMovementCardImmediately);
			playerWentToZeroCards = true;
        }
		if(currentPlayer.NumMovementCardsUsedThisTurn != 0 && currentPlayer.SpacesLeftToMove == 0)
		{
			gameplayPhaseSM.ChangeState(gameplayPhaseSM.gameplayResolveSpacePhaseState);
		}
	}

	public override void Exit()
	{
		base.Exit();
		playerStartedMoving = false;
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
        //else if (spacesAlreadyChecked.Contains(spaceToGetNeighborsFrom.NorthNeighbor) && !spacesJustChecked.Contains(spaceToGetNeighborsFrom.NorthNeighbor) && spaceToGetNeighborsFrom.ValidDirectionsFromThisSpace.Contains(Space.Direction.North))
        //{
        //    if (spaceToGetNeighborsFrom.NorthNeighbor.spaceData.DecreasesSpacesToMove)
        //    {
        //        spacesToCheckNext.Add(spaceToGetNeighborsFrom.NorthNeighbor);
        //    }
        //}

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
        //else if (spacesAlreadyChecked.Contains(spaceToGetNeighborsFrom.SouthNeighbor) && !spacesJustChecked.Contains(spaceToGetNeighborsFrom.SouthNeighbor) && spaceToGetNeighborsFrom.ValidDirectionsFromThisSpace.Contains(Space.Direction.South))
        //{
        //    if (spaceToGetNeighborsFrom.SouthNeighbor.spaceData.DecreasesSpacesToMove)
        //    {
        //        spacesToCheckNext.Add(spaceToGetNeighborsFrom.SouthNeighbor);
        //    }
        //}

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
        //else if (spacesAlreadyChecked.Contains(spaceToGetNeighborsFrom.EastNeighbor) && !spacesJustChecked.Contains(spaceToGetNeighborsFrom.EastNeighbor) && spaceToGetNeighborsFrom.ValidDirectionsFromThisSpace.Contains(Space.Direction.East))
        //{
        //    if (spaceToGetNeighborsFrom.EastNeighbor.spaceData.DecreasesSpacesToMove)
        //    {
        //        spacesToCheckNext.Add(spaceToGetNeighborsFrom.EastNeighbor);
        //    }
        //}

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
        //else if (spacesAlreadyChecked.Contains(spaceToGetNeighborsFrom.WestNeighbor) && !spacesJustChecked.Contains(spaceToGetNeighborsFrom.WestNeighbor) && spaceToGetNeighborsFrom.ValidDirectionsFromThisSpace.Contains(Space.Direction.West))
        //{
        //    if (spaceToGetNeighborsFrom.WestNeighbor.spaceData.DecreasesSpacesToMove)
        //    {
        //        spacesToCheckNext.Add(spaceToGetNeighborsFrom.WestNeighbor);
        //    }
        //}

        return spacesToCheckNext;
    }
}
