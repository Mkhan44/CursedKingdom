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
using TMPro;

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
    public GameObject playerMovementCardsDisplayPanel;
    public GameObject directionChoiceButtonHolder;
    public GameObject moveButtonPrefab;
    public float raycastLength = 2f;

    //Deck related
    [SerializeField] private DeckManager thisDeckManager;
    [SerializeField] private GameObject movementCardPrefab;
    [SerializeField] private GameObject supportCardPrefab;
    [SerializeField] private GameObject movementDeckCardHolder;
    [SerializeField] private GameObject supportDeckCardHolder;
    [SerializeField] private GameObject movementCardDiscardPileHolder;
    [SerializeField] private GameObject supportCardDiscardPileHolder;



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

    //Debug
    public TextMeshProUGUI fpsText;
    private int lastFrameIndex;
    private float[] frameDeltaTimeArray;

    //Properties
    public List<Player> Players { get => players; set => players = value; }

    //Deck related
    public DeckManager ThisDeckManager { get => thisDeckManager; set => thisDeckManager = value; }
    public GameObject MovementCardPrefab { get => movementCardPrefab; set => movementCardPrefab = value; }
    public GameObject SupportCardPrefab { get => supportCardPrefab; set => supportCardPrefab = value; }
    public GameObject MovementDeckCardHolder { get => movementDeckCardHolder; set => movementDeckCardHolder = value; }
    public GameObject SupportDeckCardHolder { get => supportDeckCardHolder; set => supportDeckCardHolder = value; }
    public GameObject MovementCardDiscardPileHolder { get => movementCardDiscardPileHolder; set => movementCardDiscardPileHolder = value; }
    public GameObject SupportCardDiscardPileHolder { get => supportCardDiscardPileHolder; set => supportCardDiscardPileHolder = value; }

    private void Start()
    {
        Application.targetFrameRate = 60;
        frameDeltaTimeArray = new float[50];

        testMapManager = GetComponent<TestMapManager>();
        playerMovementManager = GetComponent<PlayerMovementManager>();

        
        //Deck related
        ThisDeckManager = GetComponent<DeckManager>();

        //Get a list of movement cards, pass them in.
        ThisDeckManager.CreateDeck();
       

        //Deck related

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
        //TODO: CHANGE THIS TO BE MORE DYNAMIC.
        playerTempReference.GetComponent<Player>().CardsInHandHolderPanel = GameObject.Find("CardsInHandLayout");
        //1st 5 cards in player's hand.
        ThisDeckManager.ShuffleDeck(Card.CardType.Movement);
        ThisDeckManager.ShuffleDeck(Card.CardType.Support);
        ThisDeckManager.DrawCards(Card.CardType.Movement, playerTempReference.GetComponent<Player>(), 3);
        ThisDeckManager.DrawCards(Card.CardType.Support, playerTempReference.GetComponent<Player>(), 2);
        playerTempReference.GetComponent<Player>().GameplayManagerRef = this;
        playerTempReference.GetComponent<Player>().ShowHand();
        playerMovementManager.Animator = playerTempReference.GetComponent<Animator>();

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

        playerMovementCardsDisplayPanel.SetActive(true);

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
        //if (!isPlayerMoving && Input.GetKeyDown(KeyCode.Space))
        //{
        //    StartMove();

        //}
        frameDeltaTimeArray[lastFrameIndex] = Time.unscaledDeltaTime;
        lastFrameIndex = (lastFrameIndex + 1) % frameDeltaTimeArray.Length;

        fpsText.text = "FPS: " + Mathf.RoundToInt(CalculateFPS()).ToString();

        if (Input.GetKeyDown(KeyCode.Y))
        {
            SwitchCamera();
        }

        if(Input.GetKeyDown(KeyCode.R))
        {
            ReloadScene();
        }

       
    }

    //Debug function!
    private float CalculateFPS()
    {
        float total = 0f;
        foreach(float deltaTime in frameDeltaTimeArray)
        {
            total += deltaTime;
        }
        return frameDeltaTimeArray.Length / total;
    }
    //Debug function!

    public void ReloadScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }


    //Movement code for player. Should probably be in a different script...
    public void StartMove(int spacesToMove = 1)
    {
        Player playerReference = playerCharacter.GetComponent<Player>();
        playerReference.SpacesLeftToMove = spacesToMove;
        playerMovementManager.SetupMove(playerReference);
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

    private void TurnOver()
    {
        //Player's turn ends: They draw a card.
        ThisDeckManager.DrawCard(Card.CardType.Movement, playerCharacter.GetComponent<Player>());
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
