//Code written by Mohamed Riaz Khan of BukuGames.
//All code is written by me (Above name) unless otherwise stated via comments below.
//Not authorized for use outside of the Github repository of this game developed by BukuGames.

using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Card : MonoBehaviour , IPointerClickHandler
{
    protected const string hidden = "IsHidden";
    public enum CardType 
    {
        Movement,
        Support,
        Both,
        None,
    }

    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private Image cardArtworkImage;
    [SerializeField] private Image cardBorderImage;
    [SerializeField] private CardType thisCardType;
    [SerializeField] private Button clickableAreaButton;
    [SerializeField] private GameplayManager gameplayManager;
    [SerializeField] private Image backgroundSelectedGlow;
    [SerializeField] private Animator parentAnimator;

    [SerializeField] private bool cardIsSelected;
    [SerializeField] private bool selectedForDiscard;
    [SerializeField] private Vector3 originalSize;
    [SerializeField] private Vector3 hoveredSize;
    [SerializeField] private Color originalBackgroundGlowColor;

    public TextMeshProUGUI TitleText { get => titleText; set => titleText = value; }
    public Image CardArtworkImage { get => cardArtworkImage; set => cardArtworkImage = value; }
    public Image CardBorderImage { get => cardBorderImage; set => cardBorderImage = value; }
    public CardType ThisCardType { get => thisCardType; }
    public Button ClickableAreaButton { get => clickableAreaButton; set => clickableAreaButton = value; }
    public Image BackgroundSelectedGlow { get => backgroundSelectedGlow; set => backgroundSelectedGlow = value; }
    public Animator ParentAnimator { get => parentAnimator; set => parentAnimator = value; }
    public bool CardIsSelected { get => cardIsSelected; set => cardIsSelected = value; }
    public bool SelectedForDiscard { get => selectedForDiscard; set => selectedForDiscard = value; }
    public GameplayManager GameplayManager { get => gameplayManager; set => gameplayManager = value; }
    public Vector3 OriginalSize { get => originalSize; set => originalSize = value; }
    public Vector3 HoveredSize { get => hoveredSize; set => hoveredSize = value; }
    public Color OriginalBackgroundGlowColor { get => originalBackgroundGlowColor; set => originalBackgroundGlowColor = value; }

    protected void SetCardType(CardType cardType)
    {
        //Probably shouldn't use this, and should instead use the property. We just don't want things outside of the cards to touch this value...
        thisCardType = cardType;
    }

    protected virtual void SetupCard(CardData cardData)
    {
        TitleText.text = cardData.CardTitle;
        cardArtworkImage.sprite = cardData.CardArtwork;
        OriginalSize = new Vector3(this.transform.localScale.x, this.transform.localScale.y, this.transform.localScale.z);
        HoveredSize = new Vector3(OriginalSize.x + 0.2f, OriginalSize.y + 0.2f, OriginalSize.z + 0.2f);
        OriginalBackgroundGlowColor = backgroundSelectedGlow.color;
    }

    public virtual void RemoveListeners()
    {
        ClickableAreaButton.onClick.RemoveAllListeners();
    }

    public virtual void AddCardUseListener(GameplayManager gameplayManager)
    {
        
    }

    public virtual void SelectForDiscard()
    {
        selectedForDiscard = true;
        Color selectedColor = OriginalBackgroundGlowColor;
        selectedColor.a = 150;
        BackgroundSelectedGlow.color = selectedColor;
        int indexOfCurrentPlayer = GameplayManager.Players.IndexOf(GameplayManager.playerCharacter.GetComponent<Player>());
        GameplayManager.Players[indexOfCurrentPlayer].SelectCardToDiscard();
    }

    public virtual void DeselectForDiscard()
    {
        int indexOfCurrentPlayer = GameplayManager.Players.IndexOf(GameplayManager.playerCharacter.GetComponent<Player>());
        if (GameplayManager.Players[indexOfCurrentPlayer].CardsLeftToDiscard > 0)
        {
            GameplayManager.Players[indexOfCurrentPlayer].CardsLeftToDiscard += 1;
        }
        
        selectedForDiscard = false;
        BackgroundSelectedGlow.color = originalBackgroundGlowColor;
    }

    public virtual void DeselectCard()
    {
        CardIsSelected = false;
        DeselectForDiscard();
        StartCoroutine(LeaveCardEffect());
    }

    //Prolly shouldn't do this here since we can't guarantee that this card will be in the same parent as other cards.
    public virtual void DeselectOtherSelectedCards()
    {
        foreach (Transform child in this.transform.parent)
        {
            Card theCard = child.GetComponent<Card>();

            if (theCard is not null && theCard != this)
            {
                if (theCard.CardIsSelected)
                {
                    theCard.DeselectCard();
                    break;
                }
            }
        }
    }

    //Grow/Shrink card effects.

    public virtual IEnumerator HoverCardEffect()
    {
        float timePassed = 0.0f;
        float rate = 0.0f;

        rate = 1.0f / 2.0f * 3.0f;
        while (CardIsSelected)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, HoveredSize, timePassed / rate);
            timePassed += Time.unscaledDeltaTime;
            yield return null;
        }
        yield return null;
    }

    public virtual IEnumerator LeaveCardEffect()
    {
        float timePassed = 0.0f;
        float rate = 0.0f;

        rate = 1.0f / 2.0f * 3.0f;
        while (!CardIsSelected)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, OriginalSize, timePassed / rate);
            timePassed += Time.unscaledDeltaTime;
            yield return null;
        }
        yield return null;
    }

    public virtual void OnPointerClick(PointerEventData eventData)
    {
       
    }



}
