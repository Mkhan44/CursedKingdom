//Code written by Mohamed Riaz Khan of BukuGames.
//All code is written by me (Above name) unless otherwise stated via comments below.
//Not authorized for use outside of the Github repository of this game developed by BukuGames.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardDisplay : MonoBehaviour
{
    //Test.
    public MovementCardData testCard;
    [SerializeField] Card newCard;

    [SerializeField] Image cardImage;
    [SerializeField] TextMeshProUGUI cardNameText;
    [SerializeField] TextMeshProUGUI cardNumberText;
    private void Start()
    {
        newCard = new Card(testCard.DefaultCardValue);

        cardNumberText.text = newCard.CardNumber.ToString();
    }
}
