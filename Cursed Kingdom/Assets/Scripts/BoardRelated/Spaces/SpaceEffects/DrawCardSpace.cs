//Code written by Mohamed Riaz Khan of BukuGames.
//All code is written by me (Above name) unless otherwise stated via comments below.
//Not authorized for use outside of the Github repository of this game developed by BukuGames.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DrawCardSpace", menuName = "Space Effect Data/Draw Card Space", order = 0)]
public class DrawCardSpace : SpaceEffectData, ISpaceEffect
{
    [SerializeField] private CardType cardTypeToDraw;
    [SerializeField] [Range(1,10)] private int numToDraw;

    public CardType CardTypeToDraw { get => cardTypeToDraw; set => cardTypeToDraw = value; }
    public int NumToDraw { get => numToDraw; set => numToDraw = value; }

    public void EffectOfSpace(Player playerReference)
    {
        Debug.Log($"Landed on: {this.name} space and should draw: {NumToDraw} {CardTypeToDraw} card(s)");
    }
}
