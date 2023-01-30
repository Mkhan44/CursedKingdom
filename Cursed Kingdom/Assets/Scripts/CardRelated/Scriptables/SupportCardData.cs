//Code written by Mohamed Riaz Khan of BukuGames.
//All code is written by me (Above name) unless otherwise stated via comments below.
//Not authorized for use outside of the Github repository of this game developed by BukuGames.

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "Support Card Data", menuName = "Card Data/Support Card Data", order = 0)]
public class SupportCardData : CardData
{
    [Tooltip("A list of effects that this support card has.")]
    [Serializable]
    public class SupportCardEffect
    {
        [SerializeField] public SupportCardEffectData supportCardEffectData;
        [HideInInspector] public string supportTypeStringHidden;
    }

    public enum SupportCardType
    {
        Movement,
        Duel,
        Special,
    }

    [SerializeField] public List<SupportCardEffect> supportCardEffects;
    [SerializeField] private SupportCardType thisSupportCardType;
    [SerializeField] private bool isElemental;
    [SerializeField] [TextArea(3,10)] private string cardDescription;


    //Need a field for specific animation that will be played when this support card is used.
    public string CardDescription { get => cardDescription; set => cardDescription = value; }
    public SupportCardType ThisSupportCardType { get => thisSupportCardType; set => thisSupportCardType = value; }
    public bool IsElemental { get => isElemental; set => isElemental = value; }
}
