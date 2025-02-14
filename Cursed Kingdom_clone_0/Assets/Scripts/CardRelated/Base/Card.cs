//Code written by Mohamed Riaz Khan of BukuGames.
//All code is written by me (Above name) unless otherwise stated via comments below.
//Not authorized for use outside of the Github repository of this game developed by BukuGames.

using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Card : MonoBehaviour , IPointerClickHandler
{
    //Events

    public event Action<SupportCard> CardEffectWasNegated;

    protected const string ISHIDDEN = "IsHidden";

    //Animation string parameters
    protected const string ISCURSED = "isCursed";
    protected const string ISBOOSTED = "isBoosted";
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

    [SerializeField] private GameObject cardBackObject;
    [SerializeField] private CardType thisCardType;
    [SerializeField] private Button clickableAreaButton;
    [SerializeField] private GameplayManager gameplayManager;
    [SerializeField] private Image backgroundSelectedGlow;
    [SerializeField] private Animator parentAnimator;
    [SerializeField] private Animator textureAnimator;

    [SerializeField] private bool cardIsActiveHovered;
    [SerializeField] private bool selectedForDiscard;
    [SerializeField] private bool selectedForUse;
    [SerializeField] private Vector3 originalSize;
    [SerializeField] private Vector3 hoveredSize;
    [SerializeField] private Color originalBackgroundGlowColor;

    public TextMeshProUGUI TitleText { get => titleText; set => titleText = value; }
    public Image CardArtworkImage { get => cardArtworkImage; set => cardArtworkImage = value; }
    public Image CardBorderImage { get => cardBorderImage; set => cardBorderImage = value; }
    public GameObject CardBackObject { get => cardBackObject; set => cardBackObject = value; }
    public CardType ThisCardType { get => thisCardType; }
    public Button ClickableAreaButton { get => clickableAreaButton; set => clickableAreaButton = value; }
    public Image BackgroundSelectedGlow { get => backgroundSelectedGlow; set => backgroundSelectedGlow = value; }
    public Animator ParentAnimator { get => parentAnimator; set => parentAnimator = value; }
    public Animator TextureAnimator { get => textureAnimator; set => textureAnimator = value; }
    public bool CardIsActiveHovered { get => cardIsActiveHovered; set => cardIsActiveHovered = value; }
    public bool SelectedForDiscard { get => selectedForDiscard; set => selectedForDiscard = value; }
    public bool SelectedForUse { get => selectedForUse; set => selectedForUse = value; }
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

    public virtual void SelectForUse()
    {
        int numSelected;
        int maxNumPlayerCanSelect;

        CheckAmountOfCardsPlayerHasSelected(out numSelected, out maxNumPlayerCanSelect);

        if (numSelected == maxNumPlayerCanSelect)
        {
            DialogueBoxPopup.instance.ActivatePopupWithJustText("You have already selected the maximum amount of cards that can be used.", 1.5f);
            return;
        }

        SelectedForUse = true;
        Color selectedColor = OriginalBackgroundGlowColor;
        selectedColor.a = 150;
        BackgroundSelectedGlow.color = selectedColor;
        GameplayManager.GetCurrentPlayer().SelectMultipleCardsToUse();
    }

    public virtual void DeselectForUse()
    {
        int numSelected;
        int maxNumPlayerCanSelect;

        SelectedForUse = false;
        BackgroundSelectedGlow.color = originalBackgroundGlowColor;

        CheckAmountOfCardsPlayerHasSelected(out numSelected, out maxNumPlayerCanSelect);
        if (numSelected < 1)
        {
            GameplayManager.GetCurrentPlayer().MovementCardSelectedForUse = false;
            GameplayManager.GetCurrentPlayer().SupportCardSelectedForUse = false;
        }
    }
    protected void CheckAmountOfCardsPlayerHasSelected(out int numSelected, out int maxNumPlayerCanSelect)
    {
        //Check if Player has selected max they are allowed to, if they can't then don't select and make a popup.
        numSelected = 0;
        maxNumPlayerCanSelect = GameplayManager.GetCurrentPlayer().MaxMovementCardsToUse + GameplayManager.GetCurrentPlayer().ExtraMovementCardUses;

        foreach (Card card in GameplayManager.GetCurrentPlayer().CardsInhand)
        {
            if (card.SelectedForUse)
            {
                numSelected += 1;
            }
        }
    }


    public virtual void SelectForDiscard()
    {
        selectedForDiscard = true;
        Color selectedColor = OriginalBackgroundGlowColor;
        selectedColor.a = 150;
        BackgroundSelectedGlow.color = selectedColor;
        int indexOfCurrentPlayer = GameplayManager.Players.IndexOf(GameplayManager.playerCharacter.GetComponent<Player>());
        GameplayManager.GetCurrentPlayer().SelectCardForDiscard();
    }

    /// <summary>
    /// When the player has to discard x amount of cards this is what we are deselecting for.
    /// </summary>
    public virtual void DeselectForDiscard()
    {
        int indexOfCurrentPlayer = GameplayManager.Players.IndexOf(GameplayManager.playerCharacter.GetComponent<Player>());
        if (GameplayManager.GetCurrentPlayer().CardsLeftToDiscard > 0)
        {
            GameplayManager.GetCurrentPlayer().CardsLeftToDiscard += 1;
        }
        
        selectedForDiscard = false;
        BackgroundSelectedGlow.color = originalBackgroundGlowColor;
    }

    /// <summary>
    /// When the game is discarding this card to the discard pile this is what should be called.
    /// </summary>
    public virtual void DeselectDueToDiscardingCardToDiscardPile()
    {
        CardIsActiveHovered = false;
        selectedForDiscard = false;
        BackgroundSelectedGlow.color = originalBackgroundGlowColor;
        if(gameObject.activeInHierarchy)
        {
            StartCoroutine(LeaveCardEffect());
        }

    }

    public virtual void DeselectCard()
    {
        CardIsActiveHovered = false;
        if(selectedForDiscard)
        {
            DeselectForDiscard();
        }
        
        if(selectedForUse)
        {
            DeselectForUse();
        }
        
        if(gameObject.activeInHierarchy)
        {
            StartCoroutine(LeaveCardEffect());

        }
    }

    //Prolly shouldn't do this here since we can't guarantee that this card will be in the same parent as other cards.
    public virtual void DeselectOtherSelectedCards()
    {
        foreach (Transform child in this.transform.parent)
        {
            Card theCard = child.GetComponent<Card>();

            if (theCard is not null && theCard != this)
            {
                if (theCard.CardIsActiveHovered)
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
        while (CardIsActiveHovered)
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
        while (!CardIsActiveHovered)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, OriginalSize, timePassed / rate);
            timePassed += Time.unscaledDeltaTime;
            yield return null;
        }
        yield return null;
    }

    #region Status Effects/Buffs
    public virtual void ActivateCurseEffect()
    {
        
    }

    public virtual void DeactivateCurseEffect()
    {

    }

    public virtual void ActivateBoostedEffect()
    {

    }

    public virtual void DeactivateBoostedEffect()
    {

    }

    public virtual void TurnOffAllEffects()
    {

    }
    #endregion

    public virtual void OnPointerClick(PointerEventData eventData)
    {
       
    }



}
