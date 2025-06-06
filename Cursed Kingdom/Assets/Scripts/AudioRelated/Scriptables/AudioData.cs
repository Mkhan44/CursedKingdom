//Code written by Mohamed Riaz Khan of BukuGames.
//All code is written by me (Above name) unless otherwise stated via comments below.
//Not authorized for use outside of the Github repository of this game developed by BukuGames.
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AudioData", menuName = "Audio/Audio Data", order = 0)]
public class AudioData : ScriptableObject
{
    public enum MusicType
    {
        Board,
        Duel,
        Menu,
        Misc,
    }

    public enum SFXType
    {
        Misc,
    }

    [Serializable]
    public class MusicClip
    {
        public MusicType TypeOfMusic;
        [Range(0, 5)] public int levelRequiredForThisTrackToPlay = 0; 
        public AudioClip Clip;
    }

    [Serializable]
    public class SFXClip
    {
        public SFXType TypeOfSFX;
        public AudioClip Clip;
    }

    [SerializeField] private List<MusicClip> musicClips;
    [SerializeField] private List<SFXClip> sfxClips;
    [Tooltip("This should be true if the board music and duel music have a seamless transition between eachother. Only applicable for music.")]
    [SerializeField] private bool boardAndDuelAreSynced;
    [SerializeField] private bool menuMusicTracksAreSynced;

    public List<MusicClip> MusicClips { get => musicClips; set => musicClips = value; }
    public List<SFXClip> SfxClips { get => sfxClips; set => sfxClips = value; }
    public bool BoardAndDuelAreSynced { get => boardAndDuelAreSynced; set => boardAndDuelAreSynced = value; }
    public bool MenuMusicTracksAreSynced { get => menuMusicTracksAreSynced; set => menuMusicTracksAreSynced = value; }

    private void OnValidate()
    {
        if(MusicClips.Count > 0 && SfxClips.Count > 0)
        {
            Debug.LogWarning($"You have both Music and SFX clips on this data. Usually you should only have EITHER music or SFX and not both.");
        }
    }
}
