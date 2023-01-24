//Code written by Mohamed Riaz Khan of BukuGames.
//All code is written by me (Above name) unless otherwise stated via comments below.
//Not authorized for use outside of the Github repository of this game developed by BukuGames.
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class DeckManager : MonoBehaviour
{
    [SerializeField] private DeckData movementDeckData;
    [SerializeField] private DeckData supportDeckData;
    [SerializeField] private List<MovementCard> movementDeckList;
    [SerializeField] private List<SupportCard> supportDeckList;
    // [SerializeField] private List<Supportcard> supportCards;

    //Discard piles
    [SerializeField] private List<MovementCard> movementDiscardPileList;
    [SerializeField] private List<SupportCard> supportDiscardPileList;

    public DeckData MovementDeckData { get => movementDeckData; set => movementDeckData = value; }
    public DeckData SupportDeckData { get => supportDeckData; set => supportDeckData = value; }
    public List<MovementCard> MovementDeckList { get => movementDeckList; set => movementDeckList = value; }
    public List<SupportCard> SupportDeckList { get => supportDeckList; set => supportDeckList = value; }
    public List<MovementCard> MovementDiscardPileList { get => movementDiscardPileList; set => movementDiscardPileList = value; }
    public List<SupportCard> SupportDiscardPileList { get => supportDiscardPileList; set => supportDiscardPileList = value; }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="gameplayManagerRef"></param>
    public void CreateDeck(GameplayManager gameplayManagerRef)
    {
        if(MovementDeckList.Count == 0)
        {
            MovementDeckList = new();
            List<Card> movementDeckBuilder = new();

            if(movementDeckData is not null)
            {
                foreach(MovementCardData movementCardData in movementDeckData.MovementCardDatas)
                {
                    GameObject tempCard = Instantiate(gameplayManagerRef.MovementCardPrefab);
                    MovementCard movementCard = tempCard.GetComponent<MovementCard>();
                    movementCard.CardDataSetup(movementCardData);
                    tempCard.name = movementCardData.name;
                    tempCard.transform.SetParent(gameplayManagerRef.MovementDeckCardHolder.transform);
                    movementDeckBuilder.Add(movementCard);
                }

                AddToDeck(Card.CardType.Movement, movementDeckBuilder);
                
            }
        }

        //Same thing for support cards copy pasta...
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="deckType"></param>
    public void ShuffleDeck(Card.CardType deckType)
    {
        if(deckType == Card.CardType.Movement)
        {
            List<MovementCard> currentDeck = MovementDeckList;
            int count = currentDeck.Count;
            List<MovementCard> randomizedDeck = new();

            for(int i = 0; i < count; i++)
            {
                MovementCard temp = currentDeck[i];
                int randomNum = Random.Range(1, currentDeck.Count);
                currentDeck[i] = currentDeck[randomNum];
                currentDeck[randomNum] = temp;
            }

            Debug.Log("Before shuffle -> " + string.Join(", ", movementDeckList));
            movementDeckList = currentDeck;
            Debug.Log("After shuffle -> " + string.Join(", ", movementDeckList));
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="deckType"></param>
    /// <param name="cardsToAdd"></param>
    public void AddToDeck(Card.CardType deckType, List<Card> cardsToAdd)
    {
        if(deckType == Card.CardType.Movement)
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

    public void DrawCard(Card.CardType deckTypeToDrawFrom, Player playerDrawingCard)
    {
        if(deckTypeToDrawFrom == Card.CardType.Movement)
        {
            if(MovementDeckList.Count == 0)
            {
                //Shuffle discard pile into deck.
                Debug.LogWarning($"{MovementDeckList} is out of cards! Shuffling the deck...");
            }
            playerDrawingCard.DrawCard(MovementDeckList[0]);
            MovementDeckList.Remove(MovementDeckList[0]);
        }
        else
        {
            //if (SupportDeckList.Count == 0)
            //{
            //    //Shuffle discard pile into deck.
            //    Debug.LogWarning($"{SupportDeckList} is out of cards!");
            //}
            //playerDrawingCard.DrawCard(SupportDeckList[0]);
            //MovementDeckList.Remove(SupportDeckList[0]);
        }
    }

    public void DrawCards(Card.CardType deckTypeToDrawFrom, Player playerDrawingCard, int numCardsToDraw)
    {
        if (deckTypeToDrawFrom == Card.CardType.Movement)
        {
            if (MovementDeckList.Count < numCardsToDraw)
            {
                //Shuffle discard pile into the deck.
                Debug.LogWarning($"{MovementDeckList} has {MovementDeckList.Count} cards and we need to draw {numCardsToDraw}! Shuffling the deck...");
            }
            for(int i = 0; i < numCardsToDraw; i++)
            {
                //Always do 0 since we're removing it immediately after...Though maybe this might not be smart since it's so fast...?
                playerDrawingCard.DrawCard(MovementDeckList[0]);
                MovementDeckList.Remove(MovementDeckList[0]);
            }

        }
        else
        {
            if (SupportDeckList.Count < numCardsToDraw)
            {
                //Shuffle discard pile into the deck.
                Debug.LogWarning($"{SupportDeckList} has {SupportDeckList.Count} cards and we need to draw {numCardsToDraw}! Shuffling the deck...");
            }
            for (int i = 0; i < numCardsToDraw; i++)
            {
                //Always do 0 since we're removing it immediately after...Though maybe this might not be smart since it's so fast...?
                playerDrawingCard.DrawCard(SupportDeckList[0]);
                SupportDeckList.Remove(SupportDeckList[0]);
            }
        }

    }
}
