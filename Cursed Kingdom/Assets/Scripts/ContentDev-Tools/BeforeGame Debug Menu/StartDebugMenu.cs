//Code written by Mohamed Riaz Khan of BukuGames.
//All code is written by me (Above name) unless otherwise stated via comments below.
//Not authorized for use outside of the Github repository of this game developed by BukuGames.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class StartDebugMenu : MonoBehaviour
{
    public static StartDebugMenu instance;

    //Prefab to spawn in per player.
    public GameObject debugPlayerElementPrefab;
    public DebugStartData currentlySelectedStartData;


    public int numberOfPlayers = 0;
    public TMP_Dropdown numberOfPlayersDropDown;
    public TMP_Dropdown currentlySelectedStartDataDropdown;
    public TMP_InputField newStartDataInputField;
    public Button createNewStartDataButton;
    public Button saveSettingsButton;
    public Button startGameButton;
    public TextMeshProUGUI tipsText;
    public GameplayManager gameplayManager;
    private List<string> spacesOnCurrentBoardNames;


    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this);
            SetupData();
        }
        else
        {
            Destroy(this);
        }
    }

    private void SetupData()
    {
        gameplayManager = GameObject.Find("Game Manager").GetComponent<GameplayManager>();
        SetupSpacesToChooseFromList();
        SetupNumPlayersDropdownOptions();

    }

    private void SetupScriptableDropdownOptions()
    {

    }

    private void SetupNumPlayersDropdownOptions()
    {
        numberOfPlayersDropDown.ClearOptions();

        for(int i = 1; i < 4; i++)
        {
            int nextNum = i + 1;

            numberOfPlayersDropDown.options.Add(new TMP_Dropdown.OptionData(nextNum.ToString()));
        }
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
