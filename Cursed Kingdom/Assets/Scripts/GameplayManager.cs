//Code written by Mohamed Riaz Khan of BukuGames.
//All code is written by me (Above name) unless otherwise stated via comments below.
//Not authorized for use outside of the Github repository of this game developed by BukuGames.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using Random = UnityEngine.Random;

public class GameplayManager : MonoBehaviour
{
    public GameObject cardToSpawn;
    public Transform cardParentCanvas;
    public List<Space> spaces;
    public GameObject spaceHolderParent;
    public GameObject spacePrefab;
    public GameObject playerPrefab;
    public Transform playerCharacter;
    bool isPlayerMoving = false;

    public List<ClassData> classdatas;
    public List<SpaceData> spaceDatasTest;
    public List<Player> players;
    public TestMapManager testMapManager;
    public BoardManager boardManager;

    int currentListIndex;

    CinemachineVirtualCamera cinemachineVirtualCamera;

    public CinemachineVirtualCamera currentActiveCamera;
    int currentActiveCameraIndex = 0;

    public List<CinemachineVirtualCamera> cinemachineVirtualCameras;

    private void Start()
    {
        boardManager = GetComponent<BoardManager>();
        testMapManager = GetComponent<TestMapManager>();

        boardManager.SetupBoard();

        foreach (Transform child in spaceHolderParent.transform)
        {
            Space childSpace = child.GetComponent<Space>();

            if(childSpace != null)
            {
                spaces.Add(childSpace);
            }
         
        }

        //Spawn player.
        int randomSpawnSpace = Random.Range(0, spaces.Count-1);
        currentListIndex = randomSpawnSpace;
        GameObject tempPlayer = Instantiate(playerPrefab, spaces[randomSpawnSpace].transform);
        
        tempPlayer.transform.parent = null;
        players.Add(tempPlayer.GetComponent<Player>());

        playerCharacter = tempPlayer.transform;

        cinemachineVirtualCameras[0].LookAt = playerCharacter;
        cinemachineVirtualCameras[0].Follow = playerCharacter;


        foreach (CinemachineVirtualCamera camera in cinemachineVirtualCameras)
        {
            camera.enabled = false;
        }

        currentActiveCamera.enabled = true;

        InitializeSpaces();

        //TEST

        //int randomNum = Random.Range(0, classdatas.Count);

        //players[0].InitializePlayer(classdatas[randomNum]);
        //CardTest();
    }

    private void InitializeSpaces()
    {
        int spaceDataNum = 0;
        foreach(Space space in spaces)
        {
            space.spaceData = spaceDatasTest[spaceDataNum];
            space.SetupSpace();

            if (spaceDataNum < spaceDatasTest.Count -1)
            {
                spaceDataNum += 1;
            }
        }
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
    }



    public void StartMove(int spacesToMove = 1)
    {
        // Debug.Log("clicked space.");
        //We're at the end, so spawn at the first point.
        if (currentListIndex == spaces.Count - 1)
        {
            StartCoroutine(MoveTowards(spaces[0].spawnPoint, spacesToMove));
            //playerCharacter.localPosition = spaceSpawnPoints[0].position;
            currentListIndex = 0;
            //  Debug.Log("At last index.");
            return;
        }

        for (int i = 0; i < spaces.Count; i++)
        {
            if (i == currentListIndex)
            {
                StartCoroutine(MoveTowards(spaces[i + 1].spawnPoint, spacesToMove));
                //playerCharacter.localPosition = spaceSpawnPoints[i + 1].position;
                currentListIndex = i + 1;
                //  Debug.Log($"Moving to index {i+1}");
                return;
            }
        }

    }



    public IEnumerator MoveTowards(Transform targetTransform, int spacesToMove = 1)
    {
        //Test. We'll need to find a way to find out which player is currently moving.
        Player playerReference = playerCharacter.GetComponent<Player>();

        playerReference.SpacesLeftToMove = spacesToMove;

        playerReference.IsMoving = true;
        isPlayerMoving = true;
        
        float rate = 1.5f;

        if(spacesToMove > 1)
        {
            rate = 3.0f;
        }
        float finalRate;

        while (Vector3.Distance(playerCharacter.localPosition, targetTransform.position) > 0.1f)
        {
          //  Debug.Log(playerCharacter.localPosition);
            finalRate = rate * Time.deltaTime;
            playerCharacter.localPosition = Vector3.MoveTowards(playerCharacter.localPosition, targetTransform.position, finalRate);
            yield return null;
        }
        // Debug.Log("Done moving");

        isPlayerMoving = false;

        if (spacesToMove > 1)
        {
            StartMove(spacesToMove - 1);
        }
        else
        {
            playerCharacter.GetComponent<Player>().SpacesLeftToMove = 0;
        }

        yield return null;
    }

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
