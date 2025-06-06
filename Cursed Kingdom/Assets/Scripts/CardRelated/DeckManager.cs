//Code written by Mohamed Riaz Khan of BukuGames.
//All code is written by me (Above name) unless otherwise stated via comments below.
//Not authorized for use outside of the Github repository of this game developed by BukuGames.
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;
using PurrNet;

public class DeckManager : NetworkBehaviour
{
    [SerializeField] private DeckData movementDeckData;
    [SerializeField] private DeckData supportDeckData;
    [SerializeField] private List<MovementCard> movementDeckList;
    [SerializeField] private List<SupportCard> supportDeckList;
    // [SerializeField] private List<Supportcard> supportCards;

    //Discard piles
    [SerializeField] private List<MovementCard> movementDiscardPileList;
    [SerializeField] private List<SupportCard> supportDiscardPileList;

    [SerializeField] private bool isDiscarding;

    //References
    [SerializeField] private GameplayManager gameplayManager;

    public DeckData MovementDeckData { get => movementDeckData; set => movementDeckData = value; }
    public DeckData SupportDeckData { get => supportDeckData; set => supportDeckData = value; }
    public List<MovementCard> MovementDeckList { get => movementDeckList; set => movementDeckList = value; }
    public List<SupportCard> SupportDeckList { get => supportDeckList; set => supportDeckList = value; }
    public List<MovementCard> MovementDiscardPileList { get => movementDiscardPileList; set => movementDiscardPileList = value; }
    public List<SupportCard> SupportDiscardPileList { get => supportDiscardPileList; set => supportDiscardPileList = value; }
    public bool IsDiscarding { get => isDiscarding; set => isDiscarding = value; }

    //References
    public GameplayManager GameplayManager { get => gameplayManager; set => gameplayManager = value; }

    private void Awake()
    {
        GameplayManager = this.GetComponent<GameplayManager>();
        IsDiscarding = false;
    }

    protected override void OnSpawned(bool asServer)
    {
        base.OnSpawned(asServer);

        enabled = isOwner;
        
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="gameplayManagerRef"></param>
    public void CreateDeck()
    {
        if (MovementDeckList.Count == 0)
        {
            MovementDeckList = new();
            List<Card> movementDeckBuilder = new();

            if (MovementDeckData is not null)
            {
                foreach (MovementCardData movementCardData in MovementDeckData.MovementCardDatas)
                {
                    GameObject tempCard = Instantiate(GameplayManager.MovementCardPrefab);
                    MovementCard movementCard = tempCard.GetComponent<MovementCard>();
                    movementCard.GameplayManager = gameplayManager;
                    movementCard.CardDataSetup(movementCardData);
                    tempCard.name = movementCardData.name;
                    tempCard.transform.SetParent(GameplayManager.MovementDeckCardHolder.transform);
                    movementDeckBuilder.Add(movementCard);
                }

                AddToDeck(Card.CardType.Movement, movementDeckBuilder);

            }
        }

        if(SupportDeckList.Count == 0)
        {
            SupportDeckList = new();
            List<Card> supportDeckBuilder = new();

            if(SupportDeckData is not null)
            {
                foreach(SupportCardData supportCardData in SupportDeckData.SupportCardDatas)
                {
                    GameObject tempCard = Instantiate(GameplayManager.SupportCardPrefab);
                    SupportCard supportCard = tempCard.GetComponent<SupportCard>();
                    supportCard.GameplayManager = gameplayManager;
                    supportCard.CardDataSetup(supportCardData);
                    tempCard.name = supportCardData.name;
                    tempCard.transform.SetParent(GameplayManager.SupportDeckCardHolder.transform);
                    supportDeckBuilder.Add(supportCard);
                }

                AddToDeck(Card.CardType.Support, supportDeckBuilder);
            }
        }
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="deckType"></param>
    /// <param name="cardsToAdd"></param>
    public void AddToDeck(Card.CardType deckType, List<Card> cardsToAdd)
    {
        if (deckType == Card.CardType.Movement)
        {
            foreach (Card card in cardsToAdd)
            {
                MovementCard currentMovementCard = card as MovementCard;

                if (currentMovementCard != null)
                {
                    MovementDeckList.Add(currentMovementCard);
                }
                else
                {
                    Debug.LogWarning("Incorrect typing of Card.");
                }
            }
        }
        //Support deck.
        else
        {
            foreach (Card card in cardsToAdd)
            {
                SupportCard currentSupportCard = card as SupportCard;

                if (currentSupportCard != null)
                {
                    SupportDeckList.Add(currentSupportCard);
                }
                else
                {
                    Debug.LogWarning("Incorrect typing of Card.");
                }
            }
        }
    }

    public Card LookAtNextCardInDeck(Card.CardType deckToLookAt)
    {
        Card cardToReturn = null;
        if (deckToLookAt == Card.CardType.Movement)
        {
            if (MovementDeckList.Count == 0)
            {
                Debug.LogWarning($"{nameof(MovementDeckList)} is out of cards! The next card is nothing.");
            }
            else
            {
                cardToReturn = MovementDeckList[0];
            }
        }
        else if (deckToLookAt == Card.CardType.Support)
        {
            if (SupportDeckList.Count == 0)
            {
                Debug.LogWarning($"{nameof(SupportDeckList)} is out of cards! The next card is nothing.");
            }
            else
            {
                cardToReturn = SupportDeckList[0];
            }
        }
        
        return cardToReturn;
    }

    public void DiscardNextCardInDeck(Card.CardType deckToDiscardFrom)
    {
        
    }

    public void DrawCard(Card.CardType deckTypeToDrawFrom, Player playerDrawingCard)
    {
        if (deckTypeToDrawFrom == Card.CardType.Movement)
        {
            if (MovementDeckList.Count == 0)
            {
                //Shuffle discard pile into the deck.
                ShuffleDeck(deckTypeToDrawFrom, true);
                Debug.LogWarning($"{nameof(MovementDeckList)} is out of cards! Shuffling the deck...");
            }
            playerDrawingCard.DrawCard(MovementDeckList[0], true);
            MovementDeckList.Remove(MovementDeckList[0]);

            //End of turn darw
            if(!playerDrawingCard.IsHandlingSpaceEffects && !playerDrawingCard.IsHandlingSupportCardEffects && playerDrawingCard.MaxHandSizeExceeded())
            {
                gameplayManager.UpdatePlayerInfoUICardCount(playerDrawingCard);
                IsDiscarding = true;
            }
        }
        else
        {
            if (SupportDeckList.Count == 0)
            {
                //Shuffle discard pile into the deck.
                ShuffleDeck(deckTypeToDrawFrom, true);
                Debug.LogWarning($"{nameof(SupportDeckList)} is out of cards! Shuffling the deck...");
            }
            playerDrawingCard.DrawCard(SupportDeckList[0], true);
            SupportDeckList.Remove(SupportDeckList[0]);
        }

        gameplayManager.UpdatePlayerInfoUICardCount(playerDrawingCard);

        if ((playerDrawingCard.IsHandlingSpaceEffects  || playerDrawingCard.IsHandlingSupportCardEffects) && !playerDrawingCard.MaxHandSizeExceeded())
        {
            playerDrawingCard.CompletedDrawingForEffect();
        }
    }

    public void DrawCards(Card.CardType deckTypeToDrawFrom, Player playerDrawingCard, int numCardsToDraw)
    {
        List<Card> cardsToDraw = new();
        if (deckTypeToDrawFrom == Card.CardType.Movement)
        {
            if (MovementDeckList.Count < numCardsToDraw)
            {
                //Shuffle discard pile into the deck.
                ShuffleDeck(deckTypeToDrawFrom, true);
                Debug.LogWarning($"{nameof(MovementDeckList)} has {MovementDeckList.Count} cards and we need to draw {numCardsToDraw}! Shuffling the deck...");
            }
            for (int i = 0; i < numCardsToDraw; i++)
            {
                //Always do 0 since we're removing it immediately after...Though maybe this might not be smart since it's so fast...?
                cardsToDraw.Add(MovementDeckList[0]);
                MovementDeckList.Remove(MovementDeckList[0]);
            }

        }
        else
        {
            if (SupportDeckList.Count < numCardsToDraw)
            {
                //Shuffle discard pile into the deck.
                ShuffleDeck(deckTypeToDrawFrom, true);
                Debug.LogWarning($"{SupportDeckList} has {SupportDeckList.Count} cards and we need to draw {numCardsToDraw}! Shuffling the deck...");
            }
            for (int i = 0; i < numCardsToDraw; i++)
            {
                //Always do 0 since we're removing it immediately after...Though maybe this might not be smart since it's so fast...?
                cardsToDraw.Add(SupportDeckList[0]);
                SupportDeckList.Remove(SupportDeckList[0]);
            }
        }

        if(cardsToDraw.Count > 0)
        {
            playerDrawingCard.DrawCards(cardsToDraw, true);
        }

        gameplayManager.UpdatePlayerInfoUICardCount(playerDrawingCard);

        if (playerDrawingCard.IsHandlingSpaceEffects && !playerDrawingCard.MaxHandSizeExceeded())
        {
            playerDrawingCard.CompletedDrawingForEffect();
        }

    }

    public void DrawCardsFromDiscardPile(Card.CardType deckTypeToDrawFrom, Player playerDrawingCard, int numCardsToDraw)
    {
        List<Card> cardsToDraw = new();
        if (deckTypeToDrawFrom == Card.CardType.Movement)
        {
            if (MovementDiscardPileList.Count < numCardsToDraw)
            {
                Debug.LogWarning($"We're trying to draw {numCardsToDraw} cards from the Movement discard pile but there are only {MovementDiscardPileList.Count} cards to draw. Aborting draw attempt.");
                if (playerDrawingCard.IsHandlingSpaceEffects && !playerDrawingCard.MaxHandSizeExceeded())
                {
                    playerDrawingCard.CompletedDrawingForEffect();
                    return;
                }
            }
            for (int i = 0; i < numCardsToDraw; i++)
            {
                //We want to draw from the top, so we use the last one.
                cardsToDraw.Add(MovementDiscardPileList.Last());
                MovementDiscardPileList.Remove(MovementDiscardPileList.Last());
            }

        }
        else
        {
            if (MovementDiscardPileList.Count < numCardsToDraw)
            {
                Debug.LogWarning($"We're trying to draw {numCardsToDraw} cards from the Support discard pile but there are only {SupportDiscardPileList.Count} cards to draw. Aborting draw attempt.");
                if (playerDrawingCard.IsHandlingSpaceEffects && !playerDrawingCard.MaxHandSizeExceeded())
                {
                    playerDrawingCard.CompletedDrawingForEffect();
                    return;
                }
            }
            for (int i = 0; i < numCardsToDraw; i++)
            {
                //We want to draw from the top, so we use the last one.
                cardsToDraw.Add(SupportDiscardPileList.Last());
                SupportDiscardPileList.Remove(SupportDiscardPileList.Last());
            }
        }

        if(cardsToDraw.Count > 0)
        {
            playerDrawingCard.DrawCards(cardsToDraw, true);
        }

        gameplayManager.UpdatePlayerInfoUICardCount(playerDrawingCard);

        if (playerDrawingCard.IsHandlingSpaceEffects && !playerDrawingCard.MaxHandSizeExceeded())
        {
            playerDrawingCard.CompletedDrawingForEffect();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="deckType"></param>
    public void ShuffleDeck(Card.CardType deckType, bool shuffleDiscardPileIntoDeck = false)
    {
        //Add the discard pile into the deck, then shuffle everything.
        if (shuffleDiscardPileIntoDeck)
        {
            if (deckType == Card.CardType.Movement)
            {
                MovementDeckList.AddRange(MovementDiscardPileList);
                MovementDiscardPileList.Clear();
            }
            else
            {
                SupportDeckList.AddRange(SupportDiscardPileList);
                SupportDiscardPileList.Clear();
            }
        }
        if (deckType == Card.CardType.Movement)
        {
            // Debug.Log("Before shuffle -> " + string.Join(", ", MovementDeckList));
            int count = MovementDeckList.Count;

            for (int i = 0; i < count; i++)
            {
                MovementCard temp = MovementDeckList[i];
                int randomNum = Random.Range(1, MovementDeckList.Count);
                MovementDeckList[i] = MovementDeckList[randomNum];
                MovementDeckList[randomNum] = temp;
            }

           

           // Debug.Log("After shuffle -> " + string.Join(", ", MovementDeckList));
        }

        else
        {
            // Debug.Log("Before shuffle -> " + string.Join(", ", SupportDeckList));
            int count = SupportDeckList.Count;

            for (int i = 0; i < count; i++)
            {
                SupportCard temp = SupportDeckList[i];
                int randomNum = Random.Range(1, SupportDeckList.Count);
                SupportDeckList[i] = SupportDeckList[randomNum];
                SupportDeckList[randomNum] = temp;
            }

           
           // Debug.Log("After shuffle -> " + string.Join(", ", SupportDeckList));
        }

        ResetDeckTransformsHierarchy();
    }

    /// <summary>
    /// Reset the transforms of cards in the decks in the hierarchy for viewing purposes.
    /// </summary>
    public void ResetDeckTransformsHierarchy()
    {
        foreach (Card card in MovementDeckList)
        {
            card.gameObject.transform.SetParent(gameplayManager.MovementDeckCardHolder.transform);
        }

        foreach (Card card in SupportDeckList)
        {
            card.gameObject.transform.SetParent(gameplayManager.MovementDeckCardHolder.transform);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="cardTypeToDiscard"></param>
    /// <param name="cardReferenceToDiscard"></param>
    /// <param name="placeToDiscardCardFrom"> The list we're discarding from. This could be the deck, the player's hand, etc.</param>
    public void AddCardToDiscardPile(Card.CardType cardTypeToDiscard, Card cardReferenceToDiscard, List<Card> placeToDiscardCardFrom)
    {
        cardReferenceToDiscard.DeselectCard();

        if(cardTypeToDiscard == Card.CardType.Movement)
        {
            MovementCard movementCard = cardReferenceToDiscard as MovementCard;
            if(placeToDiscardCardFrom.Contains(movementCard))
            {
                placeToDiscardCardFrom.Remove(movementCard);
            }
            else
            {
                Debug.LogWarning($"Hey, we could not find {cardReferenceToDiscard.name} in {nameof(placeToDiscardCardFrom)}.");
                return;
            }

            MovementDiscardPileList.Add(movementCard);
            movementCard.transform.SetParent(gameplayManager.MovementCardDiscardPileHolder.transform);
        }

        else
        {
            SupportCard supportCard = cardReferenceToDiscard as SupportCard;
            if (placeToDiscardCardFrom.Contains(supportCard))
            {
                placeToDiscardCardFrom.Remove(supportCard);
            }
            else
            {
                Debug.LogWarning($"Hey, we could not find {cardReferenceToDiscard.name} in {nameof(placeToDiscardCardFrom)}.");
                return;
            }

            SupportDiscardPileList.Add(supportCard);
            supportCard.transform.SetParent(gameplayManager.SupportCardDiscardPileHolder.transform);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="cardTypeToDiscard"></param>
    /// <param name="cardReference"></param>
    /// <param name="placeToDiscardCardFrom"> The list we're discarding from. This could be the deck, the player's hand, etc.</param>
    public void AddCardsToDiscardPile(Card.CardType cardTypeToDiscard, List<Card> cardReferencesToDiscard, List<Card> placeToDiscardCardFrom)
    {
        foreach(Card card in cardReferencesToDiscard)
        {
            card.DeselectDueToDiscardingCardToDiscardPile();
        }

        if (cardTypeToDiscard == Card.CardType.Movement)
        {
            for(int i = 0; i < cardReferencesToDiscard.Count; i++)
            {
                MovementCard currentMovementCard = cardReferencesToDiscard[i] as MovementCard;

                if (placeToDiscardCardFrom.Contains(currentMovementCard))
                {
                    placeToDiscardCardFrom.Remove(currentMovementCard);
                }
                else
                {
                    Debug.LogWarning($"Hey, we could not find {currentMovementCard} in {nameof(placeToDiscardCardFrom)}.");
                    return;
                }

                MovementDiscardPileList.Add(currentMovementCard);
                currentMovementCard.transform.SetParent(gameplayManager.MovementCardDiscardPileHolder.transform);
            }
            
        }

        else
        {
            for (int i = 0; i < cardReferencesToDiscard.Count; i++)
            {
                SupportCard currentSupportCard = cardReferencesToDiscard[i] as SupportCard;

                if (placeToDiscardCardFrom.Contains(currentSupportCard))
                {
                    placeToDiscardCardFrom.Remove(currentSupportCard);
                }
                else
                {
                    Debug.LogWarning($"Hey, we could not find {currentSupportCard} in {nameof(placeToDiscardCardFrom)}.");
                    return;
                }

                SupportDiscardPileList.Add(currentSupportCard);
                currentSupportCard.transform.SetParent(gameplayManager.SupportCardDiscardPileHolder.transform);
            }
        }
    }
}
