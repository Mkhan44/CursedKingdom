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

    public StartDebugMenu StartDebugMenuRef;

    // Start is called before the first frame update
    private void Start()
    {
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
        foreach(Transform child in MusicAudioSourcesGameObjectTemp.transform)
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
        if (Application.isMobilePlatform)
        {
            mobileMenu.SetActive(true);
            desktopMenu.SetActive(false);
        }
        else
        {
            if (Input.anyKeyDown || Input.touchCount > 0)
            {
                title.SetTrigger(fadeOut);
                scrollMenu.SetActive(true);
                if(!DebugStartClicked)
                {
                   // PlayMusicAndFadeOutOtherOne(1);
                }
                DebugStartClicked = true;
            }
        }
    }

    public void LoadGameScene()
    {
        if(networkManager != null)
        {
            PurrSceneSettings settings = new()
            {
                isPublic = false,
                mode = LoadSceneMode.Single
            };
            networkManager.sceneModule.LoadSceneAsync("BoardGameplay" , settings);
        }
        else
        {
            SceneManager.LoadScene("BoardGameplay");
        }
    }
    
    public void ExitGame()
    { 
        Application.Quit(); 
    }

    #region Character Select

    public void PopulateCharacterSelection()
    {
        foreach(DebugStartData.PlayerDebugData playerDebugData in StartDebugMenuRef.defaultDebugStartData.playerDebugDatas)
        {
            GameObject newPlayerHolder = Instantiate(PlayerSelectionHolderPrefab, PlayerLayoutParent.transform);
            newPlayerHolder.transform.SetParent(PlayerLayoutParent.transform, false);
            PlayerMainMenuHolderDisplay newPlayerHolderDisplayRef = newPlayerHolder.GetComponent<PlayerMainMenuHolderDisplay>();
            //Determine class and populate based on that...Needa make this better.
            ClassData classDataWeAreUsing = classDatas[0];
            if(playerDebugData.typeOfClass == ClassData.ClassType.Archer)
            {
                classDataWeAreUsing = classDatas[0];
            }
            else if(playerDebugData.typeOfClass == ClassData.ClassType.Magician)
            {
                classDataWeAreUsing = classDatas[1];
            }
            else if(playerDebugData.typeOfClass == ClassData.ClassType.Thief)
            {
                classDataWeAreUsing = classDatas[2];
            }
            else if(playerDebugData.typeOfClass == ClassData.ClassType.Warrior)
            {
                classDataWeAreUsing = classDatas[3];
            }
            newPlayerHolderDisplayRef.PopulateData(classDataWeAreUsing);
            AddPlayer(newPlayerHolderDisplayRef);
            if(ActivePlayers.Count == 1)
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

    public void RemovePlayer(PlayerMainMenuHolderDisplay playerMainMenuHolderDisplayRef)
    {
        if(ActivePlayers.Count == 1)
        {
            Debug.LogWarning("You're trying to remove the final character! No way!");
            return;
        }
        playerMainMenuHolderDisplayRef.SelectedMainMenuHolderDisplay -= ChangedFocusSelectedCharacter;
        ActivePlayers.Remove(playerMainMenuHolderDisplayRef);
        Destroy(playerMainMenuHolderDisplayRef.gameObject);
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
