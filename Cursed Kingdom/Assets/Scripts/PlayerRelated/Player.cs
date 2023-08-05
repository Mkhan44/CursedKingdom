//Code written by Mohamed Riaz Khan of BukuGames.
//All code is written by me (Above name) unless otherwise stated via comments below.
//Not authorized for use outside of the Github repository of this game developed by BukuGames.

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Card;

public class Player : MonoBehaviour
{
    public int playerIDIntVal;

    //Events

    //Discard events
    public event Action<Player> TurnHasEnded;
    public event Action<Player> EffectCompleted;
    public event Action<Player> DoneDiscardingForEffect;

    //Events End

    //Properties
    [SerializeField] private int maxHealth;
    [SerializeField] private int currentHealth;
    [SerializeField] private int currentLevel;
    [SerializeField] private int spacesLeftToMove;
    [SerializeField] private int movementCardsInHand;
    [SerializeField] private int supportCardsInHand;
    [SerializeField] private int maxHandSize;
    [SerializeField] private int cardsLeftToDiscard;
    [SerializeField] private CardType validCardTypesToDiscard;
    [SerializeField] private List<Card> cardsInhand;
    [SerializeField] private GameObject movementCardsInHandHolderPanel;
    [SerializeField] private GameObject supportCardsInHandHolderPanel;
    [SerializeField] private GameObject handDisplayPanel;
    [SerializeField] private Queue<SpaceEffectData> spaceEffectsToHandle;
    [SerializeField] private bool isHandlingSpaceEffects;
    [SerializeField] private bool isMoving;
    [SerializeField] private bool isOnCooldown;
    [SerializeField] private bool isPoisoned;
    [SerializeField] private bool isCursed;
    [SerializeField] private bool isDefeated;
    [SerializeField] private bool wasAfflictedWithStatusThisTurn;
    [SerializeField] private int poisonDuration;
    [SerializeField] private int curseDuration;
    [SerializeField] private bool ableToLevelUp;
    [SerializeField] private ClassData classData;
    [SerializeField] private Space currentSpacePlayerIsOn;
    [SerializeField] private Space previousSpacePlayerWasOn;
    

    //References
    [SerializeField] private GameplayManager gameplayManagerRef;
    [SerializeField] private RuntimeAnimatorController animatorController;
    [SerializeField] private Animator animator;

   
    public int MaxHealth { get => maxHealth; set => maxHealth = value; }
    public int CurrentHealth { get => currentHealth; set => currentHealth = value; }
    //Clean this up vvvvv
    
    public int CurrentLevel { get => currentLevel; set => currentLevel = value; }
    public int SpacesLeftToMove { get => spacesLeftToMove; set => spacesLeftToMove = value; }
    public int MovementCardsInHand { get => movementCardsInHand; set => movementCardsInHand = value; }
    public int SupportCardsInHand { get => supportCardsInHand; set => supportCardsInHand = value; }
    public int MaxHandSize { get => maxHandSize; set => maxHandSize = value; }
    public int CardsLeftToDiscard { get => cardsLeftToDiscard; set => cardsLeftToDiscard = value; }
    public CardType ValidCardTypesToDiscard { get => validCardTypesToDiscard; set => validCardTypesToDiscard = value; }
    public List<Card> CardsInhand { get => cardsInhand; set => cardsInhand = value; }
    public GameObject MovementCardsInHandHolderPanel { get => movementCardsInHandHolderPanel; set => movementCardsInHandHolderPanel = value; }
    public GameObject SupportCardsInHandHolderPanel { get => supportCardsInHandHolderPanel; set => supportCardsInHandHolderPanel = value; }
    public GameObject HandDisplayPanel { get => handDisplayPanel; set => handDisplayPanel = value; }
    public Queue<SpaceEffectData> SpaceEffectsToHandle { get => spaceEffectsToHandle; set => spaceEffectsToHandle = value; }
    public bool IsHandlingSpaceEffects { get => isHandlingSpaceEffects; set => isHandlingSpaceEffects = value; }
    public bool IsMoving { get => isMoving; set => isMoving = value; }
    public bool IsOnCooldown { get => isOnCooldown; set => isOnCooldown = value; }
    public bool IsPoisoned { get => isPoisoned; set => isPoisoned = value; }
    public bool IsCursed { get => isCursed; set => isCursed = value; }
    public bool IsDefeated { get => isDefeated; set => isDefeated = value; }
    public bool WasAfflictedWithStatusThisTurn { get => wasAfflictedWithStatusThisTurn; set => wasAfflictedWithStatusThisTurn = value; }
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
                                thisRot.y = 0f;
                                break;
                            }
                        case 90f:
                            {
                                thisRot.y = 90f;
                                break;
                            }
                        case 180f:
                            {
                                thisRot.y = 180f;
                                break;
                            }
                        case 270f:
                            {
                                thisRot.y = 270f;
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
    public RuntimeAnimatorController AnimatorController { get => animatorController; set => animatorController = value; }
    public Animator Animator { get => animator; set => animator = value; }

    public void InitializePlayer(ClassData data)
    {
        ClassData = data;

        MaxHealth = data.startingHealth;
        CurrentHealth = maxHealth;
        CurrentLevel = 1;
        AbleToLevelUp = false;
        SpacesLeftToMove = 0;
        CardsLeftToDiscard = 0;
        ValidCardTypesToDiscard = CardType.None;
        SpaceEffectsToHandle = new();
        isHandlingSpaceEffects = false;
       // Debug.Log($"Player info: \n health = {CurrentHealth}, level = {CurrentLevel}, \n description: {data.description}");
    }

    //Debug version where we hardcode the class.
    public void InitializePlayer()
    {
        MaxHealth = ClassData.startingHealth;
        CurrentHealth = maxHealth;
        CurrentLevel = 1;
        AbleToLevelUp = true;
        SpacesLeftToMove = 0;
        MaxHandSize = 6;
        IsDefeated = false;
        CardsLeftToDiscard = 0;
        ValidCardTypesToDiscard = CardType.None;
        SpaceEffectsToHandle = new();
        isHandlingSpaceEffects = false;
        // Debug.Log($"Player info: \n health = {CurrentHealth}, level = {CurrentLevel}, \n description: {data.description}");
    }

    public void DebugTheSpace()
    {
        //Debug.Log(CurrentSpacePlayerIsOn);
    }

    public void LevelUp(int levelsToIncrease)
    {
        CurrentLevel += levelsToIncrease;
        if(CurrentLevel > 5) 
        {
            CurrentLevel = 5;
        }

        HandleLevelUp();
        AbleToLevelUp = false;

        GameplayManagerRef.UpdatePlayerInfoUICardCount(this);

    }

    public void HandleLevelUp()
    {
        GameplayManagerRef.UpdatePlayerLevel(this);
        GameplayManagerRef.UpdatePlayerInfoUICardCount(this);
    }

    public void DrawCard(Card card)
    {
        CardsInhand.Add(card);

        if (MaxHandSizeExceeded())
        {
            GameplayManagerRef.ThisDeckManager.IsDiscarding = true;
        }

        SetMovementCardsInHand();
        SetSupportCardsInHand();
    }

    public void DrawCards(List<Card> cards)
    {
        foreach(Card card in cards)
        {
            CardsInhand.Add(card);
        }

        if(MaxHandSizeExceeded())
        {
            GameplayManagerRef.ThisDeckManager.IsDiscarding = true;
        }

        SetMovementCardsInHand();
        SetSupportCardsInHand();

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
                card.gameObject.transform.SetParent(MovementCardsInHandHolderPanel.transform);
                card.transform.localScale = Vector3.one;
            }
            else
            {
                SupportCard currentSupportCard = card as SupportCard;
                currentSupportCard.RemoveListeners();
                currentSupportCard.AddCardUseListener(GameplayManagerRef);
                card.gameObject.transform.SetParent(SupportCardsInHandHolderPanel.transform);
                card.transform.localScale = Vector3.one;
            }
        }

        MovementCardsInHandHolderPanel.SetActive(true);
        SupportCardsInHandHolderPanel.SetActive(true);
    }

    public void HideHand()
    {
        //GameplayManagerRef.HandDisplayPanel.ShrinkHand(true);
        MovementCardsInHandHolderPanel.SetActive(false);
        SupportCardsInHandHolderPanel.SetActive(false);
    }


    public bool MaxHandSizeExceeded()
    {
        bool result = false;

        if(CardsInhand.Count > MaxHandSize)
        {
            GameplayManagerRef.OpenDebugMessenger($"Hand size exceeded. You have {CardsInhand.Count} cards in your hand. Please discard {CardsInhand.Count -  MaxHandSize} cards.");
            CardsLeftToDiscard = CardsInhand.Count - MaxHandSize;
            result = true;
        }
        else
        {
            result = false;
        }

        return result;
    }

    /// <summary>
    /// Check if the player has enough cards to discard for the cost or effect they are dealing with.
    /// </summary>
    /// <param name="cardType"></param>
    /// <param name="numToDiscard"></param>
    /// <returns></returns>
    public bool CheckIfEnoughCardsToDiscard(CardType cardType, int numToDiscard)
    {
        bool hasEnough = false;

        switch(cardType) 
        {
            case CardType.Movement:
                {
                    if(!(MovementCardsInHand < numToDiscard))
                    {
                        hasEnough = true;
                    }
                    break;
                }
            case CardType.Support:
                {
                    if (!(SupportCardsInHand < numToDiscard))
                    {
                        hasEnough = true;
                    }
                    break;
                }
            case CardType.Both:
                {
                    if (!(CardsInhand.Count < numToDiscard))
                    {
                        hasEnough = true;
                    }
                    break;
                }
            default:
                {
                    Debug.LogWarning("The type is not a valid card type! What was passed into the check function?");
                    break;
                }
        }

        return hasEnough;
    }

    /// <summary>
    /// Sets the type of card to discard as well as number needed to discard. This is usually used to pay a cost or after landing on a space.
    /// </summary>
    /// <param name="cardType"></param>
    /// <param name="numToDiscard"></param>
    public void SetCardsToDiscard(CardType cardType, int numToDiscard)
    {
        ValidCardTypesToDiscard = cardType;
        CardsLeftToDiscard = numToDiscard;
    }

    public void SelectCardToDiscard()
    {
        CardsLeftToDiscard -= 1;

        //Confirmation for discard.
        if( CardsLeftToDiscard == 0)
        {
            List<Tuple<string, string, object>> insertedParams = new();
            insertedParams.Add(Tuple.Create<string, string, object>("Yes", "DiscardTheSelectedCards", this));
            insertedParams.Add(Tuple.Create<string, string, object>("No", "DeselectAllSelectedCards", this));

            DialogueBoxPopup.instance.ActivatePopupWithButtonChoices("Are you sure you want to discard these cards?", insertedParams);
        }
    }

    public void DeselectAllSelectedCards()
    {
        foreach (Card card in CardsInhand)
        {
            if (card.SelectedForDiscard)
            {
                card.DeselectForDiscard();
                if(CardsLeftToDiscard == 0)
                {
                    CardsLeftToDiscard += 1;
                }
            }
        }
    }

    public void DiscardTheSelectedCards()
    {
        List<Card> cardsToDiscard = new();

        foreach(Card card in CardsInhand)
        {
            if(card.SelectedForDiscard)
            {
                cardsToDiscard.Add(card);
            }
        }


        //Loop through all the cards to discard and remove them from the list one by one.
        while(cardsToDiscard.Count > 0)
        {
            cardsToDiscard[0].DeselectForDiscard();
            DiscardFromHand(cardsToDiscard[0].ThisCardType, cardsToDiscard[0]);
            cardsToDiscard.RemoveAt(0);
        }

        CompletedDiscardingForEffect();
    }

    public void DiscardFromHand(Card.CardType cardType , Card cardToDiscard)
    {
        GameplayManagerRef.ThisDeckManager.AddCardToDiscardPile(cardType, cardToDiscard, CardsInhand);
        if(cardType == Card.CardType.Movement)
        {
            SetMovementCardsInHand();
        }
        else
        {
            SetSupportCardsInHand();
        }
    }

    public void DiscardAllCardsInHand()
    {
        while (CardsInhand.Count > 0)
        {
            GameplayManagerRef.ThisDeckManager.AddCardToDiscardPile(CardsInhand[0].ThisCardType, CardsInhand[0], CardsInhand);
        }

        SetMovementCardsInHand();
        SetSupportCardsInHand();
    }

    public void DiscardCardToGetToMaxHandSize(Card.CardType cardType, Card cardToDiscard)
    {

        if (CardsLeftToDiscard > 0)
        {
            DiscardFromHand(cardType, cardToDiscard);
            CardsLeftToDiscard -= 1;
            if(CardsLeftToDiscard <= 0)
            {
                GameplayManagerRef.CloseDebugMessengerPanel();
            }
            else
            {
                GameplayManagerRef.OpenDebugMessenger($"Hand size exceeded. You have {CardsInhand.Count} cards in your hand. Please discard {CardsInhand.Count - MaxHandSize} cards.");
            }
        }
        else
        {
            Debug.LogWarning("Trying to discard but handsize isn't above max amount!");
        }

        GameplayManagerRef.ThisDeckManager.IsDiscarding = false;
    }

    public void DiscardCardsToGetToMaxHandSize(Card.CardType cardType, List<Card> cardsToDiscard)
    {
        if(CardsLeftToDiscard > 0)
        {
            foreach(Card card in cardsToDiscard) 
            {
                DiscardFromHand(cardType, card);
            }

            CardsLeftToDiscard -= cardsToDiscard.Count;
            if (CardsLeftToDiscard <= 0)
            {
                GameplayManagerRef.CloseDebugMessengerPanel();
            }
            else
            {
                GameplayManagerRef.OpenDebugMessenger($"Hand size exceeded. You have {CardsInhand.Count} cards in your hand. Please discard {CardsInhand.Count - MaxHandSize} cards.");
            }
        }
        else
        {
            Debug.LogWarning("Trying to discard but handsize isn't above max amount!");
        }

        GameplayManagerRef.ThisDeckManager.IsDiscarding = false;
    }

    public void SetMovementCardsInHand()
    {
        MovementCardsInHand = 0;
        foreach (Card card in CardsInhand)
        {
            if(card is MovementCard)
            {
                card.gameObject.transform.SetParent(MovementCardsInHandHolderPanel.transform);
                MovementCardsInHand++;
            }
        }

        if(GameplayManagerRef is not null)
        {
            GameplayManagerRef.UpdatePlayerInfoUICardCount(this);
        }
        
    }

    public void SetSupportCardsInHand()
    {
        SupportCardsInHand = 0;
        foreach (Card card in CardsInhand)
        {
            if (card is SupportCard)
            {
                card.gameObject.transform.SetParent(SupportCardsInHandHolderPanel.transform);
                SupportCardsInHand++;
            }
        }

        if (GameplayManagerRef is not null)
        {
            GameplayManagerRef.UpdatePlayerInfoUICardCount(this);
        }
    }

    #region StatusEffects

    public virtual void CursePlayer(int numTurnsToCurse)
    {
        CursePlayerPriv(numTurnsToCurse);
    }

    public virtual bool CanBeCursed()
    {
        bool canBeCursed = false;
        if (IsPoisoned || IsCursed)
        {
            canBeCursed = false;
        }
        else
        {
            canBeCursed = true;
        }

        return canBeCursed;
    }

    public virtual void PoisonPlayer(int numTurnsToPoison)
    {
        PoisonPlayerPriv(numTurnsToPoison);
    }

    public virtual bool CanBePoisoned()
    {
        bool canBePoisoned = false;
        if (IsPoisoned || IsCursed)
        {
            canBePoisoned = false;
        }
        else
        {
            canBePoisoned = true;
        }

        return canBePoisoned;
    }

    

    public virtual void UpdateStatusEffectCount()
    {
        //Were just afflicted with the status. Don't do anything.
        if(WasAfflictedWithStatusThisTurn)
        {
            WasAfflictedWithStatusThisTurn = false;
            return;
        }

        if (IsCursed) 
        {
            CurseDuration -= 1;
            if(CurseDuration == 0)
            {
                GameplayManagerRef.UpdatePlayerInfoUIStatusEffect(this);
                IsCursed = false;
            }
        }

        if (IsPoisoned)
        {
            PoisonDuration -= 1;
            if (PoisonDuration == 0)
            {
                GameplayManagerRef.UpdatePlayerInfoUIStatusEffect(this);
                IsPoisoned = false;
            }
        }
    }

    private void CursePlayerPriv(int numTurnsToCurse)
    {
        if (CanBeCursed())
        {
            IsCursed = true;
            CurseDuration = numTurnsToCurse;
            WasAfflictedWithStatusThisTurn = true;
            GameplayManagerRef.UpdatePlayerInfoUIStatusEffect(this);
            //play animation of some sort...
        }
    }

    private void PoisonPlayerPriv(int numTurnsToPoison)
    {
        if (CanBePoisoned())
        {
            IsPoisoned = true;
            PoisonDuration = numTurnsToPoison;
            WasAfflictedWithStatusThisTurn = true;
            GameplayManagerRef.UpdatePlayerInfoUIStatusEffect(this);
            //play animation of some sort...
        }
    }

    #endregion

    #region HealthRelated

    public virtual void RecoverHealth(int incomingHeal)
    {
        RecoverHealthPriv(incomingHeal);
    }

    private void RecoverHealthPriv(int incomingHeal)
    {
        if (CurrentHealth + incomingHeal >= MaxHealth)
        {
            CurrentHealth = MaxHealth;
            GameplayManagerRef.UpdatePlayerHealth(this);
            //Play heal animation.
        }
        else
        {
            CurrentHealth += incomingHeal;
            GameplayManagerRef.UpdatePlayerHealth(this);
            //Play heal animation.
        }
    }

    public virtual void TakeDamage(int incomingDamage)
    {
        TakeDamagePriv(incomingDamage);
    }

    private void TakeDamagePriv(int incomingDamage)
    {
        if (CurrentHealth - incomingDamage <= 0)
        {
            CurrentHealth = 0;
            GameplayManagerRef.UpdatePlayerHealth(this);
            Defeated();
            //Play death animation.

        }
        else
        {
            CurrentHealth -= incomingDamage;
            GameplayManagerRef.UpdatePlayerHealth(this);
            //Play hurt animation.
        }
    }

    public virtual void Defeated()
    {
        DefeatedPriv();
    }

    private void DefeatedPriv()
    {
        //Discard all Player's cards since they are no longer in the game.
        DiscardAllCardsInHand();
        FinishedHandlingSpaceEffects();

        IsDefeated = true;
        //int indexOfCurrentPlayer = GameplayManagerRef.Players.IndexOf(GameplayManagerRef.playerCharacter.GetComponent<Player>());
        
        //if (GameplayManagerRef.Players[indexOfCurrentPlayer] == this)
        //{
        //    GameplayManagerRef.EndOfTurn(this);
        //}
    }

    #endregion

    #region TurnRelated

    public void StartHandlingSpaceEffects()
    {
        IsHandlingSpaceEffects = true;
        foreach (SpaceEffectData spaceEffect in spaceEffectsToHandle)
        {
            spaceEffect.SpaceEffectCompleted += ExecuteNextSpaceEffect;
        }
        ExecuteNextSpaceEffect();
    }

    public void FinishedHandlingSpaceEffects()
    {
        foreach (SpaceEffectData spaceEffect in spaceEffectsToHandle)
        {
            spaceEffect.SpaceEffectCompleted -= ExecuteNextSpaceEffect;
        }
        spaceEffectsToHandle.Clear();
        IsHandlingSpaceEffects = false;
        if(!IsMoving)
        {
            TurnIsCompleted();
        }
        
    }

    private void ExecuteNextSpaceEffect()
    {
        if(spaceEffectsToHandle.Count > 0)
        {
            Debug.Log($"spaceEffectsToHandle Count before dequeing is: {spaceEffectsToHandle.Count}");
            SpaceEffectData spaceEffectDataToHandle;
            spaceEffectDataToHandle = spaceEffectsToHandle.Dequeue();
            spaceEffectDataToHandle.LandedOnEffect(this);
            Debug.Log($"spaceEffectsToHandle Count after dequeing is: {spaceEffectsToHandle.Count}");
           // Debug.Log($"Handling space effect: {spaceEffectDataToHandle.name}");
        }
        else
        {
            Debug.Log($"All space effects are done!");
            FinishedHandlingSpaceEffects();
            //Space effects are done. Now trigger curse/poison if needed. Then end the turn.
        }
    }

    //Event triggers

    private void TurnIsCompleted()
    {
        TurnHasEnded?.Invoke(this);
    }

    private void CompletedDiscardingForEffect()
    {
        DoneDiscardingForEffect?.Invoke(this);
    }
    
    #endregion
}
