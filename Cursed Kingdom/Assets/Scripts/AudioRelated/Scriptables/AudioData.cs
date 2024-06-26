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

    public List<MusicClip> MusicClips { get => musicClips; set => musicClips = value; }
    public List<SFXClip> SfxClips { get => sfxClips; set => sfxClips = value; }

    private void OnValidate()
    {
        if(MusicClips.Count > 0 && SfxClips.Count > 0)
        {
            Debug.LogWarning($"You have both Music and SFX clips on this data. Usually you should only have EITHER music or SFX and not both.");
        }
    }
}
