//Code written by Mohamed Riaz Khan of BukuGames.
//All code is written by me (Above name) unless otherwise stated via comments below.
//Not authorized for use outside of the Github repository of this game developed by BukuGames.

using System;
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
    [SerializeField] private Image supportCardTypeImage;


    //Data
    [SerializeField] private SupportCardData supportCardData;

    public TextMeshProUGUI DescriptionText { get => descriptionText; set => descriptionText = value; }
    public SupportCardData SupportCardData { get => supportCardData; set => supportCardData = value; }
    public Image SupportCardTypeImage { get => supportCardTypeImage; set => supportCardTypeImage = value; }

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
        SupportCardData = cardData as SupportCardData;
        TitleText.text = SupportCardData.CardTitle;
        DescriptionText.text = SupportCardData.CardDescription;
        CardArtworkImage.sprite = SupportCardData.CardArtwork;
        SetupCardTypeImage();
    }

    private void SetupCardTypeImage()
    {
        SupportCardTypeIconPreset.SupportCardIconElement.SupportIconType iconTypeToCheck = default;

        if (SupportCardData.ThisSupportCardType == SupportCardData.SupportCardType.Movement)
        {
            iconTypeToCheck = SupportCardTypeIconPreset.SupportCardIconElement.SupportIconType.movement;
        }
        else if (SupportCardData.ThisSupportCardType == SupportCardData.SupportCardType.Duel)
        {
            iconTypeToCheck = SupportCardTypeIconPreset.SupportCardIconElement.SupportIconType.duel;
        }
        else if (supportCardData.ThisSupportCardType == SupportCardData.SupportCardType.Special)
        {
            iconTypeToCheck = SupportCardTypeIconPreset.SupportCardIconElement.SupportIconType.special;
        }

        SetIconImage(iconTypeToCheck, SupportCardTypeImage);

    }

    private void SetIconImage(SupportCardTypeIconPreset.SupportCardIconElement.SupportIconType typeToCheck, Image iconImage)
    {
        foreach (SupportCardTypeIconPreset.SupportCardIconElement supportCardIconElement in IconPresetsSingleton.instance.SupportCardTypeIconPreset.SupportCardIcons)
        {
            if (supportCardIconElement.SupportIconType1 == typeToCheck)
            {
                iconImage.sprite = supportCardIconElement.Sprite;
                iconImage.color = supportCardIconElement.Color;
            }
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
            Player currentPlayer = GameplayManager.GetCurrentPlayer();
            if (!handExpandUI.CurrentActiveTransform.IsExpanded)
            {
                handExpandUI.ExpandHand(ThisCardType);
            }
            else
            {
                if (ParentAnimator != null)
                {
                    if (ParentAnimator.GetBool(ISHIDDEN))
                    {
                        return;
                    }
                }
                DeselectOtherSelectedCards();

                if(currentPlayer.CardsLeftToDiscard > 0 && !SelectedForDiscard && IsValidCardTypeToDiscard(currentPlayer))
                {
                    SelectForDiscard();
                    return;
                }
                else if(currentPlayer.CardsLeftToDiscard > 0 &&  !SelectedForDiscard && !IsValidCardTypeToDiscard(currentPlayer))
                {
                    DialogueBoxPopup.instance.ActivatePopupWithJustText("You cannot select a support card to discard for this effect.", 2.0f);
                    return;
                }

                if(SelectedForDiscard)
                {
                    DeselectForDiscard();
                    return;
                }

                if (currentPlayer.MovementCardSelectedForUse)
                {
                    DialogueBoxPopup.instance.ActivatePopupWithJustText("You're currently selecting movement cards. Deselect all movement cards to select a support card.", 2.0f);
                    return;
                }

                if (!CardIsActiveHovered)
                {
                    CardIsActiveHovered = true;
                    StartCoroutine(HoverCardEffect());
                }
                else
                {
                    if(!DebugModeSingleton.instance.IsDebugActive)
                    {
                        //Checking all different ways that we can invalidate using a support card.
                        if (!currentPlayer.CanUseSupportCard())
                        {
                            DialogueBoxPopup.instance.ActivatePopupWithJustText($"You have already used a support card this turn.", 2.0f);
                            GameplayManager.HandDisplayPanel.ShrinkHand();
                            transform.localScale = OriginalSize;
                            CardIsActiveHovered = false;
                            if (GameplayManager.DuelPhaseSMRef.GetCurrentState().GetType() == typeof(DuelSelectCardsToUsePhaseState))
                            {
                                StartCoroutine(WaitAfterPopupOfWrongSupportTypeDuel());
                            }
                            return;
                        }
                        else if (GameplayManager.GameplayPhaseStatemachineRef.GetCurrentState().GetType() == typeof(GameplayMovementPhaseState) && SupportCardData.ThisSupportCardType == SupportCardData.SupportCardType.Duel)
                        {
                            DialogueBoxPopup.instance.ActivatePopupWithJustText($"You can only use this support card during duel phase!", 2.0f);
                            GameplayManager.HandDisplayPanel.ShrinkHand();
                            transform.localScale = OriginalSize;
                            CardIsActiveHovered = false;
                            return;
                        }
                        else if (GameplayManager.GameplayPhaseStatemachineRef.GetCurrentState().GetType() == typeof(GameplayDuelPhaseState) && SupportCardData.ThisSupportCardType == SupportCardData.SupportCardType.Movement)
                        {
                            DialogueBoxPopup.instance.ActivatePopupWithJustText($"You can only use this support card during movement phase!", 2.0f);
                            GameplayManager.HandDisplayPanel.ShrinkHand();
                            transform.localScale = OriginalSize;
                            CardIsActiveHovered = false;
                            if(GameplayManager.DuelPhaseSMRef.GetCurrentState().GetType() == typeof(DuelSelectCardsToUsePhaseState))
                            {
                                StartCoroutine(WaitAfterPopupSupportCardsAlreadySelectedDuel());
                            }


                            return;
                        }
                        else if (SupportCardData.ThisSupportCardType == SupportCardData.SupportCardType.Special)
                        {
                            bool isAReactionCard = false;
                            foreach (SupportCardData.SupportCardEffect effect in SupportCardData.supportCardEffects)
                            {
                                if (effect.supportCardEffectData.IsReaction)
                                {
                                    isAReactionCard = true;
                                }
                            }

                            if (isAReactionCard)
                            {
                                DialogueBoxPopup.instance.ActivatePopupWithJustText($"You can't use this card right now.", 2.0f);
                                GameplayManager.HandDisplayPanel.ShrinkHand();
                                transform.localScale = OriginalSize;
                                CardIsActiveHovered = false;

                                if (GameplayManager.DuelPhaseSMRef.GetCurrentState().GetType() == typeof(DuelSelectCardsToUsePhaseState))
                                {
                                    StartCoroutine(WaitAfterPopupSupportCardsAlreadySelectedDuel());
                                }

                                return;
                            }
                        }
                    }
                    else
                    {
                        Debug.LogWarning("You are in debug mode and we are NOT checking for correct conditions to use a Support card.");
                    }

                    List<Player> playersThatCanNegate = new();
                    playersThatCanNegate = currentPlayer.CheckIfOtherPlayersCanNegateWithoutSingleTarget();

                    if (playersThatCanNegate.Count > 0)
                    {
                        List<SupportCard> supportCards = currentPlayer.GetSupportCardsPlayersCanNegateSupportCardEffectsWith(currentPlayer);
                        //Need this to be dynamic so that if Player 1 says no, Player 2 has a chance to respond etc.
                        currentPlayer.ActivatePlayerNegateSupportCardPopup(playersThatCanNegate[0], supportCards, this, currentPlayer);
                        return;
                    }

                    if (GameplayManager.DuelPhaseSMRef.GetCurrentState().GetType() != typeof(DuelSelectCardsToUsePhaseState))
                    {
                        AttemptToUseSupportCard(currentPlayer);
                    }
                    //Might hafta check if the Player we are is the same as the current player being handled in the duelPhaseSM.
                    else if (GameplayManager.DuelPhaseSMRef.GetCurrentState().GetType() == typeof(DuelSelectCardsToUsePhaseState))
                    {
                        if(GameplayManager.DuelPhaseSMRef.duelSelectCardsToUsePhaseState.SelectedSupportCard)
                        {
                            GameplayManager.HandDisplayPanel.ShrinkHand();
                            transform.localScale = OriginalSize;
                            CardIsActiveHovered = false;

                            DialogueBoxPopup.instance.ActivatePopupWithJustText("You have already selected a support card.", 2.0f);
                            StartCoroutine(WaitAfterPopupSupportCardsAlreadySelectedDuel());
                            return;
                        }

                        List<SupportCard> supportCards = new List<SupportCard>();
                        supportCards.Add(this);
                        GameplayManager.DuelPhaseSMRef.duelSelectCardsToUsePhaseState.SelectSupportCard(supportCards);
                    }
                    
                }
            }
        }
    }

    public IEnumerator WaitAfterPopupSupportCardsAlreadySelectedDuel()
    {
        yield return new WaitForSeconds(2.0f);
        DialogueBoxPopup.instance.DeactivatePopup();
        GameplayManager.DuelPhaseSMRef.CurrentPlayerBeingHandled.PlayerInDuel.ShowHand();
    }

    public IEnumerator WaitAfterPopupOfWrongSupportTypeDuel()
    {
        yield return new WaitForSeconds(2.0f);

        List<Tuple<string, string, object, List<object>>> insertedParams = new();
        insertedParams.Add(Tuple.Create<string, string, object, List<object>>("Don't use a support card", nameof(GameplayManager.DuelPhaseSMRef.ChooseNoSupportCardToUseInDuel), GameplayManager.DuelPhaseSMRef, new List<object>()));

        DialogueBoxPopup.instance.ActivatePopupWithButtonChoices($"Player {GameplayManager.DuelPhaseSMRef.CurrentPlayerBeingHandled.PlayerInDuel.playerIDIntVal}: Please choose a support card if you wish to use one in the duel.", insertedParams, 1, "Card selection", false);

        GameplayManager.DuelPhaseSMRef.CurrentPlayerBeingHandled.PlayerInDuel.ShowHand();
    }

    public void AttemptToUseSupportCard(Player playerUsingCard, bool isCurrentPlayer = true)
    {
        bool canCostBePaid = false;
        ApplySupportCardEffects(playerUsingCard, out canCostBePaid);
        if (!canCostBePaid)
        {
            GameplayManager.HandDisplayPanel.ShrinkHand();
            transform.localScale = OriginalSize;
            CardIsActiveHovered = false;
            return;
        }
        playerUsingCard.UseSupportCard();
        playerUsingCard.CurrentSupportCardInUse = this;
        playerUsingCard.DiscardFromHand(ThisCardType, this);

        if(!isCurrentPlayer)
        {
            return;
        }

        GameplayManager.HandDisplayPanel.ShrinkHand();
        transform.localScale = OriginalSize;
        CardIsActiveHovered = false;
    }

    public override void SelectForUse()
    {
        base.SelectForUse();
        //int indexOfCurrentPlayer;
        //int numSelected;
        //int maxNumPlayerCanSelect;

        //CheckAmountOfCardsPlayerHasSelected(out indexOfCurrentPlayer, out numSelected, out maxNumPlayerCanSelect);
        //GameplayManager.Players[indexOfCurrentPlayer].SupportCardSelectedForUse = true;
    }

    public override void DeselectForUse()
    {
        base.DeselectForUse();
        //int indexOfCurrentPlayer;
        //int numSelected;
        //int maxNumPlayerCanSelect;

        //CheckAmountOfCardsPlayerHasSelected(out indexOfCurrentPlayer, out numSelected, out maxNumPlayerCanSelect);

        //if (GameplayManager.Players[indexOfCurrentPlayer].ExtraSupportCardUses > 0)
        //{
        //    if (!GameplayManager.UseSelectedCardsPanel.activeInHierarchy)
        //    {
        //        GameplayManager.UseSelectedCardsPanel.SetActive(true);
        //    }
        //    GameplayManager.UseSelectedCardsText.text = $"Selected cards: {numSelected}/{maxNumPlayerCanSelect}";
        //    GameplayManager.UseSelectedCardsButton.onClick.RemoveAllListeners();
        //    GameplayManager.UseSelectedCardsButton.onClick.AddListener(GameplayManager.Players[indexOfCurrentPlayer].UseMultipleCards);
        //}
    }

    public bool IsValidCardTypeToDiscard(Player playerReference)
    {
        bool isValid = false;

        if((playerReference.ValidCardTypesToDiscard == CardType.Support || playerReference.ValidCardTypesToDiscard == CardType.Both) && playerReference.CardsLeftToDiscard > 0)
        {
            isValid = true;
        }
        return isValid;
    }

    private void ApplySupportCardEffects(Player player, out bool canAllCostsBePaid)
    {
        canAllCostsBePaid = false;
        //For debug mode.
        SupportCardData cachedSupportCardData = SupportCardData;

        if (DebugModeSingleton.instance.IsDebugActive)
        {
            SupportCard tempSupportCard = DebugModeSingleton.instance.OverrideSupportCardUseEffect();

            if (tempSupportCard != null)
            {
                SupportCardData = tempSupportCard.SupportCardData;
            }

        }

        if (SupportCardData.supportCardEffects.Count < 1)
        {
            Debug.LogWarning("Hey, no support effects on this card currently!");
        }

        
        for (int i = 0; i < SupportCardData.supportCardEffects.Count; i++)
        {
            if (SupportCardData.supportCardEffects[i].supportCardEffectData.IsACost)
            {
                if (!SupportCardData.supportCardEffects[i].supportCardEffectData.CanCostBePaid(player))
                {
                    Debug.LogWarning($"Cost of {SupportCardData.supportCardEffects[i].supportCardEffectData.name} can't be paid. Can't execute space effects.");
                    //DialogueBoxPopup.instance.ActivatePopupWithJustText($"Cost of {SupportCardData.supportCardEffects[i].supportCardEffectData.name} can't be paid.", 2.0f);
                    canAllCostsBePaid = false;
                    break;
                }
                else
                {
                    canAllCostsBePaid = true;
                }
            }
            else
            {
                canAllCostsBePaid = true;
            }
        }

        if (canAllCostsBePaid)
        {
            Queue<SupportCardEffectData> supportCardEffectsForPlayerToHandle = new();
            int numSupportCardEffectData;
            for (int j = 0; j < SupportCardData.supportCardEffects.Count; j++)
            {
                //Do the support card effects in sequence. We'll check for any external triggers here as well.
                if (player.IsDefeated)
                {
                    break;
                }
                //This is too fast. Need a way to wait for each effect then go to the next.
                numSupportCardEffectData = j;
                supportCardEffectsForPlayerToHandle.Enqueue(SupportCardData.supportCardEffects[numSupportCardEffectData].supportCardEffectData);
            }
            //Whatever we already had from passing over effects + the new effects.
            player.SupportCardEffectsToHandle = supportCardEffectsForPlayerToHandle;
            player.StartHandlingSupportCardEffects(this);
            player.GameplayManagerRef.OnPlayerUsedASupportCard(this);
        }


        //Revert the space data back if we used debug to change it.
        if (DebugModeSingleton.instance.IsDebugActive)
        {
            SupportCardData = cachedSupportCardData;
        }
    }
}

