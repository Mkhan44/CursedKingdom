//Code written by Mohamed Riaz Khan of BukuGames.
//All code is written by me (Above name) unless otherwise stated via comments below.
//Not authorized for use outside of the Github repository of this game developed by BukuGames.
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Audio_Manager : MonoBehaviour
{
    public static Audio_Manager Instance;

    [Header("Gameobject music holders")]
    [SerializeField] private GameObject MusicHolder;
    [SerializeField] private GameObject musicAudioSourcePrefab;
    [SerializeField] private List<AudioSource> musicSources;

    [Header("Currently playing track")]
    [SerializeField] private AudioSource currentlyPlayingTrack;

    [Header("SFX Holders")]
    [SerializeField] private GameObject SFXHolder;
    [SerializeField] private GameObject sfxAudioSourcePrefab;

    [SerializeField] private List<AudioSource> sfxSources;
    private List<AudioSource> pausedAudioSources = new List<AudioSource>();

    public AudioSource CurrentlyPlayingTrack { get => currentlyPlayingTrack; set => currentlyPlayingTrack = value; }
    public List<AudioSource> MusicSources { get => musicSources; set => musicSources = value; }
    public List<AudioSource> SfxSources { get => sfxSources; set => sfxSources = value; }

    private void Awake()
    {
        Instance = this;
    }
    // Start is called before the first frame update
    private void Start()
    {
        //Gameplay scene, play music.

        //if (Level_Manager.Instance.getThisLevelType() == Level_Manager.levelType.normal)
        //{
        //    tutorialSource.gameObject.SetActive(false);
        //    currentlyPlayingTrack = easySource;
        //}
        //else
        //{
        //    easySource.gameObject.SetActive(false);
        //    mediumSource.gameObject.SetActive(false);
        //    bonusSource.gameObject.SetActive(false);
        //    hardPauseSource.gameObject.SetActive(false);
        //    currentlyPlayingTrack = tutorialSource;

        //}

        //setMusicTracks(Level_Manager.Instance.getTimePeriod());


        SetupNewSFXObjects();

    }

    public void SetupNewSFXObjects()
    {
        stopSFX();

        foreach (Transform child in SFXHolder.transform)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < SfxSources.Count; i++)
        {
            GameObject tempObj = Instantiate(sfxAudioSourcePrefab, this.transform);
            tempObj.transform.SetParent(SFXHolder.transform);
            tempObj.name = "SoundSource_" + i;
            SfxSources[i] = tempObj.GetComponent<AudioSource>();
        }
    }

    public void SetupNewMusicObjects()
    {

    }

    //SFX Functions
    public int playSFX(AudioClip soundToPlay, bool loop = false, float volume = 0.1f)
    {
        for(int i = 0; i < SfxSources.Count; i++)
        {
            if(!SfxSources[i].isPlaying)
            {
                SfxSources[i].clip = soundToPlay;
                SfxSources[i].loop = loop;
                SfxSources[i].volume = volume;
                SfxSources[i].Play();
                return i;
            }
        }

        GameObject tempObj = Instantiate(sfxAudioSourcePrefab, this.transform);
        tempObj.transform.SetParent(SFXHolder.transform);
        SfxSources.Add(tempObj.GetComponent<AudioSource>());
        tempObj.name = "SoundSource_" + SfxSources.Count;
        SfxSources[SfxSources.Count - 1].clip = soundToPlay;
        SfxSources[SfxSources.Count - 1].loop = loop;
        SfxSources[SfxSources.Count - 1].volume = volume;
        SfxSources[SfxSources.Count - 1].Play();
        Debug.LogWarning("All SFX clips are playing, we are going to add another source for more clips!");
        return SfxSources.Count - 1;
    }

    public void stopSFX(int audioIndex)
    {
        if(SfxSources[audioIndex].isPlaying)
        {
            SfxSources[audioIndex].Stop();
        }
       
    }

    public void stopSFX(string clipName)
    {
        for (int i = 0; i < SfxSources.Count; i++)
        {
            if (SfxSources[i].isPlaying && SfxSources[i].clip.name == clipName)
            {
                SfxSources[i].Stop();
            }
        }
    }

    public void stopSFX()
    {
        for (int i = 0; i < SfxSources.Count; i++)
        {
            if (SfxSources[i].isPlaying)
            {
                SfxSources[i].Stop();
            }
        }
    }

    public void togglePauseSFX()
    {
        for(int i = 0; i < SfxSources.Count; i++)
        {
          
            //if(Level_Manager.Instance.pauseStatus())
            //{
            //    if (sfxSources[i].isPlaying)
            //    {
            //        sfxSources[i].Pause();
            //        //  pausedAudioSources.Add(sfxSources[i]);
            //    }
            //}
            //else
            //{
            //    Debug.Log("Trying to unpause music.");
            //    sfxSources[i].UnPause();
            //}

            //Play pause SFX.
            
        }
       // pausedAudioSources.Clear();
    }



    /*
    void testPlaySFX()
    {
        GameObject tempObj = Instantiate(audioSourcePrefab, this.transform);
        tempObj.transform.SetParent(SFXHolder.transform);
        sfxSources.Add(tempObj.GetComponent<AudioSource>());
        tempObj.name = "SoundSource_" + (sfxSources.Count-1);
       // sfxSources[sfxSources.Count - 1].clip = soundToPlay;
       // sfxSources[sfxSources.Count - 1].Play();
        Debug.LogWarning("All SFX clips are playing, we are going to add another source for more clips!");
    }
    */
    //SFX Functions

    public void SetupMusicTracks()
    {
        //switch(theEra)
        //{
        //    case Level_Manager.timePeriod.Prehistoric:
        //        {
        //            bonusSource.clip = prehistoricMusic[0];
        //            easySource.clip = prehistoricMusic[1];
        //            mediumSource.clip = prehistoricMusic[2];
        //            hardPauseSource.clip = prehistoricMusic[3];
        //            break;
        //        }
        //    case Level_Manager.timePeriod.FeudalJapan:
        //        {
        //            bonusSource.clip = feudalJapanMusic[0];
        //            easySource.clip = feudalJapanMusic[1];
        //            mediumSource.clip = feudalJapanMusic[2];
        //            hardPauseSource.clip = feudalJapanMusic[3];
        //            break;
        //        }
        //    case Level_Manager.timePeriod.WildWest:
        //        {
        //            bonusSource.clip = wildWestMusic[0];
        //            easySource.clip = wildWestMusic[1];
        //            mediumSource.clip = wildWestMusic[2];
        //            hardPauseSource.clip = wildWestMusic[3];
        //            break;
        //        }
        //    case Level_Manager.timePeriod.Medieval:
        //        {
        //            bonusSource.clip = medMusic[0];
        //            easySource.clip = medMusic[1];
        //            mediumSource.clip = medMusic[2];
        //            hardPauseSource.clip = medMusic[3];
        //            break;
        //        }
        //    case Level_Manager.timePeriod.Future:
        //        {
        //            bonusSource.clip = futureMusic[0];
        //            easySource.clip = futureMusic[1];
        //            mediumSource.clip = futureMusic[2];
        //            hardPauseSource.clip = futureMusic[3];
        //            break;
        //        }
        //    case Level_Manager.timePeriod.tutorial:
        //        {
                    
        //            StartCoroutine(tutorialPlay());
        //            return;
        //            break;
        //        }
        //    default:
        //        {
        //            break;  
        //        }
        //}

        //DEBUG.
        
        //Prolly need to call this function BEFORE swapping the tracks...
        StartCoroutine(fadeBetweenTimeswaps(CurrentlyPlayingTrack, 1f));

        /*
        easySource.volume = 0f;
        mediumSource.volume = 0f;
        hardPauseSource.volume = 0f;
        bonusSource.volume = 0f;
        */
        

        //Have coroutine fade out the current tracks and then play the new ones.
        //Fade out should probably be before the swap above.
   
    }

    //IEnumerator tutorialPlay()
    //{
       
    //    float introLength;
    //    introLength = tutorialMusicIntro.length;
    //    tutorialSource.clip = tutorialMusicIntro;
    //    tutorialSource.volume = 1f;
    //    tutorialSource.loop = false;
    //    tutorialSource.Play();
     
    //    yield return new WaitForSecondsRealtime(introLength - 0.01f);

    //    tutorialSource.Stop();
    //    tutorialSource.clip = tutorialMusicLoop;
    //    tutorialSource.loop = true;
    //    tutorialSource.volume = 1f;
    //    tutorialSource.Play();

    //}

    //Change the music based on the difficulty we are in. If it's a bonus/timeswap wave, use the Bonus music track.
    public void ChangeMusicTrack()
    {
        //if(isSpecial)
        //{
        //    StartCoroutine(fadeBetweenDifficultyTracks(currentlyPlayingTrack, bonusSource, 1f));
        //    /*
        //    easySource.volume = 0f;
        //    mediumSource.volume = 0f;
        //    hardPauseSource.volume = 0f;
        //    bonusSource.volume = 1f;
        //    */
        //    //  currentlyPlayingTrack = bonusSource;
        //    return;
        //}

        //switch(theDifficulty)
        //{
        //    case Wave_Spawner.waveDiff.easy:
        //        {
        //            StartCoroutine(fadeBetweenDifficultyTracks(currentlyPlayingTrack, easySource, 1f));

        //            /*
        //            easySource.volume = 1f;
        //            mediumSource.volume = 0f;
        //            hardPauseSource.volume = 0f;
        //            bonusSource.volume = 0f;
        //            */
        //            // currentlyPlayingTrack = easySource;

        //            break;
        //        }
        //    case Wave_Spawner.waveDiff.medium:
        //        {
        //            StartCoroutine(fadeBetweenDifficultyTracks(currentlyPlayingTrack, mediumSource, 1f));

        //            /*
        //            easySource.volume = 0f;
        //            mediumSource.volume = 1f;
        //            hardPauseSource.volume = 0f;
        //            bonusSource.volume = 0f;
        //            */
        //           // currentlyPlayingTrack = mediumSource;
        //            break;
        //        }
        //    case Wave_Spawner.waveDiff.hardPause:
        //        {
        //            StartCoroutine(fadeBetweenDifficultyTracks(currentlyPlayingTrack, hardPauseSource, 1f));

        //            /*
        //            easySource.volume = 0f;
        //            mediumSource.volume = 0f;
        //            hardPauseSource.volume = 1f;
        //            bonusSource.volume = 0f;
        //            */
        //            //currentlyPlayingTrack = hardPauseSource;
        //            break;
        //        }
        //    default:
        //        {
        //            Debug.LogWarning("We couldn't find the difficulty in the switch!");
        //            break;
        //        }
        //}

     
    }

    IEnumerator fadeBetweenTimeswaps(AudioSource currentTrackToFade, float fadeInTargetVolume)
    {
        //Fade out current track.
        
        //if(Wave_Spawner.Instance != null)
        //{
        //    while(!Wave_Spawner.Instance.getIntroFinishedStatus())
        //    {
        //        yield return null;
        //    }
        //}

        //while(currentTrackToFade.volume > 0)
        //{
        //    Debug.Log("During Timeswap loop: " + currentTrackToFade.volume + " = the volume of current track");
        //    if(currentTrackToFade.volume > 0)
        //    {
        //        currentTrackToFade.volume -= 0.1f;
        //    }

        //    yield return new WaitForSeconds(0.2f);
        //}

        //yield return new WaitForSeconds(0.5f);

        //bonusSource.Play();
        //easySource.Play();
        //mediumSource.Play();
        //hardPauseSource.Play();

        ////Fade in new track.
        //while (easySource.volume < fadeInTargetVolume)
        //{
        //    easySource.volume += 0.1f;
        //    yield return new WaitForSeconds(0.1f);
        //}

        //currentlyPlayingTrack = easySource;
        yield return null;
    }
    IEnumerator fadeBetweenDifficultyTracks(AudioSource currentTrackToFadeOut, AudioSource nextTrackToFadeIn, float fadeInTargetVolume)
    {
        CurrentlyPlayingTrack = nextTrackToFadeIn;

        //Debug.Log("BEFORE LOOP: " + currentTrackToFadeOut.volume + " = the volume of the current track & " + nextTrackToFadeIn.volume + " = the volume of the next track!");
        while(currentTrackToFadeOut.volume > 0 && nextTrackToFadeIn.volume < fadeInTargetVolume)
        {
          //  Debug.Log("DURING LOOP: " + currentTrackToFadeOut.volume + " = the volume of the current track & " + nextTrackToFadeIn.volume + " = the volume of the next track!");
            //Failsafe for if one is completed and the other is not.
            if (currentTrackToFadeOut.volume > 0)
            {
                currentTrackToFadeOut.volume -= 0.1f;
            }

            if(nextTrackToFadeIn.volume < fadeInTargetVolume)
            {
                nextTrackToFadeIn.volume += 0.1f;
            }

            yield return new WaitForSeconds(0.1f);
            //yield return null;
        }

        //Ensure that they are at the proper settings.
       // Debug.Log("The current track we are fading out is: " + currentTrackToFadeOut.clip.name + " And the next track is: " + nextTrackToFadeIn.clip.name);
        currentTrackToFadeOut.volume = 0f;
        nextTrackToFadeIn.volume = fadeInTargetVolume;

        //currentlyPlayingTrack = nextTrackToFadeIn;
        yield return null;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void muteCurrentTrack()
    {
        CurrentlyPlayingTrack.volume = 0f;
    }

    public void unmuteCurrentTrack()
    {
        CurrentlyPlayingTrack.volume = 1f;
    }
}
