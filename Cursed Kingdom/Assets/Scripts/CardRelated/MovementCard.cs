//Code written by Mohamed Riaz Khan of BukuGames.
//All code is written by me (Above name) unless otherwise stated via comments below.
//Not authorized for use outside of the Github repository of this game developed by BukuGames.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System.Net;

public class MovementCard : Card
{
    //Data
    [SerializeField] private MovementCardData movementCardData;
    [SerializeField] private int movementCardValue;
    //Used for if we need to temporarily halve or increase a movement card's value while in the player's hand due to item being used or curse, etc.
    [SerializeField] private int tempCardValue;
    [SerializeField] private int tempIncreaseValue;
    [SerializeField] private int tempDecreaseValue;

    //References
    [SerializeField] private TextMeshProUGUI movementValueText;

    public MovementCardData MovementCardData { get => movementCardData; set => movementCardData = value; }
    public int MovementCardValue { get => movementCardValue; set => movementCardValue = value; }
    public int TempCardValue { get => tempCardValue; set => tempCardValue = value; }
    public int TempIncreaseValue { get => tempIncreaseValue; set => tempIncreaseValue = value; }
    public int TempDecreaseValue { get => tempDecreaseValue; set => tempDecreaseValue = value; }

    private void Start()
    {
        SetCardType(CardType.Movement);
    }

    public void CardDataSetup(MovementCardData movementCardData)
    {
        SetupCard(movementCardData);
    }

    protected override void SetupCard(CardData cardData)
    {
        base.SetupCard(cardData);
        MovementCardData = cardData as MovementCardData;
        MovementCardValue = MovementCardData.MovementValue;
        TempCardValue = 0;
        TitleText.text = "Movement";
        movementValueText.text = MovementCardValue.ToString();
    }

    public void ManipulateMovementValue(bool halfTheValue = false, bool increaseTheValue = false, int valueToIncreaseBy = 0)
    {
        TempIncreaseValue += valueToIncreaseBy;

        if (halfTheValue && increaseTheValue)
        {
            Debug.LogWarning("You're trying to both half and increase the value of the card!! We're not gonna change the value.");
            return;
        }

        if(halfTheValue)
        {
            //Stack with Curse.
            if(TempCardValue != 0)
            {
                float tempNum = (float)TempCardValue;
                TempCardValue = Mathf.CeilToInt(tempNum / 2.0f);
            }
            else
            {
                float tempNum = (float)MovementCardValue;
                TempCardValue = Mathf.CeilToInt(tempNum / 2.0f);
            }
            movementValueText.text = TempCardValue.ToString();
            
            return;
        }

        if(increaseTheValue)
        {
            if(TempCardValue == 0)
            {
                TempCardValue = MovementCardValue;
            }
            TempCardValue += valueToIncreaseBy;
            movementValueText.text = TempCardValue.ToString();
            return;
        }
    }

    public void ResetMovementValue()
    {
        movementValueText.text = MovementCardValue.ToString();
        TempIncreaseValue = 0;
        TempCardValue = 0;
    }

    public void RevertBoostedCardValue()
    {
        if(TempIncreaseValue == 0)
        {
            return;
        }

        int revertedValue = TempCardValue - TempIncreaseValue;
        if (revertedValue > 0)
        {
            TempCardValue = revertedValue;
            TempIncreaseValue = 0;
        }
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
            Player thePlayer = GameplayManager.Players[indexOfCurrentPlayer];
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

                if (thePlayer.CardsLeftToDiscard > 0 && !SelectedForDiscard && IsValidCardTypeToDiscard(thePlayer))
                {
                    SelectForDiscard();
                    return;
                }
                else if (thePlayer.CardsLeftToDiscard > 0 && !SelectedForDiscard && !IsValidCardTypeToDiscard(thePlayer))
                {
                    Debug.LogWarning("Hey! You can't select this card to discard.");
                    return;
                }

                if (SelectedForDiscard)
                {
                    DeselectForDiscard();
                    return;
                }

                if (GameplayManager.Players[indexOfCurrentPlayer].SupportCardSelectedForUse)
                {
                    DialogueBoxPopup.instance.ActivatePopupWithJustText("You're currently selecting support cards. Deselect all support cards to select a movement card.", 2.0f);
                    return;
                }

                int maxNumPlayerCanSelect = thePlayer.MaxMovementCardsToUse + thePlayer.ExtraMovementCardUses;

                if (maxNumPlayerCanSelect > 1 && thePlayer.NumMovementCardsUsedThisTurn <= 1 && !SelectedForUse)
                {
                    SelectForUse();
                    return;
                }

                if (SelectedForUse)
                {
                    DeselectForUse();
                    return;
                }

                if (!CardIsActiveHovered)
                {
                    CardIsActiveHovered = true;
                    StartCoroutine(HoverCardEffect());
                }
                else
                {
                    AttemptToMove(thePlayer);
                }
            }
        }
    }

    public void AttemptToMove(Player thePlayer)
    {
        if (TempCardValue > 0)
        {
            if (thePlayer.CanUseMovementCard())
            {
                thePlayer.UseMovementCard();
                GameplayManager.StartMove(TempCardValue);
                ResetMovementValue();
                if (!thePlayer.IsCursed)
                {
                    DeactivateCurseEffect();
                }
            }
            else
            {
                DialogueBoxPopup.instance.ActivatePopupWithJustText($"You can't use anymore movement cards this turn.", 2.0f);
                GameplayManager.HandDisplayPanel.ShrinkHand();
                transform.localScale = OriginalSize;
                CardIsActiveHovered = false;
                return;
            }

        }
        else
        {
            if (thePlayer.CanUseMovementCard())
            {
                thePlayer.UseMovementCard();
                GameplayManager.StartMove(MovementCardValue);
            }
            else
            {
                DialogueBoxPopup.instance.ActivatePopupWithJustText($"You can't use anymore movement cards this turn.", 2.0f);
                GameplayManager.HandDisplayPanel.ShrinkHand();
                transform.localScale = OriginalSize;
                CardIsActiveHovered = false;
                return;
            }
        }

        thePlayer.DiscardFromHand(ThisCardType, this);

        GameplayManager.HandDisplayPanel.ShrinkHand();
        transform.localScale = OriginalSize;
        CardIsActiveHovered = false;
    }

    public override void SelectForUse()
    {
        base.SelectForUse();
        int indexOfCurrentPlayer;
        int numSelected;
        int maxNumPlayerCanSelect;

        CheckAmountOfCardsPlayerHasSelected(out indexOfCurrentPlayer, out numSelected, out maxNumPlayerCanSelect);
        GameplayManager.Players[indexOfCurrentPlayer].MovementCardSelectedForUse = true;
    }
    public override void DeselectForUse()
    {
        base.DeselectForUse();
        int indexOfCurrentPlayer;
        int numSelected;
        int maxNumPlayerCanSelect;

        CheckAmountOfCardsPlayerHasSelected(out indexOfCurrentPlayer, out numSelected, out maxNumPlayerCanSelect);

        if (GameplayManager.Players[indexOfCurrentPlayer].ExtraMovementCardUses > 0)
        {
            if (!GameplayManager.UseSelectedCardsPanel.activeInHierarchy)
            {
                GameplayManager.UseSelectedCardsPanel.SetActive(true);
            }
            GameplayManager.UseSelectedCardsText.text = $"Selected cards: {numSelected}/{maxNumPlayerCanSelect}";
            GameplayManager.UseSelectedCardsButton.onClick.RemoveAllListeners();
            GameplayManager.UseSelectedCardsButton.onClick.AddListener(GameplayManager.Players[indexOfCurrentPlayer].UseMultipleCards);
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


    #region Status Effects
    public override void ActivateCurseEffect()
    {
        TextureAnimator.SetBool(isCursed, true);
    }

    public override void DeactivateCurseEffect()
    {
        TextureAnimator.SetBool(isCursed, false);
    }

    #endregion
}
