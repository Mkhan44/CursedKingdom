//Code written by Mohamed Riaz Khan of BukuGames.
//All code is written by me (Above name) unless otherwise stated via comments below.
//Not authorized for use outside of the Github repository of this game developed by BukuGames.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Draw Card", menuName = "Space Effect Data/Draw Card", order = 0)]
public class DrawCardSpace : SpaceEffectData, ISpaceEffect
{
    public enum CardType
    {
        MovementCard,
        SupportCard,
        Both,
    }

    [SerializeField] private CardType cardTypeToDraw;
    [SerializeField] [Range(0,10)] private int numToDraw;

    public void EffectOfSpace(Player playerReference, bool afterDuel = false, bool startOfTurn = false)
    {
        throw new System.NotImplementedException();
    }
}
