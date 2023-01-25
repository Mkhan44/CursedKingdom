//Code written by Mohamed Riaz Khan of BukuGames.
//All code is written by me (Above name) unless otherwise stated via comments below.
//Not authorized for use outside of the Github repository of this game developed by BukuGames.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public int playerIDIntVal;

    //Properties
    [SerializeField] private int maxHealth;
    [SerializeField] private int currentHealth;
    [SerializeField] private int currentLevel;
    [SerializeField] private int spacesLeftToMove;
    [SerializeField] private int movementCardsInHand;
    [SerializeField] private int supportCardsInHand;
    [SerializeField] private int maxHandSize;
    [SerializeField] private List<Card> cardsInhand;
    [SerializeField] private GameObject cardsInHandHolderPanel;
    [SerializeField] private bool isMoving;
    [SerializeField] private bool isOnCooldown;
    [SerializeField] private bool isPoisoned;
    [SerializeField] private bool isCursed;
    [SerializeField] private int poisonDuration;
    [SerializeField] private int curseDuration;
    [SerializeField] private bool ableToLevelUp;
    [SerializeField] private ClassData classData;
    [SerializeField] private Space currentSpacePlayerIsOn;
    [SerializeField] private Space previousSpacePlayerWasOn;

    //References
    [SerializeField] private GameplayManager gameplayManagerRef;

   
    public int MaxHealth { get => maxHealth; set => maxHealth = value; }
    //Clean this up vvvvv
    public int CurrentHealth { get => currentHealth; set { if ((currentHealth + value) > MaxHealth) currentHealth = maxHealth; else { currentHealth = value; } }  }
    public int CurrentLevel { get => currentLevel; set => currentLevel = value; }
    public int SpacesLeftToMove { get => spacesLeftToMove; set => spacesLeftToMove = value; }
    public int MovementCardsInHand { get => movementCardsInHand; set => movementCardsInHand = value; }
    public int SupportCardsInHand { get => supportCardsInHand; set => supportCardsInHand = value; }
    public int MaxHandSize { get => maxHandSize; set => maxHandSize = value; }
    public List<Card> CardsInhand { get => cardsInhand; set => cardsInhand = value; }

    public GameObject CardsInHandHolderPanel { get => cardsInHandHolderPanel; set => cardsInHandHolderPanel = value; }
    public bool IsMoving { get => isMoving; set => isMoving = value; }
    public bool IsOnCooldown { get => isOnCooldown; set => isOnCooldown = value; }
    public bool IsPoisoned { get => isPoisoned; set => isPoisoned = value; }
    public bool IsCursed { get => isCursed; set => isCursed = value; }
    public int PoisonDuration { get => poisonDuration; set => poisonDuration = value; }
    public int CurseDuration { get => curseDuration; set => curseDuration = value; }
    public bool AbleToLevelUp { get => ableToLevelUp; set => ableToLevelUp = value; }
    public ClassData ClassData { get => classData; set => classData = value; }
    public Space CurrentSpacePlayerIsOn
    {
        get => currentSpacePlayerIsOn; 

        set
        {
            //Check if we have moved to a new part of the board. If we did then increase the rotation. If we hit the center don't do anything...need to account for that though.
            if(currentSpacePlayerIsOn != null)
            {
                //Might want to lerp this so it's not a sudden jump.
                if (value.transform.parent != currentSpacePlayerIsOn.transform.parent && value.transform.parent.name != "LastSpaceHolder")
                {
                    Vector3 thisRot = this.transform.localEulerAngles;
                    float parentRot;
                    parentRot = value.transform.parent.localEulerAngles.y;
                    switch(parentRot)
                    {
                        case 0f:
                            {
                                thisRot.y = 180f;
                                break;
                            }
                        case 90f:
                            {
                                thisRot.y = 270f;
                                break;
                            }
                        case 180f:
                            {
                                thisRot.y = 0f;
                                break;
                            }
                        case 270f:
                            {
                                thisRot.y = 90f;
                                break;
                            }
                            //Nothing needed to change.
                        default:
                            {
                                
                                break;
                            }
                    }
                    
                    transform.localEulerAngles = thisRot;
                }
            }

            currentSpacePlayerIsOn = value;
        } 
    }

    public Space PreviousSpacePlayerWasOn { get => previousSpacePlayerWasOn; set => previousSpacePlayerWasOn = value; }
    public GameplayManager GameplayManagerRef { get => gameplayManagerRef; set => gameplayManagerRef = value; }

    public void InitializePlayer(ClassData data)
    {
        ClassData = data;

        MaxHealth = data.startingHealth;
        currentHealth = maxHealth;
        CurrentLevel = 1;
        AbleToLevelUp = false;
        SpacesLeftToMove = 0;
       // Debug.Log($"Player info: \n health = {CurrentHealth}, level = {CurrentLevel}, \n description: {data.description}");
    }

    public void DebugTheSpace()
    {
        Debug.Log(CurrentSpacePlayerIsOn);
    }

    public void LevelUp()
    {
        CurrentLevel += 1;
        AbleToLevelUp = false;
    }

    public void HandleLevelUp(int level)
    {
        switch(level)
        {
            case 2:
                {
                    Debug.Log("lol");
                    break;
                }
            default:
                {
                    Debug.Log("Def.");
                    break;
                }
        }
    }

    public void DrawCard(Card card)
    {
        CardsInhand.Add(card);
    }

    public void DrawCards(List<Card> cards)
    {
        foreach(Card card in cards)
        {
            CardsInhand.Add(card);
        }
    }

    //Shows this player's hand on screen.
    public void ShowHand()
    {
        foreach(Card card in CardsInhand)
        {
            if(card.ThisCardType == Card.CardType.Movement)
            {
                MovementCard currentMovementCard = card as MovementCard;
                currentMovementCard.RemoveListeners();
                currentMovementCard.AddCardUseListener(GameplayManagerRef);
                card.gameObject.transform.SetParent(CardsInHandHolderPanel.transform);
            }
        }

        cardsInHandHolderPanel.SetActive(true);
    }

    public void HideHand()
    {
        cardsInHandHolderPanel.SetActive(false);
    }

    public void DiscardAfterUse(Card.CardType cardType , Card cardToDiscard)
    {
        GameplayManagerRef.ThisDeckManager.AddCardToDiscardPile(cardType, cardToDiscard, CardsInhand);
    }
}
