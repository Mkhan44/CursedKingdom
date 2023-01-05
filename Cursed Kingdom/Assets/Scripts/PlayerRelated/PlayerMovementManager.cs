//Code written by Mohamed Riaz Khan of BukuGames.
//All code is written by me (Above name) unless otherwise stated via comments below.
//Not authorized for use outside of the Github repository of this game developed by BukuGames.
using System.Collections;
using System.Collections.Generic;
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

    private void FixedUpdate()
    {
        
    }

    public void SetupMove(Player playerToMove)
    {
        int validSpacesCount = 0;

        Space currentSpacePlayerIsOn = playerToMove.CurrentSpacePlayerIsOn;

        //Figure out how to know which spaces are valid for the Player to travel to. If there is more than 1, give them an option...Otherwise just move the Player.

        if(CheckIfValidSpaceToMoveTo(playerToMove, currentSpacePlayerIsOn.NorthNeighbor))
        {
            validSpacesCount += 1;
            CreateDirectionChoicePopup(playerToMove, currentSpacePlayerIsOn.NorthNeighbor);
        }

    
        if (CheckIfValidSpaceToMoveTo(playerToMove, currentSpacePlayerIsOn.SouthNeighbor))
        {
            validSpacesCount += 1;
            CreateDirectionChoicePopup(playerToMove, currentSpacePlayerIsOn.SouthNeighbor);
        }
        

        if (CheckIfValidSpaceToMoveTo(playerToMove, currentSpacePlayerIsOn.EastNeighbor))
        {
            validSpacesCount += 1;
            CreateDirectionChoicePopup(playerToMove, currentSpacePlayerIsOn.EastNeighbor);
        }
        

        if (CheckIfValidSpaceToMoveTo(playerToMove, currentSpacePlayerIsOn.WestNeighbor))
        {
            validSpacesCount += 1;
            CreateDirectionChoicePopup(playerToMove, currentSpacePlayerIsOn.WestNeighbor);
        }
        
    }

    private bool CheckIfValidSpaceToMoveTo(Player player, Space spaceToTryMovingTo)
    {
        if(spaceToTryMovingTo is null)
        {
            return false;
        }

        if(player.PreviousSpacePlayerWasOn != spaceToTryMovingTo)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private void CreateDirectionChoicePopup(Player playerToMove, Space targetSpaceToMoveTo)
    {
        GameObject directionButtonObj = Instantiate(gameplayManagerRef.moveButtonPrefab, gameplayManagerRef.directionChoiceButtonHolder.transform);
        TestCardMoveButton directionButton = directionButtonObj.GetComponent<TestCardMoveButton>();
        Button directionButtonButton = directionButtonObj.GetComponent<Button>();
        directionButton.buttonType = TestCardMoveButton.MoveButtonType.Direction;
        directionButton.moveText.text = $"Move to: {targetSpaceToMoveTo.spaceData.spaceName}";
        directionButtonButton.onClick.AddListener(() => ChooseDirection(targetSpaceToMoveTo, playerToMove, playerToMove.SpacesLeftToMove));
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
            gameplayManagerRef.testMoveButtonParent.SetActive(true);
            playerCharacter.GetComponent<Player>().SpacesLeftToMove = 0;
        }

        yield return null;
    }

    private Space GetNextSpace(GameObject playerCurrentSpace, Vector3 raycastDirection)
    {
        Space nextSpace = default;

        //Raycast. We need to do all 4 directions and see what spaces are around the player.
        GameObject lastHitObject;
        Ray ray = new Ray(playerCurrentSpace.transform.position, raycastDirection);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, RayCastLength))
        {
            lastHitObject = hit.transform.gameObject;
            nextSpace = lastHitObject.transform.parent.gameObject.GetComponent<Space>();
            // Debug.Log($"We hit: {lastHitObject.transform.parent.gameObject.name} , nextSpace is now: {nextSpace.spaceData.spaceName}");
        }
        else
        {
            Debug.LogWarning($"{ray} Didn't hit anything.");
            return null;
        }
        return nextSpace;
    }

    private void MovementExecute(Player playerToMove, Space targetSpace)
    {
        
    }
}
