//Code written by Mohamed Riaz Khan of BukuGames.
//All code is written by me (Above name) unless otherwise stated via comments below.
//Not authorized for use outside of the Github repository of this game developed by BukuGames.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using UnityEngine.SceneManagement;

public class StartDebugMenu : MonoBehaviour
{
    public event Action CheckIfPanelShouldTurnOn;

    public static StartDebugMenu instance;
    public bool useScriptable;
    public bool turnOffPanel;
    public CanvasGroup canvasGroup;

    //Prefab to spawn in per player.
    public GameObject debugPlayerElementPrefab;
    public Transform debugElementParentPanel;
    public DebugStartData defaultDebugStartData;
    public DebugStartData currentlySelectedStartData;


    public int numberOfPlayers = 0;
    public TMP_Dropdown numberOfPlayersDropDown;
    public TMP_Dropdown currentlySelectedStartDataDropdown;
    public TMP_InputField newStartDataInputField;
    public Button createNewStartDataButton;
    public Button useDefaultSettingsButton;
    public Button startGameButton;
    public TextMeshProUGUI tipsText;
    public GameplayManager gameplayManager;
    private List<string> spacesOnCurrentBoardNames;

    List<StartDebugPlayerElement> playerElements;


    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(transform.parent.gameObject);
            useScriptable = false;
            CheckIfPanelShouldTurnOn += CheckIfPanelShouldTurnOnOnRestart;
            startGameButton.onClick.AddListener(StartGame);
            useDefaultSettingsButton.onClick.AddListener(UseDefaultSettings);
            SetupData();
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
        else
        {
            Destroy(transform.parent.gameObject);
        }
    }

    private void CheckIfPanelShouldTurnOnOnRestart()
    {
        if(turnOffPanel)
        {
            TurnOffPanel();
        }
    }

    public void TurnOffPanel()
    {
        canvasGroup.alpha = 0.0f;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;
    }

    public void StartGame()
    {
        turnOffPanel = true;
        useScriptable = true;
        TurnOffPanel();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        CheckIfPanelShouldTurnOn?.Invoke();
    }

    public void UseDefaultSettings()
    {
        turnOffPanel = true;
        useScriptable = false;
        TurnOffPanel();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        CheckIfPanelShouldTurnOn?.Invoke();
    }

    //This will also need to be called everytime the scene is restarted because we'll need to get the next instance of the gameplayManager!!!!
    private void SetupData()
    {
        gameplayManager = GameObject.Find("Game Manager").GetComponent<GameplayManager>();
        currentlySelectedStartData = defaultDebugStartData;
        SetupSpacesToChooseFromList();
        SetupNumPlayersDropdownOptions();
        PullInitialDataFromScriptable();
    }

    #region Setup each character dropdown events

    private void DropdownOnValueChanged(StartDebugPlayerElement startDebugPlayerElement)
    {
        //This is bad because we're just doing all of them even though only 1 value is edited but since it's debug meh it's cool.

        foreach(StartDebugPlayerElement element in playerElements)
        {
            if(element == startDebugPlayerElement)
            {
                int index = playerElements.IndexOf(element);

                currentlySelectedStartData.playerDebugDatas[index].typeOfClass = (ClassData.ClassType)element.classDropdown.value;
                currentlySelectedStartData.playerDebugDatas[index].startingHealthOverride = element.healthOverrideDropdown.value;
                currentlySelectedStartData.playerDebugDatas[index].startingSpaceNameOverride = element.startSpaceDropdown.captionText.text;
                currentlySelectedStartData.playerDebugDatas[index].startingLevelOverride = element.levelOverrideDropdown.value;
                currentlySelectedStartData.playerDebugDatas[index].movementCardsToStartWithOverride = element.movementCardsInHandOverrideDropdown.value;
                currentlySelectedStartData.playerDebugDatas[index].supportCardsToStartWithOverride = element.supportCardsInHandOverrideDropdown.value;
            }
        }
    }

    private void NumPlayersOnValueChanged(int value)
    {
        if(value < 2)
        {
            tipsText.text = $"You cannot have less than 2 Players. Please select at least '2' in the dropdown box.";
            return;
        }
        tipsText.text = $"Tips";
        currentlySelectedStartData.numberOfPlayersToUse = numberOfPlayersDropDown.value;
        foreach(Transform child in debugElementParentPanel)
        {
            Destroy(child.gameObject);
        }

        PullInitialDataFromScriptable();
    }

    #endregion

    private void PullInitialDataFromScriptable()
    {
        numberOfPlayers = currentlySelectedStartData.numberOfPlayersToUse;
        int currentPlayerIndexInData = 1;
        playerElements = new();
        foreach(DebugStartData.PlayerDebugData playerDebugData in defaultDebugStartData.playerDebugDatas)
        {
            currentPlayerIndexInData++;
            GameObject newPlayerDebugElement = Instantiate(debugPlayerElementPrefab, debugElementParentPanel);
            StartDebugPlayerElement playerDebugElementScript = newPlayerDebugElement.GetComponent<StartDebugPlayerElement>();
            playerDebugElementScript.playerNumberText.text = $"Player {currentPlayerIndexInData - 1}";
            SetupPlayerDebugElementScriptDropdowns(playerDebugElementScript);
            PopulateDataFromScriptableToEachPlayer(playerDebugElementScript, currentPlayerIndexInData - 2);
            playerDebugElementScript.ValueChanged += DropdownOnValueChanged;

            playerElements.Add(playerDebugElementScript);

            if (currentPlayerIndexInData > numberOfPlayers)
            {
                break;
            }
        }
        numberOfPlayersDropDown.onValueChanged.RemoveAllListeners();
        numberOfPlayersDropDown.onValueChanged.AddListener(NumPlayersOnValueChanged);

        numberOfPlayersDropDown.value = currentlySelectedStartData.numberOfPlayersToUse;
    }

    
    private void SetupPlayerDebugElementScriptDropdowns(StartDebugPlayerElement playerDebugElementScript)
    {
        playerDebugElementScript.classDropdown.ClearOptions();
        playerDebugElementScript.healthOverrideDropdown.ClearOptions();
        playerDebugElementScript.startSpaceDropdown.ClearOptions();
        playerDebugElementScript.levelOverrideDropdown.ClearOptions();
        playerDebugElementScript.movementCardsInHandOverrideDropdown.ClearOptions();
        playerDebugElementScript.supportCardsInHandOverrideDropdown.ClearOptions();

        //Class dropdown
        playerDebugElementScript.classDropdown.options.Add(new TMP_Dropdown.OptionData ("Magician"));
        playerDebugElementScript.classDropdown.options.Add(new TMP_Dropdown.OptionData("Thief"));
        playerDebugElementScript.classDropdown.options.Add(new TMP_Dropdown.OptionData("Warrior"));
        playerDebugElementScript.classDropdown.options.Add(new TMP_Dropdown.OptionData("Archer"));

        int i = 0;

        //Health override dropdown
        for(i = 0; i < 16; i++)
        {
            int currentIndex = i;
            playerDebugElementScript.healthOverrideDropdown.options.Add(new TMP_Dropdown.OptionData(currentIndex.ToString()));
        }

        //Reset i though I don't think we need to since we are doing that in the next for loop lol.
        i = 0;

        //level, movement & support override dropdowns
        for (i = 0; i < 6; i++)
        {
            int currentIndex = i;
            playerDebugElementScript.levelOverrideDropdown.options.Add(new TMP_Dropdown.OptionData(currentIndex.ToString()));
            playerDebugElementScript.movementCardsInHandOverrideDropdown.options.Add(new TMP_Dropdown.OptionData(currentIndex.ToString()));
            playerDebugElementScript.supportCardsInHandOverrideDropdown.options.Add(new TMP_Dropdown.OptionData(currentIndex.ToString()));
        }

        //Space names.
        SetupSpacesToChooseFromDropdownOptions(playerDebugElementScript.startSpaceDropdown);
    }

    private void PopulateDataFromScriptableToEachPlayer(StartDebugPlayerElement playerDebugElementScript, int playerIndex)
    {
        DebugStartData.PlayerDebugData debugData = currentlySelectedStartData.playerDebugDatas[playerIndex];

        foreach (TMP_Dropdown.OptionData dropdownElement in playerDebugElementScript.classDropdown.options)
        {
            if(dropdownElement.text.ToLower() == debugData.typeOfClass.ToString().ToLower())
            {
                int indexOfElement = playerDebugElementScript.classDropdown.options.IndexOf(dropdownElement);
                playerDebugElementScript.classDropdown.value = indexOfElement;
                break;
            }
        }

        playerDebugElementScript.healthOverrideDropdown.value = debugData.startingHealthOverride;

        foreach(TMP_Dropdown.OptionData dropdownElement in playerDebugElementScript.startSpaceDropdown.options)
        {
            if(dropdownElement.text == debugData.startingSpaceNameOverride)
            {
                int indexOfElement = playerDebugElementScript.startSpaceDropdown.options.IndexOf(dropdownElement);
                playerDebugElementScript.startSpaceDropdown.value = indexOfElement;
                break;
            }
        }

        playerDebugElementScript.levelOverrideDropdown.value = debugData.startingLevelOverride;
        playerDebugElementScript.movementCardsInHandOverrideDropdown.value = debugData.movementCardsToStartWithOverride;
        playerDebugElementScript.supportCardsInHandOverrideDropdown.value = debugData.supportCardsToStartWithOverride;
    }

    private void SetupNumPlayersDropdownOptions()
    {
        numberOfPlayersDropDown.ClearOptions();

        for(int i = 0; i < 5; i++)
        {
            int nextNum = i;
            numberOfPlayersDropDown.options.Add(new TMP_Dropdown.OptionData(nextNum.ToString()));
        }

        numberOfPlayersDropDown.value = currentlySelectedStartData.numberOfPlayersToUse;
    }

    private void SetupSpacesToChooseFromList()
    {
        if (gameplayManager == null)
        {
            Debug.LogError("Trying to use the debug menu but can't find the gameplay manager!");
        }

        if (gameplayManager.boardPrefab != null)
        {
            spacesOnCurrentBoardNames = new();

            GameObject boardHolder;

            boardHolder = Instantiate(gameplayManager.boardPrefab);

            foreach (Transform child in boardHolder.transform)
            {
                //Rows
                foreach (Transform childChild in child)
                {
                    Space childSpace = childChild.GetComponent<Space>();

                    if (childSpace != null && childSpace.gameObject.activeInHierarchy)
                    {
                        spacesOnCurrentBoardNames.Add(childSpace.spaceData.spaceName);
                    }
                }
            }

            Destroy(boardHolder);
        }
    }

    private void SetupSpacesToChooseFromDropdownOptions(TMP_Dropdown spacesDropdownList)
    {
        spacesDropdownList.ClearOptions();
        spacesDropdownList.options.Add(new TMP_Dropdown.OptionData("No override"));
        foreach (string name in  spacesOnCurrentBoardNames)
        {
            spacesDropdownList.options.Add(new TMP_Dropdown.OptionData(name));
        }
    }

}
