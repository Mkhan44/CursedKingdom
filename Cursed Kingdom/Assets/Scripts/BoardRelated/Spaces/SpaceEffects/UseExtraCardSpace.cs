//Code written by Mohamed Riaz Khan of BukuGames.
//All code is written by me (Above name) unless otherwise stated via comments below.
//Not authorized for use outside of the Github repository of this game developed by BukuGames.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "UseExtraCardSpaceEffect", menuName = "Space Effect Data/Use Extra Card Space", order = 0)]
public class UseExtraCardSpace : SpaceEffectData, ISpaceEffect
{

    [SerializeField] private CardType cardTypeToUse;
    [Tooltip("This is number of extra cards the player can use. 1 in this value would = 2 max cards.")]
    [SerializeField] [Range(1, 10)] private int numExtraToUse = 1;

    public CardType CardTypeToDiscard { get => cardTypeToUse; set => cardTypeToUse = value; }
    public int NumToDiscard { get => numExtraToUse; set => numExtraToUse = value; }

    //Should be something on the Player script we can increase for the turn to have max amount of cards able to be used.
    public override void EffectOfSpace(Player playerReference)
    {
        base.EffectOfSpace(playerReference);
        Debug.Log($"Landed on: {this.name} space and can use {NumToDiscard} extra {CardTypeToDiscard} card(s) this turn.");
    }
}
