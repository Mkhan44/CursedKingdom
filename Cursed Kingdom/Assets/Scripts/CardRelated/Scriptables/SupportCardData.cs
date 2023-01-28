//Code written by Mohamed Riaz Khan of BukuGames.
//All code is written by me (Above name) unless otherwise stated via comments below.
//Not authorized for use outside of the Github repository of this game developed by BukuGames.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "Support Card Data", menuName = "Card Data/Support Card Data", order = 0)]
public class SupportCardData : CardData
{
    [SerializeField] [TextArea(3,10)] private string cardDescription;

    //Need a field for specific animation that will be played when this support card is used.
    public string CardDescription { get => cardDescription; set => cardDescription = value; }
}
