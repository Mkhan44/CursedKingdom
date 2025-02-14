//Code written by Mohamed Riaz Khan of BukuGames.
//All code is written by me (Above name) unless otherwise stated via comments below.
//Not authorized for use outside of the Github repository of this game developed by BukuGames.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

public class Audio_Manager : MonoBehaviour
{
    public static Audio_Manager Instance;

    public event Action<AudioData> NewMusicObjectsSetup;


    [Header("Gameobject music holders")]
    [SerializeField] private GameObject MusicHolder;
    [SerializeField] private GameObject musicAudioSourcePrefab;
    [SerializeField] private List<AudioSource> musicSources;

    [Header("Currently playing track")]
    private CurrentlyPlayingMusicInfo currentlyPlayingMusicInformation = new();

    [Header("SFX Holders")]
    [SerializeField] private GameObject SFXHolder;
    [SerializeField] private GameObject sfxAudioSourcePrefab;

    [SerializeField] private List<AudioSource> sfxSources;
    private List<AudioSource> pausedAudioSources = new List<AudioSource>();

    [Header("DebugStuff")]
    [Range(-1,1)] public float defaultMusicVolume = -1f;

    private AudioData currentMusicAudioData;

    public CurrentlyPlayingMusicInfo CurrentlyPlayingMusicInformation { get => currentlyPlayingMusicInformation; set => currentlyPlayingMusicInformation = value; }
    public List<AudioSource> MusicSources { get => musicSources; set => musicSources = value; }
    public List<AudioSource> SfxSources { get => sfxSources; set => sfxSources = value; }
    public AudioData CurrentMusicAudioData { get => currentMusicAudioData; set => currentMusicAudioData = value; }

    public class CurrentlyPlayingMusicInfo
    {
        [SerializeField] private AudioSource currentlyPlayingTrackSource = default;
        [SerializeField] private AudioData.MusicClip musicClip = default;
        public AudioSource CurrentlyPlayingTrackSource { get => currentlyPlayingTrackSource; set => currentlyPlayingTrackSource = value; }
        public AudioData.MusicClip MusicClip { get => musicClip; set => musicClip = value; }
    }

    private void Awake()
    {
        Instance = this;
    }
    // Start is called before the first frame update
    private void Start()
    {
        NewMusicObjectsSetup += SetupMusicTracks;

        MusicSources = new();
        SfxSources = new();
        SetupNewSFXObjects();

    }

    public void SetupNewSFXObjects()
    {
        //StopSFX();

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

    public void SetupNewMusicObjects(AudioData newAudioData)
    {
        StopMusic();

        foreach (Transform child in MusicHolder.transform)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < MusicSources.Count; i++)
        {
            GameObject tempObj = Instantiate(musicAudioSourcePrefab, this.transform);
            tempObj.transform.SetParent(MusicHolder.transform);
            tempObj.name = "MusicSource_" + i;
            MusicSources[i] = tempObj.GetComponent<AudioSource>();
        }

        SetupNewMusicObjectsEvent(newAudioData);
    }

    #region SFX Methods
    public int PlaySFX(AudioClip soundToPlay, bool loop = false, float volume = 1.0f)
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

    public void StopSFX(int audioIndex)
    {
        if(SfxSources[audioIndex].isPlaying)
        {
            SfxSources[audioIndex].Stop();
        }
    }

    public void StopSFX(string clipName)
    {
        for (int i = 0; i < SfxSources.Count; i++)
        {
            if (SfxSources[i].isPlaying && SfxSources[i].clip.name == clipName)
            {
                SfxSources[i].Stop();
            }
        }
    }

    public void StopSFX()
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

    #endregion

    //For a single track.
    public void PlayMusic(AudioClip soundToPlay, AudioData.MusicClip musicClip, bool loop = true, float volume = 1.0f)
    {
        if(defaultMusicVolume >= 0 && volume != 0)
        {
            volume = defaultMusicVolume;
        }

        for (int i = 0; i < MusicSources.Count; i++)
        {
            if (!MusicSources[i].isPlaying)
            {
                MusicSources[i].clip = soundToPlay;
                MusicSources[i].loop = loop;
                MusicSources[i].volume = volume;
                MusicSources[i].Play();
                if (volume > 0f)
                {
                    if(CurrentlyPlayingMusicInformation.CurrentlyPlayingTrackSource != null)
                    {
                        StartCoroutine(FadeBetweenMusicTracks(musicClip, MusicSources[i], volume));
                    }
                    else
                    {
                        CurrentlyPlayingMusicInformation.MusicClip = musicClip;
                        CurrentlyPlayingMusicInformation.CurrentlyPlayingTrackSource = MusicSources[i];
                    }
                }
                return;
            }
        }

        GameObject tempObj = Instantiate(musicAudioSourcePrefab, this.transform);
        tempObj.transform.SetParent(MusicHolder.transform);
        MusicSources.Add(tempObj.GetComponent<AudioSource>());
        tempObj.name = "MusicSource_" + MusicSources.Count;
        MusicSources[MusicSources.Count - 1].clip = soundToPlay;
        MusicSources[MusicSources.Count - 1].loop = loop;
        MusicSources[MusicSources.Count - 1].volume = volume;
        MusicSources[MusicSources.Count - 1].Play();
        if(volume > 0f)
        {
            if (CurrentlyPlayingMusicInformation.CurrentlyPlayingTrackSource != null)
            {
                StartCoroutine(FadeBetweenMusicTracks(musicClip, MusicSources[MusicSources.Count - 1], volume));
            }
            else
            {
                CurrentlyPlayingMusicInformation.MusicClip = musicClip;
                CurrentlyPlayingMusicInformation.CurrentlyPlayingTrackSource = MusicSources[MusicSources.Count - 1];
            }
        }
        Debug.LogWarning("All Music clips are playing, we are going to add another source for more clips!");
        return;
    }

    public void PlayAlreadyLoadedMusic(AudioSource audioSourceToPlay, AudioData.MusicClip musicClip, bool pausePreviouslyPlayingTrack = false, bool loop = true, float volume = 1.0f)
    {
        if (defaultMusicVolume >= 0 && volume != 0)
        {
            volume = defaultMusicVolume;
        }

        audioSourceToPlay.volume = 0f;
        audioSourceToPlay.Play();

        //Fade out other song.
        if (pausePreviouslyPlayingTrack)
        {
            StartCoroutine(FadeBetweenMusicTracks(musicClip, audioSourceToPlay, volume, false, true));
        }
        else
        {
            StartCoroutine(FadeBetweenMusicTracks(musicClip, audioSourceToPlay, volume));
        }
        
    }

    //Maybe we change this to be a method called "TransitionBetweenDuelAndBoardMusic" and basically we have something to keep track of if we are playing 'board' music or 'duel' music.
    //If this method is called it mutes the current one that's playing and turns on the opposite. I.E. if we were in a duel and just finished; call this and it forces us into board music.
    public void TransitionBetweenDuelAndBoardMusic(AudioData audioData = null, bool loop = true, float volume = 1.0f)
    {
        if (defaultMusicVolume >= 0 && volume != 0)
        {
            volume = defaultMusicVolume;
        }

        if(CurrentMusicAudioData.BoardAndDuelAreSynced)
        {
            //CurrentlyPlayingMusicInformation.CurrentlyPlayingTrackSource.volume = 0f;
        }
        else
        {
            if(CurrentlyPlayingMusicInformation.MusicClip.TypeOfMusic == AudioData.MusicType.Duel)
            {
               // StopMusic(CurrentlyPlayingMusicInformation.CurrentlyPlayingTrackSource);
            }
            else
            {
               // CurrentlyPlayingMusicInformation.CurrentlyPlayingTrackSource.Pause();
            }
        }
        

        if (CurrentMusicAudioData != null && audioData == null)
        {
            switch (CurrentlyPlayingMusicInformation.MusicClip.TypeOfMusic)
            {
                case AudioData.MusicType.Board:
                    {
                        foreach (AudioData.MusicClip musicClip in CurrentMusicAudioData.MusicClips)
                        {
                            if (musicClip.TypeOfMusic == AudioData.MusicType.Duel)
                            {
                                if(CurrentMusicAudioData.BoardAndDuelAreSynced)
                                {
                                    foreach (AudioSource audioSource in MusicSources)
                                    {
                                        if (audioSource.clip == musicClip.Clip)
                                        {
                                            StartCoroutine(FadeBetweenMusicTracks(musicClip, audioSource, volume));
                                            break;
                                        }
                                    }
                                }
                                else
                                {
                                    foreach (AudioSource audioSource in MusicSources)
                                    {
                                        if (audioSource.clip == musicClip.Clip)
                                        {
                                            PlayAlreadyLoadedMusic(audioSource, musicClip);
                                            break;
                                        }
                                    }
                                }
                                
                            }
                        }
                        break;
                    }
                case AudioData.MusicType.Duel:
                    {
                        foreach (AudioData.MusicClip musicClip in CurrentMusicAudioData.MusicClips)
                        {
                            if (musicClip.TypeOfMusic == AudioData.MusicType.Board)
                            {
                                foreach (AudioSource audioSource in MusicSources)
                                {
                                    if (audioSource.clip == musicClip.Clip)
                                    {
                                        if(!audioSource.isPlaying)
                                        {
                                            audioSource.UnPause();
                                        }

                                        StartCoroutine(FadeBetweenMusicTracks(musicClip, audioSource, volume));
                                        break;
                                    }
                                }
                            }
                        }
                        break;
                    }
                default:
                    {
                        break;
                    }
            }
        }
    }
    public void SetupMusicTracks(AudioData audioData)
    {
        CurrentMusicAudioData = audioData;

        //Fade out whatever the current playing track is.

        //Only 1 track so just play that one.
        if (CurrentMusicAudioData.MusicClips.Count == 1f)
        {
            PlayMusic(CurrentMusicAudioData.MusicClips[0].Clip, CurrentMusicAudioData.MusicClips[0]);
            return;
        }

        bool isFirstTrack = true;
        foreach(AudioData.MusicClip musicClip in CurrentMusicAudioData.MusicClips)
        {
            //Play all music that should be simultaneous here.
            if(CurrentMusicAudioData.BoardAndDuelAreSynced)
            {
                if(isFirstTrack)
                {
                    isFirstTrack = false;
                    PlayMusic(musicClip.Clip, musicClip, true);
                    continue;
                }
                PlayMusic(musicClip.Clip, musicClip, true, 0f);
                continue;
            }

            if(musicClip.TypeOfMusic == AudioData.MusicType.Board)
            {
                if(isFirstTrack)
                {
                    isFirstTrack = false;
                    PlayMusic(musicClip.Clip, musicClip, true);
                    continue;
                }
                PlayMusic(musicClip.Clip, musicClip, true, 0f);
                continue;
            }
            else if(musicClip.TypeOfMusic == AudioData.MusicType.Duel)
            {
                PlayMusic(musicClip.Clip, musicClip, true, 0f);
                StopMusic(MusicSources[MusicSources.Count - 1]);
                continue;
            }
        }
    }

    private IEnumerator FadeBetweenMusicTracks(AudioData.MusicClip musicClipToPlay, AudioSource sourceToPlay, float targetVolume, bool fadeBetweenSimultaneously = true, bool stopPreviousTrack = false, bool pausePreviousTrack = false, float delayBetweenTracks = 0.0f)
    {

        //Can't exceed 1 so we don't want an infinite loop.
        if (targetVolume > 1)
        {
            targetVolume = 1;
        }
        else if (targetVolume < 0)
        {
            targetVolume = 0.1f;
        }

        //Do this if we want to emulate fading between difficulties in RPRT.
        if (fadeBetweenSimultaneously)
        {
            while (CurrentlyPlayingMusicInformation.CurrentlyPlayingTrackSource.volume > 0f && sourceToPlay.volume < targetVolume)
            {
                CurrentlyPlayingMusicInformation.CurrentlyPlayingTrackSource.volume -= 0.1f;

                sourceToPlay.volume += 0.1f;

                yield return new WaitForSeconds(0.1f);
            }

            if (stopPreviousTrack)
            {
                CurrentlyPlayingMusicInformation.CurrentlyPlayingTrackSource.Stop();
            }

            if (pausePreviousTrack)
            {
                CurrentlyPlayingMusicInformation.CurrentlyPlayingTrackSource.Pause();
            }

            CurrentlyPlayingMusicInformation.MusicClip = musicClipToPlay;
            CurrentlyPlayingMusicInformation.CurrentlyPlayingTrackSource = sourceToPlay;

            yield break;
        }

        //Do this if we want to fade out the current song, have a slight delay, and then fade in the new song. Similar to time period switching in RPRT.
        while(CurrentlyPlayingMusicInformation.CurrentlyPlayingTrackSource.volume > 0f)
        {
            CurrentlyPlayingMusicInformation.CurrentlyPlayingTrackSource.volume -= 0.1f;

            yield return new WaitForSeconds(0.1f);
        }

        if(stopPreviousTrack)
        {
            CurrentlyPlayingMusicInformation.CurrentlyPlayingTrackSource.Stop();
        }

        if(pausePreviousTrack)
        {
            CurrentlyPlayingMusicInformation.CurrentlyPlayingTrackSource.Pause();
        }

        yield return new WaitForSeconds(delayBetweenTracks);

        CurrentlyPlayingMusicInformation.MusicClip = musicClipToPlay;
        CurrentlyPlayingMusicInformation.CurrentlyPlayingTrackSource = sourceToPlay;

        while(CurrentlyPlayingMusicInformation.CurrentlyPlayingTrackSource.volume < targetVolume)
        {
            CurrentlyPlayingMusicInformation.CurrentlyPlayingTrackSource.volume += 0.1f;
            yield return new WaitForSeconds(0.1f);
        }

        yield return null;
    }

    private IEnumerator FadeOutCurrentMusicTrack(AudioSource audioSource, bool stopTrackAfterFading = false)
    {
        while (audioSource.volume > 0f)
        {
            audioSource.volume -= 0.1f;

            yield return new WaitForSeconds(0.2f);
        }

        if(stopTrackAfterFading)
        {
            audioSource.Stop();
        }

        yield return null;
    }

    public IEnumerator FadeOutCurrentMusicTrackThenSetupNewMusicTracks(AudioData audioData)
    {
        if(CurrentlyPlayingMusicInformation.CurrentlyPlayingTrackSource == null)
        {
            SetupNewMusicObjects(audioData);
            yield break;
        }

        while (CurrentlyPlayingMusicInformation.CurrentlyPlayingTrackSource.volume > 0f)
        {
            CurrentlyPlayingMusicInformation.CurrentlyPlayingTrackSource.volume -= 0.1f;

            yield return new WaitForSeconds(0.1f);
        }

        CurrentlyPlayingMusicInformation.CurrentlyPlayingTrackSource.Stop();

        SetupNewMusicObjects(audioData);
        yield return null;
    }

    public void StopMusic(bool fadeOut = false)
    {
        //Do a coroutine that fades it and throws an event prolly.
        if(fadeOut && CurrentlyPlayingMusicInformation.CurrentlyPlayingTrackSource != null)
        {
            StartCoroutine(FadeOutCurrentMusicTrack(CurrentlyPlayingMusicInformation.CurrentlyPlayingTrackSource, true));
            for (int i = 0; i < MusicSources.Count; i++)
            {
                if (MusicSources[i].isPlaying && MusicSources[i] != CurrentlyPlayingMusicInformation.CurrentlyPlayingTrackSource)
                {
                    MusicSources[i].Stop();
                }
            }
            return;
        }

        for (int i = 0; i < MusicSources.Count; i++)
        {
            if (MusicSources[i].isPlaying)
            {
                MusicSources[i].Stop();
            }
        }
    }

    public void StopMusic(AudioSource audioSourceToStop, bool fadeout = false)
    {
        if(fadeout)
        {
            StartCoroutine(FadeOutCurrentMusicTrack(audioSourceToStop, true));
            return;
        }

        audioSourceToStop.Stop();
    }

    IEnumerator fadeBetweenDifficultyTracks(AudioSource currentTrackToFadeOut, AudioSource nextTrackToFadeIn, float fadeInTargetVolume)
    {
        //CurrentlyPlayingTrack = nextTrackToFadeIn;

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
    public void muteCurrentTrack()
    {
        //CurrentlyPlayingTrack.volume = 0f;
    }

    public void unmuteCurrentTrack()
    {
        //CurrentlyPlayingTrack.volume = 1f;
    }

    //Events

    public void SetupNewMusicObjectsEvent(AudioData newAudioData)
    {
        NewMusicObjectsSetup?.Invoke(newAudioData);
    }
}
