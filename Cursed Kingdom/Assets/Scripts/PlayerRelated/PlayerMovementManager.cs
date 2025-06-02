//Code written by Mohamed Riaz Khan of BukuGames.
//All code is written by me (Above name) unless otherwise stated via comments below.
//Not authorized for use outside of the Github repository of this game developed by BukuGames.
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

//This script should be attached to the Game Manager object in the hierarchy.
public class PlayerMovementManager : MonoBehaviour
{

	private const string ISMOVINGPARAMETER = "IsMoving";
	[SerializeField] private GameplayManager gameplayManagerRef;
	[SerializeField] private float rayCastLength;
	[SerializeField] private Animator animator;

	//Debug
	[SerializeField] private bool hasBeenOveriddenOnce;

	public GameplayManager GameplayManagerRef { get => gameplayManagerRef; set => gameplayManagerRef = value; }
	public float RayCastLength { get => rayCastLength; set => rayCastLength = value; }
	public Animator Animator { get => animator; set => animator = value; }
	public bool HasBeenOveriddenOnce { get => hasBeenOveriddenOnce; }

	private void Start()
	{
		GameplayManagerRef = GetComponent<GameplayManager>();
	}

	private void Started()
	{
		
	}

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

		gameplayManagerRef.spacesToMoveText.text = $"Spaces left: {playerToMove.SpacesLeftToMove}";


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
				StartCoroutine(MoveTowards(spaceChoicesToGive[0], playerToMove, playerToMove.SpacesLeftToMove));
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
				StartCoroutine(MoveTowards(currentSpacePlayerIsOn.NorthNeighbor, playerToMove, playerToMove.SpacesLeftToMove));
			}

			if (validDirection == Space.Direction.South)
			{
				StartCoroutine(MoveTowards(currentSpacePlayerIsOn.SouthNeighbor, playerToMove, playerToMove.SpacesLeftToMove));
			}

			if (validDirection == Space.Direction.East)
			{
				StartCoroutine(MoveTowards(currentSpacePlayerIsOn.EastNeighbor, playerToMove, playerToMove.SpacesLeftToMove));
			}

			if (validDirection == Space.Direction.West)
			{
				StartCoroutine(MoveTowards(currentSpacePlayerIsOn.WestNeighbor, playerToMove, playerToMove.SpacesLeftToMove));
			}
		}

	  //  playerToMove.HideHand();

	}

	public void SetupMoveReverse(Player playerToMove, Player playerMakingDecisions = null)
	{
		if(playerMakingDecisions is null)
		{
			playerMakingDecisions = playerToMove;
		}

		Space currentSpacePlayerIsOn = playerToMove.CurrentSpacePlayerIsOn;

		if(DebugModeSingleton.instance.IsDebugActive)
		{
			if(!hasBeenOveriddenOnce)
			{
				DebugModeSingleton.instance.OverrideCurrentPlayerSpacesLeftToMove(playerToMove);
				hasBeenOveriddenOnce = true;
			}
		}

		gameplayManagerRef.spacesToMoveText.text = $"Spaces left: {playerToMove.SpacesLeftToMove}";

		//Go through each of the neighbors of the current space the player is on.
		//If that neighbor does not exist or is a 'validdirectionfromthisspace' , don't include it.
		//If that neighbor exists but is NOT a in the valid direction, add it.
		//Also take into account barricade spaces and direction spaces.

		List<Space> spaceChoicesToGive = new();
		if(currentSpacePlayerIsOn.NorthNeighbor != null && currentSpacePlayerIsOn.NorthNeighbor.ValidDirectionsFromThisSpace.Contains(Space.Direction.South))
		{
			if(CheckIfValidSpaceToMoveToReverse(playerToMove, currentSpacePlayerIsOn.NorthNeighbor))
			{
				spaceChoicesToGive.Add(currentSpacePlayerIsOn.NorthNeighbor);
			}
		}

		if(currentSpacePlayerIsOn.SouthNeighbor != null && currentSpacePlayerIsOn.SouthNeighbor.ValidDirectionsFromThisSpace.Contains(Space.Direction.North))
		{
			if(CheckIfValidSpaceToMoveToReverse(playerToMove, currentSpacePlayerIsOn.SouthNeighbor))
			{
				spaceChoicesToGive.Add(currentSpacePlayerIsOn.SouthNeighbor);
			}
		}

		if(currentSpacePlayerIsOn.EastNeighbor != null && currentSpacePlayerIsOn.EastNeighbor.ValidDirectionsFromThisSpace.Contains(Space.Direction.West))
		{
			if(CheckIfValidSpaceToMoveToReverse(playerToMove, currentSpacePlayerIsOn.EastNeighbor))
			{
				spaceChoicesToGive.Add(currentSpacePlayerIsOn.EastNeighbor);
			}
		}

		if(currentSpacePlayerIsOn.WestNeighbor != null && currentSpacePlayerIsOn.WestNeighbor.ValidDirectionsFromThisSpace.Contains(Space.Direction.East))
		{
			if(CheckIfValidSpaceToMoveToReverse(playerToMove, currentSpacePlayerIsOn.WestNeighbor))
			{
				spaceChoicesToGive.Add(currentSpacePlayerIsOn.WestNeighbor);
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
			StartCoroutine(MoveTowardsReverse(spaceChoicesToGive[0], playerToMove, playerToMove.SpacesLeftToMove));
		}
		else
		{
			Debug.LogError("Could not find a valid space to travel to from your current space in reverse. Check the current space's valid directions?");
			return;
		}

		//Multiple valid spaces you can go from this space.
		// if (currentSpacePlayerIsOn.ValidDirectionsFromThisSpace.Count > 1)
		// {
		// 	List<Space> spaceChoicesToGive = new();
		// 	foreach(Space.Direction direction in currentSpacePlayerIsOn.ValidDirectionsFromThisSpace)
		// 	{
		// 		if(direction == Space.Direction.North)
		// 		{
		// 			if (CheckIfValidSpaceToMoveToReverse(playerToMove, currentSpacePlayerIsOn.NorthNeighbor))
		// 			{
		// 				spaceChoicesToGive.Add(currentSpacePlayerIsOn.NorthNeighbor);
		// 			}
		// 		}
		// 		if (direction == Space.Direction.South)
		// 		{
		// 			if (CheckIfValidSpaceToMoveToReverse(playerToMove, currentSpacePlayerIsOn.SouthNeighbor))
		// 			{
		// 				spaceChoicesToGive.Add(currentSpacePlayerIsOn.SouthNeighbor);
		// 			}
		// 		}
		// 		if (direction == Space.Direction.West)
		// 		{
		// 			if (CheckIfValidSpaceToMoveToReverse(playerToMove, currentSpacePlayerIsOn.WestNeighbor))
		// 			{
		// 				spaceChoicesToGive.Add(currentSpacePlayerIsOn.WestNeighbor);
		// 			}
		// 		}
		// 		if (direction == Space.Direction.East)
		// 		{
		// 			if (CheckIfValidSpaceToMoveToReverse(playerToMove, currentSpacePlayerIsOn.EastNeighbor))
		// 			{
		// 				spaceChoicesToGive.Add(currentSpacePlayerIsOn.EastNeighbor);
		// 			}                    
		// 		}
		// 	}

		

		// 	if(spaceChoicesToGive.Count > 1)
		// 	{
		// 		CreateDirectionChoicePopup(playerToMove, spaceChoicesToGive, true);
		// 	}
		// 	else
		// 	{
		// 		StartCoroutine(MoveTowardsReverse(spaceChoicesToGive[0], playerToMove, playerToMove.SpacesLeftToMove));
		// 	}
		// }
		// //Only 1 valid way to go from this space.
		// else
		// {
		// 	Space.Direction validDirection;

		// 	if (currentSpacePlayerIsOn.ValidDirectionsFromThisSpace.Count == 1)
		// 	{
		// 		validDirection = currentSpacePlayerIsOn.ValidDirectionsFromThisSpace.First();
		// 	}
		// 	else
		// 	{
		// 		Debug.LogError($"There should only be 1 element in the HashSet of valid directions. However, the count is: {currentSpacePlayerIsOn.ValidDirectionsFromThisSpace.Count}");
		// 		return;
		// 	}
			

		// 	if(validDirection == Space.Direction.North)
		// 	{
		// 		StartCoroutine(MoveTowardsReverse(currentSpacePlayerIsOn.NorthNeighbor, playerToMove, playerToMove.SpacesLeftToMove));
		// 	}

		// 	if (validDirection == Space.Direction.South)
		// 	{
		// 		StartCoroutine(MoveTowardsReverse(currentSpacePlayerIsOn.SouthNeighbor, playerToMove, playerToMove.SpacesLeftToMove));
		// 	}

		// 	if (validDirection == Space.Direction.East)
		// 	{
		// 		StartCoroutine(MoveTowardsReverse(currentSpacePlayerIsOn.EastNeighbor, playerToMove, playerToMove.SpacesLeftToMove));
		// 	}

		// 	if (validDirection == Space.Direction.West)
		// 	{
		// 		StartCoroutine(MoveTowardsReverse(currentSpacePlayerIsOn.WestNeighbor, playerToMove, playerToMove.SpacesLeftToMove));
		// 	}
		// }

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

		if(player.CurrentLevel < spaceToTryMovingTo.spaceData.LevelRequirementToGoToThisSpace) 
		{
			isValid = false;
		}

		return isValid;
	}

	private void CreateDirectionChoicePopup(Player playerToMove, List<Space> targetSpacesToMoveTo, bool isMovingReverse = false, Player playerWhoChoosesDirection = null)
	{
		for(int i = 0; i < targetSpacesToMoveTo.Count; i++)
		{
			int currentIndex = i;
			GameObject directionButtonObj = Instantiate(gameplayManagerRef.moveButtonPrefab, gameplayManagerRef.directionChoiceButtonHolder.transform);
			TestCardMoveButton directionButton = directionButtonObj.GetComponent<TestCardMoveButton>();
			Button directionButtonButton = directionButtonObj.GetComponent<Button>();
			directionButton.buttonType = TestCardMoveButton.MoveButtonType.Direction;
			directionButton.moveText.text = $"Move to: {targetSpacesToMoveTo[currentIndex].spaceData.spaceName}";
			directionButtonButton.onClick.AddListener(() => ChooseDirection(targetSpacesToMoveTo[currentIndex], playerToMove, playerToMove.SpacesLeftToMove, isMovingReverse));
		}

		//Have some logic here if playerWhoChoosesDirection is not null to lock the playerToMove's controls and unlock the player who is choosing's controls.

		GameplayManagerRef.HandDisplayPanel.gameObject.SetActive(false);
	}

	//Used when a direction is chosen. This function should be called by clicking a button.
	public void ChooseDirection(Space targetSpace, Player playerReference, int spacesLeftToMove, bool isMovingReverse = false)
	{
		if(!isMovingReverse)
		{
			StartCoroutine(MoveTowards(targetSpace, playerReference, spacesLeftToMove));
		}
		else
		{
			StartCoroutine(MoveTowardsReverse(targetSpace, playerReference, spacesLeftToMove));
		}
		
		//Cleanup button holder.
		foreach (Transform child in gameplayManagerRef.directionChoiceButtonHolder.transform)
		{
			Destroy(child.gameObject);
		}

		GameplayManagerRef.HandDisplayPanel.gameObject.SetActive(true);
	}

	public IEnumerator MoveTowards(Space spaceToMoveTo, Player playerReference, int spacesToMove = 1)
	{
		Transform playerCharacter = playerReference.gameObject.transform;
		Animator = playerCharacter.GetComponent<Animator>();
		bool decreaseSpacesToMove = true;

		if(!spaceToMoveTo.spaceData.DecreasesSpacesToMove)
		{
			decreaseSpacesToMove = false;
		}

		Vector3 targetPosition = spaceToMoveTo.spawnPoint.position;

		playerReference.PreviousSpacePlayerWasOn = playerReference.CurrentSpacePlayerIsOn;
		playerReference.SpacesLeftToMove = spacesToMove;
		
		playerReference.IsMoving = true;
		gameplayManagerRef.isPlayerMoving = true;
		//playerReference.StateMachineRef.ChangeState(playerReference.StateMachineRef.playerCharacterMoveState);
		//Animator.SetBool(ISMOVINGPARAMETER, true);
	   // playerReference.HideHand();

		float rate = 3.0f;

		if (spacesToMove > 1)
		{
			rate = 3.0f;
		}
		float finalRate;

		while (Vector3.Distance(playerCharacter.localPosition, targetPosition) > 0.15f)
		{
			//  Debug.Log(playerCharacter.localPosition);
			finalRate = rate * Time.deltaTime;
			//playerCharacter.localPosition = 
			Vector3 smoothedMovement = Vector3.MoveTowards(playerCharacter.localPosition, targetPosition, finalRate);
			playerCharacter.position = smoothedMovement;
			//playerRigidBody.MovePosition(smoothedMovement);
			yield return new WaitForFixedUpdate();
		}
		// Debug.Log("Done moving");
		
		gameplayManagerRef.isPlayerMoving = false;
		//playerReference.StateMachineRef.ChangeState(playerReference.StateMachineRef.playerCharacterIdleState);
		//Animator.SetBool(ISMOVINGPARAMETER, false);

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
				//Check if multiple characters are on the space, and move everyone accordingly if so.
				if (!spaceToMoveTo.playersOnThisSpace.Contains(playerReference))
				{
					spaceToMoveTo.playersOnThisSpace.Add(playerReference);
				}

				if (spaceToMoveTo.playersOnThisSpace.Count > 1 && !spaceToMoveTo.haveSeparatedPlayersAlready)
				{
					spaceToMoveTo.MoveMultiplePlayersOnSpace();
				}
			}
			else
			{
				SetupMove(playerReference);
			}
		}
		gameplayManagerRef.spacesToMoveText.text = $"Spaces left: {playerReference.SpacesLeftToMove}";

		yield return null;
	}

	public IEnumerator MoveTowardsReverse(Space spaceToMoveTo, Player playerReference, int spacesToMove = 1)
	{
		Transform playerCharacter = playerReference.gameObject.transform;
		Animator = playerCharacter.GetComponent<Animator>();
		bool decreaseSpacesToMove = true;

		if(!spaceToMoveTo.spaceData.DecreasesSpacesToMove)
		{
			decreaseSpacesToMove = false;
		}

		Vector3 targetPosition = spaceToMoveTo.spawnPoint.position;

		playerReference.PreviousSpacePlayerWasOn = playerReference.CurrentSpacePlayerIsOn;
		playerReference.SpacesLeftToMove = spacesToMove;
		

		playerReference.IsMoving = true;
		gameplayManagerRef.isPlayerMoving = true;

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
		
		gameplayManagerRef.isPlayerMoving = false;

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
				hasBeenOveriddenOnce = false;
				//Make sure we set the previous space to null after this since the next time the player moves forward, it won't let them
				//since the space they just moved from in reverse would be the space they need to move from forward now.
				playerReference.PreviousSpacePlayerWasOn = null;

				//Check if multiple characters are on the space, and move everyone accordingly if so.
				if (!spaceToMoveTo.playersOnThisSpace.Contains(playerReference))
				{
					spaceToMoveTo.playersOnThisSpace.Add(playerReference);
				}

				if (spaceToMoveTo.playersOnThisSpace.Count > 1 && !spaceToMoveTo.haveSeparatedPlayersAlready)
				{
					spaceToMoveTo.MoveMultiplePlayersOnSpace();
				}
			}
			else
			{
				SetupMoveReverse(playerReference);
			}
		}
		gameplayManagerRef.spacesToMoveText.text = $"Spaces left: {playerReference.SpacesLeftToMove}";

		yield return null;
	}


	private void MovementExecute(Player playerToMove, Space targetSpace)
	{
		
	}
}
