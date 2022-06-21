//Code written by Mohamed Riaz Khan of BukuGames.
//All code is written by me (Above name) unless otherwise stated via comments below.
//Not authorized for use outside of the Github repository of this game developed by BukuGames.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class TestManager : MonoBehaviour
{
    public GameObject cardToSpawn;
    public Transform cardParentCanvas;
    public List<Transform> spaceSpawnPoints;
    public GameObject spaceHolderParent;
    public GameObject spacePrefab;
    public Transform playerCharacter;
    bool isPlayerMoving = false;

    public List<TestPlayer> players;
    public TestMapManager testMapManager;

    int currentListIndex;

    CinemachineVirtualCamera cinemachineVirtualCamera;

    public CinemachineVirtualCamera currentActiveCamera;
    int currentActiveCameraIndex = 0;

    public List<CinemachineVirtualCamera> cinemachineVirtualCameras;

    private void Start()
    {
        testMapManager = GetComponent<TestMapManager>();
        currentListIndex = 0;

        foreach (Transform child in spaceHolderParent.transform)
        {
            spaceSpawnPoints.Add(child.GetChild(0));
        }

        foreach(CinemachineVirtualCamera camera in cinemachineVirtualCameras)
        {
            camera.enabled = false;
        }

        currentActiveCamera.enabled = true;

        
        //CardTest();
    }

    private void SpawnSpaces()
    {

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
        if (currentListIndex == spaceSpawnPoints.Count - 1)
        {
            StartCoroutine(MoveTowards(spaceSpawnPoints[0], spacesToMove));
            //playerCharacter.localPosition = spaceSpawnPoints[0].position;
            currentListIndex = 0;
            //  Debug.Log("At last index.");
            return;
        }

        for (int i = 0; i < spaceSpawnPoints.Count; i++)
        {
            if (i == currentListIndex)
            {
                StartCoroutine(MoveTowards(spaceSpawnPoints[i + 1], spacesToMove));
                //playerCharacter.localPosition = spaceSpawnPoints[i + 1].position;
                currentListIndex = i + 1;
                //  Debug.Log($"Moving to index {i+1}");
                return;
            }
        }

    }



    public IEnumerator MoveTowards(Transform targetTransform, int spacesToMove = 1)
    {
        isPlayerMoving = true;
        float rate = 1.5f;

        if(spacesToMove > 1)
        {
            rate = 2.0f;
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
            testMapManager.ActivateHighlight();
        }

        //For now this is the UI camera.
        else
        {
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
