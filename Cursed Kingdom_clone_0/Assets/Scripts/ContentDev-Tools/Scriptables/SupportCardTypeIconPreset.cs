//Code written by Mohamed Riaz Khan of BukuGames.
//All code is written by me (Above name) unless otherwise stated via comments below.
//Not authorized for use outside of the Github repository of this game developed by BukuGames.

using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SupportCardIconPreset", menuName = "Content Tools/Support Card Type Icon Preset data", order = 0)]
public class SupportCardTypeIconPreset : ScriptableObject
{
    [Serializable]
    public class SupportCardIconElement
    {
        public enum SupportIconType
        {
            movement,
            duel,
            special,
        }

        [SerializeField] private SupportIconType supportIconType;
        [SerializeField] private Sprite sprite;
        [ColorUsage(true, true)][SerializeField] private Color color;


        public SupportIconType SupportIconType1 { get => supportIconType; set => supportIconType = value; }
        public Sprite Sprite { get => sprite; set => sprite = value; }
        public Color Color { get => color; set => color = value; }
    }

    [SerializeField] List<SupportCardIconElement> supportCardIcons = new List<SupportCardIconElement>();

    [SerializeField] private Sprite movementSprite;
    [SerializeField] private Sprite duelSprite;
    [SerializeField] private Sprite specialSprite;
    public Sprite MovementSprite { get => movementSprite;}
    public Sprite DuelSprite { get => duelSprite;}
    public Sprite SpecialSprite { get => specialSprite;}
    public List<SupportCardIconElement> SupportCardIcons { get => supportCardIcons; set => supportCardIcons = value; }

    private void OnValidate()
    {
        List<SupportCardIconElement.SupportIconType> duplicateTypeCheck = new();
        foreach (SupportCardIconElement element in supportCardIcons)
        {
            if (!duplicateTypeCheck.Contains(element.SupportIconType1))
            {
                duplicateTypeCheck.Add(element.SupportIconType1);
            }
            else
            {
                Debug.LogWarning($"Hey you have more than 1 of the same type of icon type in your preset! This will cause issues.");
                break;
            }
        }
    }
}
