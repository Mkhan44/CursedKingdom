//Code written by Mohamed Riaz Khan of BukuGames.
//All code is written by me (Above name) unless otherwise stated via comments below.
//Not authorized for use outside of the Github repository of this game developed by BukuGames.

using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class PlayerAI : MonoBehaviour
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
        yield return new WaitForSeconds(AIDelaySpeedInSeconds);
        playerReference.GameplayManagerRef.HandDisplayPanel.ExpandHand(Card.CardType.Movement);
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
        //Choose ranodmly from both decks.
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

    #region Dialogue Box Selection methods

    public IEnumerator SelectFirstOptionDialogueBoxChoice(bool skipDelay = false)
    {
        if(!skipDelay)
        {
            yield return new WaitForSeconds(AIDelaySpeedInSeconds);
        }
        
        List<Button> buttons = DialogueBoxPopup.instance.GetCurrentPopupChoices();
        buttons[0].onClick.Invoke();
    }

    public IEnumerator SelectLastOptionDialogueBoxChoice()
    {
        yield return new WaitForSeconds(AIDelaySpeedInSeconds);
        List<Button> buttons = DialogueBoxPopup.instance.GetCurrentPopupChoices();
        buttons[buttons.Count-1].onClick.Invoke();
    }

    public IEnumerator SelectRandomOptionDialogueBoxChoice()
    {
        yield return new WaitForSeconds(AIDelaySpeedInSeconds);

        List<Button> buttons = DialogueBoxPopup.instance.GetCurrentPopupChoices();

        int randomIndex = Random.Range(0, buttons.Count);
       // Debug.Log($"Player is an AI and we are attempting to select a random dialogue box option the value of {randomIndex}");
        buttons[randomIndex].onClick.Invoke();
    }

    #endregion
}
