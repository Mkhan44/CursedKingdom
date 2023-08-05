//Code written by Mohamed Riaz Khan of BukuGames.
//All code is written by me (Above name) unless otherwise stated via comments below.
//Not authorized for use outside of the Github repository of this game developed by BukuGames.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class MovementCard : Card
{
    //Data
    [SerializeField] private MovementCardData movementCardData;
    [SerializeField] private int movementCardValue;

    //References
    [SerializeField] private TextMeshProUGUI movementValueText;

    public MovementCardData MovementCardData { get => movementCardData; set => movementCardData = value; }
    public int MovementCardValue { get => movementCardValue; set => movementCardValue = value; }


    private void Start()
    {
        SetCardType(CardType.Movement);
    }

    public void CardDataSetup(MovementCardData movementCardData)
    {
        SetupCard(movementCardData);
    }

    public override void AddCardUseListener(GameplayManager gameplayManager)
    {
        //base.AddCardUseListener(gameplayManager);
        //ClickableAreaButton.onClick.AddListener(() => gameplayManager.StartMove(MovementCardValue));
        //ClickableAreaButton.onClick.AddListener(() => gameplayManager.Players[0].DiscardAfterUse(ThisCardType, this));
        //ClickableAreaButton.onClick.AddListener(() => gameplayManager.HandDisplayPanel.ShrinkHand());
        //ClickableAreaButton.onClick.AddListener(() => RemoveListeners());
    }

    protected override void SetupCard(CardData cardData)
    {
        base.SetupCard(cardData);
        MovementCardData = cardData as MovementCardData;
        MovementCardValue = movementCardData.MovementValue;
        TitleText.text = "Movement";
        movementValueText.text = MovementCardValue.ToString();
    }

    public override void OnPointerClick(PointerEventData eventData)
    {
        //If card is small, search through and call 'leaveCardEffect' on all other cards in hand.
        //Once that happens, make this card big with 'hovercardeffect' and have the outline effect popup.
        //If card is big, use it and then shrink the panel.
        //DON'T WANT TO KEEP DOING GETCOMPONENT ON THIS!
        PlayerHandDisplayUI handExpandUI = GetComponentInParent<PlayerHandDisplayUI>();
        ParentAnimator = GetComponentInParent<Animator>();

        if (handExpandUI is not null)
        {
            int indexOfCurrentPlayer = GameplayManager.Players.IndexOf(GameplayManager.playerCharacter.GetComponent<Player>());
            if (!handExpandUI.CurrentActiveTransform.IsExpanded)
            {
                handExpandUI.ExpandHand(ThisCardType, GameplayManager.Players.IndexOf(GameplayManager.playerCharacter.GetComponent<Player>()));
                
            }
            else
            {
                if(ParentAnimator != null)
                {
                    if(ParentAnimator.GetBool(hidden))
                    {
                        return;
                    }
                }
                DeselectOtherSelectedCards();

                if (GameplayManager.Players[indexOfCurrentPlayer].CardsLeftToDiscard > 0 && !SelectedForDiscard && IsValidCardTypeToDiscard(GameplayManager.Players[indexOfCurrentPlayer]))
                {
                    SelectForDiscard();
                    return;
                }
                else if (GameplayManager.Players[indexOfCurrentPlayer].CardsLeftToDiscard > 0 && !SelectedForDiscard && !IsValidCardTypeToDiscard(GameplayManager.Players[indexOfCurrentPlayer]))
                {
                    Debug.LogWarning("Hey! You can't select this card to discard.");
                    return;
                }

                if (SelectedForDiscard)
                {
                    DeselectForDiscard();
                    return;
                }

                if (!CardIsSelected)
                {
                    CardIsSelected = true;
                    StartCoroutine(HoverCardEffect());
                }
                else
                {
                    if (GameplayManager.ThisDeckManager.IsDiscarding)
                    {
                        GameplayManager.Players[indexOfCurrentPlayer].DiscardCardToGetToMaxHandSize(ThisCardType, this);
                    }
                    else
                    {
                        GameplayManager.StartMove(MovementCardValue);
                        GameplayManager.Players[indexOfCurrentPlayer].DiscardFromHand(ThisCardType, this);
                    }

                    GameplayManager.HandDisplayPanel.ShrinkHand();
                    transform.localScale = OriginalSize;
                    CardIsSelected = false;
                }
            }
        }
    }

    public bool IsValidCardTypeToDiscard(Player playerReference)
    {
        bool isValid = false;

        if ((playerReference.ValidCardTypesToDiscard == CardType.Movement || playerReference.ValidCardTypesToDiscard == CardType.Both) && playerReference.CardsLeftToDiscard > 0)
        {
            isValid = true;
        }
        return isValid;
    }
}
