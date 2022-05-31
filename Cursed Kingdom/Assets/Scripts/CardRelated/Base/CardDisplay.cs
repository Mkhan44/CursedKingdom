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
    public CardData testCard;
    [SerializeField] private Card newCard;

    [SerializeField] private Image cardImage;
    [SerializeField] private TextMeshProUGUI cardNameText;
    [SerializeField] private TextMeshProUGUI cardNumberText;
    protected virtual void Start()
    {
        if(testCard is MovementCardData)
        {
            MovementCardData tempMovementCardData = (MovementCardData)testCard;
            newCard = new Card(tempMovementCardData.MovementValue, tempMovementCardData.CardName, tempMovementCardData.CardSprite);
        }
        //For now we only have 2 so this will be a support. However, we may need to make this scalable just in case for future use.
        else if(testCard is SupportCardData)
        {
            SupportCardData tempSupportCardData = (SupportCardData)testCard;
            newCard = new Card(0, tempSupportCardData.CardName, tempSupportCardData.CardSprite);
        }    
        else
        {
            return;
        }

        if(newCard.CardNumber > 0)
        {
            cardNumberText.text = newCard.CardNumber.ToString();
        }
        else
        {
            cardNumberText.gameObject.SetActive(false);
        }
        
        cardNameText.text = newCard.CardName.ToString();
        cardImage.sprite = newCard.CardSprite;
    }
}
