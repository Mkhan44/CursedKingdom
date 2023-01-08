//Code written by Mohamed Riaz Khan of BukuGames.
//All code is written by me (Above name) unless otherwise stated via comments below.
//Not authorized for use outside of the Github repository of this game developed by BukuGames.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Cinemachine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class GameplayManager : MonoBehaviour
{
    public GameObject cardToSpawn;
    public Transform cardParentCanvas;
    public List<Space> spaces;
    public GameObject spaceHolderParent;
    public GameObject spacePrefab;
    public GameObject playerPrefab;
    public GameObject boardParent;
    public GameObject boardPrefab;
    public Transform playerCharacter;
    public bool isPlayerMoving = false;

    //Movement code for character -- Need to extract this out of here.
    public PlayerMovementManager playerMovementManager;
    public GameObject testMoveButtonParent;
    public GameObject directionChoiceButtonHolder;
    public GameObject moveButtonPrefab;
    public float raycastLength = 2f;


    [SerializeField] private List<Player> players;

    public List<ClassData> classdatas;
    public List<SpaceData> spaceDatasTest;
    public TestMapManager testMapManager;
    public BoardManager boardManager;

    int currentListIndex;

    CinemachineVirtualCamera cinemachineVirtualCamera;

    public CinemachineVirtualCamera currentActiveCamera;
    int currentActiveCameraIndex = 0;

    public List<CinemachineVirtualCamera> cinemachineVirtualCameras;

    //Properties
    public List<Player> Players { get => players; set => players = value; }

    private void Start()
    {
        testMapManager = GetComponent<TestMapManager>();
        playerMovementManager = GetComponent<PlayerMovementManager>();

        GameObject boardHolder;
        if (boardPrefab == null)
        {
            Debug.LogError("Missing board prefab. Do you need to generate a board?");
            return;
        }
        else
        {
            boardHolder = Instantiate(boardPrefab, boardParent.transform);
        }

        boardManager = boardHolder.GetComponent<BoardManager>();


        foreach (Transform child in boardHolder.transform)
        {
            //Rows
            foreach (Transform childChild in child)
            {
                Space childSpace = childChild.GetComponent<Space>();

                if (childSpace != null && childSpace.gameObject.activeInHierarchy)
                {
                    spaces.Add(childSpace);
                }
            }
        }

        //Spawn player.
        int randomSpawnSpace = Random.Range(0, spaces.Count-1);
        currentListIndex = randomSpawnSpace;
        GameObject playerTempReference = Instantiate(playerPrefab, spaces[randomSpawnSpace].spawnPoint);
        
        playerTempReference.transform.parent = null;
        playerTempReference.transform.localScale = playerPrefab.transform.localScale;
        playerTempReference.transform.position = spaces[randomSpawnSpace].spawnPoint.position;


        Players.Add(playerTempReference.GetComponent<Player>());

        playerCharacter = playerTempReference.transform;

        cinemachineVirtualCameras[0].LookAt = playerCharacter;
        cinemachineVirtualCameras[0].Follow = playerCharacter;

        cinemachineVirtualCameras[1].LookAt = spaces[spaces.Count-1].gameObject.transform;
        cinemachineVirtualCameras[1].Follow = spaces[spaces.Count - 1].gameObject.transform;


        foreach (CinemachineVirtualCamera camera in cinemachineVirtualCameras)
        {
            camera.enabled = false;
        }

        testMoveButtonParent.SetActive(true);

        currentActiveCamera.enabled = true;

        boardManager.StartupSetupSpaces();

        //Get all space neighbors for board movement..
        foreach(Space space in spaces)
        {
            space.SpaceTravelSetup();
        }


        //TEST

        //int randomNum = Random.Range(0, classdatas.Count);

        //players[0].InitializePlayer(classdatas[randomNum]);
        //CardTest();
    }


    private void Update()
    {
        if (!isPlayerMoving && Input.GetKeyDown(KeyCode.Space))
        {
            StartMove();
           
        }

        if (Input.GetKeyDown(KeyCode.Y))
        {
            SwitchCamera();
        }

        if(Input.GetKeyDown(KeyCode.R))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name); 
        }

       
    }


    //Movement code for player. Should probably be in a different script...
    public void StartMove(int spacesToMove = 1)
    {
        Player playerReference = playerCharacter.GetComponent<Player>();
        playerReference.SpacesLeftToMove = spacesToMove;
        playerMovementManager.SetupMove(playerReference);
        ////Test. We'll need to find a way to find out which player is currently moving.
        //Player playerReference = playerCharacter.GetComponent<Player>();

        //GameObject playerCurrentSpace = playerReference.CurrentSpacePlayerIsOn.gameObject;

        //List<Space> spacesNextToCurrent = new List<Space>();


        //Space nextSpace;

        //nextSpace = GetNextSpace(playerCurrentSpace, playerCurrentSpace.transform.forward);
        //if (nextSpace != null && nextSpace.name != playerReference.PreviousSpacePlayerWasOn.name)
        //{
        //    spacesNextToCurrent.Add(nextSpace);
        //    //Debug.Log($"Added {nextSpace} to the spacesNextToCurrent: Count = {spacesNextToCurrent.Count}");
        //}
        //nextSpace = GetNextSpace(playerCurrentSpace, -playerCurrentSpace.transform.forward);
        //if (nextSpace != null && nextSpace.name != playerReference.PreviousSpacePlayerWasOn.name)
        //{
        //    spacesNextToCurrent.Add(nextSpace);
        //    //Debug.Log($"Added {nextSpace} to the spacesNextToCurrent: Count = {spacesNextToCurrent.Count}");
        //}
        //nextSpace = GetNextSpace(playerCurrentSpace, playerCurrentSpace.transform.right);
        //if (nextSpace != null && nextSpace.name != playerReference.PreviousSpacePlayerWasOn.name)
        //{
        //    spacesNextToCurrent.Add(nextSpace);
        //   // Debug.Log($"Added {nextSpace} to the spacesNextToCurrent: Count = {spacesNextToCurrent.Count}");
        //}
        //nextSpace = GetNextSpace(playerCurrentSpace, -playerCurrentSpace.transform.right);
        //if (nextSpace != null && nextSpace.name != playerReference.PreviousSpacePlayerWasOn.name)
        //{
        //    spacesNextToCurrent.Add(nextSpace);
        //    //Debug.Log($"Added {nextSpace} to the spacesNextToCurrent: Count = {spacesNextToCurrent.Count}");
        //}

        ////Figure out how to know which spaces are valid for the Player to travel to. If there is more than 1, give them an option...Otherwise just move the Player.

        //if (spacesNextToCurrent.Count < 2 && spacesNextToCurrent.Count != 0)
        //{
        //    StartCoroutine(MoveTowards(spacesNextToCurrent[0].spawnPoint.position, playerReference, spacesToMove));
        //}
        //else
        //{
        //    if(spacesNextToCurrent.Count == 0)
        //    {
        //        Debug.LogError($"Couldn't find anything with a Raycast value of: {raycastLength}. Increasing by 0.5f.");
        //        raycastLength += 0.5f;
        //        StartMove(spacesToMove);
        //        return;
        //    }
        //    for (int i = 0; i < spacesNextToCurrent.Count; i++)
        //    {
        //        GameObject directionButtonObj = Instantiate(moveButtonPrefab, directionChoiceButtonHolder.transform);
        //        TestCardMoveButton directionButton = directionButtonObj.GetComponent<TestCardMoveButton>();
        //        Button directionButtonButton = directionButtonObj.GetComponent<Button>();
        //        directionButton.buttonType = TestCardMoveButton.MoveButtonType.Direction;
        //        Space nextSpaceInList = spacesNextToCurrent[i];
        //        directionButton.moveText.text = $"Move to: {nextSpaceInList.spaceData.spaceName}";
        //        directionButtonButton.onClick.AddListener(() => ChooseDirection(nextSpaceInList, playerReference, spacesToMove));

        //    }
        //}


        ////We're at the end, so spawn at the first point.
        ////if (currentListIndex == spaces.Count - 1)
        ////{
        ////    StartCoroutine(MoveTowards(spaces[0].spawnPoint.position, playerReference, spacesToMove));
        ////    //playerCharacter.localPosition = spaceSpawnPoints[0].position;
        ////    currentListIndex = 0;
        ////    Debug.Log("At last index.");
        ////    return;
        ////}

        ////for (int i = 0; i < spaces.Count; i++)
        ////{
        ////    if (i == currentListIndex)
        ////    {
        ////        StartCoroutine(MoveTowards(spaces[i + 1].spawnPoint.position, playerReference, spacesToMove));
        ////        //playerCharacter.localPosition = spaceSpawnPoints[i + 1].position;
        ////        currentListIndex = i + 1;
        ////         Debug.Log($"Moving to space: {spaces[i+1].name} You have {spacesToMove - 1} spaces left.");
        ////        return;
        ////    }
        ////}

    }

    ////Used when a direction is chosen. This function should be called by clicking a button.
    //public void ChooseDirection(Space targetSpace,Player playerReference, int spacesLeftToMove)
    //{
    //    StartCoroutine(MoveTowards(targetSpace.spawnPoint.position, playerReference, spacesLeftToMove));

    //    //Cleanup button holder.
    //    foreach(Transform child in directionChoiceButtonHolder.transform)
    //    {
    //        Destroy(child.gameObject);
    //    }
    //}

    //public Space GetNextSpace(GameObject playerCurrentSpace, Vector3 raycastDirection)
    //{
    //    Space nextSpace = default;

    //    //Raycast. We need to do all 4 directions and see what spaces are around the player.
    //    GameObject lastHitObject;
    //    Ray ray = new Ray(playerCurrentSpace.transform.position, raycastDirection);
    //    RaycastHit hit;
    //    if (Physics.Raycast(ray, out hit, raycastLength))
    //    {
    //        lastHitObject = hit.transform.gameObject;
    //        nextSpace = lastHitObject.transform.parent.gameObject.GetComponent<Space>();
    //       // Debug.Log($"We hit: {lastHitObject.transform.parent.gameObject.name} , nextSpace is now: {nextSpace.spaceData.spaceName}");
    //    }
    //    else
    //    {
    //        Debug.LogWarning($"{ray} Didn't hit anything.");
    //        return null;
    //    }
    //    return nextSpace;
    //}



    //public IEnumerator MoveTowards(Vector3 targetPosition, Player playerReference, int spacesToMove = 1)
    //{
    //    playerReference.PreviousSpacePlayerWasOn = playerReference.CurrentSpacePlayerIsOn;
    //    playerReference.SpacesLeftToMove = spacesToMove;

    //    playerReference.IsMoving = true;
    //    isPlayerMoving = true;

        
    //    float rate = 3.0f;

    //    if(spacesToMove > 1)
    //    {
    //        rate = 3.0f;
    //    }
    //    float finalRate;

    //    while (Vector3.Distance(playerCharacter.localPosition, targetPosition) > 0.1f)
    //    {
    //      //  Debug.Log(playerCharacter.localPosition);
    //        finalRate = rate * Time.deltaTime;
    //        playerCharacter.localPosition = Vector3.MoveTowards(playerCharacter.localPosition, targetPosition, finalRate);
    //        yield return null;
    //    }
    //    // Debug.Log("Done moving");

    //    isPlayerMoving = false;

    //    if (spacesToMove > 1)
    //    {
    //        StartMove(spacesToMove - 1);
    //    }
    //    else
    //    {
    //        testMoveButtonParent.SetActive(true);
    //        playerCharacter.GetComponent<Player>().SpacesLeftToMove = 0;
    //    }

    //    yield return null;
    //}

    //Right now this only works for 2 cameras. Anymore we'll have to specify the target based on what's clicked.
    private void SwitchCamera(int index = 0)
    {
        index = currentActiveCameraIndex;
        currentActiveCamera.enabled = false;
        if(currentActiveCameraIndex == cinemachineVirtualCameras.Count - 1)
        {
            currentActiveCameraIndex = 0;
            currentActiveCamera = cinemachineVirtualCameras[currentActiveCameraIndex];
            
        }

        else
        {
            testMapManager.ActivateHighlight();
            currentActiveCameraIndex++;
        }

        currentActiveCamera = cinemachineVirtualCameras[currentActiveCameraIndex];
        currentActiveCamera.enabled = true;
    }

    void TestFunc()
    {
        cinemachineVirtualCamera = GameObject.Find("Player Cam").GetComponent<CinemachineVirtualCamera>();

        cinemachineVirtualCamera.m_Lens.Dutch = 55;
    }

    void CardTest()
    {
        if (cardParentCanvas != null)
        {
            Instantiate(cardToSpawn, cardParentCanvas);
        }
    }


}
