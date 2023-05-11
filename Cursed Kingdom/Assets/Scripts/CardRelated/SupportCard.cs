//Code written by Mohamed Riaz Khan of BukuGames.
//All code is written by me (Above name) unless otherwise stated via comments below.
//Not authorized for use outside of the Github repository of this game developed by BukuGames.

using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SupportCard : Card
{
    //References
    [SerializeField] private TextMeshProUGUI descriptionText;


    //Data
    [SerializeField] private SupportCardData supportCardData;

    public TextMeshProUGUI DescriptionText { get => descriptionText; set => descriptionText = value; }
    public SupportCardData SupportCardData { get => supportCardData; set => supportCardData = value; }


    private void Start()
    {
        SetCardType(CardType.Support);
    }

    public void CardDataSetup(SupportCardData supportCardData)
    {
        SetupCard(supportCardData);
    }

    public override void AddCardUseListener(GameplayManager gameplayManager)
    {
        //base.AddCardUseListener(gameplayManager);
        //ClickableAreaButton.onClick.AddListener(() => gameplayManager.Players[0].DiscardAfterUse(ThisCardType, this));
        //ClickableAreaButton.onClick.AddListener(() => gameplayManager.HandDisplayPanel.ShrinkHand());
        //ClickableAreaButton.onClick.AddListener(() => RemoveListeners());
    }

    protected override void SetupCard(CardData cardData)
    {
        base.SetupCard(cardData);
        supportCardData = cardData as SupportCardData;
        TitleText.text = supportCardData.CardTitle;
        DescriptionText.text = supportCardData.CardDescription;
        CardArtworkImage.sprite = supportCardData.CardArtwork;
    }

    public override void OnPointerClick(PointerEventData eventData)
    {
        //If card is small, search through and call 'leaveCardEffect' on all other cards in hand.
        //Once that happens, make this card big with 'hovercardeffect' and have the outline effect popup.
        //If card is big, use it and then shrink the panel.
        //DON'T WANT TO KEEP DOING GETCOMPONENT ON THIS!
        PlayerHandDisplayUI handExpandUI = GetComponentInParent<PlayerHandDisplayUI>();

        if (handExpandUI is not null)
        {
            if (!handExpandUI.IsExpanded)
            {
                handExpandUI.ExpandHand(ThisCardType);
            }
            else
            {
                DeselectOtherSelectedCards();

                if (!CardIsSelected)
                {
                    CardIsSelected = true;
                    StartCoroutine(HoverCardEffect());
                }
                else
                {
                    if (GameplayManager.ThisDeckManager.IsDiscarding)
                    {
                        GameplayManager.Players[0].DiscardCardToGetToMaxHandSize(ThisCardType, this);
                    }
                    else
                    {
                        GameplayManager.Players[0].DiscardAfterUse(ThisCardType, this);
                    }
                    
                    GameplayManager.HandDisplayPanel.ShrinkHand();
                    transform.localScale = OriginalSize;
                    CardIsSelected = false;
                }
            }
        }
    }
}

