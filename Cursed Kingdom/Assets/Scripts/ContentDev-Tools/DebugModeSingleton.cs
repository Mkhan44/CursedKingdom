//Code written by Mohamed Riaz Khan of BukuGames.
//All code is written by me (Above name) unless otherwise stated via comments below.
//Not authorized for use outside of the Github repository of this game developed by BukuGames.
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DebugModeSingleton : MonoBehaviour
{
    public static DebugModeSingleton instance;

    [SerializeField] private GameplayManager gameplayManager;
    [SerializeField] private GameObject debugUIPanel;
    [SerializeField] private TextMeshProUGUI debugButtonToggleText;
    [SerializeField] private TMP_Dropdown movementOverrideDropdown;
    [SerializeField] private TMP_Dropdown spaceLandedOverrideDropdown;
    [SerializeField] private bool isDebugActive;
    [SerializeField] private string spaceLandedOverrideName;
    [SerializeField] private int movementNumberOverride;

    public bool IsDebugActive { get => isDebugActive; }
    public int MovementNumberOverride { get => movementNumberOverride; }

    // [SerializeField] 

    private void Awake()
    {
        if(instance == null) 
        {
            instance = this;
        }

        debugUIPanel.SetActive(false);
        debugButtonToggleText.text = "DEBUG MODE: OFF";
        isDebugActive = false;
    }

    private void Start()
    {
        gameplayManager = GameObject.Find("Game Manager").GetComponent<GameplayManager>();
    }
    public void ToggleDebugMode()
    {
        if(IsDebugActive) 
        {
            debugUIPanel.SetActive(false);
            debugButtonToggleText.text = "DEBUG MODE: OFF";
            isDebugActive = false;
        }
        else
        {
            debugUIPanel.SetActive(true);
            debugButtonToggleText.text = "DEBUG MODE: ON";
            isDebugActive = true;
        }
    }

    public void SetOverrideMovementNumber()
    {
        if(movementOverrideDropdown.value != 0)
        {
            Debug.Log("The current value of the dropdown is: " + (movementOverrideDropdown.value));
            movementNumberOverride = (movementOverrideDropdown.value);
        }
        else
        {
            movementNumberOverride = 0;
        }
        
    }
    public void OverrideCurrentPlayerSpacesLeftToMove(Player player)
    {
        Debug.Log("IN DEBUG MODE OVERRIDING PLAYER'S SPACES LEFT TO MOVE!");
        if(movementNumberOverride == 0)
        {
            Debug.Log("Override value is 0. Not gonna do anything.");
            return;
        }
        player.SpacesLeftToMove = movementNumberOverride;
    }

    public void SetupOverrideSpaceLandEffectDropdownOptions(List<Space> gameBoardSpaces)
    {
        spaceLandedOverrideDropdown.options.Clear();
        List<string> spaceNameStrings = new();
        List<Sprite> spaceSprites = new();

        foreach(Space space in gameBoardSpaces)
        {
            string spaceName = space.spaceData.spaceName;
            if (!spaceNameStrings.Contains(spaceName))
            {
                spaceNameStrings.Add(spaceName);
                spaceSprites.Add(space.spaceData.spaceSprite);
            }
        }
        spaceLandedOverrideDropdown.options.Add(new TMP_Dropdown.OptionData("No Override"));

        spaceLandedOverrideDropdown.AddOptions(spaceNameStrings);

        //Start at 1 to skip the "No override".
        for(int i = 1; i < spaceLandedOverrideDropdown.options.Count; i++)
        {
            spaceLandedOverrideDropdown.options[i].image = spaceSprites[i-1];
        }

    }
    public void SetOverrideSpaceLandEffect()
    {
        spaceLandedOverrideName = spaceLandedOverrideDropdown.captionText.text;
    }

    public Space OverrideSpaceLandEffect()
    {
        Space spaceToReturn = null;
        Debug.Log("IN DEBUG MODE OVERRIDING THE SPACE THAT THE PLAYER WILL LAND ON!");
        if (!string.IsNullOrEmpty(spaceLandedOverrideName)  && spaceLandedOverrideName != spaceLandedOverrideDropdown.options[0].text && gameplayManager != null)
        {
            foreach(Space space in gameplayManager.spaces)
            {
                if(space.spaceData.spaceName == spaceLandedOverrideName)
                {
                    spaceToReturn = space;
                    break;
                }
            }
        }
        return spaceToReturn;
    }

    public void ReloadScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

}