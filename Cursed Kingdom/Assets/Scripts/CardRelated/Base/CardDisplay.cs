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
    [SerializeField] private Card newCard;

    [SerializeField] private Image cardImage;
    [SerializeField] private TextMeshProUGUI cardNameText;
    [SerializeField] private TextMeshProUGUI cardNumberText;
    protected virtual void Start()
    {
        newCard = new Card(testCard.MovementValue, testCard.CardName, testCard.CardSprite);

        cardNumberText.text = newCard.CardNumber.ToString();
        cardNameText.text = newCard.CardName.ToString();
        cardImage.sprite = newCard.CardSprite;
    }
}
