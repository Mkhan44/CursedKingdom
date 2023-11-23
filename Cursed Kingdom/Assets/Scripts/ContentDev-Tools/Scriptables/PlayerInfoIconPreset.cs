//Code written by Mohamed Riaz Khan of BukuGames.
//All code is written by me (Above name) unless otherwise stated via comments below.
//Not authorized for use outside of the Github repository of this game developed by BukuGames.

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerInfoIconPreset", menuName = "Content Tools/Player Info Icon Preset data", order = 0)]
public class PlayerInfoIconPreset : ScriptableObject
{
    [Serializable]
    public class InfoIconElement
    {
        public enum InfoIconType
        {
            poisonIcon,
            curseicon,
            movementCardIcon,
            supportCardIcon,
            heartIcon,

        }

        [SerializeField] private InfoIconType infoIconType;
        [SerializeField] private Sprite activeSprite;
        [SerializeField] private Sprite inactiveSprite;
        [ColorUsage(true, true)][SerializeField] private Color activeColor;
        [ColorUsage(true, true)][SerializeField] private Color inactiveColor;


        public InfoIconType InfoIconType1 { get => infoIconType; set => infoIconType = value; }
        public Sprite ActiveSprite { get => activeSprite; set => activeSprite = value; }
        public Sprite InactiveSprite { get => inactiveSprite; set => inactiveSprite = value; }
        public Color ActiveColor { get => activeColor; set => activeColor = value; }
        public Color InactiveColor { get => inactiveColor; set => inactiveColor = value; }
    }
    [SerializeField] private List<InfoIconElement> infoIconElements = new List<InfoIconElement>();
    public List<InfoIconElement> InfoIconElements { get => infoIconElements; set => infoIconElements = value; }
    private void OnValidate()
    {
        List<InfoIconElement.InfoIconType> duplicateTypeCheck = new();
        foreach (InfoIconElement element in infoIconElements)
        {
            if (!duplicateTypeCheck.Contains(element.InfoIconType1))
            {
                duplicateTypeCheck.Add(element.InfoIconType1);
            }
            else
            {
                Debug.LogWarning($"Hey you have more than 1 of the same type of info icon type in your preset! This will cause issues.");
                break;
            }
        }
    }
}
