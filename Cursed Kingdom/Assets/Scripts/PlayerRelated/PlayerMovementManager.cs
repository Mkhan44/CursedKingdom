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

        foreach(SpaceData.SpaceEffect spaceEffect in spaceToTryMovingTo.spaceData.spaceEffects)
        {
            BarricadeSpace barricadeSpace = spaceEffect.spaceEffectData as BarricadeSpace;
            if(barricadeSpace is not null)
            {
                if(player.CurrentLevel < barricadeSpace.LevelNeededToPass) 
                {
                    isValid = false;
                    break;
                }
            }
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

        GameplayManagerRef.HandDisplayPanel.gameObject.SetActive(false);
    }

    //Used when a direction is chosen. This function should be called by clicking a button.
    public void ChooseDirection(Space targetSpace, Player playerReference, int spacesLeftToMove)
    {
        StartCoroutine(MoveTowards(targetSpace, playerReference, spacesLeftToMove));

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
        Animator.SetBool(ISMOVINGPARAMETER, true);
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
        Animator.SetBool(ISMOVINGPARAMETER, false);

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

    public IEnumerator MoveTowardsMultiSpace(List<Vector3> targetPositions, List<Player> playerReferences)
    {
        //if (movingCoroutine != null)
        //{
        //    StopCoroutine(movingCoroutine);
        //    null;
        //}

        for(int i = 0; i < playerReferences.Count; i++)
        {
            Transform playerCharacter = playerReferences[i].gameObject.transform;
            Animator = playerCharacter.GetComponent<Animator>();


            //playerReferences[i].IsMoving = true;
            //gameplayManagerRef.isPlayerMoving = true;
            Animator.SetBool(ISMOVINGPARAMETER, true);
            // playerReference.HideHand();

            float rate = 5.0f;
            float finalRate;

            //while (Vector3.Distance(playerCharacter.localPosition, targetPosition) > 0.15f)
            //{
            //    finalRate = rate * Time.deltaTime;
            //    Vector3 smoothedMovement = Vector3.MoveTowards(playerCharacter.localPosition, targetPosition, finalRate);
            //    playerCharacter.position = smoothedMovement;
            //    yield return new WaitForFixedUpdate();
            //}

            finalRate = rate * Time.deltaTime;
            Vector3 smoothedMovement = Vector3.MoveTowards(playerCharacter.localPosition, targetPositions[i], finalRate);
            playerCharacter.position = smoothedMovement;

            yield return new WaitForSeconds(0.5f);

            playerCharacter.position = targetPositions[i];

            //gameplayManagerRef.isPlayerMoving = false;
            Animator.SetBool(ISMOVINGPARAMETER, false);
            Debug.Log("Animator for: " + playerReferences[i].ClassData.name + " should be false and is: " + Animator.GetBool(ISMOVINGPARAMETER));
        }
        yield return null;
    }


    private void MovementExecute(Player playerToMove, Space targetSpace)
    {
        
    }
}
