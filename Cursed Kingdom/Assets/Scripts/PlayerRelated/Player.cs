//Code written by Mohamed Riaz Khan of BukuGames.
//All code is written by me (Above name) unless otherwise stated via comments below.
//Not authorized for use outside of the Github repository of this game developed by BukuGames.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using static Card;
using static ClassData;
using Random = UnityEngine.Random;

public class Player : MonoBehaviour
{
	public int playerIDIntVal;

	//Events

	public event Action<Player> TurnHasEnded;
	public event Action<Player> EffectCompleted;
	public event Action<Player> DoneDiscardingForEffect;
	public event Action<Player> DoneAttackingForEffect;
	public event Action<Player> DoneDrawingCard;
	public event Action<Player> DoneActivatingAbilityEffect;
	public event Action<Player> DoneActivatingEliteAbilityEffect;
	public event Action<Player> StatusEffectUpdateCompleted;

	//Events End

	//Consts
	[SerializeField] public const string NEGATIVEEFFECT = "NegativeEffect";
	[SerializeField] public const string POSITIVEEFFECT = "PositiveEffect";
	
	//State machine related
	private PlayerCharacterSM stateMachineRef;
	

	//TEMPORARY GET RID OF THESE.
	public bool startOfTurnEffect;
	public bool endOfTurnSpaceEffects;

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
	[SerializeField] private bool isMoving;
	[SerializeField] private bool isChoosingDirection;

	//Abilities
	[SerializeField] private List<NegativeCooldownEffects> negativeCooldownEffects; 
	[SerializeField] private bool isOnCooldown;
	[SerializeField] private bool wentOnCooldownThisTurn;
	[SerializeField] private bool isHandlingAbilityActivation;
	[SerializeField] private bool isHandlingEliteAbilityActivation;
	[SerializeField] private bool canUseEliteAbility;


	//Effect handling
	[SerializeField] private Queue<SpaceEffectData> spaceEffectsToHandle;
	[SerializeField] private SpaceEffectData currentSpaceEffectDataToHandle;
	[SerializeField] private List<SpaceEffectData> tempSpaceEffectsToHandle;
	[SerializeField] private bool isHandlingSpaceEffects;

	[SerializeField] private Queue<SupportCardEffectData> supportCardEffectsToHandle;
	[SerializeField] private SupportCardEffectData currentSupportCardEffectTohandle;
	[SerializeField] private List<SupportCardEffectData> tempSupportCardEffectsToHandle;
	[SerializeField] private bool isHandlingSupportCardEffects;

	//Status Effects
	[SerializeField] private bool isPoisoned;
	[SerializeField] private bool isCursed;
	[SerializeField] private bool isDefeated;
	[SerializeField] private bool wasAfflictedWithStatusThisTurn;
	[SerializeField] private int poisonDuration;
	[SerializeField] private int curseDuration;
	[SerializeField] private List<StatusEffectImmunityEliteAbility.StatusEffects> statusEffectImmunities;

	//CardsInHand
	[SerializeField] private int numSupportCardsUsedThisTurn;
	[SerializeField] private int maxSupportCardsToUse;
	[SerializeField] private int extraSupportCardUses;
	[SerializeField] private int numMovementCardsUsedThisTurn;
	[SerializeField] private int maxMovementCardsToUse;
	[SerializeField] private int extraMovementCardUses;
	[SerializeField] private bool movementCardSelectedForUse;
	[SerializeField] private bool supportCardSelectedForUse;

	[SerializeField] private bool ableToLevelUp;
	[SerializeField] private ClassData classData;
	[SerializeField] private Space currentSpacePlayerIsOn;
	[SerializeField] private Space previousSpacePlayerWasOn;
	

	//References
	[SerializeField] private GameplayManager gameplayManagerRef;
	[SerializeField] private RuntimeAnimatorController animatorController;
	[SerializeField] private Animator animator;

   
	public PlayerCharacterSM StateMachineRef { get => stateMachineRef; set => stateMachineRef = value; }
	public int MaxHealth { get => maxHealth; set => maxHealth = value; }
	public int CurrentHealth { get => currentHealth; set => currentHealth = value; }
	//Clean this up vvvvv
	
	public bool IsMoving { get => isMoving; set => isMoving = value; }
    public bool IsChoosingDirection { get => isChoosingDirection; set => isChoosingDirection = value; }
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
	public bool IsOnCooldown { get => isOnCooldown; set => isOnCooldown = value; }
	public bool WentOnCooldownThisTurn { get => wentOnCooldownThisTurn; set => wentOnCooldownThisTurn = value; }
	public bool IsHandlingAbilityActivation { get => isHandlingAbilityActivation; set => isHandlingAbilityActivation = value; }
	public bool IsHandlingEliteAbilityActivation { get => isHandlingEliteAbilityActivation; set => isHandlingEliteAbilityActivation = value; }
	public bool CanUseEliteAbility { get => canUseEliteAbility; set => canUseEliteAbility = value; }
	public List<NegativeCooldownEffects> NegativeCooldownEffects { get => negativeCooldownEffects; set => negativeCooldownEffects = value; }
	public Queue<SpaceEffectData> SpaceEffectsToHandle { get => spaceEffectsToHandle; set => spaceEffectsToHandle = value; }
	public bool IsHandlingSpaceEffects { get => isHandlingSpaceEffects; set => isHandlingSpaceEffects = value; }
	public Queue<SupportCardEffectData> SupportCardEffectsToHandle { get => supportCardEffectsToHandle; set => supportCardEffectsToHandle = value; }
	public bool IsHandlingSupportCardEffects { get => isHandlingSupportCardEffects; set => isHandlingSupportCardEffects = value; }
	public bool IsPoisoned { get => isPoisoned; set => isPoisoned = value; }
	public bool IsCursed { get => isCursed; set => isCursed = value; }
	public bool IsDefeated { get => isDefeated; set => isDefeated = value; }
	public bool WasAfflictedWithStatusThisTurn { get => wasAfflictedWithStatusThisTurn; set => wasAfflictedWithStatusThisTurn = value; }
	public int PoisonDuration { get => poisonDuration; set => poisonDuration = value; }
	public int CurseDuration { get => curseDuration; set => curseDuration = value; }
	public List<StatusEffectImmunityEliteAbility.StatusEffects> StatusEffectImmunities 
	{
		get => statusEffectImmunities; 

		set
		{
			if(StatusEffectImmunities.Count > 0)
			{
				StatusEffectImmunities.Clear();
			}
			statusEffectImmunities = value;
		}
	}
	public int MaxSupportCardsToUse { get => maxSupportCardsToUse; set => maxSupportCardsToUse = value; }
	public int NumSupportCardsUsedThisTurn { get => numSupportCardsUsedThisTurn; set => numSupportCardsUsedThisTurn = value; }
	public int ExtraSupportCardUses { get => extraSupportCardUses; set => extraSupportCardUses = value; }
	public int MaxMovementCardsToUse { get => maxMovementCardsToUse; set => maxMovementCardsToUse = value; }
	public int NumMovementCardsUsedThisTurn { get => numMovementCardsUsedThisTurn; set => numMovementCardsUsedThisTurn = value; }
	public int ExtraMovementCardUses { get => extraMovementCardUses; set => extraMovementCardUses = value; }
	public bool MovementCardSelectedForUse { get => movementCardSelectedForUse; set => movementCardSelectedForUse = value; }
	public bool SupportCardSelectedForUse { get => supportCardSelectedForUse; set => supportCardSelectedForUse = value; }
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

		SetupPlayerInfo();
		// Debug.Log($"Player info: \n health = {CurrentHealth}, level = {CurrentLevel}, \n description: {data.description}");
	}

	//Debug version where we hardcode the class.
	public void InitializePlayer()
	{
		SetupPlayerInfo();
		// Debug.Log($"Player info: \n health = {CurrentHealth}, level = {CurrentLevel}, \n description: {data.description}");
	}

	private void SetupPlayerInfo()
	{
		StateMachineRef = GetComponent<PlayerCharacterSM>();
		MaxHealth = ClassData.startingHealth;
		CurrentHealth = maxHealth;
		CurrentLevel = 1;
		AbleToLevelUp = true;
		SpacesLeftToMove = 0;
		MaxHandSize = 6;
		MaxSupportCardsToUse = ClassData.maxSupportCardsToUsePerTurn;
		NumSupportCardsUsedThisTurn = 0;
		MaxMovementCardsToUse = ClassData.maxMovementCardsToUsePerTurn;
		NumMovementCardsUsedThisTurn = 0;
		MovementCardSelectedForUse = false;
		SupportCardSelectedForUse = false;

		IsOnCooldown = false;
		IsMoving = false;
		IsChoosingDirection = false;
        WentOnCooldownThisTurn = false;
		NegativeCooldownEffects = ClassData.negativeCooldownEffects;
		IsHandlingAbilityActivation = false;
		IsHandlingEliteAbilityActivation = false;
		CanUseEliteAbility = false;
		endOfTurnSpaceEffects = false;
		StatusEffectImmunities = new();

		IsDefeated = false;
		CardsLeftToDiscard = 0;
		ValidCardTypesToDiscard = CardType.None;
		SpaceEffectsToHandle = new();
		tempSpaceEffectsToHandle = new();
		isHandlingSpaceEffects = false;
		GameplayManagerRef.SpaceArtworkPopupDisplay.SpaceArtworkDisplayTurnOff += ApplyCurrentSpaceEffects;

		//DEBUG
	   // UseEliteAbility();
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
			CanUseEliteAbility = true;
			if (ClassData.eliteAbilityData.IsPassive)
			{
				UseEliteAbility();
			}
			else if (ClassData.eliteAbilityData.CanBeManuallyActivated)
			{
				GameplayManagerRef.UseEliteAbilityButton.gameObject.transform.parent.gameObject.SetActive(true);
				GameplayManagerRef.UseEliteAbilityButton.onClick.RemoveAllListeners();
				GameplayManagerRef.UseEliteAbilityButton.onClick.AddListener(UseEliteAbility);
			}
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

	//DEBUG
	public void ChangeLevel(int levelToMakePlayer)
	{
		CurrentLevel = levelToMakePlayer;
		if(CurrentLevel >= 5)
		{
			//Unless we want to keep max at 5.
			CurrentLevel = 5;
			CanUseEliteAbility = true;
			if(ClassData.eliteAbilityData.IsPassive)
			{
				UseEliteAbility();
			}
			else if(ClassData.eliteAbilityData.CanBeManuallyActivated)
			{
				GameplayManagerRef.UseEliteAbilityButton.gameObject.transform.parent.gameObject.SetActive(true);
				GameplayManagerRef.UseEliteAbilityButton.onClick.RemoveAllListeners();
				GameplayManagerRef.UseEliteAbilityButton.onClick.AddListener(UseEliteAbility);
			}
		}
		else if(levelToMakePlayer > 0)
		{
			CurrentLevel = levelToMakePlayer;
		}

	}
	//DEBUG

	#region AttackPlayers
	public void ActivatePlayerToAttackSelectionPopup(int numPlayersToChoose, int damageToGive, bool isElemental = false)
	{
		List<Tuple<Sprite, string, object, List<object>>> insertedParams = new();
		

		foreach (Player player in GameplayManagerRef.Players)
		{
			if(!player.IsDefeated && player != this)
			{
				List<object> paramsList = new();
				paramsList.Add(player);
				paramsList.Add(damageToGive);
				paramsList.Add(isElemental);
				insertedParams.Add(Tuple.Create<Sprite, string, object, List<object>>(player.ClassData.defaultPortraitImage, nameof(SelectPlayerToAttack), this, paramsList));
			}
		}

		DialogueBoxPopup.instance.ActivatePopupWithImageChoices("Select the Player you wish to attack.", insertedParams, 1, "Attack");
	}

	/// <summary>
	/// Takes in a list of objects in this order: Player playerToAttack, int DamageToInflict, bool isElemental
	/// </summary>
	/// <returns></returns>
	public IEnumerator SelectPlayerToAttack(List<object> objects)
	{
		yield return null;
		Player playerTarget = objects[0] as Player;
		int damageToTake = (int)objects[1];
		bool isElemental = (bool)objects[2];

		if (playerTarget != null)
		{
			if(damageToTake > 0)
			{
				if(IsHandlingSupportCardEffects && isElemental)
				{
					//Allow target player to respond with elemental barrier if they have it.
					playerTarget.TakeDamage(damageToTake);
				}
				else
				{
					playerTarget.TakeDamage(damageToTake);
				}

				
			   // Debug.Log($"Calling the method to attack the player. Player {playerTarget} will take {damageToTake} Damage!");
			}
		}
		else
		{
			Debug.LogWarning($"Calling the method to attack the player but don't have a target...");
		}
		
		if(IsHandlingSpaceEffects || IsHandlingSupportCardEffects)
		{
			CompletedAttackingEffect();
		}
	}

	public void AttackAllOtherPlayers(int damageToGive, bool isElemental = false)
	{
		foreach(Player player in GameplayManagerRef.Players)
		{
			if (!player.IsDefeated && player != this)
			{
				//Will need a way to queue up death scene if a player in this chain of events is dealt a killing blow from this attack.
				player.TakeDamage(damageToGive);
			}
		}
		//Put some dialogue box here for now to showcase that we've attacked all other players. Will need a log entry + animation here.

		if (IsHandlingSpaceEffects || IsHandlingSupportCardEffects)
		{
			CompletedAttackingEffect();
		}
	}

	#region TakeCardsFromOpponentsSpaceEffect
	public bool CheckIfCanTakeCardsFromOtherPlayersHand(int numCardsToDiscard, Card.CardType cardTypeToDiscard, int numCardsToTakeFromOpponent, Card.CardType cardTypeToTakeFromOpponent)
	{
		bool canTake = false;
		int numCardsPlayerCanTake = 0;

		CheckIfOtherPlayersHaveEnoughCardsToTake(numCardsToTakeFromOpponent, cardTypeToTakeFromOpponent, ref canTake, ref numCardsPlayerCanTake);

		return canTake;
	}
	/// <summary>
	/// For TakeCardsFromOpponents space effect.
	/// </summary>
	/// <param name="cardTypeToDiscard"></param>
	/// <param name="numCardsPlayerCanDiscard"></param>
	/// <returns></returns>
	private int CheckIfCurrentPlayerCanDiscardEnoughCards(CardType cardTypeToDiscard, int numCardsPlayerCanDiscard)
	{
		//Check if Player has enough to discard.
		if (cardTypeToDiscard == CardType.Movement)
		{
			foreach (Player player in GameplayManagerRef.Players)
			{
				if (player == this)
				{
					foreach (Card card in player.CardsInhand)
					{
						MovementCard movementCard = (MovementCard)card;
						if (movementCard != null)
						{
							numCardsPlayerCanDiscard++;
						}
					}
				}
			}
		}
		else if (cardTypeToDiscard == CardType.Support)
		{
			foreach (Player player in GameplayManagerRef.Players)
			{
				if (player == this)
				{
					foreach (Card card in player.CardsInhand)
					{
						SupportCard supportCard = (SupportCard)card;
						if (supportCard != null)
						{
							numCardsPlayerCanDiscard++;
						}
					}
				}
			}
		}
		else
		{
			foreach (Player player in GameplayManagerRef.Players)
			{
				if (player == this)
				{
					foreach (Card card in player.CardsInhand)
					{
						numCardsPlayerCanDiscard++;
					}
				}
			}
		}

		return numCardsPlayerCanDiscard;
	}
	/// <summary>
	/// For TakeCardsFromOpponents space effect.
	/// </summary>
	/// <param name="numCardsToTakeFromOpponent"></param>
	/// <param name="cardTypeToTakeFromOpponent"></param>
	/// <param name="canTake"></param>
	/// <param name="numCardsPlayerCanTake"></param>
	private void CheckIfOtherPlayersHaveEnoughCardsToTake(int numCardsToTakeFromOpponent, CardType cardTypeToTakeFromOpponent, ref bool canTake, ref int numCardsPlayerCanTake)
	{
		//Check if other players have enough of the card type we want to take.
		if (cardTypeToTakeFromOpponent == CardType.Movement)
		{
			foreach (Player player in GameplayManagerRef.Players)
			{
				if (player != this)
				{
					if (player.MovementCardsInHand < numCardsToTakeFromOpponent)
					{
						canTake = false;
						break;
					}
				}
			}
		}
		else if (cardTypeToTakeFromOpponent == CardType.Support)
		{
			foreach (Player player in GameplayManagerRef.Players)
			{
				if (player != this)
				{

					if (player.SupportCardsInHand < numCardsToTakeFromOpponent)
					{
						canTake = false;
						break;
					}
				}
			}
		}
		else
		{
			foreach (Player player in GameplayManagerRef.Players)
			{
				if (player != this)
				{
					if (player.CardsInhand.Count < numCardsToTakeFromOpponent)
					{
						canTake = false;
						break;
					}
				}
			}
		}
		canTake = true;
	}

	public void ActivatePlayerToTakeCardsFromSelectionPopup(int numCardsToDiscard, Card.CardType cardTypeToDiscard, int numCardsToTakeFromOpponent, Card.CardType cardTypeToTakeFromOpponent)
	{
		List<Tuple<Sprite, string, object, List<object>>> insertedParams = new();


		foreach (Player player in GameplayManagerRef.Players)
		{
			if (!player.IsDefeated && player != this)
			{
				if(player.CardsInhand.Count < (numCardsToDiscard + numCardsToTakeFromOpponent))
				{
					continue;
				}
				List<object> paramsList = new();
				paramsList.Add(player);
				paramsList.Add(numCardsToDiscard);
				paramsList.Add(cardTypeToDiscard);
				paramsList.Add(numCardsToTakeFromOpponent);
				paramsList.Add(cardTypeToTakeFromOpponent);

				insertedParams.Add(Tuple.Create<Sprite, string, object, List<object>>(player.ClassData.defaultPortraitImage, nameof(SelectPlayerToTakeCardsFrom), this, paramsList));
			}
		}

		DialogueBoxPopup.instance.ActivatePopupWithImageChoices("Select the Player you wish to take cards from.", insertedParams, 1, "Attack");
	}

	/// <summary>
	/// Takes in a list of objects in this order: Player playerToAttack, int numCardsToDiscard, Card.CardType cardTypeToDiscard, int numCardsToTakeFromOpponent, Card.CardType cardTypeToTakeFromOpponent
	/// </summary>
	/// <returns></returns>
	public IEnumerator SelectPlayerToTakeCardsFrom(List<object> objects)
	{
		yield return null;
		Player playerToAttack = (Player)objects[0];
		int numCardsToDiscard = (int)objects[1];
		Card.CardType cardTypeToDiscard = (Card.CardType)objects[2];
		int numCardsToTakeFromOpponent = (int)objects[3];
		Card.CardType cardTypeToTakeFromOpponent = (Card.CardType)objects[4];

		//Cards to discard.
		for(int i = 0; i < numCardsToDiscard; i++)
		{
			if (cardTypeToDiscard == CardType.Movement)
			{
				int cardToTakeIndex = Random.Range(0, playerToAttack.MovementCardsInHand);
				List<MovementCard> movementCardsInHand = new List<MovementCard>();

				foreach (Card card in playerToAttack.CardsInhand)
				{
					MovementCard movementCard = (MovementCard)card;

					if (movementCard != null)
					{
						movementCardsInHand.Add(movementCard);
					}
				}

				//Discard it from the player who we're attacking's hand.
				playerToAttack.DiscardFromHand(movementCardsInHand[cardToTakeIndex].ThisCardType, movementCardsInHand[cardToTakeIndex]);
			}
			if (cardTypeToDiscard == CardType.Support)
			{
				int cardToTakeIndex = Random.Range(0, playerToAttack.SupportCardsInHand);
				List<SupportCard> supportCardsInHand = new List<SupportCard>();

				foreach (Card card in playerToAttack.CardsInhand)
				{
					SupportCard supportCard = (SupportCard)card;

					if (supportCard != null)
					{
						supportCardsInHand.Add(supportCard);
					}
				}

				//Discard it from the player who we're attacking's hand.
				playerToAttack.DiscardFromHand(supportCardsInHand[cardToTakeIndex].ThisCardType, supportCardsInHand[cardToTakeIndex]);
			}
			else
			{
				int cardToTakeIndex = Random.Range(0, playerToAttack.CardsInhand.Count);
				List<Card> cardsInHand = new List<Card>();

				//Discard it from the player who we're attacking's hand.
				playerToAttack.DiscardFromHand(playerToAttack.CardsInhand[cardToTakeIndex].ThisCardType, playerToAttack.CardsInhand[cardToTakeIndex]);
			}
		}


		//Cards to take.
		for (int i = 0; i < numCardsToTakeFromOpponent; i++)
		{
			if (cardTypeToTakeFromOpponent == CardType.Movement)
			{
				int cardToTakeIndex = Random.Range(0, playerToAttack.MovementCardsInHand);
				List<MovementCard> movementCardsInHand = new List<MovementCard>();

				foreach (Card card in playerToAttack.CardsInhand)
				{
					MovementCard movementCard = (MovementCard)card;

					if (movementCard != null)
					{
						movementCardsInHand.Add(movementCard);
					}
				}

				//Remove it from the player we're attacking's hand, then add it to the currentPlayer's hand.
				Card cardToDraw = movementCardsInHand[cardToTakeIndex];
				playerToAttack.DiscardFromHand(movementCardsInHand[cardToTakeIndex].ThisCardType, movementCardsInHand[cardToTakeIndex]);
				DrawCard(cardToDraw);
			}
			if (cardTypeToTakeFromOpponent == CardType.Support)
			{
				int cardToTakeIndex = Random.Range(0, playerToAttack.SupportCardsInHand);
				List<SupportCard> supportCardsInHand = new List<SupportCard>();

				foreach (Card card in playerToAttack.CardsInhand)
				{
					SupportCard supportCard = (SupportCard)card;

					if (supportCard != null)
					{
						supportCardsInHand.Add(supportCard);
					}
				}

				//Remove it from the player we're attacking's hand, then add it to the currentPlayer's hand.
				Card cardToDraw = supportCardsInHand[cardToTakeIndex];
				playerToAttack.DiscardFromHand(supportCardsInHand[cardToTakeIndex].ThisCardType, supportCardsInHand[cardToTakeIndex]);
				DrawCard(cardToDraw);
			}
			else
			{
				int cardToTakeIndex = Random.Range(0, playerToAttack.CardsInhand.Count);

				//Remove it from the player we're attacking's hand, then add it to the currentPlayer's hand.
				Card cardToDraw = playerToAttack.CardsInhand[cardToTakeIndex];
				playerToAttack.DiscardFromHand(playerToAttack.CardsInhand[cardToTakeIndex].ThisCardType, playerToAttack.CardsInhand[cardToTakeIndex]);
				DrawCard(cardToDraw);
			}
		}

		if(IsHandlingSpaceEffects)
		{
			CompletedAttackingEffect();
		}
	}

	#endregion

	#endregion

	#region CardsInHandMethods


	public void SelectCardTypeToDrawPopup(int numCardsToDraw)
	{
		List<Tuple<Sprite, string, object, List<object>>> insertedParams = new();

		List<object> movementParamsList = new();

		CardType movementType = CardType.Movement;
		movementParamsList.Add(movementType);
		movementParamsList.Add(numCardsToDraw);
		insertedParams.Add(Tuple.Create<Sprite, string, object, List<object>>(Resources.Load<Sprite>("CardArtwork/CardBacksFullArtwork/Movementbackfull"), nameof(SelectCardToDraw), this, movementParamsList));

		List<object> supportParamsList = new();

		CardType supportType = CardType.Support;
		supportParamsList.Add(supportType);
		supportParamsList.Add(numCardsToDraw);
		insertedParams.Add(Tuple.Create<Sprite, string, object, List<object>>(Resources.Load<Sprite>("CardArtwork/CardBacksFullArtwork/Supportbackfull"), nameof(SelectCardToDraw), this, supportParamsList));

		DialogueBoxPopup.instance.ActivatePopupWithImageChoices($"Select which deck you would like to draw {numCardsToDraw} card(s) from.", insertedParams, 1, "Draw");
	}

	/// <summary>
	/// Takes in a list of objects in this order: Card.CardType typeOfCard, int cardsToDraw.
	/// </summary>
	/// <returns></returns>
	public IEnumerator SelectCardToDraw(List<object> objects)
	{
		yield return null;
		CardType cardType = (CardType)objects[0];
		int numCardsToDraw = (int)objects[1];

		if(numCardsToDraw > 1)
		{
			GameplayManagerRef.ThisDeckManager.DrawCards(cardType, this, numCardsToDraw);
		}
		else
		{
			GameplayManagerRef.ThisDeckManager.DrawCard(cardType, this);
		}
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


		//Just in case the Player draws a new card and is cursed we need the new card's value to also be halved.
		if (IsCursed)
		{
			MovementCard tempMovementCard = (MovementCard)card;
			if(tempMovementCard != null)
			{
				tempMovementCard.ChangeMovementValue(true);
				CurseCardAnimationEffect();
			}
		}

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

		//Just in case the Player draws a new card and is cursed we need the new card's value to also be halved.
		if (IsCursed)
		{
			//Just in case the Player draws a new card and is cursed we need the new card's value to also be halved.
			if (IsCursed)
			{
				foreach(Card card in cards)
				{
					MovementCard tempMovementCard = (MovementCard)card;
					if (tempMovementCard != null)
					{
						tempMovementCard.ChangeMovementValue(true);
						CurseCardAnimationEffect();
					}
				}
			}
		}
	}

	//Shows this player's hand on screen.
	public void ShowHand()
	{
		foreach (Card card in CardsInhand)
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

		//For Curse animation on cards.
		if (IsCursed)
		{
			CurseCardAnimationEffect();
		}

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
		   // GameplayManagerRef.OpenDebugMessenger($"Hand size exceeded. You have {CardsInhand.Count} cards in your hand. Please discard {CardsInhand.Count -  MaxHandSize} cards.");
			DialogueBoxPopup.instance.ActivatePopupWithJustText($"Hand size exceeded. You have {CardsInhand.Count} cards in your hand. Please discard {CardsInhand.Count - MaxHandSize} card(s).", 0, "Discard");
			SetCardsToDiscard(CardType.Both, CardsInhand.Count - MaxHandSize);
			HideHand();
			ShowHand();
			GameplayManagerRef.HandDisplayPanel.ShrinkHand();
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

	public void SelectCardForDiscard()
	{
		CardsLeftToDiscard -= 1;

		//Confirmation for discard.
		if( CardsLeftToDiscard == 0)
		{
			List<Tuple<string, string, object, List<object>>> insertedParams = new();
			insertedParams.Add(Tuple.Create<string, string, object, List<object>>("Yes", nameof(DiscardTheSelectedCards), this, new List<object>()));
			insertedParams.Add(Tuple.Create<string, string, object, List<object>>("No", nameof(DeselectAllSelectedCardsForDiscard), this, new List<object>()));

			DialogueBoxPopup.instance.ActivatePopupWithButtonChoices("Are you sure you want to discard the selected card(s)?", insertedParams, 1, "Confirm Selection");
		}
	}

	/// <summary>
	/// If the user decides to reselect cards that is what this option is for.
	/// </summary>
	public IEnumerator DeselectAllSelectedCardsForDiscard()
	{
		yield return null;

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

		if(ValidCardTypesToDiscard != CardType.Both)
		{
			DialogueBoxPopup.instance.ActivatePopupWithJustText($"Please select {CardsLeftToDiscard} {ValidCardTypesToDiscard} card(s) to discard.", 0, "Discard");
		}
		else
		{
			if(MaxHandSizeExceeded())
			{
				DialogueBoxPopup.instance.ActivatePopupWithJustText($"Hand size exceeded. You have {CardsInhand.Count} cards in your hand. Please discard {CardsInhand.Count - MaxHandSize} card(s).", 0, "Discard");
			}
			else
			{
				DialogueBoxPopup.instance.ActivatePopupWithJustText($"Please select {CardsLeftToDiscard} Movement and/or Support cards to discard.", 0, "Discard");
			}
		}
	}

	public IEnumerator DiscardTheSelectedCards()
	{
		yield return null;

		bool discardingDueToMaxHand = MaxHandSizeExceeded();

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

		if(discardingDueToMaxHand && !IsHandlingSpaceEffects)
		{
			if(!IsHandlingSupportCardEffects)
			{
				ResetSupportCardUsageCount();
				ResetMovementCardUsageCount();
				TurnIsCompleted();
			}
			else
			{
				CompletedDiscardingForEffect();
			}
		}
		else
		{
			CompletedDiscardingForEffect();
		}

		ValidCardTypesToDiscard = CardType.None;
		CardsLeftToDiscard = 0;
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

	public void UseMovementCard()
	{
		NumMovementCardsUsedThisTurn += 1;
	}

	public void ResetMovementCardUsageCount()
	{
		ExtraMovementCardUses = 0;
		NumMovementCardsUsedThisTurn = 0;
	}

	public bool CanUseMovementCard()
	{
		bool canUse;
		int maximumAmountTouse = ExtraMovementCardUses + MaxMovementCardsToUse;

		if(NumMovementCardsUsedThisTurn >= maximumAmountTouse)
		{
			canUse = false;
		}
		else
		{
			canUse = true;
		}

		return canUse;
	}

	public void UseSupportCard()
	{
		NumSupportCardsUsedThisTurn += 1;
	}

	public void ResetSupportCardUsageCount()
	{
		ExtraSupportCardUses = 0;
		NumSupportCardsUsedThisTurn = 0;
	}

	public bool CanUseSupportCard()
	{
		bool canUse;
		int maximumAmountTouse = ExtraSupportCardUses + MaxSupportCardsToUse;

		if (NumSupportCardsUsedThisTurn >= maximumAmountTouse)
		{
			canUse = false;
		}
		else
		{
			canUse = true;
		}

		return canUse;
	}

	public void IncreaseMaxCardUses(int numToIncreaseBy, CardType cardType)
	{
		if(cardType == CardType.Movement)
		{
			ExtraMovementCardUses += numToIncreaseBy;
		}
		else
		{
			ExtraSupportCardUses += numToIncreaseBy;
		}

		if(IsHandlingAbilityActivation)
		{
			CompletedAbilityActivation();
		}
		
	}

	public void SelectMultipleCardsToUse()
	{
		int numSelected = 0;
		int maxNumPlayerCanSelect = MaxMovementCardsToUse + ExtraMovementCardUses;

		foreach (Card card in CardsInhand)
		{
			if (card.SelectedForUse)
			{
				numSelected += 1;
			}
		}

		if(!GameplayManagerRef.UseSelectedCardsPanel.activeInHierarchy)
		{
			GameplayManagerRef.UseSelectedCardsPanel.SetActive(true);
		}

		GameplayManagerRef.UseSelectedCardsText.text = $"Selected cards: {numSelected}/{maxNumPlayerCanSelect}";
		GameplayManagerRef.UseSelectedCardsButton.onClick.RemoveAllListeners();
		GameplayManagerRef.UseSelectedCardsButton.onClick.AddListener(UseMultipleCards);
		List<Tuple<string, string, object>> insertedParams = new();

		if (numSelected < maxNumPlayerCanSelect)
		{
			//insertedParams.Add(Tuple.Create<string, string, object>("Select more", nameof(SelectMoreCardsToUse), this));
			//insertedParams.Add(Tuple.Create<string, string, object>("Use selected", nameof(UseMultipleCards), this));

			//DialogueBoxPopup.instance.ActivatePopupWithButtonChoices($"Do you want to select more cards to use? (Max {maxNumPlayerCanSelect})", insertedParams, null, 1, "Confirm Selection");
			
		}
		else
		{
			//insertedParams.Add(Tuple.Create<string, string, object>("Yes", nameof(UseMultipleCards), this));
			//insertedParams.Add(Tuple.Create<string, string, object>("No", nameof(DeselectAllSelectedCardsForUse), this));

			//DialogueBoxPopup.instance.ActivatePopupWithButtonChoices("Are you sure you want to use the selected movement cards together?", insertedParams, null, 1, "Confirm Selection");
		}

	}

	public IEnumerator SelectMoreCardsToUse()
	{
		yield return null;

		DialogueBoxPopup.instance.DeactivatePopup();
	}


	/// <summary>
	/// Takes in a list of objects in this order: List<Card> cards to use.
	/// </summary>
	/// <returns></returns>
	public void UseMultipleCards()
	{
		int totalToUse = 0;
		List<Card> cardsToDiscard = new();
		foreach (Card card in CardsInhand)
		{
			if(card is MovementCard)
			{
				MovementCard movementCard = (MovementCard)card;

				if(movementCard.SelectedForUse)
				{
					cardsToDiscard.Add(card);

					if (movementCard.TempCardValue > 0)
					{
						totalToUse += movementCard.TempCardValue;
						movementCard.ResetMovementValue();
						if (!IsCursed)
						{
							movementCard.DeactivateCurseEffect();
						}
					}
					else
					{
						totalToUse += movementCard.MovementCardValue;
					}

					movementCard.transform.localScale = movementCard.OriginalSize;
					movementCard.CardIsActiveHovered = false;
				}
			}
		}

		for (int i = 0; i < cardsToDiscard.Count; i++)
		{
			UseMovementCard();
			DiscardFromHand(cardsToDiscard[i].ThisCardType, cardsToDiscard[i]);
		}

		MovementCardSelectedForUse = false;
		SupportCardSelectedForUse = false;
		GameplayManagerRef.UseSelectedCardsButton.onClick.RemoveAllListeners();
		GameplayManagerRef.UseSelectedCardsPanel.SetActive(false);
		GameplayManagerRef.HandDisplayPanel.ShrinkHand();
		GameplayManagerRef.StartMove(totalToUse);
		
	}

	/// <summary>
	/// If the user decides to reselect cards to use multiple of that is what this option is for.
	/// </summary>
	public IEnumerator DeselectAllSelectedCardsForUse()
	{
		yield return null;

		foreach (Card card in CardsInhand)
		{
			if (card.SelectedForUse)
			{
				card.DeselectForUse();
			}
		}

		DialogueBoxPopup.instance.ActivatePopupWithJustText("Movement cards deselected.", 1f);
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

	#endregion

	#region StatusEffects

	public virtual void SetStatusImmunities(List<StatusEffectImmunityEliteAbility.StatusEffects> immunities)
	{
		SetStatusImmunitiesPriv(immunities);
	}

	public virtual void CursePlayer(int numTurnsToCurse)
	{
		CursePlayerPriv(numTurnsToCurse);
	}

	public virtual bool CanBeCursed()
	{
		bool canBeCursed = false;
		bool isImmune = false;

		if (StatusEffectImmunities.Count > 0)
		{
			foreach (StatusEffectImmunityEliteAbility.StatusEffects immunity in StatusEffectImmunities)
			{
				if (immunity == StatusEffectImmunityEliteAbility.StatusEffects.Poison)
				{
					isImmune = true;
					break;
				}
			}
			if (isImmune)
			{
				canBeCursed = false;
				UseEliteAbility();
				DialogueBoxPopup.instance.ActivatePopupWithJustText("Immune to curse!");
				CompletedEliteAbilityActivation();
				return canBeCursed;
			}
		}


		if ((IsPoisoned || IsCursed) && !WasAfflictedWithStatusThisTurn)
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
		bool isImmune = false;

		if(StatusEffectImmunities.Count > 0)
		{
			foreach(StatusEffectImmunityEliteAbility.StatusEffects immunity in  StatusEffectImmunities)
			{
				if(immunity == StatusEffectImmunityEliteAbility.StatusEffects.Poison)
				{
					isImmune = true;
					break;
				}
			}
			if(isImmune)
			{
				canBePoisoned = false;
				UseEliteAbility();
				DialogueBoxPopup.instance.ActivatePopupWithJustText("Immune to poison!");
				CompletedEliteAbilityActivation();
				return canBePoisoned;
			}
		}

		if (((IsPoisoned || IsCursed) && !WasAfflictedWithStatusThisTurn))
		{
			canBePoisoned = false;
		}
		else
		{
			canBePoisoned = true;
		}

		return canBePoisoned;
	}

	public virtual void PoisonEffect()
	{
		PoisonEffectPriv();
	}

	public virtual void CurseEffect()
	{
		CurseEffectPriv();
	}

	public virtual void CurseCardAnimationEffect()
	{
		foreach (Card card in CardsInhand)
		{
			MovementCard tempMovementCard = card as MovementCard;

			if (tempMovementCard != null)
			{
				tempMovementCard.ActivateCurseEffect();
			}
		}
	}

	public virtual void CurePoison()
	{
		PoisonDuration = 0;
		WasAfflictedWithStatusThisTurn = false;
		GameplayManagerRef.UpdatePlayerInfoUIStatusEffect(this);
		IsPoisoned = false;
		CompletedStatusEffectUpdate();
	}

	public virtual void CureCurse()
	{
		CurseDuration = 0;
		WasAfflictedWithStatusThisTurn = false;
		//If player is on a 'halve movement cards when on this space' space, this will still reset to original value. Can't have that, need to only increase it by half it's value.
		ResetMovementCardsInHandValues();
		GameplayManagerRef.UpdatePlayerInfoUIStatusEffect(this);
		IsCursed = false;
		CompletedStatusEffectUpdate();
	}

	public virtual void UpdateStatusEffectCount(bool isEndOfTurn = true)
	{
		//Were just afflicted with the status. Don't do anything.
		if(WasAfflictedWithStatusThisTurn && isEndOfTurn)
		{
			WasAfflictedWithStatusThisTurn = false;
			CompletedStatusEffectUpdate();
			return;
		}

		if (IsCursed) 
		{
			CurseDuration -= 1;

			if(CurseDuration == 0)
			{
				//If player is on a 'halve movement cards when on this space' space, this will still reset to original value. Can't have that, need to only increase it by half it's value.
				ResetMovementCardsInHandValues();

				GameplayManagerRef.UpdatePlayerInfoUIStatusEffect(this);
				IsCursed = false;
			}
		}

		if (IsPoisoned)
		{
			PoisonEffect();
			PoisonDuration -= 1;
			if (PoisonDuration == 0)
			{
				GameplayManagerRef.UpdatePlayerInfoUIStatusEffect(this);
				IsPoisoned = false;
			}
		}

		CompletedStatusEffectUpdate();
	}

	public void ResetMovementCardsInHandValues()
	{
		foreach (Card card in CardsInhand)
		{
			MovementCard tempMovementCard = card as MovementCard;

			if (tempMovementCard != null)
			{
				tempMovementCard.ResetMovementValue();
				tempMovementCard.DeactivateCurseEffect();
			}
		}
	}

	private void SetStatusImmunitiesPriv(List<StatusEffectImmunityEliteAbility.StatusEffects> immunities)
	{
		List<StatusEffectImmunityEliteAbility.StatusEffects> copyImmunities = new();

		foreach (StatusEffectImmunityEliteAbility.StatusEffects status in immunities)
		{
			copyImmunities.Add(status);
		}
		StatusEffectImmunities = copyImmunities;
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

	private void PoisonEffectPriv()
	{
		TakeDamage(1);
		//Play poison animation particle effect or something before player takes damage.
	}

	private void CurseEffectPriv()
	{
		HalveAllMovementCardsInHand();
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

	#region Space Effects
	public void StartHandlingSpaceEffects()
	{
		IsHandlingSpaceEffects = true;
		foreach (SpaceEffectData spaceEffect in spaceEffectsToHandle)
		{
			spaceEffect.SpaceEffectCompleted += ExecuteNextSpaceEffect;
			tempSpaceEffectsToHandle.Add(spaceEffect);
		}
		ExecuteNextSpaceEffect();
	}

	public void HandleEndOfTurnSpaceEffects()
	{
		if(!endOfTurnSpaceEffects)
		{
			endOfTurnSpaceEffects = true;
			ApplyCurrentSpaceEffects(this);
		}

		GameplayManagerRef.ThisDeckManager.DrawCard(CardType.Movement, this);

		if (!IsMoving && !MaxHandSizeExceeded())
		{
			ResetSupportCardUsageCount();
			ResetMovementCardUsageCount();
			ResetMovementCardsInHandValues();
			if(IsCursed)
			{
				CurseEffect();
			}
			endOfTurnSpaceEffects = false;
			TurnIsCompleted();
		}
	}

	public void FinishedHandlingSpaceEffects()
	{
		foreach(SpaceEffectData spaceEffect in tempSpaceEffectsToHandle)
		{
			spaceEffect.SpaceEffectCompleted -= ExecuteNextSpaceEffect;
		}

		spaceEffectsToHandle.Clear();
		tempSpaceEffectsToHandle.Clear();
		currentSpaceEffectDataToHandle = null;
		IsHandlingSpaceEffects = false;

		//Change this check to be based on state of the game rather than movement cards.
		if (NumMovementCardsUsedThisTurn == 0)
		{
			return;
		}


		HandleEndOfTurnSpaceEffects();
	   
	}

	private void ExecuteNextSpaceEffect()
	{
		if(spaceEffectsToHandle.Count > 0)
		{
			currentSpaceEffectDataToHandle = spaceEffectsToHandle.Dequeue();

			//Change this check to be based on state of the game rather than movement cards.
			if(NumMovementCardsUsedThisTurn == 0)
			{
				currentSpaceEffectDataToHandle.StartOfTurnEffect(this);
			}
			else if(endOfTurnSpaceEffects)
			{
				currentSpaceEffectDataToHandle.EndOfTurnEffect(this);
			}
			else
			{
				currentSpaceEffectDataToHandle.LandedOnEffect(this);
			}
			
		}
		else
		{
			FinishedHandlingSpaceEffects();
		}
	}

	//This can stack. Example: Cursed + also on a space that halves movement card values.
	public void HalveAllMovementCardsInHand()
	{
		foreach (Card card in CardsInhand)
		{
			MovementCard tempMovementCard = card as MovementCard;

			if (tempMovementCard != null)
			{
				tempMovementCard.ChangeMovementValue(true);
				tempMovementCard.ActivateCurseEffect();
			}
		}
	}
	#endregion

	#region SupportCardEffectHandlers
	public void StartHandlingSupportCardEffects()
	{
		IsHandlingSupportCardEffects = true;

		foreach (SupportCardEffectData supportCardEffect in supportCardEffectsToHandle)
		{
			supportCardEffect.SupportCardEffectCompleted += ExecuteNextSupportCardEffect;
			tempSupportCardEffectsToHandle.Add(supportCardEffect);
		}
		ExecuteNextSupportCardEffect();
	}

	public void FinishedHandlingSupportCardEffects()
	{

		foreach (SupportCardEffectData supportCardEffect in tempSupportCardEffectsToHandle)
		{
			supportCardEffect.SupportCardEffectCompleted -= ExecuteNextSupportCardEffect;
		}

		SupportCardEffectsToHandle.Clear();
		tempSupportCardEffectsToHandle.Clear();
		currentSupportCardEffectTohandle = null;
		IsHandlingSupportCardEffects = false;

		//We'll need to up the counter of the amount of Support cards the user has used this turn here.
	}

	private void ExecuteNextSupportCardEffect()
	{
		if (supportCardEffectsToHandle.Count > 0)
		{
			currentSupportCardEffectTohandle = supportCardEffectsToHandle.Dequeue();
			currentSupportCardEffectTohandle.EffectOfCard(this);
		}
		else
		{
			FinishedHandlingSupportCardEffects();
		}
	}

	#endregion

	#region Space activation
	private void ApplyCurrentSpaceEffects(Player player)
	{
		if (GameplayManagerRef.playerCharacter.GetComponent<Player>() == this)
		{
			//We'll check states for the phases of the game here to determine whether or not to use the landed on effect/passing effect/start of turn effect.
			if (startOfTurnEffect)
			{
				CurrentSpacePlayerIsOn.ApplyStartOfTurnSpaceEffects(this);
			}
			else if(endOfTurnSpaceEffects)
			{
				CurrentSpacePlayerIsOn.ApplyEndOfTurnSpaceEffects(this);
			}
			else
			{
				CurrentSpacePlayerIsOn.ApplyLandedOnSpaceEffects(this);
			}
			startOfTurnEffect = false;

		}
	}
	#endregion

	#region Ability Effects
	public void UseAbility()
	{
		if(ClassData.abilityData != null)
		{
			ClassData.abilityData.ActivateEffect(this);
			if(ClassData.abilityData.CanBeManuallyActivated)
			{
				GameplayManagerRef.UseAbilityButton.transform.parent.gameObject.SetActive(false);
			}
		}
	}
	public void ActivateAbilityEffects()
	{
		//Play animation for Player activating their ability!!!!
		IsHandlingAbilityActivation = true;
	}

	public void UpdateCooldownStatus()
	{
		if(WentOnCooldownThisTurn && IsOnCooldown)
		{
			WentOnCooldownThisTurn = false;

			if(ClassData.abilityData.CanBeManuallyActivated)
			{
				GameplayManagerRef.UseAbilityButton.transform.parent.gameObject.SetActive(false);
			}
		}
		else
		{
			IsOnCooldown = false;
			GameplayManagerRef.UpdatePlayerCooldownText(this);
		}
	}

	#endregion

	#region Elite Ability Effects

	public void UseEliteAbility()
	{
		//Check if cost can be paid since some Elite abilities require a cost to be paid!!!!

		if (ClassData.eliteAbilityData != null)
		{
			ClassData.eliteAbilityData.ActivateEffect(this);
			if (ClassData.eliteAbilityData.CanBeManuallyActivated)
			{
				//GameplayManagerRef.UseAbilityButton.transform.parent.gameObject.SetActive(false);
			}
		}
	}
	public void ActivateEliteAbilityEffects()
	{
		//Play animation for Player activating their Elite ability!!!!
		IsHandlingEliteAbilityActivation = true;
	}

	#endregion
	//Event triggers

	public void TurnIsCompleted()
	{
		TurnHasEnded?.Invoke(this);
	}

	public void CompletedAttackingEffect()
	{
		DoneAttackingForEffect?.Invoke(this);
	}

	public void CompletedDiscardingForEffect()
	{
		DoneDiscardingForEffect?.Invoke(this);
	}

	public void CompletedDrawingForEffect()
	{
		DoneDrawingCard?.Invoke(this);
	}

	public void CompletedAbilityActivation()
	{
		IsHandlingAbilityActivation = false;

		if(ClassData.abilityData.PutsCharacterInCooldown)
		{
			IsOnCooldown = true;
			WentOnCooldownThisTurn = true;
		}
		GameplayManagerRef.UpdatePlayerCooldownText(this);
		DoneActivatingAbilityEffect?.Invoke(this);
	}

	public void CompletedEliteAbilityActivation()
	{
		IsHandlingEliteAbilityActivation = false;
		DoneActivatingEliteAbilityEffect?.Invoke(this);
	}

	public void CompletedStatusEffectUpdate()
	{
		StatusEffectUpdateCompleted?.Invoke(this);
	}
	
	#endregion
}
