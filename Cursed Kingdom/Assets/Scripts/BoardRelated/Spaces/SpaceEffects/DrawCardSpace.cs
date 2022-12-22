//Code written by Mohamed Riaz Khan of BukuGames.
//All code is written by me (Above name) unless otherwise stated via comments below.
//Not authorized for use outside of the Github repository of this game developed by BukuGames.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DrawCardSpaceEffect", menuName = "Space Effect Data/Draw Card Space", order = 0)]
public class DrawCardSpace : SpaceEffectData, ISpaceEffect
{
    [SerializeField] private CardType cardTypeToDraw;
    [SerializeField] [Range(1,10)] private int numToDraw = 1;
    [Tooltip("If the user can pick any card type, this should be true. This overrides the cardTypeToDraw field.")]
    [SerializeField] private bool canBeEitherCard;

    public CardType CardTypeToDraw { get => cardTypeToDraw; set => cardTypeToDraw = value; }
    public int NumToDraw { get => numToDraw; set => numToDraw = value; }
    public bool CanBeEitherCard { get => canBeEitherCard; set => canBeEitherCard = value; }

    public void EffectOfSpace(Player playerReference)
    {
        Debug.Log($"Landed on: {this.name} space and should draw: {NumToDraw} {CardTypeToDraw} card(s)");
    }
}
