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
using System;

public class GameplayManager : MonoBehaviour
{
	//For fade panel.
	public const string ISFADING = "IsFading";

    public event Action<KeyCode> MoveHighlightedSpaceIconGroundView;
	public event Action<KeyCode> MoveHighlightedSpaceIconOverheadView;
	public event Action<SupportCard> CurrentSupportCardThatWasJustUsed;
	
	//event if all players die
	public event Action<Player> AllPlayersDefeated;
	
	//State machines
	private GameplayPhaseSM  gameplayPhaseStatemachineRef;
	private DuelPhaseSM duelPhaseSMRef;

	public GameObject cardToSpawn;
	public List<Space> spaces;
	public GameObject spaceHolderParent;
	public GameObject spacePrefab;
	public GameObject playerPrefab;
	public GameObject boardParent;
	public GameObject boardPrefab;
	public GameObject decksHolderPrefab;
	public GameObject PlayerInfoCanvas;
	public SpacesPlayerWillLandOnDisplay SpacesPlayerWillLandOnParent;
	public Transform playerCharacter;
	public bool isPlayerMoving = false;

	//Movement code for character -- Need to extract this out of here.
	public PlayerMovementManager playerMovementManager;
	public GameObject cardDisplayPanelParent;
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
	[SerializeField] private GameObject useSelectedCardsPanel;
	[SerializeField] private TextMeshProUGUI useSelectedCardsText;
	[SerializeField] private Button useSelectedCardsButton;
	[SerializeField] private Button useMovementCardNoCardsInHandButton;

	//Duel related
	[SerializeField] private GameObject playerDuelPrefab;
	[SerializeField] public GameObject duelPlaneSpawnPointsParent;

	//Support card usage.
	[SerializeField] private SupportCard currentSupportCardBeingUsed;

	//Gameboard Data
	[SerializeField] private TopDownMapDisplay topDownMapDisplay;
	[SerializeField] private SpaceArtworkPopupDisplay spaceArtworkPopupDisplay;

	//Player UI related
	[SerializeField] private PlayerHandDisplayUI handDisplayPanel;

	//Player ability related
	[SerializeField] private Button useAbilityButton;
	[SerializeField] private Button useEliteAbilityButton;


	[SerializeField] private List<Player> players;
	[SerializeField] private List<PlayerInfoDisplay> playerInfoDisplays;

	public List<ClassData> classdatas;
	public List<SpaceData> spaceDatasTest;
	public MapManager mapManager;
	public BoardManager boardManager;

	//Cameras
	[SerializeField] private GameplayCameraManager gameplayCameraManagerRef;
	private CinemachineVirtualCamera cinemachineVirtualCamera;

	public CinemachineVirtualCamera currentActiveCamera;
	private int currentActiveCameraIndex = 0;

	public List<CinemachineVirtualCamera> cinemachineVirtualCameras;

	//Debug
	public TextMeshProUGUI fpsText;
	public bool lockFPS;
	[Range(10, 200)] public int fpsCap = 60;
	public bool isSpeedupOn;
	public int speedupValue = 3;
	private int lastFrameIndex;
	private float[] frameDeltaTimeArray;
	public TextMeshProUGUI spacesToMoveText;
	public GameObject debugMessagePanel;
	public bool useStartDebugMenu;
	[Range(1,4)] public int numPlayersToStartWith = 1;

	//Properties
	public List<Player> Players { get => players; set => players = value; }
	
	//State machines
	public GameplayPhaseSM GameplayPhaseStatemachineRef {get => gameplayPhaseStatemachineRef; set => gameplayPhaseStatemachineRef = value;}
    public DuelPhaseSM DuelPhaseSMRef { get => duelPhaseSMRef; set => duelPhaseSMRef = value; }

    //Duel related
    public GameObject PlayerDuelPrefab { get => playerDuelPrefab; set => playerDuelPrefab = value; }

    //Deck related
    public DeckManager ThisDeckManager { get => thisDeckManager; set => thisDeckManager = value; }
	public GameObject MovementCardPrefab { get => movementCardPrefab; set => movementCardPrefab = value; }
	public GameObject SupportCardPrefab { get => supportCardPrefab; set => supportCardPrefab = value; }
	public GameObject MovementDeckCardHolder { get => movementDeckCardHolder; set => movementDeckCardHolder = value; }
	public GameObject SupportDeckCardHolder { get => supportDeckCardHolder; set => supportDeckCardHolder = value; }
	public GameObject MovementCardDiscardPileHolder { get => movementCardDiscardPileHolder; set => movementCardDiscardPileHolder = value; }
	public GameObject SupportCardDiscardPileHolder { get => supportCardDiscardPileHolder; set => supportCardDiscardPileHolder = value; }
	public GameObject UseSelectedCardsPanel { get => useSelectedCardsPanel; set => useSelectedCardsPanel = value; }
    public SupportCard CurrentSupportCardBeingUsed { get => currentSupportCardBeingUsed; set => currentSupportCardBeingUsed = value; }
	public TextMeshProUGUI UseSelectedCardsText { get => useSelectedCardsText; set => useSelectedCardsText = value; }
	public Button UseSelectedCardsButton { get => useSelectedCardsButton; set => useSelectedCardsButton = value; }
    public Button UseMovementCardNoCardsInHandButton { get => useMovementCardNoCardsInHandButton; set => useMovementCardNoCardsInHandButton = value; }
	public Button UseAbilityButton { get => useAbilityButton; set => useAbilityButton = value; }
	public Button UseEliteAbilityButton { get => useEliteAbilityButton; set => useEliteAbilityButton = value; }
	public List<PlayerInfoDisplay> PlayerInfoDisplays { get => playerInfoDisplays; set => playerInfoDisplays = value; }
	public PlayerHandDisplayUI HandDisplayPanel { get => handDisplayPanel; set => handDisplayPanel = value; }
	public TopDownMapDisplay TopDownMapDisplay { get => topDownMapDisplay; set => topDownMapDisplay = value; }
	public SpaceArtworkPopupDisplay SpaceArtworkPopupDisplay { get => spaceArtworkPopupDisplay; set => spaceArtworkPopupDisplay = value; }
    public GameplayCameraManager GameplayCameraManagerRef { get => gameplayCameraManagerRef; set => gameplayCameraManagerRef = value; }

    private void Start()
	{
		isSpeedupOn = false;
		FPSCounter();
		

		if(!useStartDebugMenu && StartDebugMenu.instance != null)
		{
            StartDebugMenu.instance.TurnOffPanel();
        }

		mapManager = GetComponent<MapManager>();
		playerMovementManager = GetComponent<PlayerMovementManager>();
		gameplayPhaseStatemachineRef = GetComponent<GameplayPhaseSM>();
		DuelPhaseSMRef = GetComponent<DuelPhaseSM>();



        //Deck related
        ThisDeckManager = GetComponent<DeckManager>();
		UseSelectedCardsPanel.SetActive(false);

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

        ThisDeckManager.ShuffleDeck(Card.CardType.Movement);
        ThisDeckManager.ShuffleDeck(Card.CardType.Support);

        //Spawn in the players!
        GetPlayerStatsFromDebug();

		GameplayCameraManagerRef = GameObject.Find("Camera Manager").GetComponent<GameplayCameraManager>();
        cinemachineVirtualCameras[0].LookAt = playerCharacter;
		cinemachineVirtualCameras[0].Follow = playerCharacter;

		cinemachineVirtualCameras[1].LookAt = spaces[spaces.Count - 1].gameObject.transform;
		cinemachineVirtualCameras[1].Follow = spaces[spaces.Count - 1].gameObject.transform;

        foreach (CinemachineVirtualCamera camera in cinemachineVirtualCameras)
		{
			camera.enabled = false;
		}

		HandDisplayPanel.gameObject.SetActive(true);
		HandDisplayPanel.GameplayManagerRef = this;

        currentActiveCamera.enabled = true;

		boardManager.StartupSetupSpaces();

		//Get all space neighbors for board movement..
		foreach (Space space in spaces)
		{
			space.SpaceTravelSetup();
		}

		//For debug mode.
		DebugModeSingleton.instance.SetupOverrideSpaceLandEffectDropdownOptions(spaces);
		DebugModeSingleton.instance.SetupOverrideSupportCardEffectDropdownOptions(ThisDeckManager.SupportDeckData.SupportCardDatas);


		//DialogueBoxPopup.instance.ActivatePopup("This is a test option.", 4);

		//TEST

		//int randomNum = Random.Range(0, classdatas.Count);

		//players[0].InitializePlayer(classdatas[randomNum]);
		//CardTest();
	}

	public void GetPlayerStatsFromDebug()
	{
		if(StartDebugMenu.instance != null && StartDebugMenu.instance.useScriptable)
		{
            List<GameObject> tempPlayerReferences = new();
            //Spawn in the players.
            for (int i = 0; i < StartDebugMenu.instance.currentlySelectedStartData.numberOfPlayersToUse; i++)
            {

				ClassData.ClassType classType = StartDebugMenu.instance.currentlySelectedStartData.playerDebugDatas[i].typeOfClass;
				switch(classType)
				{
					case ClassData.ClassType.Magician:
						{
                            tempPlayerReferences.Add(SpawnPlayersStart(classdatas[0], i));
                            break;
						}
                    case ClassData.ClassType.Thief:
                        {
                            tempPlayerReferences.Add(SpawnPlayersStart(classdatas[1], i));
                            break;
                        }
                    case ClassData.ClassType.Warrior:
                        {
                            tempPlayerReferences.Add(SpawnPlayersStart(classdatas[2], i));
                            break;
                        }
                    case ClassData.ClassType.Archer:
                        {
                            tempPlayerReferences.Add(SpawnPlayersStart(classdatas[3], i));
                            break;
                        }
                }
            }


            if (Players.Count > 1)
            {
                //Assuming Player canvas is already setup. We will need to change this to be more dynamic.
                int childNum = 0;
                foreach (Transform child in PlayerInfoCanvas.transform)
                {
                    if (childNum + 1 > Players.Count)
                    {
                        break;
                    }
                    PlayerInfoDisplay tempPlayerInfoDisp = child.GetComponent<PlayerInfoDisplay>();
                    if (tempPlayerInfoDisp is not null)
                    {
                        PlayerInfoDisplays.Add(tempPlayerInfoDisp);
                        tempPlayerInfoDisp.SetupPlayerInfo(Players[childNum]);
                        childNum++;
                    }
                }
            }
            //for debug purposes with only 1 player.
            else
            {
                PlayerInfoDisplay tempPlayerInfoDisp = PlayerInfoCanvas.transform.GetChild(1).GetComponent<PlayerInfoDisplay>();
                PlayerInfoDisplays.Add(tempPlayerInfoDisp);
                tempPlayerInfoDisp.SetupPlayerInfo(Players[0]);
            }



            playerCharacter = tempPlayerReferences[0].transform;

            //DEBUG.
            Players[0].ShowHand();
            if (Players[0].ClassData.abilityData.CanBeManuallyActivated)
            {
                UseAbilityButton.gameObject.transform.parent.gameObject.SetActive(true);
                UseAbilityButton.onClick.RemoveAllListeners();
                UseAbilityButton.onClick.AddListener(Players[0].UseAbility);
            }
            else
            {
                UseAbilityButton.gameObject.transform.parent.gameObject.SetActive(false);
            }
			
			//Inputting '0' as the playerDebugDatas index for debugging purposes. This is because right now the first player always goes first. WILL NEED TO CHANGE THIS EVENTUALLY.

			if (StartDebugMenu.instance != null && StartDebugMenu.instance.useScriptable && StartDebugMenu.instance.currentlySelectedStartData.playerDebugDatas[0].startingLevelOverride > 1)
			{
				if(StartDebugMenu.instance.currentlySelectedStartData.playerDebugDatas[0].startingLevelOverride >= 5)
				{
					Players[0].CanUseEliteAbility = true;
					if (Players[0].ClassData.eliteAbilityData.IsPassive)
					{
						Players[0].UseEliteAbility();
					}
					else if (Players[0].ClassData.eliteAbilityData.CanBeManuallyActivated)
					{
						UseEliteAbilityButton.gameObject.transform.parent.gameObject.SetActive(true);
						UseEliteAbilityButton.onClick.RemoveAllListeners();
						UseEliteAbilityButton.onClick.AddListener(Players[0].UseEliteAbility);
					}
				}
				else
				{
					UseEliteAbilityButton.transform.parent.gameObject.SetActive(false);
				}
			}
			else
			{
				//No player should start at level 5.
            	UseEliteAbilityButton.transform.parent.gameObject.SetActive(false);
			}

           

            HandDisplayPanel.SetCurrentActiveHandUI(0);
            //DEBUG.
        }
        else
		{
            List<GameObject> tempPlayerReferences = new();
            //Spawn in the players.
            for (int i = 0; i < numPlayersToStartWith; i++)
            {
                tempPlayerReferences.Add(SpawnPlayersStart(classdatas[i]));
            }


            if (Players.Count > 1)
            {
                //Assuming Player canvas is already setup. We will need to change this to be more dynamic.
                int childNum = 0;
                foreach (Transform child in PlayerInfoCanvas.transform)
                {
                    if (childNum + 1 > Players.Count)
                    {
                        break;
                    }
                    PlayerInfoDisplay tempPlayerInfoDisp = child.GetComponent<PlayerInfoDisplay>();
                    if (tempPlayerInfoDisp is not null)
                    {
                        PlayerInfoDisplays.Add(tempPlayerInfoDisp);
                        tempPlayerInfoDisp.SetupPlayerInfo(Players[childNum]);
                        childNum++;
                    }
                }
            }
            //for debug purposes with only 1 player.
            else
            {
                PlayerInfoDisplay tempPlayerInfoDisp = PlayerInfoCanvas.transform.GetChild(1).GetComponent<PlayerInfoDisplay>();
                PlayerInfoDisplays.Add(tempPlayerInfoDisp);
                tempPlayerInfoDisp.SetupPlayerInfo(Players[0]);
            }



            playerCharacter = tempPlayerReferences[0].transform;

            //DEBUG.
            Players[0].ShowHand();
            if (Players[0].ClassData.abilityData.CanBeManuallyActivated)
            {
                UseAbilityButton.gameObject.transform.parent.gameObject.SetActive(true);
                UseAbilityButton.onClick.RemoveAllListeners();
                UseAbilityButton.onClick.AddListener(Players[0].UseAbility);
            }
            else
            {
                UseAbilityButton.gameObject.transform.parent.gameObject.SetActive(false);
            }


            HandDisplayPanel.SetCurrentActiveHandUI(0);
            //DEBUG.
        }
    }


	private GameObject SpawnPlayersStart(ClassData playerClass, int playerNum = 0)
	{
		//Spawn player.
		GameObject playerTempReference = null;
		int spaceToSpawnPlayer = -1;

		if(StartDebugMenu.instance != null && StartDebugMenu.instance.useScriptable)
		{
			foreach(Space spaceToTry in spaces)
			{
				if (StartDebugMenu.instance.currentlySelectedStartData.playerDebugDatas[playerNum].startingSpaceNameOverride == spaceToTry.spaceData.spaceName)
				{
					spaceToSpawnPlayer = spaces.IndexOf(spaceToTry);
                    playerTempReference = Instantiate(playerPrefab, spaces[spaceToSpawnPlayer].spawnPoint);
                }
			}
			if(spaceToSpawnPlayer == -1)
			{
                spaceToSpawnPlayer = 0;
                playerTempReference = Instantiate(playerPrefab, spaces[spaceToSpawnPlayer].spawnPoint);
                Debug.LogWarning($"We couldn't match a space with the debug start scriptable space! Could possibly not be overriding anything.");
            }
        }
		else
		{
            spaceToSpawnPlayer = 0;
            playerTempReference = Instantiate(playerPrefab, spaces[spaceToSpawnPlayer].spawnPoint);
        }

		playerTempReference.transform.parent = null;
		playerTempReference.transform.localScale = playerPrefab.transform.localScale;
		playerTempReference.transform.position = spaces[spaceToSpawnPlayer].spawnPoint.position;
		//TODO: CHANGE THIS TO BE MORE DYNAMIC.
		Player playerTempReferencePlayer = playerTempReference.GetComponent<Player>();
		playerTempReferencePlayer.ClassData = playerClass;
		spaces[spaceToSpawnPlayer].playersOnThisSpace.Add(playerTempReferencePlayer);
		//END TODO:

		//setup animation
		Animator playerAnimator = playerTempReferencePlayer.GetComponent<Animator>();
		playerMovementManager.Animator = playerAnimator;

		if (playerClass.animatorController != null)
		{
			playerAnimator.runtimeAnimatorController = playerClass.animatorController;
		}
		
		//Setup player hand UI
		GameObject decksHolder = Instantiate(decksHolderPrefab, cardDisplayPanelParent.transform);
		GameObject playerMovementCardPanel = decksHolder.transform.GetChild(0).GetChild(0).gameObject;
		GameObject playerSupportCardPanel = decksHolder.transform.GetChild(1).GetChild(0).gameObject;
		GameObject noMovementCardsInHandButtonObj = decksHolder.transform.GetChild(0).GetChild(1).gameObject;
		Button noMovementCardsInHandButton = noMovementCardsInHandButtonObj.GetComponent<Button>();

		HandDisplayPanel.AddNewHandUI(playerMovementCardPanel.GetComponent<RectTransform>(), playerSupportCardPanel.GetComponent<RectTransform>(), noMovementCardsInHandButton);

		playerTempReferencePlayer.MovementCardsInHandHolderPanel = playerMovementCardPanel;
		playerTempReferencePlayer.SupportCardsInHandHolderPanel = playerSupportCardPanel;
		playerTempReferencePlayer.HandDisplayPanel = HandDisplayPanel.gameObject;
		playerTempReferencePlayer.NoMovementCardsInHandButton = noMovementCardsInHandButton;
		
		//1st 5 cards in player's hand.
		playerTempReferencePlayer.GameplayManagerRef = this;
		playerTempReferencePlayer.InitializePlayer(playerNum);

		if (StartDebugMenu.instance != null && StartDebugMenu.instance.useScriptable)
		{
			int numMovementCardsToDraw = StartDebugMenu.instance.currentlySelectedStartData.playerDebugDatas[playerNum].movementCardsToStartWithOverride;
			int numSupportCardsToDraw = StartDebugMenu.instance.currentlySelectedStartData.playerDebugDatas[playerNum].supportCardsToStartWithOverride;

			if(numMovementCardsToDraw > 0)
			{
                ThisDeckManager.DrawCards(Card.CardType.Movement, playerTempReferencePlayer, numMovementCardsToDraw);
            }
			else
			{
                ThisDeckManager.DrawCards(Card.CardType.Movement, playerTempReferencePlayer, 3);
            }

            if (numSupportCardsToDraw > 0)
            {
                ThisDeckManager.DrawCards(Card.CardType.Support, playerTempReferencePlayer, numSupportCardsToDraw);
            }
            else
            {
                ThisDeckManager.DrawCards(Card.CardType.Support, playerTempReferencePlayer, 2);
            }
		}
		else
		{
            ThisDeckManager.DrawCards(Card.CardType.Movement, playerTempReferencePlayer, 3);
            ThisDeckManager.DrawCards(Card.CardType.Support, playerTempReferencePlayer, 2);
        }
		playerTempReferencePlayer.SetSupportCardsInHand();
		playerTempReferencePlayer.SetMovementCardsInHand();
		playerTempReferencePlayer.HideHand();
		playerTempReferencePlayer.MovementCardsInHandHolderPanel.SetActive(false);
		playerTempReferencePlayer.SupportCardsInHandHolderPanel.SetActive(false);

		//Setup event subscriptions:
		//playerTempReferencePlayer.TurnHasEnded += EndOfTurn;
		spacesToMoveText.text = "Spaces left: 0";

		Players.Add(playerTempReferencePlayer);

		//Need to change this to be based on turn order.
		playerTempReferencePlayer.playerIDIntVal = Players.Count;
		return playerTempReference;
	}

	private void Update()
	{
		if(fpsText is not null && DebugModeSingleton.instance.IsDebugActive)
		{
			frameDeltaTimeArray[lastFrameIndex] = Time.unscaledDeltaTime;
			lastFrameIndex = (lastFrameIndex + 1) % frameDeltaTimeArray.Length;
			fpsText.text = "FPS: " + Mathf.RoundToInt(CalculateFPS()).ToString();
		}

		//Debug time speed up

		if (Input.GetKeyDown(KeyCode.S))
		{
			ToggleSpeedUp();
		}
		
		//Map controls.
		if (Input.GetKeyDown(KeyCode.Y))
		{
			ToggleOverheadMapCamera();
		}

		if (Input.GetKeyDown(KeyCode.U))
		{
			ToggleGroundMapCamera();
		}

		if (Input.GetKeyDown(KeyCode.UpArrow))
		{
			HandleMapKeyInput(KeyCode.UpArrow);
		}
		if (Input.GetKeyDown(KeyCode.DownArrow))
		{
			HandleMapKeyInput(KeyCode.DownArrow);
		}
		if (Input.GetKeyDown(KeyCode.LeftArrow))
		{
			HandleMapKeyInput(KeyCode.LeftArrow);
		}
		if (Input.GetKeyDown(KeyCode.RightArrow))
		{
			HandleMapKeyInput(KeyCode.RightArrow);
		}

		if(Input.GetKeyDown(KeyCode.R))
		{
			ReloadScene();
		}

	   
	}

	public void HandleMapKeyInput(KeyCode keyCodePressed)
	{
		MoveHighlightedSpaceIconGroundView?.Invoke(keyCodePressed);
		MoveHighlightedSpaceIconOverheadView?.Invoke(keyCodePressed);
	}

	public void ReloadScene()
	{
		SceneManager.LoadScene(SceneManager.GetActiveScene().name);
	}


	//Movement code for player. Should probably be in a different script...
	public void StartMove(int spacesToMove = 1)
	{
		Player playerReference = playerCharacter.GetComponent<Player>();
		GetCurrentPlayer().SpacesLeftToMove = spacesToMove;
		playerReference.StateMachineRef.ChangeState(playerReference.StateMachineRef.playerCharacterMoveState);
		//playerMovementManager.SetupMove(playerReference);
		GetCurrentPlayer().HideHand();
	}

	private int GetCurrentPlayerIndex(Player playerRef)
	{
		int playerNum = Players.IndexOf(playerRef);

		if (playerNum is not -1)
		{
			return playerNum;
		}
		else
		{
			return -1;
		}

	}

	public Player GetCurrentPlayer()
	{
	   Player currentPlayer = playerCharacter.GetComponent<Player>();

		return currentPlayer;
	}

	public AudioData GetCurrentAreaPlayerIsInAudioData()
	{
		AudioData currentRowPlayerIsOnAudioData = default;
        foreach (BoardSpacesData boardSpacesData in boardManager.boardSpacesData)
        {
            if (currentRowPlayerIsOnAudioData != null)
            {
                break;
            }
            foreach (SpaceData spaceData in boardSpacesData.perimeterSpaces)
            {
                if (spaceData == GetCurrentPlayer().CurrentSpacePlayerIsOn.spaceData)
                {
                    currentRowPlayerIsOnAudioData = boardSpacesData.AreaMusic;
                    break;
                }
            }

            if (currentRowPlayerIsOnAudioData != null)
            {
                break;
            }
            foreach (SpaceData spaceData in boardSpacesData.insideSpaces)
            {
                if (spaceData == GetCurrentPlayer().CurrentSpacePlayerIsOn.spaceData)
                {
                    currentRowPlayerIsOnAudioData = boardSpacesData.AreaMusic;
                    break;
                }
            }
        }

		return currentRowPlayerIsOnAudioData;
    }

	#region PlayerInfoUI Functions

	public void UpdatePlayerInfoUICardCount(Player playerRef)
	{
		int playerNum = GetCurrentPlayerIndex(playerRef);

		if(playerNum is not -1)
		{
			PlayerInfoDisplays[playerNum].UpdateCardTotals();
		}
	}

	public void UpdatePlayerInfoUIStatusEffect(Player playerRef)
	{
		int playerNum = GetCurrentPlayerIndex(playerRef);

		if (playerNum is not -1)
		{
			PlayerInfoDisplays[playerNum].UpdateStatusEffect();
		}
	}

	public void UpdatePlayerLevel(Player playerRef)
	{
		int playerNum = GetCurrentPlayerIndex(playerRef);

		if (playerNum is not -1)
		{
			PlayerInfoDisplays[playerNum].UpdateLevel();
		}
	}

	public void UpdatePlayerHealth(Player playerRef)
	{
		int playerNum = GetCurrentPlayerIndex(playerRef);

		if (playerNum is not -1)
		{
			PlayerInfoDisplays[playerNum].UpdatePlayerCurrentHealth();
		}
	}

	public void UpdatePlayerMaxHealth(Player playerRef)
	{
        int playerNum = GetCurrentPlayerIndex(playerRef);

        if (playerNum is not -1)
        {
            PlayerInfoDisplays[playerNum].UpdatePlayerMaxHealth(playerRef);
        }
    }

	public void UpdatePlayerCooldownText(Player playerRef)
	{
		int playerNum = GetCurrentPlayerIndex(playerRef);

		if (playerNum is not -1)
		{
			PlayerInfoDisplays[playerNum].UpdateCooldownText();
		}
	}

	#endregion


	//Right now this only works for 2 cameras. Anymore we'll have to specify the target based on what's clicked.
	public void ToggleOverheadMapCamera()
	{
		int index = 0;
		index = currentActiveCameraIndex;
		currentActiveCamera.enabled = false;
		int indexOfPlayer = Players.IndexOf(playerCharacter.GetComponent<Player>());

		if (!mapManager.IsViewingMapOverhead)
		{
			//Hardcoded right now. Need this index to change later to be more dynamic.
			index = 1;
			currentActiveCameraIndex = index;
			currentActiveCamera = cinemachineVirtualCameras[currentActiveCameraIndex];
			mapManager.ActivateHighlightOverheadView(Players[indexOfPlayer].CurrentSpacePlayerIsOn);
			currentActiveCameraIndex++;
			mapManager.MobileMapControlsPanel.SetActive(true);

        }
		else
		{
			currentActiveCameraIndex = 0;
			currentActiveCamera = cinemachineVirtualCameras[currentActiveCameraIndex];
			mapManager.DisableCurrentHighlightedSpaceOverheadView(players[indexOfPlayer].CurrentSpacePlayerIsOn);
            mapManager.MobileMapControlsPanel.SetActive(false);
        }

		currentActiveCamera.enabled = true;
	}

	public void ToggleGroundMapCamera()
	{
		int index = 0;
		index = currentActiveCameraIndex;
		currentActiveCamera.enabled = false;
		int indexOfPlayer = Players.IndexOf(playerCharacter.GetComponent<Player>());

		if (!mapManager.IsViewingMapGround)
		{
			//Hardcoded right now. Need this index to change later to be more dynamic.
			index = 2;
			currentActiveCameraIndex = index;
			currentActiveCamera = cinemachineVirtualCameras[currentActiveCameraIndex];
			mapManager.SetupGroundViewCamera(currentActiveCamera, Players[indexOfPlayer].CurrentSpacePlayerIsOn);
			currentActiveCameraIndex++;
            mapManager.MobileMapControlsPanel.SetActive(true);
        }
		else
		{
			currentActiveCameraIndex = 0;
			currentActiveCamera = cinemachineVirtualCameras[currentActiveCameraIndex];
			mapManager.DisableHighlightedSpaceGroundView(players[indexOfPlayer].CurrentSpacePlayerIsOn);
            mapManager.MobileMapControlsPanel.SetActive(false);
        }

		currentActiveCamera.enabled = true;
	}

	public void ActivateStartTurnPopup(Player nextPlayer, Space spaceStartEffectToTrigger)
	{
		List<Tuple<string, string, object, List<object>>> insertedParams = new();

		List<object> paramsList = new();
		paramsList.Add(nextPlayer);
		paramsList.Add(spaceStartEffectToTrigger);

		insertedParams.Add(Tuple.Create<string, string, object, List<object>>("Start!", nameof(StartTurnConfirmation), this, paramsList));

		DialogueBoxPopup.instance.ActivatePopupWithButtonChoices($"Player {nextPlayer.playerIDIntVal}'s turn!", insertedParams, 1,"Turn start!");
	}

	/// <summary>
	/// Takes in a list of objects in this order: Player nextPlayer.
	/// </summary>
	/// <returns></returns>
	public IEnumerator StartTurnConfirmation(List<object> objects)
	{
		yield return null;
		Player player = (Player)objects[0];
		Space spaceEffectToTrigger = (Space)objects[1];

		if (DebugModeSingleton.instance.IsDebugActive)
		{
			Space tempSpace = DebugModeSingleton.instance.OverrideSpaceLandEffect();
			Space currentSpace = player.CurrentSpacePlayerIsOn;
			if (tempSpace != null)
			{
				SpaceArtworkPopupDisplay.TurnOnDisplay(tempSpace, player);
			}
			else
			{
				SpaceArtworkPopupDisplay.TurnOnDisplay(spaceEffectToTrigger, player);
			}
		}
		else
		{
			SpaceArtworkPopupDisplay.TurnOnDisplay(spaceEffectToTrigger, player);
		}
	}

	public bool IsPlayerDefeated(Player playerToCheck)
	{
		bool isDefeated = false;

		if(playerToCheck.IsDefeated)
		{
			isDefeated = true;
		}
		else
		{
			isDefeated = false;
		}

		return isDefeated;
	}

	public void CheckIfAllPlayersButOneDefeated(Player playerThatWasJustDefeated)
	{
		int numPlayersDefeated = 0;
		Player nonDefeatedPlayer = null;

		foreach(Player player in Players)
		{
			if(player.IsDefeated)
			{
				numPlayersDefeated++;
			}
			else
			{
                nonDefeatedPlayer = player;
            }
		}

		Debug.Log($"Num players defeated is: {numPlayersDefeated}");
		if(numPlayersDefeated < Players.Count - 1)
		{
			return;
        }

        AllPlayersDefeated?.Invoke(nonDefeatedPlayer);
		playerCharacter = nonDefeatedPlayer.GetComponent<Transform>();
        gameplayPhaseStatemachineRef.ChangeState(gameplayPhaseStatemachineRef.gameplayVictoryPhaseState);
    }

    public void Victory(Player playerWhoWins)
	{
		playerCharacter = Players[Players.IndexOf(playerWhoWins)].GetComponent<Transform>();
		cinemachineVirtualCameras[0].LookAt = playerCharacter;
		cinemachineVirtualCameras[0].Follow = playerCharacter;
		ActivatePopupAtVictoryScreen();

    }

	public void ActivatePopupAtVictoryScreen()
	{
        List<Tuple<string, string, object, List<object>>> insertedParams = new();

        List<object> paramsList = new();
        insertedParams.Add(Tuple.Create<string, string, object, List<object>>("Restart", nameof(RestartGame), this, paramsList));
        insertedParams.Add(Tuple.Create<string, string, object, List<object>>("Quit", nameof(CloseGame), this, paramsList));

		DialogueBoxPopup.instance.ActivatePopupWithButtonChoices($"Congratulations! Player {GetCurrentPlayer().playerIDIntVal} is the winner!", insertedParams, headerText: "Victory!");

    }

	public IEnumerator RestartGame()
	{
		DebugModeSingleton.instance.ReloadScene();
		yield return null;
	}

	public IEnumerator CloseGame()
	{
		Application.Quit();
#if UNITY_EDITOR
        //Stop playing the scene
        UnityEditor.EditorApplication.isPlaying = false;
#endif
        yield return null;
	}

	//Event trigger
	public void OnPlayerUsedASupportCard(SupportCard supportCardUsed)
	{
		CurrentSupportCardThatWasJustUsed?.Invoke(supportCardUsed);
		CurrentSupportCardBeingUsed = supportCardUsed;
	}



	#region Debug functions
	private void FPSCounter()
	{
		if (lockFPS)
		{
			Application.targetFrameRate = fpsCap;
		}

		frameDeltaTimeArray = new float[50];
	}

	//Debug function!
	private float CalculateFPS()
	{
		float total = 0f;
		foreach (float deltaTime in frameDeltaTimeArray)
		{
			total += deltaTime;
		}
		return frameDeltaTimeArray.Length / total;
	}
	//Debug function!

	public void OpenDebugMessenger(string message)
	{
		debugMessagePanel.SetActive(true);
		debugMessagePanel.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = message;
	}

	public void CloseDebugMessengerPanel()
	{
		debugMessagePanel.SetActive(false);
	}

	public void ToggleSpeedUp()
	{
		isSpeedupOn = !isSpeedupOn;

		if(isSpeedupOn)
		{
			Time.timeScale = speedupValue;
			if(DebugModeSingleton.instance != null)
			{
				DebugModeSingleton.instance.TimeScaleToggleText.text = "StartDebugMenu: OFF";
			}
		}
		else
		{
			Time.timeScale = 1f;
			if(DebugModeSingleton.instance != null)
			{
				DebugModeSingleton.instance.TimeScaleToggleText.text = "StartDebugMenu: ON";
			}
		}
	}

	void TestFunc()
	{
		cinemachineVirtualCamera = GameObject.Find("Player Cam").GetComponent<CinemachineVirtualCamera>();

		cinemachineVirtualCamera.m_Lens.Dutch = 55;
	}
	#endregion


}
