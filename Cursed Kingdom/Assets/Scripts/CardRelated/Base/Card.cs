//Code written by Mohamed Riaz Khan of BukuGames.
//All code is written by me (Above name) unless otherwise stated via comments below.
//Not authorized for use outside of the Github repository of this game developed by BukuGames.

using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Card : MonoBehaviour , IPointerEnterHandler , IPointerExitHandler
{
    public enum CardType 
    {
        Movement,
        Support,
    }

    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private Image cardArtworkImage;
    [SerializeField] private Image cardBorderImage;
    [SerializeField] private CardType thisCardType;
    [SerializeField] private Button clickableAreaButton;

    [SerializeField] private bool hoveredOverCard;
    [SerializeField] private Vector3 originalSize;
    [SerializeField] private Vector3 hoveredSize;

    public TextMeshProUGUI TitleText { get => titleText; set => titleText = value; }
    public Image CardArtworkImage { get => cardArtworkImage; set => cardArtworkImage = value; }
    public Image CardBorderImage { get => cardBorderImage; set => cardBorderImage = value; }
    public CardType ThisCardType { get => thisCardType; }
    public Button ClickableAreaButton { get => clickableAreaButton; set => clickableAreaButton = value; }

    protected void SetCardType(CardType cardType)
    {
        //Probably shouldn't use this, and should instead use the property. We just don't want things outside of the cards to touch this value...
        thisCardType = cardType;
    }

    protected virtual void SetupCard(CardData cardData)
    {
        TitleText.text = cardData.CardTitle;
        cardArtworkImage.sprite = cardData.CardArtwork;
        originalSize = new Vector3(this.transform.localScale.x, this.transform.localScale.y, this.transform.localScale.z);
        hoveredSize = new Vector3(originalSize.x + 0.2f, originalSize.y + 0.2f, originalSize.z + 0.2f);
    }

    public virtual void RemoveListeners()
    {
        ClickableAreaButton.onClick.RemoveAllListeners();
    }

    public virtual void AddCardUseListener(GameplayManager gameplayManager)
    {
        
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        //DON'T WANT TO KEEP DOING GETCOMPONENT ON THIS!
        PlayerHandDisplayUI handExpandUI = GetComponentInParent<PlayerHandDisplayUI>();

        if(handExpandUI is not null)
        {
            if(!handExpandUI.IsExpanded)
            {
                handExpandUI.ExpandHand(ThisCardType);
            }
            else
            {
                //Grow effect for this card.
                hoveredOverCard = true;
                StartCoroutine(HoverCardEffect());
            }
        }
    }

    public virtual void OnPointerExit(PointerEventData eventData)
    {
        hoveredOverCard = false;
        StartCoroutine(LeaveCardEffect());
    }

    //Grow/Shrink card effects.

    public IEnumerator HoverCardEffect()
    {
        float timePassed = 0.0f;
        float rate = 0.0f;

        rate = 1.0f / 2.0f * 3.0f;
        while (hoveredOverCard)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, hoveredSize, timePassed / rate);
            timePassed += Time.unscaledDeltaTime;
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
            timePassed += Time.unscaledDeltaTime;
            yield return null;
        }
        yield return null;
    }
}
