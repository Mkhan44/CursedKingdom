//Code written by Mohamed Riaz Khan of BukuGames.
//All code is written by me (Above name) unless otherwise stated via comments below.
//Not authorized for use outside of the Github repository of this game developed by BukuGames.

using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Card : MonoBehaviour
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
    }

    public virtual void RemoveListeners()
    {
        ClickableAreaButton.onClick.RemoveAllListeners();
    }

    public virtual void AddCardUseListener(GameplayManager gameplayManager)
    {
        
    }
}
