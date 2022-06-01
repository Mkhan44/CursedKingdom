//Code written by Mohamed Riaz Khan of BukuGames.
//All code is written by me (Above name) unless otherwise stated via comments below.
//Not authorized for use outside of the Github repository of this game developed by BukuGames.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class CardDisplay : MonoBehaviour , IPointerEnterHandler , IPointerDownHandler , IPointerExitHandler
{
    //Test.
    public CardData testCard;
    [SerializeField] private Card newCard;

    [SerializeField] private Image cardImage;
    [SerializeField] private TextMeshProUGUI cardNameText;
    [SerializeField] private TextMeshProUGUI cardNumberText;

    //Test Interactions.
    [SerializeField] private bool hoveredOverCard;

    [SerializeField] private Vector3 originalSize;
    [SerializeField] private Vector3 hoveredSized;

    public virtual void OnPointerDown(PointerEventData eventData)
    {
        
    }

    public virtual void OnPointerEnter(PointerEventData eventData)
    {
        hoveredOverCard = true;
        StartCoroutine(HoverCardEffect());
    }

    public virtual void OnPointerExit(PointerEventData eventData)
    {
        hoveredOverCard = false;
        StartCoroutine(LeaveCardEffect());
    }

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

    public IEnumerator HoverCardEffect()
    {
        float timePassed = 0.0f;
        float rate = 0.0f;

        rate = 1.0f / 2.0f * 3.0f;
        while (hoveredOverCard)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, hoveredSized, timePassed / rate);
            timePassed += Time.deltaTime;
            yield return null;
        }
        yield return null;
    }

    public IEnumerator LeaveCardEffect()
    {
        float timePassed = 0.0f;
        float rate = 0.0f;

        rate = 1.0f / 2.0f * 3.0f;
        while (!hoveredOverCard)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, originalSize, timePassed / rate);
            timePassed += Time.deltaTime;
            yield return null;
        }
        yield return null;
    }
}
