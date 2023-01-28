//Code written by Mohamed Riaz Khan of BukuGames.
//All code is written by me (Above name) unless otherwise stated via comments below.
//Not authorized for use outside of the Github repository of this game developed by BukuGames.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SupportCardEffectData : ScriptableObject , ISupportEffect
{
    public enum SupportCardType
    {
        Board,
        Duel,
        Special,
    }

    [SerializeField] private SupportCardType thisSupportCardType;
    [SerializeField] private bool isElemental = false;
    

    public SupportCardType ThisSupportCardType { get => thisSupportCardType; set => thisSupportCardType = value; }
    public bool IsElemental { get => isElemental; set => isElemental = value; }

    public virtual void EffectOfCard(Player playerReference, Card cardPlayed = null)
    {

    }
}
