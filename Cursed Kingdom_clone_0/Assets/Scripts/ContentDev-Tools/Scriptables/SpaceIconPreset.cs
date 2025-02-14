//Code written by Mohamed Riaz Khan of BukuGames.
//All code is written by me (Above name) unless otherwise stated via comments below.
//Not authorized for use outside of the Github repository of this game developed by BukuGames.

using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SpaceIconPreset", menuName = "Content Tools/Space Icon Preset data", order = 0)]
public class SpaceIconPreset : ScriptableObject
{
    [Serializable]
    public class SpaceIconElement
    {
        public enum SpaceIconType
        {
            drawMovementCard,
            drawSupportCard,
            poison,
            curse,
            recoverHealth,
            loseHealth,
            nonDuel,
            specialAttack,
            barricade,
            arrow,
            attack,
            conferenceRoom,
            discard,
            halveMovementCard,
            levelUp,
            useExtraCard,

        }

        [SerializeField] private SpaceIconType spaceIconType;
        [SerializeField] private Sprite sprite;
        [ColorUsage(true, true)][SerializeField] private Color color;


        public SpaceIconType SpaceIconType1 { get => spaceIconType; set => spaceIconType = value; }
        public Sprite Sprite { get => sprite; set => sprite = value; }
        public Color Color { get => color; set => color = value; }
    }
    [SerializeField] private List<SpaceIconElement> spaceIconElements = new List<SpaceIconElement>();
    public List<SpaceIconElement> SpaceIconElements { get => spaceIconElements; set => spaceIconElements = value; }
    
    private void OnValidate()
    {
        List<SpaceIconElement.SpaceIconType> duplicateTypeCheck = new();
        foreach(SpaceIconElement element in SpaceIconElements)
        {
            if(!duplicateTypeCheck.Contains(element.SpaceIconType1))
            {
                duplicateTypeCheck.Add(element.SpaceIconType1);
            }
            else
            {
                Debug.LogWarning($"Hey you have more than 1 of the same type of icon type in your preset! This will cause issues.");
                break;
            }
        }
    }
}
