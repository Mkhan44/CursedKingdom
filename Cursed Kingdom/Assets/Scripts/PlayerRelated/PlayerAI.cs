//Code written by Mohamed Riaz Khan of BukuGames.
//All code is written by me (Above name) unless otherwise stated via comments below.
//Not authorized for use outside of the Github repository of this game developed by BukuGames.

using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using PurrNet;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class PlayerAI : NetworkBehaviour
{
    public const float ORIGINALAIDELAYSPEEDINSECONDS = 1.5f;
    [SerializeField] private Player playerReference;
    public float AIDelaySpeedInSeconds;

    public Player PlayerReference { get => playerReference; set => playerReference = value; }

    public void InitializeAI(Player player)
    {
        PlayerReference = player;
        AIDelaySpeedInSeconds = ORIGINALAIDELAYSPEEDINSECONDS;
        Subscribe();
    }

    public void ChangeDelaySpeed(float newDelaySpeed, bool returnToDefault = false)
    {
        if(returnToDefault)
        {
            AIDelaySpeedInSeconds = ORIGINALAIDELAYSPEEDINSECONDS;
        }
        AIDelaySpeedInSeconds = newDelaySpeed;
    }

    private void Subscribe()
    {
        PlayerReference.StartDiscardingCards += StartDiscardingCards;
    }

    private void Unsubscribe()
    {
        PlayerReference.StartDiscardingCards -= StartDiscardingCards;
    }

    #region Movement phase methods

    public IEnumerator ChooseMovementCardToUseMovementPhase()
    {
        if((PlayerReference.MaxMovementCardsToUse + PlayerReference.ExtraMovementCardUses) > 1)
        {
            StartCoroutine(SelectMultipleMovementCardsToMoveWith());
            yield break;
        }
        yield return new WaitForSeconds(AIDelaySpeedInSeconds);
        PlayerReference.GameplayManagerRef.HandDisplayPanel.ExpandHand(Card.CardType.Movement);
        yield return new WaitForSeconds(AIDelaySpeedInSeconds);

        int movementCardToUseIndex = Random.Range(0,PlayerReference.MovementCardsInHandCount);
        MovementCard movementCardToUse = PlayerReference.GetMovementCardsInHand()[movementCardToUseIndex];
        yield return new WaitForSeconds(AIDelaySpeedInSeconds);
        movementCardToUse.OnPointerClick(null);
        yield return new WaitForSeconds(AIDelaySpeedInSeconds);
        movementCardToUse.OnPointerClick(null);
        //Debug.Log($"Player is an AI and we are attempting to use a random movement card witht he value of {movementCardToUse.MovementCardValue}");
    }

    public IEnumerator DrawAndUseMovementCardMovementPhase()
    {
        yield return new WaitForSeconds(AIDelaySpeedInSeconds);
        playerReference.GameplayManagerRef.HandDisplayPanel.ExpandHand(Card.CardType.Movement);
        yield return new WaitForSeconds(AIDelaySpeedInSeconds);

        PlayerReference.NoMovementCardsInHandButton.onClick.RemoveAllListeners();
        PlayerReference.DrawThenUseMovementCardImmediatelyMovement();
    }

    public IEnumerator SelectMultipleMovementCardsToMoveWith()
    {
        //We have 0 movement cards in hand so just draw and use next one.
        if(PlayerReference.MovementCardsInHandCount == 0)
        {
            //We click the use next card button so I think we don't needa do the selected cards thing...Might still needa turn off the button though.
            StartCoroutine(DrawAndUseMovementCardMovementPhase());
            yield break;
        }
        //We only have 1 so select that 1 and then use it.
        else if(PlayerReference.MovementCardsInHandCount == 1)
        {
            yield return new WaitForSeconds(AIDelaySpeedInSeconds);
            PlayerReference.GameplayManagerRef.HandDisplayPanel.ExpandHand(Card.CardType.Movement);
            yield return new WaitForSeconds(AIDelaySpeedInSeconds);
            PlayerReference.GetMovementCardsInHand()[0].SelectForUse();
            yield return new WaitForSeconds(AIDelaySpeedInSeconds);

            Image selectedCardsButtonTargetGraphicImage = PlayerReference.GameplayManagerRef.UseSelectedCardsButton.targetGraphic.GetComponent<Image>();
            selectedCardsButtonTargetGraphicImage.color = PlayerReference.GameplayManagerRef.UseSelectedCardsButton.colors.highlightedColor;

            yield return new WaitForSeconds(0.6f);

            PlayerReference.GameplayManagerRef.UseSelectedCardsButton.onClick.Invoke();

            selectedCardsButtonTargetGraphicImage.color = PlayerReference.GameplayManagerRef.UseSelectedCardsButton.colors.normalColor;

            yield break;
        }

        int numMovementCardsTotal = PlayerReference.MaxMovementCardsToUse + PlayerReference.ExtraMovementCardUses;

        List<int> randomCardsToUseMultiple = new();

        for(int i = 0; i < numMovementCardsTotal; i++)
        {
            int currentRandomNum = Random.Range(0, PlayerReference.MovementCardsInHandCount);
            while(randomCardsToUseMultiple.Count != 0 && randomCardsToUseMultiple.Contains(currentRandomNum))
            {
                currentRandomNum = Random.Range(0, PlayerReference.MovementCardsInHandCount);
            }
            randomCardsToUseMultiple.Add(currentRandomNum);
        }
        
        yield return new WaitForSeconds(AIDelaySpeedInSeconds);
        PlayerReference.GameplayManagerRef.HandDisplayPanel.ExpandHand(Card.CardType.Movement);
        yield return new WaitForSeconds(AIDelaySpeedInSeconds);

        for(int j = 0; j < randomCardsToUseMultiple.Count; j++)
        {
            int index = j;
            PlayerReference.GetMovementCardsInHand()[randomCardsToUseMultiple[index]].SelectForUse();
            yield return new WaitForSeconds(AIDelaySpeedInSeconds);
        }

        Image targetGraphicImage = PlayerReference.GameplayManagerRef.UseSelectedCardsButton.targetGraphic.GetComponent<Image>();
        targetGraphicImage.color = PlayerReference.GameplayManagerRef.UseSelectedCardsButton.colors.highlightedColor;

        yield return new WaitForSeconds(0.6f);

        PlayerReference.GameplayManagerRef.UseSelectedCardsButton.onClick.Invoke();

        targetGraphicImage.color = PlayerReference.GameplayManagerRef.UseSelectedCardsButton.colors.normalColor;

        yield return null;
    }

    #region Ability related
    public IEnumerator UseAbility()
    {
        if(!PlayerReference.ClassData.abilityData.CanBeManuallyActivated)
        {
            //Force the event to trigger.
            PlayerReference.CompletedAbilityActivation();
            yield break;
        }
        yield return new WaitForSeconds(AIDelaySpeedInSeconds);
        PlayerReference.GameplayManagerRef.UseAbilityButton.onClick.Invoke();
        yield return new WaitForSeconds(AIDelaySpeedInSeconds);
    }

    public IEnumerator UseEliteAbility()
    {
        if(!PlayerReference.ClassData.eliteAbilityData.CanBeManuallyActivated || !PlayerReference.ClassData.eliteAbilityData.CanCostBePaid(PlayerReference, true))
        {
            //Force the event to trigger.
            PlayerReference.CompletedEliteAbilityActivation();
            yield break;
        }

        yield return new WaitForSeconds(AIDelaySpeedInSeconds);
        PlayerReference.GameplayManagerRef.UseEliteAbilityButton.onClick.Invoke();
        yield return new WaitForSeconds(AIDelaySpeedInSeconds);
    }

    #endregion

    #region Direction Selection

    public IEnumerator SelectRandomDirectionToMoveIn()
    {
        int randomIndex = Random.Range(0, PlayerReference.GameplayManagerRef.directionChoiceButtonHolder.transform.childCount);

        yield return new WaitForSeconds(AIDelaySpeedInSeconds);

        Button selectedButton = PlayerReference.GameplayManagerRef.directionChoiceButtonHolder.transform.GetChild(randomIndex).GetComponent<Button>();

        if(selectedButton != null)
        {
            selectedButton.onClick.Invoke();
        }
    }


    #endregion

    #region UseSupportCardMovementPhase

    public IEnumerator UseRandomSupportCardMovementPhase()
    {
        yield return null;

        //GO through all support cards in hand, if any are movement phase and cost can be paid then attempt to use them.
        List<SupportCard> validMovementPhaseSupportCards = new();

        foreach(SupportCard card in PlayerReference.GetSupportCardsInHand())
        {
            if(card.SupportCardData.ThisSupportCardType == SupportCardData.SupportCardType.Movement)
            {
                validMovementPhaseSupportCards.Add(card);
            }
        }
        if(validMovementPhaseSupportCards.Count == 0)
        {
            Debug.Log("No valid support cards for the AI to use during movement phase. Aborting random support card usage.");
            if(PlayerReference.MovementCardsInHandCount == 0)
            {
                PlayerReference.GameplayManagerRef.GameplayPhaseStatemachineRef.gameplayMovementPhaseState.DrawAndUseMovementCardAI();
            }
            else
            {
                PlayerReference.GameplayManagerRef.GameplayPhaseStatemachineRef.gameplayMovementPhaseState.UseMovementCardAI();
            }
            
            yield break;
        }

        int randomCardIndex = Random.Range(0, validMovementPhaseSupportCards.Count);
        SupportCard supportCardReference = validMovementPhaseSupportCards[randomCardIndex];

        foreach(SupportCardData.SupportCardEffect supportCardEffect in supportCardReference.SupportCardData.supportCardEffects)
        {
            if(supportCardEffect.supportCardEffectData.IsACost)
            {
                if(!supportCardEffect.supportCardEffectData.CanCostBePaid(PlayerReference, true))
                {
                    if(PlayerReference.MovementCardsInHandCount == 0)
                    {
                        PlayerReference.GameplayManagerRef.GameplayPhaseStatemachineRef.gameplayMovementPhaseState.DrawAndUseMovementCardAI();
                    }
                    else
                    {
                        PlayerReference.GameplayManagerRef.GameplayPhaseStatemachineRef.gameplayMovementPhaseState.UseMovementCardAI();
                    }
                    yield break;
                }
            }

        }
        int indexInHandTouse = PlayerReference.GetSupportCardsInHand().IndexOf(supportCardReference);


        playerReference.GameplayManagerRef.HandDisplayPanel.ShrinkHand();
        yield return new WaitForSeconds(AIDelaySpeedInSeconds);
        playerReference.GameplayManagerRef.HandDisplayPanel.ExpandHand(Card.CardType.Support);
        yield return new WaitForSeconds(AIDelaySpeedInSeconds);

        SupportCard supportCardToUse = PlayerReference.GetSupportCardsInHand()[indexInHandTouse];
        yield return new WaitForSeconds(AIDelaySpeedInSeconds);
        supportCardToUse.OnPointerClick(null);
        yield return new WaitForSeconds(AIDelaySpeedInSeconds);
        supportCardToUse.OnPointerClick(null);

    }

    #endregion

    #region Discarding Cards
    
    /// <summary>
    /// (For now randomly) Picks random movement cards to discard. Usually this is for exceeding hand size but also can be used when needing to discard for support cards or effects.
    /// </summary>
    ///

    public void StartDiscardingCards(Player playerThatsDiscarding)
    {
        PlayerReference.GameplayManagerRef.HandDisplayPanel.ShrinkHand();
        if(playerThatsDiscarding.ValidCardTypesToDiscard == Card.CardType.Movement)
        {
            StartCoroutine(DiscardMovementCards());
        }
        else if(playerThatsDiscarding.ValidCardTypesToDiscard == Card.CardType.Support)
        {
            StartCoroutine(DiscardSupportCards());
        }
        else
        {
            StartCoroutine(DiscardAnyCards());
        }
    }
    public IEnumerator DiscardMovementCards()
    {
        List<int> randomCardsToDiscardIndeces = new();

        for(int i = 0; i < PlayerReference.CardsLeftToDiscard; i++)
        {
            int currentRandomNum = Random.Range(0, PlayerReference.MovementCardsInHandCount);
            while(randomCardsToDiscardIndeces.Count != 0 && randomCardsToDiscardIndeces.Contains(currentRandomNum))
            {
                currentRandomNum = Random.Range(0, PlayerReference.MovementCardsInHandCount);
            }
            randomCardsToDiscardIndeces.Add(currentRandomNum);
        }
        
        yield return new WaitForSeconds(AIDelaySpeedInSeconds);
        PlayerReference.GameplayManagerRef.HandDisplayPanel.ExpandHand(Card.CardType.Movement);
        yield return new WaitForSeconds(AIDelaySpeedInSeconds);

        for(int j = 0; j < randomCardsToDiscardIndeces.Count; j++)
        {
            int index = j;
            PlayerReference.GetMovementCardsInHand()[randomCardsToDiscardIndeces[index]].SelectForDiscard();
            yield return new WaitForSeconds(AIDelaySpeedInSeconds);
        }

        StartCoroutine(SelectFirstOptionDialogueBoxChoice(true));
    }

    public IEnumerator DiscardSupportCards()
    {
        List<int> randomCardsToDiscardIndeces = new();

        for(int i = 0; i < PlayerReference.CardsLeftToDiscard; i++)
        {
            int currentRandomNum = Random.Range(0, PlayerReference.SupportCardsInHandCount);
            while(randomCardsToDiscardIndeces.Count != 0 && randomCardsToDiscardIndeces.Contains(currentRandomNum))
            {
                currentRandomNum = Random.Range(0, PlayerReference.SupportCardsInHandCount);
            }
            randomCardsToDiscardIndeces.Add(currentRandomNum);
        }
        
        yield return new WaitForSeconds(AIDelaySpeedInSeconds);
        PlayerReference.GameplayManagerRef.HandDisplayPanel.ExpandHand(Card.CardType.Support);
        yield return new WaitForSeconds(AIDelaySpeedInSeconds);

        for(int j = 0; j < randomCardsToDiscardIndeces.Count; j++)
        {
            int index = j;
            PlayerReference.GetSupportCardsInHand()[randomCardsToDiscardIndeces[index]].SelectForDiscard();
            yield return new WaitForSeconds(AIDelaySpeedInSeconds);
        }

        StartCoroutine(SelectFirstOptionDialogueBoxChoice(true));
    }

    public IEnumerator DiscardAnyCards()
    {
        List<int> randomCardsToDiscardMovementIndeces = new();
        List<int> randomCardsToDiscardSupportIndeces = new();

        //Just choose movement cards.
        if(PlayerReference.SupportCardsInHandCount == 0)
        {
            StartCoroutine(DiscardMovementCards());
            Debug.Log("Player's support card in hand count is 0. Gonna discard movement cards as an AI.");
            yield break;
        }
        //Just choose support cards.
        else if(PlayerReference.MovementCardsInHandCount == 0)
        {
            StartCoroutine(DiscardSupportCards());
            Debug.Log("Player's movement card in hand count is 0. Gonna discard support cards as an AI.");
            yield break;
        }
        //Choose randomly from both decks.
        else
        {
            int deckToChooseFromIndex = 0;
            int currentRandomNum = 0;

            for(int i = 0; i < PlayerReference.CardsLeftToDiscard; i++)
            {
                //Select support cards for the rest since we've chosen all movements in our hand.
                if(randomCardsToDiscardMovementIndeces.Count >= PlayerReference.MovementCardsInHandCount)
                {
                    currentRandomNum = Random.Range(0, PlayerReference.SupportCardsInHandCount);

                    while(randomCardsToDiscardSupportIndeces.Count != 0 && randomCardsToDiscardSupportIndeces.Contains(currentRandomNum))
                    {
                        currentRandomNum = Random.Range(0, PlayerReference.SupportCardsInHandCount);
                    }
                    randomCardsToDiscardSupportIndeces.Add(currentRandomNum);
                }
                //Select movement cards for the rest since we've chosen all supports in our hand.
                else if(randomCardsToDiscardSupportIndeces.Count >= PlayerReference.SupportCardsInHandCount)
                {
                    currentRandomNum = Random.Range(0, PlayerReference.MovementCardsInHandCount);

                    while(randomCardsToDiscardMovementIndeces.Count != 0 && randomCardsToDiscardMovementIndeces.Contains(currentRandomNum))
                    {
                        currentRandomNum = Random.Range(0, PlayerReference.MovementCardsInHandCount);
                    }
                    randomCardsToDiscardMovementIndeces.Add(currentRandomNum);
                }
                //We still have free reign on what to select so pick a random deck.
                else
                {
                    deckToChooseFromIndex = Random.Range(0,1);
                    if(deckToChooseFromIndex == 0)
                    {
                        currentRandomNum = Random.Range(0, PlayerReference.MovementCardsInHandCount);

                        while(randomCardsToDiscardMovementIndeces.Count != 0 && randomCardsToDiscardMovementIndeces.Contains(currentRandomNum))
                        {
                            currentRandomNum = Random.Range(0, PlayerReference.MovementCardsInHandCount);
                        }
                        randomCardsToDiscardMovementIndeces.Add(currentRandomNum);
                    }
                    else
                    {
                        currentRandomNum = Random.Range(0, PlayerReference.SupportCardsInHandCount);

                        while(randomCardsToDiscardSupportIndeces.Count != 0 && randomCardsToDiscardSupportIndeces.Contains(currentRandomNum))
                        {
                            currentRandomNum = Random.Range(0, PlayerReference.SupportCardsInHandCount);
                        }
                        randomCardsToDiscardSupportIndeces.Add(currentRandomNum);
                    }
                }
            }
            
            //Debug.Log($"We picked: {randomCardsToDiscardMovementIndeces.Count} movement cards and {randomCardsToDiscardSupportIndeces.Count} support cards to discard.");
            yield return new WaitForSeconds(AIDelaySpeedInSeconds);
            if(randomCardsToDiscardMovementIndeces.Count != 0)
            {
                 PlayerReference.GameplayManagerRef.HandDisplayPanel.ExpandHand(Card.CardType.Movement);
                 yield return new WaitForSeconds(AIDelaySpeedInSeconds);

                 for(int j = 0; j < randomCardsToDiscardMovementIndeces.Count; j++)
                 {
                    int index = j;
                    PlayerReference.GetMovementCardsInHand()[randomCardsToDiscardMovementIndeces[index]].SelectForDiscard();
                    yield return new WaitForSeconds(AIDelaySpeedInSeconds);
                 }
            }

            if(randomCardsToDiscardSupportIndeces.Count != 0)
            {
                PlayerReference.GameplayManagerRef.HandDisplayPanel.ShrinkHand();
                yield return new WaitForSeconds(AIDelaySpeedInSeconds);

                PlayerReference.GameplayManagerRef.HandDisplayPanel.ExpandHand(Card.CardType.Support);
                yield return new WaitForSeconds(AIDelaySpeedInSeconds);

                 for(int k = 0; k < randomCardsToDiscardSupportIndeces.Count; k++)
                 {
                    int index = k;
                    PlayerReference.GetSupportCardsInHand()[randomCardsToDiscardSupportIndeces[index]].SelectForDiscard();
                    yield return new WaitForSeconds(AIDelaySpeedInSeconds);
                 }

            }

            StartCoroutine(SelectFirstOptionDialogueBoxChoice(true));
        }
    }


    #endregion

    #endregion

    #region Duel Phase Methods

    /// <summary>
    /// We want the AI to auto-select a card and essentially add it to their 'selected movement cards' list.
    /// </summary>
    public void SelectMovementCardForDuel(DuelPlayerInformation duelPlayerInformation)
    {
        //Might hafta turn this into another method and wait till an event is triggered to signify the draw being completed.
        if(PlayerReference.MovementCardsInHandCount < 1)
        {
            //If there's no cards in hand, draw the most recent card and select it.
            PlayerReference.GameplayManagerRef.ThisDeckManager.DrawCard(Card.CardType.Movement, PlayerReference);
            foreach (Card card in PlayerReference.CardsInhand)
            {
                if (card is MovementCard)
                {
                    //Dunno if we should curse this card if the Player is cursed since technically it's not a card from their hand...
                    MovementCard movementCard = (MovementCard)card;
                    duelPlayerInformation.SelectedMovementCards.Add(movementCard);
                    //movementCard.AddCardToSelectedCardsDuel(true);
                    Debug.Log($"AI player didn't have a movement card so we will use the newly drawn one from the deck with a value of: {movementCard.MovementCardValue}");
                    break;
                }
            }

        }
        //Randomize picking a card in the player's hand. If they can select more than 1 this is where we will handle that.
        else
        {
            int movementCardToUseIndex = Random.Range(0,PlayerReference.MovementCardsInHandCount);
            MovementCard movementCardToUse = PlayerReference.GetMovementCardsInHand()[movementCardToUseIndex];
            duelPlayerInformation.SelectedMovementCards.Add(movementCardToUse);
            Debug.Log($"AI player {duelPlayerInformation.PlayerInDuel.playerIDIntVal} selected a movement card with value: {movementCardToUse.MovementCardValue} for the duel.");
        }
    }

    public void SelectSupportCardForDuel(DuelPlayerInformation duelPlayerInformation)
    {
        //GO through all support cards in hand, if any are duel phase and cost can be paid then attempt to use them.
        List<SupportCard> validDuelPhaseSupportCards = new();

        foreach(SupportCard card in PlayerReference.GetSupportCardsInHand())
        {
            if(card.SupportCardData.ThisSupportCardType == SupportCardData.SupportCardType.Duel)
            {
                validDuelPhaseSupportCards.Add(card);
            }
        }
        if(validDuelPhaseSupportCards.Count == 0)
        {
            Debug.Log("No valid support cards for the AI to use during duel phase. Aborting random support card usage.");
            SelectMovementCardForDuel(duelPlayerInformation);
            return;
        }

        int randomCardIndex = Random.Range(0, validDuelPhaseSupportCards.Count);
        SupportCard supportCardReference = validDuelPhaseSupportCards[randomCardIndex];

        foreach(SupportCardData.SupportCardEffect supportCardEffect in supportCardReference.SupportCardData.supportCardEffects)
        {
            if(supportCardEffect.supportCardEffectData.IsACost)
            {
                if(!supportCardEffect.supportCardEffectData.CanCostBePaid(duelPlayerInformation, supportCardReference, true))
                {
                    Debug.Log("Cost for valid support card for the AI to use during duel phase could not be paid. Aborting random support card usage.");
                    SelectMovementCardForDuel(duelPlayerInformation);
                    return;
                }
            }
        }

        int indexInHandTouse = PlayerReference.GetSupportCardsInHand().IndexOf(supportCardReference);
        SupportCard supportCardToUse = PlayerReference.GetSupportCardsInHand()[indexInHandTouse];
        duelPlayerInformation.SelectedSupportCards.Add(supportCardToUse);
        Debug.Log($"AI player {duelPlayerInformation.PlayerInDuel.playerIDIntVal} selected the support card: {supportCardToUse.SupportCardData.CardTitle} for the duel.");

        //Might want to make an event here instead of just calling the method...
        SelectMovementCardForDuel(duelPlayerInformation);
    }


    #endregion

    #region Dialogue Box Selection methods

    public IEnumerator SelectFirstOptionDialogueBoxChoice(bool skipDelay = false)
    {
        if(!skipDelay)
        {
            if(Time.timeScale > 1)
            {
                yield return new WaitForSecondsRealtime(1.0f);
            }
            yield return new WaitForSeconds(AIDelaySpeedInSeconds);
        }
        
        List<Button> buttons = DialogueBoxPopup.instance.GetCurrentPopupChoices();
        Button buttonToSelect = buttons[0];
        StartCoroutine(SelectTheButton(buttonToSelect));
    }

    public IEnumerator SelectLastOptionDialogueBoxChoice()
    {
        if(Time.timeScale > 1)
        {
            yield return new WaitForSecondsRealtime(1.0f);
        }

        yield return new WaitForSeconds(AIDelaySpeedInSeconds);
        List<Button> buttons = DialogueBoxPopup.instance.GetCurrentPopupChoices();
        Button buttonToSelect = buttons[buttons.Count-1];
        StartCoroutine(SelectTheButton(buttonToSelect));
    }

    public IEnumerator SelectRandomOptionDialogueBoxChoice()
    {
        if(Time.timeScale > 1)
        {
            yield return new WaitForSecondsRealtime(1.0f);
        }

        yield return new WaitForSeconds(AIDelaySpeedInSeconds);

        List<Button> buttons = DialogueBoxPopup.instance.GetCurrentPopupChoices();
        
        int randomIndex = Random.Range(0, buttons.Count);
        // Debug.Log($"Player is an AI and we are attempting to select a random dialogue box option the value of {randomIndex}");
        Button buttonToSelect = buttons[randomIndex];
        StartCoroutine(SelectTheButton(buttonToSelect));
    }

    public IEnumerator SelectTheButton(Button buttonToSelect)
    {
        Image targetGraphicImage = buttonToSelect.targetGraphic.GetComponent<Image>();
        targetGraphicImage.color = buttonToSelect.colors.highlightedColor;
        if(buttonToSelect.colors.normalColor.a == 0)
        {
            ColorBlock colorBlock = buttonToSelect.colors;
            colorBlock.normalColor = buttonToSelect.colors.highlightedColor;
            buttonToSelect.colors = colorBlock;
            targetGraphicImage.color = new Color(targetGraphicImage.color.r, targetGraphicImage.color.g, targetGraphicImage.color.b, 255f);
        }

        yield return new WaitForSeconds(0.6f);

        buttonToSelect.onClick.Invoke();
    }

    #endregion
}
