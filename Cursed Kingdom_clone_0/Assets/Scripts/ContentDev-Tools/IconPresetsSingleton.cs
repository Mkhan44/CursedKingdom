//Code written by Mohamed Riaz Khan of BukuGames.
//All code is written by me (Above name) unless otherwise stated via comments below.
//Not authorized for use outside of the Github repository of this game developed by BukuGames.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IconPresetsSingleton : MonoBehaviour
{
    public static IconPresetsSingleton instance;

    [SerializeField] UserUIPreset uiPresetData;
    [SerializeField] private SpaceIconPreset spaceIconPreset;
    [SerializeField] private SupportCardTypeIconPreset supportCardTypeIconPreset;
    [SerializeField] private PlayerInfoIconPreset playerInfoIconPreset;
    [SerializeField] private bool overridePresets;

    public SpaceIconPreset SpaceIconPreset { get => spaceIconPreset; set => spaceIconPreset = value; }
    public SupportCardTypeIconPreset SupportCardTypeIconPreset { get => supportCardTypeIconPreset; set => supportCardTypeIconPreset = value; }
    public PlayerInfoIconPreset PlayerInfoIconPreset { get => playerInfoIconPreset; set => playerInfoIconPreset = value; }
    public UserUIPreset UiPresetData { get => uiPresetData; set => uiPresetData = value; }

    private void OnValidate()
    {
        if (UiPresetData != null && !overridePresets)
        {
            SpaceIconPreset = UiPresetData.SpaceIconPresetData;
            SupportCardTypeIconPreset = UiPresetData.SupportIconPresetData;
            PlayerInfoIconPreset = UiPresetData.PlayerInfoIconPresetData;
        }
        
    }
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }
    }
}
