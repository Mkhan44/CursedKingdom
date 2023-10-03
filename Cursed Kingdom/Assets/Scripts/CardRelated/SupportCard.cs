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
    [SerializeField] private Image supportCardTypeImage;


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
        SupportCardData = cardData as SupportCardData;
        TitleText.text = SupportCardData.CardTitle;
        DescriptionText.text = SupportCardData.CardDescription;
        CardArtworkImage.sprite = SupportCardData.CardArtwork;
        SetupCardTypeImage();
    }

    private void SetupCardTypeImage()
    {
        if (SupportCardData.ThisSupportCardType == SupportCardData.SupportCardType.Movement)
        {
            supportCardTypeImage.sprite = IconPresetsSingleton.instance.SupportCardTypeIconPreset.MovementSprite;
        }
        else if (SupportCardData.ThisSupportCardType == SupportCardData.SupportCardType.Duel)
        {
            supportCardTypeImage.sprite = IconPresetsSingleton.instance.SupportCardTypeIconPreset.DuelSprite;
        }
        else
        {
            supportCardTypeImage.sprite = IconPresetsSingleton.instance.SupportCardTypeIconPreset.SpecialSprite;
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
            if (!handExpandUI.CurrentActiveTransform.IsExpanded)
            {
                handExpandUI.ExpandHand(ThisCardType, GameplayManager.Players.IndexOf(GameplayManager.playerCharacter.GetComponent<Player>()));
            }
            else
            {
                if (ParentAnimator != null)
                {
                    if (ParentAnimator.GetBool(hidden))
                    {
                        return;
                    }
                }
                DeselectOtherSelectedCards();

                if(GameplayManager.Players[indexOfCurrentPlayer].CardsLeftToDiscard > 0 && !SelectedForDiscard && IsValidCardTypeToDiscard(GameplayManager.Players[indexOfCurrentPlayer]))
                {
                    SelectForDiscard();
                    return;
                }
                else if(GameplayManager.Players[indexOfCurrentPlayer].CardsLeftToDiscard > 0 &&  !SelectedForDiscard && !IsValidCardTypeToDiscard(GameplayManager.Players[indexOfCurrentPlayer]))
                {
                    Debug.LogWarning("Hey! You can't select this card to discard.");
                    return;
                }

                if(SelectedForDiscard)
                {
                    DeselectForDiscard();
                    return;
                }

                if (GameplayManager.Players[indexOfCurrentPlayer].MovementCardSelectedForUse)
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
                    if(!GameplayManager.Players[indexOfCurrentPlayer].CanUseSupportCard())
                    {
                        DialogueBoxPopup.instance.ActivatePopupWithJustText($"You have already used a support card this turn.", 2.0f);
                        GameplayManager.HandDisplayPanel.ShrinkHand();
                        transform.localScale = OriginalSize;
                        CardIsActiveHovered = false;
                        return;
                    }

                    bool canCostBePaid = false;
                    ApplySupportCardEffects(GameplayManager.Players[indexOfCurrentPlayer], out canCostBePaid);
                    if (!canCostBePaid)
                    {
                        GameplayManager.HandDisplayPanel.ShrinkHand();
                        transform.localScale = OriginalSize;
                        CardIsActiveHovered = false;
                        return;
                    }
                    GameplayManager.Players[indexOfCurrentPlayer].UseSupportCard();
                    GameplayManager.Players[indexOfCurrentPlayer].DiscardFromHand(ThisCardType, this);
                    GameplayManager.HandDisplayPanel.ShrinkHand();
                    transform.localScale = OriginalSize;
                    CardIsActiveHovered = false;
                }
            }
        }
    }

    public override void SelectForUse()
    {
        base.SelectForUse();
        int indexOfCurrentPlayer;
        int numSelected;
        int maxNumPlayerCanSelect;

        CheckAmountOfCardsPlayerHasSelected(out indexOfCurrentPlayer, out numSelected, out maxNumPlayerCanSelect);
        GameplayManager.Players[indexOfCurrentPlayer].SupportCardSelectedForUse = true;
    }

    public override void DeselectForUse()
    {
        base.DeselectForUse();
        int indexOfCurrentPlayer;
        int numSelected;
        int maxNumPlayerCanSelect;

        CheckAmountOfCardsPlayerHasSelected(out indexOfCurrentPlayer, out numSelected, out maxNumPlayerCanSelect);

        if (GameplayManager.Players[indexOfCurrentPlayer].ExtraSupportCardUses > 0)
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
                    DialogueBoxPopup.instance.ActivatePopupWithJustText($"Cost of {SupportCardData.supportCardEffects[i].supportCardEffectData.name} can't be paid.", 2.0f);
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
            player.StartHandlingSupportCardEffects();
        }


        //Revert the space data back if we used debug to change it.
        if (DebugModeSingleton.instance.IsDebugActive)
        {
            SupportCardData = cachedSupportCardData;
        }
    }
}

