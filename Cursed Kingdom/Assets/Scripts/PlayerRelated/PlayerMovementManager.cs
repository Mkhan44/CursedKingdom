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
    [SerializeField] private GameplayManager gameplayManagerRef;
    [SerializeField] private float rayCastLength;

    public GameplayManager GameplayManagerRef { get => gameplayManagerRef; set => gameplayManagerRef = value; }
    public float RayCastLength { get => rayCastLength; set => rayCastLength = value; }

    private void Start()
    {
        GameplayManagerRef = GetComponent<GameplayManager>();
    }

    public void SetupMove(Player playerToMove)
    {

        Space currentSpacePlayerIsOn = playerToMove.CurrentSpacePlayerIsOn;

        //Figure out how to know which spaces are valid for the Player to travel to. If there is more than 1, give them an option...Otherwise just move the Player.

        //Just 1 valid space. No choice needed.
        //if(currentSpacePlayerIsOn.NorthNeighbor is null && currentSpacePlayerIsOn.SouthNeighbor is null && currentSpacePlayerIsOn.WestNeighbor != null)
        //{
        //    StartCoroutine(MoveTowards(currentSpacePlayerIsOn.WestNeighbor.spawnPoint.position, playerToMove, playerToMove.SpacesLeftToMove));
        //    return;
        //}
        //else if(currentSpacePlayerIsOn.EastNeighbor is null && currentSpacePlayerIsOn.SouthNeighbor is null && currentSpacePlayerIsOn.WestNeighbor != null)
        //{
        //    StartCoroutine(MoveTowards(currentSpacePlayerIsOn.WestNeighbor.spawnPoint.position, playerToMove, playerToMove.SpacesLeftToMove));
        //    return;
        //}

        //Multiple valid spaces you can go from this space.
        if(currentSpacePlayerIsOn.ValidDirectionsFromThisSpace.Count > 1)
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
                StartCoroutine(MoveTowards(spaceChoicesToGive[0].spawnPoint.position, playerToMove, playerToMove.SpacesLeftToMove));
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
                StartCoroutine(MoveTowards(currentSpacePlayerIsOn.NorthNeighbor.spawnPoint.position, playerToMove, playerToMove.SpacesLeftToMove));
            }

            if (validDirection == Space.Direction.South)
            {
                StartCoroutine(MoveTowards(currentSpacePlayerIsOn.SouthNeighbor.spawnPoint.position, playerToMove, playerToMove.SpacesLeftToMove));
            }

            if (validDirection == Space.Direction.East)
            {
                StartCoroutine(MoveTowards(currentSpacePlayerIsOn.EastNeighbor.spawnPoint.position, playerToMove, playerToMove.SpacesLeftToMove));
            }

            if (validDirection == Space.Direction.West)
            {
                StartCoroutine(MoveTowards(currentSpacePlayerIsOn.WestNeighbor.spawnPoint.position, playerToMove, playerToMove.SpacesLeftToMove));
            }
        }
        
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

        return isValid;
    }

    private void CreateDirectionChoicePopup(Player playerToMove, List<Space> targetSpacesToMoveTo)
    {
        for(int i = 0; i < targetSpacesToMoveTo.Count; i++)
        {
            int currentIndex = i;
            GameObject directionButtonObj = Instantiate(gameplayManagerRef.moveButtonPrefab, gameplayManagerRef.directionChoiceButtonHolder.transform);
            TestCardMoveButton directionButton = directionButtonObj.GetComponent<TestCardMoveButton>();
            Button directionButtonButton = directionButtonObj.GetComponent<Button>();
            directionButton.buttonType = TestCardMoveButton.MoveButtonType.Direction;
            directionButton.moveText.text = $"Move to: {targetSpacesToMoveTo[currentIndex].spaceData.spaceName}";
            directionButtonButton.onClick.AddListener(() => ChooseDirection(targetSpacesToMoveTo[currentIndex], playerToMove, playerToMove.SpacesLeftToMove));
        }

        
    }

    //Used when a direction is chosen. This function should be called by clicking a button.
    public void ChooseDirection(Space targetSpace, Player playerReference, int spacesLeftToMove)
    {
        StartCoroutine(MoveTowards(targetSpace.spawnPoint.position, playerReference, spacesLeftToMove));

        //Cleanup button holder.
        foreach (Transform child in gameplayManagerRef.directionChoiceButtonHolder.transform)
        {
            Destroy(child.gameObject);
        }
    }

    public IEnumerator MoveTowards(Vector3 targetPosition, Player playerReference, int spacesToMove = 1)
    {
        Transform playerCharacter = playerReference.gameObject.transform;
        Rigidbody playerRigidBody = playerReference.gameObject.GetComponent<Rigidbody>();

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

        if (spacesToMove > 1)
        {
            playerReference.SpacesLeftToMove -= 1;
            SetupMove(playerReference);
        }
        else
        {
            gameplayManagerRef.playerMovementCardsDisplayPanel.SetActive(true);
            playerCharacter.GetComponent<Player>().SpacesLeftToMove = 0;
        }

        yield return null;
    }

    private void MovementExecute(Player playerToMove, Space targetSpace)
    {
        
    }
}
