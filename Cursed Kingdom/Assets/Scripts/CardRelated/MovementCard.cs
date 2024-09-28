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
using Unity.VisualScripting;
using System;

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

    public void ManipulateMovementValue(bool divideValue = false, float divideTheValueBy = 0f, bool multiplyValue = false, float multipleTheValueBy = 0f,  bool increaseTheValue = false, int valueToIncreaseBy = 0)
    {
        TempIncreaseValue += valueToIncreaseBy;

        int typeOfChangeWeAreTryingToDo = 0;
        if(divideValue)
        {
            typeOfChangeWeAreTryingToDo += 1;
        }
        if(multiplyValue)
        {
            typeOfChangeWeAreTryingToDo += 1;
        }
        if(increaseTheValue)
        {
            typeOfChangeWeAreTryingToDo += 1;
        }

        if (typeOfChangeWeAreTryingToDo > 1)
        {
            Debug.LogWarning("You're trying to both half and increase and multiply the value of the card!! We're not gonna change the value.");
            return;
        }

        if(divideValue)
        {
            //Stack with Curse.
            if(TempCardValue != 0)
            {
                float tempNum = (float)TempCardValue;
                TempCardValue = Mathf.CeilToInt(tempNum / divideTheValueBy);
            }
            else
            {
                float tempNum = (float)MovementCardValue;
                TempCardValue = Mathf.CeilToInt(tempNum / divideTheValueBy);
            }
            movementValueText.text = TempCardValue.ToString();
            
            return;
        }

        if(multiplyValue)
        {
            //Stack with Curse.
            if(TempCardValue != 0)
            {
                float tempNum = (float)TempCardValue;
                TempCardValue = Mathf.CeilToInt(tempNum * multipleTheValueBy);
            }
            else
            {
                float tempNum = (float)MovementCardValue;
                TempCardValue = Mathf.CeilToInt(tempNum * multipleTheValueBy);
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

            if(TempCardValue > MovementCardValue)
            {
                ActivateBoostedEffect();
            }
            
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
            DeactivateBoostedEffect();
        }
    }

    public int GetCurrentCardValue()
    {
        int currentCardValue = 0;

        if(TempCardValue > 0)
        {
            currentCardValue = TempCardValue;
        }
        else
        {
            currentCardValue = MovementCardValue;
        }

        return currentCardValue;
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
            Player thePlayer = GameplayManager.GetCurrentPlayer();
            if (!handExpandUI.CurrentActiveTransform.IsExpanded)
            {
                handExpandUI.ExpandHand(ThisCardType);
            }
            else
            {
                if(ParentAnimator != null)
                {
                    if(ParentAnimator.GetBool(ISHIDDEN))
                    {
                        return;
                    }
                }
                
                DeselectOtherSelectedCards();

                //DISCARDING CARDS
                if (thePlayer.CardsLeftToDiscard > 0 && !SelectedForDiscard && IsValidCardTypeToDiscard(thePlayer))
                {
                    SelectForDiscard();
                    return;
                }
                else if (thePlayer.CardsLeftToDiscard > 0 && !SelectedForDiscard && !IsValidCardTypeToDiscard(thePlayer))
                {
                    DialogueBoxPopup.instance.ActivatePopupWithJustText("You cannot select a movement card to discard for this effect.", 2.0f);
                    return;
                }

                if (SelectedForDiscard)
                {
                    DeselectForDiscard();
                    return;
                }

                if (GameplayManager.GetCurrentPlayer().SupportCardSelectedForUse)
                {
                    DialogueBoxPopup.instance.ActivatePopupWithJustText("You're currently selecting support cards. Deselect all support cards to select a movement card.", 2.0f);
                    return;
                }
                //DISCARDING CARDS

                //MULTIPLE MOVEMENT CARDS
                int maxNumPlayerCanSelect = thePlayer.MaxMovementCardsToUse + thePlayer.ExtraMovementCardUses;

                if (maxNumPlayerCanSelect > 1 && thePlayer.NumMovementCardsUsedThisTurn <= 1 && !SelectedForUse)
                {
                    SelectForUse();
                    GameplayManager.SpacesPlayerWillLandOnParent.TurnOnDisplay(GameplayManager.GameplayPhaseStatemachineRef.gameplayMovementPhaseState.FindValidSpaces(thePlayer, thePlayer.CurrentSumOfSpacesToMove));

                    return;
                }

                if (SelectedForUse)
                {
                    DeselectForUse();
                    GameplayManager.SpacesPlayerWillLandOnParent.TurnOnDisplay(GameplayManager.GameplayPhaseStatemachineRef.gameplayMovementPhaseState.FindValidSpaces(thePlayer, thePlayer.CurrentSumOfSpacesToMove));

                    return;
                }
                //MULTIPLE MOVEMENT CARDS

                if (!CardIsActiveHovered)
                {
                    CardIsActiveHovered = true;
                    StartCoroutine(HoverCardEffect());
                    //Calculate spaces that are valid and display what space player may land on.
                    thePlayer.CurrentSumOfSpacesToMove = 0;
                    if (TempCardValue > 0)
                    {
                        thePlayer.CurrentSumOfSpacesToMove += TempCardValue;
                    }
                    else
                    {
                        thePlayer.CurrentSumOfSpacesToMove += MovementCardValue;
                    }

                    if (GameplayManager.DuelPhaseSMRef.GetCurrentState().GetType() != typeof(DuelSelectCardsToUsePhaseState))
                    {
                        GameplayManager.SpacesPlayerWillLandOnParent.TurnOnDisplay(GameplayManager.GameplayPhaseStatemachineRef.gameplayMovementPhaseState.FindValidSpaces(thePlayer, thePlayer.CurrentSumOfSpacesToMove));
                    }
                }
                else
                {
                    if(GameplayManager.DuelPhaseSMRef.GetCurrentState().GetType() != typeof(DuelSelectCardsToUsePhaseState))
                    {
                        AttemptToMove(thePlayer);
                        thePlayer.CurrentSumOfSpacesToMove = 0;
                        GameplayManager.SpacesPlayerWillLandOnParent.TurnOffDisplay();
                    }
                    //Might hafta check if the Player we are is the same as the current player being handled in the duelPhaseSM.
                    else if(GameplayManager.DuelPhaseSMRef.GetCurrentState().GetType() == typeof(DuelSelectCardsToUsePhaseState))
                    {
                        if (GameplayManager.DuelPhaseSMRef.duelSelectCardsToUsePhaseState.SelectedMovementCard)
                        {
                            GameplayManager.HandDisplayPanel.ShrinkHand();
                            transform.localScale = OriginalSize;
                            CardIsActiveHovered = false;
                            DialogueBoxPopup.instance.ActivatePopupWithJustText("You've already selected a movement card.", 2.0f);

                            StartCoroutine(WaitAfterPopupOfMovementAlreadySelectedDuel());
                            return;
                        }
                        AddCardToSelectedCardsDuel();
                    }
                }
            }
        }
    }

    public void AddCardToSelectedCardsDuel(bool wasJustDrawn = false)
    {
        List<MovementCard> movementCards = new List<MovementCard>();
        movementCards.Add(this);
        GameplayManager.DuelPhaseSMRef.duelSelectCardsToUsePhaseState.SelectMovementCard(movementCards, wasJustDrawn);
    }

    //THIS IS TEMPORARY, NEED TO REMOVE THIS AND MAKE IT BETTER.
    public IEnumerator WaitAfterPopupOfMovementAlreadySelectedDuel()
    {
        yield return new WaitForSeconds(2.0f);
        DialogueBoxPopup.instance.DeactivatePopup();
        GameplayManager.DuelPhaseSMRef.CurrentPlayerBeingHandled.PlayerInDuel.ShowHand();
    }

    public void AttemptToMove(Player thePlayer)
    {
        if (TempCardValue > 0)
        {
            if (thePlayer.CanUseMovementCard())
            {
                thePlayer.UseMovementCard();
                GameplayManager.StartMove(TempCardValue);
                if(TempCardValue > MovementCardValue)
                {
                    DeactivateBoostedEffect();
                }
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

        if(thePlayer.CardsInhand.Contains(this))
        {
            thePlayer.DiscardFromHand(ThisCardType, this);
        }
        //If this is the top card of the deck.
        else if(thePlayer.GameplayManagerRef.ThisDeckManager.MovementDeckList[0] == this)
        {
            thePlayer.GameplayManagerRef.ThisDeckManager.DiscardNextCardInDeck(CardType.Movement);
        }

        
        GameplayManager.HandDisplayPanel.ShrinkHand();
        transform.localScale = OriginalSize;
        CardIsActiveHovered = false;
    }

    public override void SelectForUse()
    {
        base.SelectForUse();
        int numSelected;
        int maxNumPlayerCanSelect;

        CheckAmountOfCardsPlayerHasSelected(out numSelected, out maxNumPlayerCanSelect);
        if(SelectedForUse)
        {
            if (TempCardValue > 0)
            {
                GameplayManager.GetCurrentPlayer().CurrentSumOfSpacesToMove += TempCardValue;
            }
            else
            {
                GameplayManager.GetCurrentPlayer().CurrentSumOfSpacesToMove += MovementCardValue;
            }
        }

        GameplayManager.GetCurrentPlayer().MovementCardSelectedForUse = true;
    }
    public override void DeselectForUse()
    {
        base.DeselectForUse();
        int numSelected;
        int maxNumPlayerCanSelect;

        CheckAmountOfCardsPlayerHasSelected(out numSelected, out maxNumPlayerCanSelect);

        Player currentPlayer = GameplayManager.GetCurrentPlayer();

        //Need a way for if it's the player drawing then using a card to not count that towards the count...

        if (currentPlayer.ExtraMovementCardUses > 0)
        {
            if (GameplayManager.GameplayPhaseStatemachineRef.GetCurrentState() != GameplayManager.GameplayPhaseStatemachineRef.gameplayDuelPhaseState && !GameplayManager.UseSelectedCardsPanel.activeInHierarchy)
            {
                GameplayManager.UseSelectedCardsPanel.SetActive(true);
            }
            GameplayManager.UseSelectedCardsText.text = $"Selected cards: {numSelected}/{maxNumPlayerCanSelect}";
            GameplayManager.UseSelectedCardsButton.onClick.RemoveAllListeners();
            GameplayManager.UseSelectedCardsButton.onClick.AddListener(currentPlayer.UseMultipleCards);
            if(numSelected == 0)
            {
                GameplayManager.UseSelectedCardsPanel.gameObject.SetActive(false);
            }

            if (TempCardValue > 0)
            {
                currentPlayer.CurrentSumOfSpacesToMove -= TempCardValue;
            }
            else
            {
                currentPlayer.CurrentSumOfSpacesToMove -= MovementCardValue;
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


    #region Status Effects/Buffs
    public override void ActivateCurseEffect()
    {
        TextureAnimator.SetBool(ISCURSED, true);
    }

    public override void DeactivateCurseEffect()
    {
        TextureAnimator.SetBool(ISCURSED, false);
    }

    public override void ActivateBoostedEffect()
    {
        TextureAnimator.SetBool(ISBOOSTED, true);
    }

    public override void DeactivateBoostedEffect()
    {
        TextureAnimator.SetBool(ISBOOSTED, false);
    }

    public override void TurnOffAllEffects()
    {
        TextureAnimator.SetBool(ISCURSED, false);
        TextureAnimator.SetBool(ISBOOSTED, false);
    }



    #endregion
}
