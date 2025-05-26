//Code written by Mohamed Riaz Khan of BukuGames.
//All code is written by me (Above name) unless otherwise stated via comments below.
//Not authorized for use outside of the Github repository of this game developed by BukuGames.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using PurrNet;
using PurrNet.Modules;
using System;
using PurrNet.Logging;
public class MenuManager : NetworkBehaviour
{
    public TextMeshProUGUI versionText;
    public TextMeshProUGUI versionTextMobile;
    public TextMeshProUGUI mainText;
    public Animator scroll;
    public Animator title;
    public GameObject scrollMenu;
    public GameObject desktopMenu;
    public GameObject mobileMenu;
    public AudioData MenuMusicAudioData;
    public GameObject MusicAudioSourcesGameObjectTemp;
    public List<AudioSource> MusicAudioSourcesTemp;

    public bool DebugStartClicked;
    public string fadeOut = "FadeOut";

    //Testing
    public List<ClassData> classDatas;

    //General references:
    public List<PlayerMainMenuHolderDisplay> ActivePlayers;
    public PlayerMainMenuHolderDisplay CurrentlySelectedMainMenuHolderDisplay;

    //General prefabs
    public GameObject PlayerSelectionHolderPrefab;

    //Desktop references

    public GameObject PlayerLayoutParent;


    //Mobile references

    public GameObject PlayerLayoutParentMobile;

    public StartDebugMenu StartDebugMenuRef;
    
    private void Start()
    {
        Invoke("Started" , 0.1f);
    }

    private void Started()
    {
        if (Application.isMobilePlatform)
         {
            mobileMenu.SetActive(true);
            desktopMenu.SetActive(false);
         }
        versionText.text = "ver. " + Application.version;
        versionTextMobile.text = versionText.text;
        DebugStartClicked = false;
        if(Audio_Manager.Instance.CurrentlyPlayingMusicInformation.CurrentlyPlayingTrackSource == null || (Audio_Manager.Instance.CurrentMusicAudioData != null && Audio_Manager.Instance.CurrentMusicAudioData != MenuMusicAudioData))
        {
            //Pass in our audioData and play music from there since it's the first time we're playing music.
            Audio_Manager.Instance.NewMusicObjectsSetup += PopulateAudioObjectsList;
            Audio_Manager.Instance.StartCoroutine(Audio_Manager.Instance.FadeOutCurrentMusicTrackThenSetupNewMusicTracks(MenuMusicAudioData));
        }
        /* mobileMenu.SetActive(false);
        desktopMenu.SetActive(false);
        scrollMenu.SetActive(false); */
    }

    private void PopulateAudioObjectsList(AudioData data)
    {
        Audio_Manager.Instance.NewMusicObjectsSetup -= PopulateAudioObjectsList;
        MusicAudioSourcesTemp.Clear();
        foreach (Transform child in MusicAudioSourcesGameObjectTemp.transform)
        {
            AudioSource audioSource = child.GetComponent<AudioSource>();
            MusicAudioSourcesTemp.Add(audioSource);
        }
    }

    public void PlayMusicAndFadeOutOtherOne(int numTrack)
    {
        if(numTrack > MenuMusicAudioData.MusicClips.Count || numTrack < 0 || MenuMusicAudioData.MusicClips.Count == 1)
        {
            return;
        }

        Audio_Manager.Instance.PlayAlreadyLoadedMusic(MusicAudioSourcesTemp[numTrack], MenuMusicAudioData.MusicClips[numTrack], false, true, 1, true);

    }


    // Update is called once per frame
    private void Update()
    {
        // if (Application.isMobilePlatform)
        // {
        //     mobileMenu.SetActive(true);
        //     desktopMenu.SetActive(false);
        // }
        //else
        //{
            if (Input.anyKeyDown || Input.touchCount > 0 && !Application.isMobilePlatform)
            {
                title.SetTrigger(fadeOut);
                scrollMenu.SetActive(true);
                if(!DebugStartClicked)
                {
                   // PlayMusicAndFadeOutOtherOne(1);
                }
                DebugStartClicked = true;
            }
        //}
    }

    public void LoadGameScene(bool DontShowDebugMenu = true)
    {
        UpdateDebugStartData();
        StartDebugMenuRef.StartGame();
        
        // if (networkManager != null)
        // {
        //     StartDebugMenuRef.turnOffPanel = DontShowDebugMenu;
        //     StartDebugMenuRef.useScriptable = true;
        //     UpdateDebugStartData();
        //     PurrSceneSettings settings = new()
        //     {
        //         isPublic = false,
        //         mode = LoadSceneMode.Single
        //     };
        //     networkManager.sceneModule.LoadSceneAsync("BoardGameplay", settings);
        // }
        // else
        // {
        //     SceneManager.LoadScene("BoardGameplay");
        // }
    }
    
    public void ExitGame()
    { 
        Application.Quit(); 
    }

    #region Character Select

    public void PopulateCharacterSelection(bool isMobile = false)
    {
        GameObject playerParentLayoutHolder = PlayerLayoutParent;
        if (!isMobile)
        {
            playerParentLayoutHolder = PlayerLayoutParent;
        }
        else
        {
            playerParentLayoutHolder = PlayerLayoutParentMobile;
        }


        foreach (DebugStartData.PlayerDebugData playerDebugData in StartDebugMenuRef.defaultDebugStartData.playerDebugDatas)
        {
            GameObject newPlayerHolder = Instantiate(PlayerSelectionHolderPrefab, playerParentLayoutHolder.transform);
            newPlayerHolder.transform.SetParent(playerParentLayoutHolder.transform, false);
            PlayerMainMenuHolderDisplay newPlayerHolderDisplayRef = newPlayerHolder.GetComponent<PlayerMainMenuHolderDisplay>();
            //Determine class and populate based on that...Needa make this better.
            ClassData classDataWeAreUsing = classDatas[0];
            if (playerDebugData.typeOfClass == ClassData.ClassType.Archer)
            {
                classDataWeAreUsing = classDatas[0];
            }
            else if (playerDebugData.typeOfClass == ClassData.ClassType.Magician)
            {
                classDataWeAreUsing = classDatas[1];
            }
            else if (playerDebugData.typeOfClass == ClassData.ClassType.Thief)
            {
                classDataWeAreUsing = classDatas[2];
            }
            else if (playerDebugData.typeOfClass == ClassData.ClassType.Warrior)
            {
                classDataWeAreUsing = classDatas[3];
            }
            newPlayerHolderDisplayRef.PopulateData(classDataWeAreUsing);
            AddPlayer(newPlayerHolderDisplayRef);
            if (ActivePlayers.Count == 1)
            {
                ChangedFocusSelectedCharacter(ActivePlayers[0]);
            }
        }
    }

    public void AddPlayer(PlayerMainMenuHolderDisplay playerMainMenuHolderDisplayRef)
    {
        ActivePlayers.Add(playerMainMenuHolderDisplayRef);
        playerMainMenuHolderDisplayRef.SelectedMainMenuHolderDisplay += ChangedFocusSelectedCharacter;
    }

    public void AddPlayerManually(bool isMobile = false)
    {
        GameObject playerParentLayoutHolder = PlayerLayoutParent;
        if (!isMobile)
        {
            playerParentLayoutHolder = PlayerLayoutParent;
        }
        else
        {
            playerParentLayoutHolder = PlayerLayoutParentMobile;
        }

        if (ActivePlayers.Count >= 4)
        {
            Debug.LogWarning("Already have 4 players.");
            return;
        }
        GameObject newPlayerHolder = Instantiate(PlayerSelectionHolderPrefab, playerParentLayoutHolder.transform);
        newPlayerHolder.transform.SetParent(playerParentLayoutHolder.transform, false);
        PlayerMainMenuHolderDisplay newPlayerHolderDisplayRef = newPlayerHolder.GetComponent<PlayerMainMenuHolderDisplay>();
        newPlayerHolderDisplayRef.PopulateData(classDatas[0]);
        AddPlayer(newPlayerHolderDisplayRef);
        ChangedFocusSelectedCharacter(ActivePlayers[ActivePlayers.Count - 1]);
        StartDebugMenuRef.NumPlayersOnValueChanged(ActivePlayers.Count);
    }

    public void RemovePlayer(PlayerMainMenuHolderDisplay playerMainMenuHolderDisplayRef)
    {
        if (ActivePlayers.Count == 2)
        {
            Debug.LogWarning("You must have at least 2 players to play!");
            return;
        }
        playerMainMenuHolderDisplayRef.SelectedMainMenuHolderDisplay -= ChangedFocusSelectedCharacter;
        ActivePlayers.Remove(playerMainMenuHolderDisplayRef);
        Destroy(playerMainMenuHolderDisplayRef.gameObject);
        StartDebugMenuRef.NumPlayersOnValueChanged(ActivePlayers.Count);
    }

    public void RemovePlayerManually()
    {
        if (ActivePlayers.Count == 2)
        {
            Debug.LogWarning("You must have at least 2 players to play!");
            return;
        }
        PlayerMainMenuHolderDisplay playerToRemove = ActivePlayers[ActivePlayers.Count - 1];
        if (CurrentlySelectedMainMenuHolderDisplay == playerToRemove)
        {
            CurrentlySelectedMainMenuHolderDisplay = ActivePlayers[ActivePlayers.Count - 2];
        }
        playerToRemove.SelectedMainMenuHolderDisplay -= ChangedFocusSelectedCharacter;
        ActivePlayers.Remove(playerToRemove);
        Destroy(playerToRemove.gameObject);
        StartDebugMenuRef.NumPlayersOnValueChanged(ActivePlayers.Count);
    }

    public void ChangeCharacter(ClassData classDataToChangeTo)
    {
        CurrentlySelectedMainMenuHolderDisplay.ChangeCharacter(classDataToChangeTo);
    }

    public void ChangedFocusSelectedCharacter(PlayerMainMenuHolderDisplay playerMainMenuHolderDisplayRef)
    {
        CurrentlySelectedMainMenuHolderDisplay = playerMainMenuHolderDisplayRef;
        Debug.Log($"Currently focused player in the char selection screen is player: {ActivePlayers.IndexOf(playerMainMenuHolderDisplayRef)} the {playerMainMenuHolderDisplayRef.ClassText}");
    }


    public void UpdateDebugStartData()
    {
        if (ActivePlayers.Count != StartDebugMenuRef.defaultDebugStartData.playerDebugDatas.Count)
        {
            Debug.LogWarning("Different amount of players compared to the debug data right now!");
        }
        int index = 0;
        foreach (PlayerMainMenuHolderDisplay playerMainMenuHolderDisplay in ActivePlayers)
        {
            if (playerMainMenuHolderDisplay.classDataRef.classType == ClassData.ClassType.Magician)
            {
                StartDebugMenuRef.defaultDebugStartData.playerDebugDatas[index].startingSpaceNameOverride = "Mage Start";
            }
            else if (playerMainMenuHolderDisplay.classDataRef.classType == ClassData.ClassType.Archer)
            {
                StartDebugMenuRef.defaultDebugStartData.playerDebugDatas[index].startingSpaceNameOverride = "Archer Start";
            }
            else if (playerMainMenuHolderDisplay.classDataRef.classType == ClassData.ClassType.Thief)
            {
                StartDebugMenuRef.defaultDebugStartData.playerDebugDatas[index].startingSpaceNameOverride = "Thief Start";
            }
            else if (playerMainMenuHolderDisplay.classDataRef.classType == ClassData.ClassType.Warrior)
            {
                StartDebugMenuRef.defaultDebugStartData.playerDebugDatas[index].startingSpaceNameOverride = "Warrior Start";
            }

            StartDebugMenuRef.defaultDebugStartData.playerDebugDatas[index].typeOfClass = playerMainMenuHolderDisplay.classDataRef.classType;

            index++;
        }
    }

    public void RemoveAllPlayers()
    {
        foreach(PlayerMainMenuHolderDisplay playerRef in ActivePlayers)
        {
            playerRef.SelectedMainMenuHolderDisplay -= ChangedFocusSelectedCharacter;
            Destroy(playerRef.gameObject);
        }

        CurrentlySelectedMainMenuHolderDisplay = null;
        ActivePlayers.Clear();
    }

    #endregion

}
