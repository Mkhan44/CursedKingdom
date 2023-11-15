//Code written by Mohamed Riaz Khan of BukuGames.
//All code is written by me (Above name) unless otherwise stated via comments below.
//Not authorized for use outside of the Github repository of this game developed by BukuGames.

using UnityEngine;

[CreateAssetMenu(fileName = "User UI Preset", menuName = "Content Tools/User UI Preset data", order = 0)]
public class UserUIPreset : ScriptableObject
{
    [SerializeField] private SpaceIconPreset spaceIconPresetData;
    [SerializeField] private SupportCardTypeIconPreset supportIconPresetData;

    public SpaceIconPreset SpaceIconPresetData { get => spaceIconPresetData; set => spaceIconPresetData = value; }
    public SupportCardTypeIconPreset SupportIconPresetData { get => supportIconPresetData; set => supportIconPresetData = value; }
}
