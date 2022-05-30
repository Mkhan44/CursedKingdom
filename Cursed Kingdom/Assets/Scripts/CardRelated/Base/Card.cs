//Code written by Mohamed Riaz Khan of BukuGames.
//All code is written by me (Above name) unless otherwise stated via comments below.
//Not authorized for use outside of the Github repository of this game developed by BukuGames.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Card
{
    [SerializeField] int cardNumber;
    [SerializeField] string cardName;
    [SerializeField] Sprite cardSprite;
  
    public int CardNumber { get => cardNumber; set => cardNumber = value; }
    public string CardName { get => cardName; set => cardName = value; }
    public Sprite CardSprite { get => cardSprite; set => cardSprite = value; }
   

    public Card(int cardNumber = 0, string cardName = null, Image cardImage = null, Sprite cardSprite = null)
    {
        CardNumber = cardNumber;
        CardName = cardName;
        CardSprite = cardSprite;
    }
}
