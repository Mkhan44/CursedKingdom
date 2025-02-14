//Code written by Mohamed Riaz Khan of BukuGames.
//All code is written by me (Above name) unless otherwise stated via comments below.
//Not authorized for use outside of the Github repository of this game developed by BukuGames.
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class PlayerCharacterMoveState : BaseState
{
	PlayerCharacterSM playerCharacterSM;
	private const string stateName = "PlayerCharacterMoveState";

    //Debug
    [SerializeField] private bool hasBeenOveriddenOnce;
	[SerializeField] private Player playerMakingDecisions;
    public bool HasBeenOveriddenOnce { get => hasBeenOveriddenOnce; }
    public Player PlayerMakingDecisions { get => playerMakingDecisions; set => playerMakingDecisions = value; }

    public PlayerCharacterMoveState(PlayerCharacterSM stateMachine) : base(stateName, stateMachine)
	{
		playerCharacterSM = stateMachine as PlayerCharacterSM;
		playerMakingDecisions = null;
    }

	public override void Enter()
	{
		base.Enter();
		playerCharacterSM.playerAnimator.SetBool(playerCharacterSM.ISMOVINGPARAMETER, true);
	}


	public override void UpdateLogic()
	{
		base.UpdateLogic();
		if(!playerCharacterSM.player.IsMoving && !playerCharacterSM.player.IsChoosingDirection)
		{
			if(playerCharacterSM.player.IsMovingInReverse)
			{
				SetupMoveReverse(playerCharacterSM.player);
			}
			else
			{
				SetupMove(playerCharacterSM.player);
			}
		}
	}
	public override void Exit()
	{
		//Right now this only works for if we revert boosts after moving. When we handle duel stuff we'll hafta also do it there.
		PlayerMakingDecisions = null;
		playerCharacterSM.player.RevertAllBoostedMovementCardValuesInHand();
    }


	//External methods

	public void SetupMove(Player playerToMove)
	{
		Space currentSpacePlayerIsOn = playerToMove.CurrentSpacePlayerIsOn;

		if(DebugModeSingleton.instance.IsDebugActive)
		{
			if(!hasBeenOveriddenOnce)
			{
				DebugModeSingleton.instance.OverrideCurrentPlayerSpacesLeftToMove(playerToMove);
				hasBeenOveriddenOnce = true;
			}
		}

        playerToMove.GameplayManagerRef.spacesToMoveText.text = $"Spaces left: {playerToMove.SpacesLeftToMove}";


		//Multiple valid spaces you can go from this space.
		if (currentSpacePlayerIsOn.ValidDirectionsFromThisSpace.Count > 1)
		{
			List<Space> spaceChoicesToGive = new();
			foreach(Space.Direction direction in currentSpacePlayerIsOn.ValidDirectionsFromThisSpace)
			{
				if(direction == Space.Direction.North)
				{
					if (CheckIfValidSpaceToMoveTo(playerToMove, currentSpacePlayerIsOn.NorthNeighbor))
					{
						spaceChoicesToGive.Add(currentSpacePlayerIsOn.NorthNeighbor);
					}
				}
				if (direction == Space.Direction.South)
				{
					if (CheckIfValidSpaceToMoveTo(playerToMove, currentSpacePlayerIsOn.SouthNeighbor))
					{
						spaceChoicesToGive.Add(currentSpacePlayerIsOn.SouthNeighbor);
					}
				}
				if (direction == Space.Direction.West)
				{
					if (CheckIfValidSpaceToMoveTo(playerToMove, currentSpacePlayerIsOn.WestNeighbor))
					{
						spaceChoicesToGive.Add(currentSpacePlayerIsOn.WestNeighbor);
					}
				}
				if (direction == Space.Direction.East)
				{
					if (CheckIfValidSpaceToMoveTo(playerToMove, currentSpacePlayerIsOn.EastNeighbor))
					{
						spaceChoicesToGive.Add(currentSpacePlayerIsOn.EastNeighbor);
					}                    
				}
			}

			if(spaceChoicesToGive.Count <= 0)
			{
				Debug.LogError("Could not find a valid space to travel to from your current space. Check the current space's valid directions?");
				return;
			}

			if(spaceChoicesToGive.Count > 1)
			{
				CreateDirectionChoicePopup(playerToMove, spaceChoicesToGive);
			}
			else
			{
				if(playerCharacterSM.player.SpacesLeftToMove > 0)
				{
					playerCharacterSM.StartCoroutine(MoveTowards(spaceChoicesToGive[0], playerToMove, playerToMove.SpacesLeftToMove));
				}
			}
		}
		//Only 1 valid way to go from this space.
		else
		{
			Space.Direction validDirection;

			if (currentSpacePlayerIsOn.ValidDirectionsFromThisSpace.Count == 1)
			{
				validDirection = currentSpacePlayerIsOn.ValidDirectionsFromThisSpace.First();
			}
			else
			{
				Debug.LogError($"There should only be 1 element in the HashSet of valid directions. However, the count is: {currentSpacePlayerIsOn.ValidDirectionsFromThisSpace.Count}");
				return;
			}
			

			if(validDirection == Space.Direction.North)
			{
				playerCharacterSM.StartCoroutine(MoveTowards(currentSpacePlayerIsOn.NorthNeighbor, playerToMove, playerToMove.SpacesLeftToMove));
			}

			if (validDirection == Space.Direction.South)
			{
				playerCharacterSM.StartCoroutine(MoveTowards(currentSpacePlayerIsOn.SouthNeighbor, playerToMove, playerToMove.SpacesLeftToMove));
			}

			if (validDirection == Space.Direction.East)
			{
				playerCharacterSM.StartCoroutine(MoveTowards(currentSpacePlayerIsOn.EastNeighbor, playerToMove, playerToMove.SpacesLeftToMove));
			}

			if (validDirection == Space.Direction.West)
			{
				playerCharacterSM.StartCoroutine(MoveTowards(currentSpacePlayerIsOn.WestNeighbor, playerToMove, playerToMove.SpacesLeftToMove));
			}
		}

	}

	public void SetupMoveReverse(Player playerToMove, Player playerMakingDecisions = null)
	{
		playerMakingDecisions = PlayerMakingDecisions;
		Space currentSpacePlayerIsOn = playerToMove.CurrentSpacePlayerIsOn;

		if(DebugModeSingleton.instance.IsDebugActive)
		{
			if(!hasBeenOveriddenOnce)
			{
				DebugModeSingleton.instance.OverrideCurrentPlayerSpacesLeftToMove(playerToMove);
				hasBeenOveriddenOnce = true;
			}
		}

        playerToMove.GameplayManagerRef.spacesToMoveText.text = $"Spaces left: {playerToMove.SpacesLeftToMove}";

		if(playerMakingDecisions is null)
		{
			playerMakingDecisions = playerToMove;
		}

		//Go through each of the neighbors of the current space the player is on.
		//If that neighbor does not exist or is a 'validdirectionfromthisspace' , don't include it.
		//If that neighbor exists but is NOT a in the valid direction, add it.
		//Also take into account barricade spaces and direction spaces.

		List<Space> spaceChoicesToGive = new();
		if(currentSpacePlayerIsOn.NorthNeighbor != null)
		{
			Space spaceToCheck = GetNeighborWithInvalidDirectionFromOriginalSpace(currentSpacePlayerIsOn, currentSpacePlayerIsOn.NorthNeighbor);
			if(spaceToCheck != null && CheckIfValidSpaceToMoveToReverse(playerToMove, spaceToCheck))
			{
				spaceChoicesToGive.Add(spaceToCheck);
			}
		}

		if(currentSpacePlayerIsOn.SouthNeighbor != null)
		{
			Space spaceToCheck = GetNeighborWithInvalidDirectionFromOriginalSpace(currentSpacePlayerIsOn, currentSpacePlayerIsOn.SouthNeighbor);
			if(spaceToCheck != null && CheckIfValidSpaceToMoveToReverse(playerToMove, spaceToCheck))
			{
				spaceChoicesToGive.Add(spaceToCheck);
			}
		}

		if(currentSpacePlayerIsOn.EastNeighbor != null)
		{
			Space spaceToCheck = GetNeighborWithInvalidDirectionFromOriginalSpace(currentSpacePlayerIsOn, currentSpacePlayerIsOn.EastNeighbor);
			if(spaceToCheck != null && CheckIfValidSpaceToMoveToReverse(playerToMove, spaceToCheck))
			{
				spaceChoicesToGive.Add(spaceToCheck);
			}
		}

		if(currentSpacePlayerIsOn.WestNeighbor != null)
		{
			Space spaceToCheck = GetNeighborWithInvalidDirectionFromOriginalSpace(currentSpacePlayerIsOn, currentSpacePlayerIsOn.WestNeighbor);
			if(spaceToCheck != null && CheckIfValidSpaceToMoveToReverse(playerToMove, spaceToCheck))
			{
				spaceChoicesToGive.Add(spaceToCheck);
			}
		}

		if(spaceChoicesToGive.Count > 1)
		{
			bool isMovingInReverse = true;
			CreateDirectionChoicePopup(playerToMove, spaceChoicesToGive, isMovingInReverse, playerMakingDecisions);
		}
		//Only 1 space we can move to.
		else if(spaceChoicesToGive.Count == 1)
		{
			playerCharacterSM.StartCoroutine(MoveTowardsReverse(spaceChoicesToGive[0], playerToMove, playerToMove.SpacesLeftToMove));
		}
		else
		{
			Debug.LogError("Could not find a valid space to travel to from your current space in reverse. Check the current space's valid directions?");
			return;
		}
	}

	/// <summary>
	/// Pass in the space we are on. Pass in the neighbor of that space we are checking.
	/// Now on the neighbor of that space; We find which direction the original space is in. It has to be one of them.
	/// Once we find that, We figure out if that type of neighbor is a valid direction for the space. I.E. if it's the north neighbor, is 'north' a valid direction
	/// to travel from that space to get to our original space. If it is; that means we can't travel from OUR space back to that one so we good.
	/// </summary>
	public Space GetNeighborWithInvalidDirectionFromOriginalSpace(Space originalSpace, Space spaceNeighborToCheck)
	{
		Space spaceToReturn = null;

		if(spaceNeighborToCheck.NorthNeighbor == originalSpace)
		{
			if(spaceNeighborToCheck.ValidDirectionsFromThisSpace.Contains(Space.Direction.North))
			{
				spaceToReturn = spaceNeighborToCheck;
				return spaceToReturn;
			}
		}

		if(spaceNeighborToCheck.SouthNeighbor == originalSpace)
		{
			if(spaceNeighborToCheck.ValidDirectionsFromThisSpace.Contains(Space.Direction.South))
			{
				spaceToReturn = spaceNeighborToCheck;
				return spaceToReturn;
			}
		}

		if(spaceNeighborToCheck.EastNeighbor == originalSpace)
		{
			if(spaceNeighborToCheck.ValidDirectionsFromThisSpace.Contains(Space.Direction.East))
			{
				spaceToReturn = spaceNeighborToCheck;
				return spaceToReturn;
			}
		}

		if(spaceNeighborToCheck.WestNeighbor == originalSpace)
		{
			if(spaceNeighborToCheck.ValidDirectionsFromThisSpace.Contains(Space.Direction.West))
			{
				spaceToReturn = spaceNeighborToCheck;
				return spaceToReturn;
			}
		}

		return spaceToReturn;
	}

	private bool CheckIfValidSpaceToMoveTo(Player player, Space spaceToTryMovingTo)
	{
		bool isValid = true;

		if(spaceToTryMovingTo is null)
		{
			isValid = false;
			return isValid;
		}


		if (player.PreviousSpacePlayerWasOn == spaceToTryMovingTo)
		{
			isValid = false;
		}

		if(player.CurrentLevel < spaceToTryMovingTo.spaceData.LevelRequirementToGoToThisSpace) 
		{
			isValid = false;
		}

		return isValid;
	}

	private bool CheckIfValidSpaceToMoveToReverse(Player player, Space spaceToTryMovingTo)
	{
		bool isValid = true;
		
		if(spaceToTryMovingTo is null)
		{
			isValid = false;
			return isValid;
		}

		if (player.PreviousSpacePlayerWasOn == spaceToTryMovingTo)
		{
			isValid = false;
		}

		if(player.CurrentLevel < spaceToTryMovingTo.spaceData.LevelRequirementToGoToThisSpace) 
		{
			isValid = false;
		}

		return isValid;
	}

	private void CreateDirectionChoicePopup(Player playerToMove, List<Space> targetSpacesToMoveTo, bool isMovingInReverse = false, Player playerMakingDecisions = null)
	{
		playerToMove.IsChoosingDirection = true;
        playerCharacterSM.playerAnimator.SetBool(playerCharacterSM.ISMOVINGPARAMETER, false);
        for (int i = 0; i < targetSpacesToMoveTo.Count; i++)
		{
			int currentIndex = i;
			GameObject directionButtonObj = GameObject.Instantiate(playerToMove.GameplayManagerRef.moveButtonPrefab, playerToMove.GameplayManagerRef.directionChoiceButtonHolder.transform);
			TestCardMoveButton directionButton = directionButtonObj.GetComponent<TestCardMoveButton>();
			Button directionButtonButton = directionButtonObj.GetComponent<Button>();
			directionButton.buttonType = TestCardMoveButton.MoveButtonType.Direction;
			directionButton.moveText.text = $"Move to: {targetSpacesToMoveTo[currentIndex].spaceData.spaceName}";
			directionButtonButton.onClick.AddListener(() => ChooseDirection(targetSpacesToMoveTo[currentIndex], playerToMove, playerToMove.SpacesLeftToMove, isMovingInReverse));
		}

		//Here we lock player's controls and give it to the player making decisions if they are not null. Once decision is made we revert control.
        playerToMove.GameplayManagerRef.HandDisplayPanel.gameObject.SetActive(false);
		//We might wanna keep this on for the player who's choosing even if it's not the current player.
		if(isMovingInReverse)
		{
			playerToMove.GameplayManagerRef.SpacesPlayerWillLandOnParent.TurnOnDisplay(playerToMove.GameplayManagerRef.GameplayPhaseStatemachineRef.gameplayMovementPhaseState.FindValidSpaces(playerToMove, playerToMove.SpacesLeftToMove));
		}
		

		if(playerToMove.PlayerAIReference != null)
		{
			playerToMove.PlayerAIReference.StartCoroutine(playerToMove.PlayerAIReference.SelectRandomDirectionToMoveIn());
		}
    }

	//Used when a direction is chosen. This function should be called by clicking a button.
	public void ChooseDirection(Space targetSpace, Player playerReference, int spacesLeftToMove, bool isMovingInReverse = false)
	{
		//Here we pass back control to the player who's moving. Though it may be taken away again when they're done moving if it's reverse/someone else making them move.
		playerReference.IsChoosingDirection = false;
        playerCharacterSM.playerAnimator.SetBool(playerCharacterSM.ISMOVINGPARAMETER, true);
		if(isMovingInReverse)
		{
			playerCharacterSM.StartCoroutine(MoveTowardsReverse(targetSpace, playerReference, spacesLeftToMove));
		}
		else
		{
			playerCharacterSM.StartCoroutine(MoveTowards(targetSpace, playerReference, spacesLeftToMove));
		}
        

		//Cleanup button holder.
		foreach (Transform child in playerReference.GameplayManagerRef.directionChoiceButtonHolder.transform)
		{
			GameObject.Destroy(child.gameObject);
		}

		playerReference.GameplayManagerRef.SpacesPlayerWillLandOnParent.TurnOffDisplay();

		if(PlayerMakingDecisions == null || PlayerMakingDecisions == playerCharacterSM.player)
		{
			playerReference.GameplayManagerRef.HandDisplayPanel.gameObject.SetActive(true);
		}
	}

	public IEnumerator MoveTowards(Space spaceToMoveTo, Player playerReference, int spacesToMove = 1)
	{
		Transform playerCharacter = playerReference.gameObject.transform;
		bool decreaseSpacesToMove = true;

		if(!spaceToMoveTo.spaceData.DecreasesSpacesToMove)
		{
			decreaseSpacesToMove = false;
		}

		Vector3 targetPosition = spaceToMoveTo.spawnPoint.position;

		playerReference.PreviousSpacePlayerWasOn = playerReference.CurrentSpacePlayerIsOn;
		playerReference.SpacesLeftToMove = spacesToMove;
		

		playerReference.IsMoving = true;
        playerReference.GameplayManagerRef.isPlayerMoving = true;

		float rate = 3.0f;

		if (spacesToMove > 1)
		{
			rate = 3.0f;
		}
		float finalRate;

		while (Vector3.Distance(playerCharacter.localPosition, targetPosition) > 0.15f)
		{
			finalRate = rate * Time.deltaTime;
			Vector3 smoothedMovement = Vector3.MoveTowards(playerCharacter.localPosition, targetPosition, finalRate);
			playerCharacter.position = smoothedMovement;
			yield return new WaitForFixedUpdate();
		}

        playerReference.GameplayManagerRef.isPlayerMoving = false;

		if (spacesToMove > 1)
		{
			if(decreaseSpacesToMove)
			{
				playerReference.SpacesLeftToMove -= 1;
			}
			
			SetupMove(playerReference);
		}
		else
		{
			if (decreaseSpacesToMove)
			{
				playerCharacter.position = targetPosition;
				playerReference.SpacesLeftToMove = 0;
				hasBeenOveriddenOnce = false;
                playerReference.StateMachineRef.ChangeState(playerReference.StateMachineRef.playerCharacterIdleState);

                //Check if multiple characters are on the space, and move everyone accordingly if so.
                if (!spaceToMoveTo.playersOnThisSpace.Contains(playerReference))
				{
					spaceToMoveTo.playersOnThisSpace.Add(playerReference);
				}

				if (spaceToMoveTo.playersOnThisSpace.Count > 1 && !spaceToMoveTo.haveSeparatedPlayersAlready)
				{
                    List<Vector3> positionsToMoveTowards = new();
                    if (spaceToMoveTo.playersOnThisSpace.Count > 1 && spaceToMoveTo.playersOnThisSpace.Count < 5)
                    {
                        spaceToMoveTo.haveSeparatedPlayersAlready = true;

                        for (int i = 0; i < spaceToMoveTo.playersOnThisSpace.Count; i++)
                        {
                            Vector3 positionToMoveTowards = spaceToMoveTo.multiplePlayersSameSpacePositionsParent.transform.GetChild(i).position;
                            positionsToMoveTowards.Add(positionToMoveTowards);
                            spaceToMoveTo.playersOnThisSpace[i].transform.position = positionToMoveTowards;

                        }
                        playerCharacterSM.StartCoroutine(MoveTowardsMultiSpace(positionsToMoveTowards, spaceToMoveTo.playersOnThisSpace));
                        spaceToMoveTo.haveSeparatedPlayersAlready = false;
                    }
                    else
                    {
                        if (spaceToMoveTo.playersOnThisSpace.Count == 1 && spaceToMoveTo.playersOnThisSpace[0].transform.position != spaceToMoveTo.spawnPoint.position)
                        {
                            positionsToMoveTowards.Add(spaceToMoveTo.spawnPoint.position);
                            playerCharacterSM.StartCoroutine(MoveTowardsMultiSpace(positionsToMoveTowards, spaceToMoveTo.playersOnThisSpace));
                        }
                    }
                }
			}
			else
			{
				SetupMove(playerReference);
			}
		}
        playerReference.GameplayManagerRef.spacesToMoveText.text = $"Spaces left: {playerReference.SpacesLeftToMove}";

		yield return null;
	}

	public IEnumerator MoveTowardsReverse(Space spaceToMoveTo, Player playerReference, int spacesToMove = 1)
	{
		Transform playerCharacter = playerReference.gameObject.transform;
		bool decreaseSpacesToMove = true;

		if(!spaceToMoveTo.spaceData.DecreasesSpacesToMove)
		{
			decreaseSpacesToMove = false;
		}

		Vector3 targetPosition = spaceToMoveTo.spawnPoint.position;

		playerReference.PreviousSpacePlayerWasOn = playerReference.CurrentSpacePlayerIsOn;
		playerReference.SpacesLeftToMove = spacesToMove;
		

		playerReference.IsMoving = true;
		playerReference.GameplayManagerRef.isPlayerMoving = true;

		float rate = 3.0f;

		if (spacesToMove > 1)
		{
			rate = 3.0f;
		}
		float finalRate;

		while (Vector3.Distance(playerCharacter.localPosition, targetPosition) > 0.15f)
		{
			finalRate = rate * Time.deltaTime;
			Vector3 smoothedMovement = Vector3.MoveTowards(playerCharacter.localPosition, targetPosition, finalRate);
			playerCharacter.position = smoothedMovement;
			yield return new WaitForFixedUpdate();
		}
		
		playerReference.GameplayManagerRef.isPlayerMoving = false;

		if (spacesToMove > 1)
		{
			if(decreaseSpacesToMove)
			{
				playerReference.SpacesLeftToMove -= 1;
			}
			
			SetupMoveReverse(playerReference);
		}
		else
		{
			if (decreaseSpacesToMove)
			{
				playerCharacter.position = targetPosition;
				playerReference.SpacesLeftToMove = 0;
				playerReference.PreviousSpacePlayerWasOn = null;
				playerReference.IsMovingInReverse = false;
				hasBeenOveriddenOnce = false;
                playerReference.StateMachineRef.ChangeState(playerReference.StateMachineRef.playerCharacterIdleState);

                //Check if multiple characters are on the space, and move everyone accordingly if so.
                if (!spaceToMoveTo.playersOnThisSpace.Contains(playerReference))
				{
					spaceToMoveTo.playersOnThisSpace.Add(playerReference);
				}

				if (spaceToMoveTo.playersOnThisSpace.Count > 1 && !spaceToMoveTo.haveSeparatedPlayersAlready)
				{
                    List<Vector3> positionsToMoveTowards = new();
                    if (spaceToMoveTo.playersOnThisSpace.Count > 1 && spaceToMoveTo.playersOnThisSpace.Count < 5)
                    {
                        spaceToMoveTo.haveSeparatedPlayersAlready = true;

                        for (int i = 0; i < spaceToMoveTo.playersOnThisSpace.Count; i++)
                        {
                            Vector3 positionToMoveTowards = spaceToMoveTo.multiplePlayersSameSpacePositionsParent.transform.GetChild(i).position;
                            positionsToMoveTowards.Add(positionToMoveTowards);
                            spaceToMoveTo.playersOnThisSpace[i].transform.position = positionToMoveTowards;

                        }
                        playerCharacterSM.StartCoroutine(MoveTowardsMultiSpace(positionsToMoveTowards, spaceToMoveTo.playersOnThisSpace));
                        spaceToMoveTo.haveSeparatedPlayersAlready = false;
                    }
                    else
                    {
                        if (spaceToMoveTo.playersOnThisSpace.Count == 1 && spaceToMoveTo.playersOnThisSpace[0].transform.position != spaceToMoveTo.spawnPoint.position)
                        {
                            positionsToMoveTowards.Add(spaceToMoveTo.spawnPoint.position);
                            playerCharacterSM.StartCoroutine(MoveTowardsMultiSpace(positionsToMoveTowards, spaceToMoveTo.playersOnThisSpace));
                        }
                    }
                }
				else
				{

					playerReference.GameplayManagerRef.FinishedMovingInReverse();
				}
			}
			else
			{
				SetupMoveReverse(playerReference);
			}
		}
		playerReference.GameplayManagerRef.spacesToMoveText.text = $"Spaces left: {playerReference.SpacesLeftToMove}";

		yield return null;
	}

	public IEnumerator MoveTowardsMultiSpace(List<Vector3> targetPositions, List<Player> playerReferences)
	{
		for(int i = 0; i < playerReferences.Count; i++)
		{
			Transform playerCharacter = playerReferences[i].gameObject.transform;
            //playerReferences[i].IsMoving = true;
            //playerReferences[i].GameplayManagerRef.isPlayerMoving = true;
			playerReferences[i].StateMachineRef.playerAnimator.SetBool(playerReferences[i].StateMachineRef.ISMOVINGPARAMETER, true);

            float rate = 5.0f;
			float finalRate;

			finalRate = rate * Time.deltaTime;
			Vector3 smoothedMovement = Vector3.MoveTowards(playerCharacter.localPosition, targetPositions[i], finalRate);
			playerCharacter.position = smoothedMovement;

			yield return new WaitForSeconds(0.5f);

			playerCharacter.position = targetPositions[i];
            //playerReferences[i].GameplayManagerRef.isPlayerMoving = false;
            //playerReferences[i].IsMoving = false;
            playerReferences[i].StateMachineRef.playerAnimator.SetBool(playerReferences[i].StateMachineRef.ISMOVINGPARAMETER, false);
        }
		
		playerReferences[0].GameplayManagerRef.FinishedMovingInReverse();
		yield return null;
	}

}
