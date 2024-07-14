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
	public event Action<Player> FinishedHandlingCurrentSpaceEffects;
	public event Action<Player> DoneDiscardingForEffect;
	public event Action<Player> DoneAttackingForEffect;
	public event Action<Player> DoneRecoveringHealthEffect;
	public event Action<Player> DoneDrawingCard;
	public event Action<Player> DoneActivatingAbilityEffect;
	public event Action<Player> DoneActivatingEliteAbilityEffect;
	public event Action<Player> StatusEffectUpdateCompleted;
	public event Action<Player> HasBeenDefeated;
	public event Action<Player> BillboardLookAtCamera;
    public event Action<Player> BillboarForwardCamera;

    //Events End

    //Consts
    [SerializeField] public const string NEGATIVEEFFECT = "NegativeEffect";
	[SerializeField] public const string POSITIVEEFFECT = "PositiveEffect";
	[SerializeField] public const string ISCASTING = "IsCasting";
	[SerializeField] public const string ISHURT = "IsHurt";
	[SerializeField] public const string ISMOVING = "IsMoving";
	[SerializeField] public const string ISTRANSITIONINGTODUEL = "IsTransitioningToDuel";
    [SerializeField] public const string ISDUELINGIDLE = "IsDuelingIdle";
	[SerializeField] public const string ISIDLE = "IsIdle";

    //State machine related
    private PlayerCharacterSM stateMachineRef;

	//Properties
	[SerializeField] private int maxHealth;
	[SerializeField] private int currentHealth;
	[SerializeField] private int currentLevel;
	[SerializeField] private int spacesLeftToMove;
	[SerializeField] private int currentSumOfSpacesToMove;
	[SerializeField] private int movementCardsInHandCount;
	[SerializeField] private int supportCardsInHandCount;
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
	[SerializeField] private SupportCard currentSupportCardInUse;
	[SerializeField] private bool triedToNegateCurrentSupportCard;

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
	[SerializeField] private Button noMovementCardsInHandButton;

	[SerializeField] private bool ableToLevelUp;
	[SerializeField] private ClassData classData;
	[SerializeField] private Space currentSpacePlayerIsOn;
	[SerializeField] private Space previousSpacePlayerWasOn;

	//Duel related
	[SerializeField] private int rangeOfSpacesToLookForDuelOpponents;

	//Sound related
	

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
    public int CurrentSumOfSpacesToMove { get => currentSumOfSpacesToMove; set => currentSumOfSpacesToMove = value; }
    public int MovementCardsInHandCount { get => movementCardsInHandCount; set => movementCardsInHandCount = value; }
	public int SupportCardsInHandCount { get => supportCardsInHandCount; set => supportCardsInHandCount = value; }
	public int MaxHandSize { get => maxHandSize; set => maxHandSize = value; }
	public int CardsLeftToDiscard { get => cardsLeftToDiscard; set => cardsLeftToDiscard = value; }
	public CardType ValidCardTypesToDiscard { get => validCardTypesToDiscard; set => validCardTypesToDiscard = value; }
	public List<Card> CardsInhand { get => cardsInhand; set => cardsInhand = value; }
	public GameObject MovementCardsInHandHolderPanel { get => movementCardsInHandHolderPanel; set => movementCardsInHandHolderPanel = value; }
	public GameObject SupportCardsInHandHolderPanel { get => supportCardsInHandHolderPanel; set => supportCardsInHandHolderPanel = value; }
	public GameObject HandDisplayPanel { get => handDisplayPanel; set => handDisplayPanel = value; }
	public Button NoMovementCardsInHandButton { get => noMovementCardsInHandButton; set => noMovementCardsInHandButton = value; }
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
    public SupportCard CurrentSupportCardInUse { get => currentSupportCardInUse; set => currentSupportCardInUse = value; }
    public bool TriedToNegateCurrentSupportCard { get => triedToNegateCurrentSupportCard; set => triedToNegateCurrentSupportCard = value; }
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

    //Duel related
    public int RangeOfSpacesToLookForDuelOpponents { get => rangeOfSpacesToLookForDuelOpponents; set => rangeOfSpacesToLookForDuelOpponents = value; }

	//References
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
	public void InitializePlayer(int playerNum = 0)
	{
		SetupPlayerInfo(playerNum);
		// Debug.Log($"Player info: \n health = {CurrentHealth}, level = {CurrentLevel}, \n description: {data.description}");
	}

	private void SetupPlayerInfo(int playerNum = 0)
	{
		StateMachineRef = GetComponent<PlayerCharacterSM>();
		if (StartDebugMenu.instance != null && StartDebugMenu.instance.useScriptable && StartDebugMenu.instance.currentlySelectedStartData.playerDebugDatas[playerNum].startingHealthOverride > 0)
		{
			MaxHealth = StartDebugMenu.instance.currentlySelectedStartData.playerDebugDatas[playerNum].startingHealthOverride;
        }
		else
		{
            MaxHealth = ClassData.startingHealth;
        }
        
		CurrentHealth = maxHealth;

        if (StartDebugMenu.instance != null && StartDebugMenu.instance.useScriptable && StartDebugMenu.instance.currentlySelectedStartData.playerDebugDatas[playerNum].startingLevelOverride > 1)
        {
			if(StartDebugMenu.instance.currentlySelectedStartData.playerDebugDatas[playerNum].startingLevelOverride > 1)
			{
				LevelUp(StartDebugMenu.instance.currentlySelectedStartData.playerDebugDatas[playerNum].startingLevelOverride);
            }
			//Loop through levelups for whatever level we are at. So do a for loop if override = 5 as if player passed level up space 4 times.
			
        }
        else
        {
            CurrentLevel = 1;
        }
        
		AbleToLevelUp = true;
		SpacesLeftToMove = 0;
		CurrentSumOfSpacesToMove = 0;

        if (StartDebugMenu.instance != null && StartDebugMenu.instance.useScriptable && StartDebugMenu.instance.currentlySelectedStartData.playerDebugDatas[playerNum].movementCardsToStartWithOverride +
            StartDebugMenu.instance.currentlySelectedStartData.playerDebugDatas[playerNum].movementCardsToStartWithOverride > 6)
        {
			MaxHandSize = StartDebugMenu.instance.currentlySelectedStartData.playerDebugDatas[playerNum].movementCardsToStartWithOverride +
			StartDebugMenu.instance.currentlySelectedStartData.playerDebugDatas[playerNum].movementCardsToStartWithOverride;
        }
		else
		{
            MaxHandSize = 6;
        }
        
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
		StatusEffectImmunities = new();

		IsDefeated = false;
		CardsLeftToDiscard = 0;
		ValidCardTypesToDiscard = CardType.None;
		SpaceEffectsToHandle = new();
		tempSpaceEffectsToHandle = new();
		isHandlingSpaceEffects = false;
		TriedToNegateCurrentSupportCard = false;

		//Duel related
		RangeOfSpacesToLookForDuelOpponents = 3;

        //Subscriptions
        GameplayManagerRef.SpaceArtworkPopupDisplay.SpaceArtworkDisplayTurnOff += ApplyCurrentSpaceEffects;
        HasBeenDefeated += GameplayManagerRef.CheckIfAllPlayersButOneDefeated;

		//DEBUG
		// UseEliteAbility();


    }

	public void DebugTheSpace()
	{
		//Debug.Log(CurrentSpacePlayerIsOn);
	}

	public void LevelUp(int levelsToIncrease)
	{
		for(int i = 0; i < levelsToIncrease; i++)
		{
            HandleLevelUp();
        }
		
        AbleToLevelUp = false;
		if(CurrentLevel >= 5) 
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
        GameplayManagerRef.UpdatePlayerLevel(this);
    }

	public void HandleLevelUp()
	{
		if (CurrentLevel < 5)
		{
            MaxHealth += 1;
            GameplayManagerRef.UpdatePlayerMaxHealth(this);
            RecoverHealth(1);
            MaxHandSize += 1;
            GameplayManagerRef.UpdatePlayerInfoUICardCount(this);
        }
		else
		{
            RecoverHealth(1);
        }
		CurrentLevel++;
        
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

    #region AttackStatusEffects
    public void ActivatePlayerToAttackStatusEffectSelectionPopup(int numPlayersToChoose, string statusType, int statusDuration)
    {
        List<Tuple<Sprite, string, object, List<object>>> insertedParams = new();


        foreach (Player player in GameplayManagerRef.Players)
        {
            if (!player.IsDefeated && player != this)
            {
                List<object> paramsList = new();
                paramsList.Add(player);
				paramsList.Add(statusType);
				paramsList.Add(statusDuration);
                insertedParams.Add(Tuple.Create<Sprite, string, object, List<object>>(player.ClassData.defaultPortraitImage, nameof(SelectPlayerToAttackStatusEffect), this, paramsList));
            }
        }

        DialogueBoxPopup.instance.ActivatePopupWithImageChoices("Select the Player you wish to attack.", insertedParams, 1, "Attack");
    }

    /// <summary>
    /// Takes in a list of objects in this order: Player playerToAttack, int DamageToInflict, bool isElemental
    /// </summary>
    /// <returns></returns>
    public IEnumerator SelectPlayerToAttackStatusEffect(List<object> objects)
    {
        yield return null;
        Player playerTarget = objects[0] as Player;
        string statusType = (string)objects[1];
		int statusDuration = (int)objects[2];

        if (playerTarget != null)
        {
            if(statusType == "curse")
			{
				if(!playerTarget.IsPoisoned && !playerTarget.IsCursed)
				{
					StartCoroutine(GameplayManagerRef.GameplayCameraManagerRef.StatusEffectOpponentCutInPopup(this, playerTarget, statusType, statusDuration));
					yield break;
                }
			}
			else
			{
                if (!playerTarget.IsPoisoned && !playerTarget.IsCursed)
                {
					List<SupportCard> supportCardsToBlockWith = GetSupportCardsPlayerCanBlockPoisonWith(playerTarget);
					if(supportCardsToBlockWith.Count > 0 && playerTarget.NumSupportCardsUsedThisTurn < playerTarget.MaxSupportCardsToUse)
					{
						ActivatePlayerBlockPoisonSelectionPopup(playerTarget, supportCardsToBlockWith, statusDuration);
						yield break;
					}

					List<SupportCard> supportCardsToNegateWith = GetSupportCardsPlayersCanNegateSupportCardEffectsWith(playerTarget);
                    if (supportCardsToNegateWith.Count > 0 && playerTarget.NumSupportCardsUsedThisTurn < playerTarget.MaxSupportCardsToUse && !TriedToNegateCurrentSupportCard)
                    {
						TriedToNegateCurrentSupportCard = true;
                        ActivatePlayerNegateSupportCardPopup(playerTarget, supportCardsToNegateWith, GameplayManagerRef.CurrentSupportCardBeingUsed, GameplayManagerRef.GetCurrentPlayer());
                        yield break;
                    }

                    StartCoroutine(GameplayManagerRef.GameplayCameraManagerRef.StatusEffectOpponentCutInPopup(this, playerTarget, statusType, statusDuration));
                    yield break;
                }
            }
        }
        else
        {
            Debug.LogWarning($"Calling the method to attack the player but don't have a target...");
        }

        if (IsHandlingSpaceEffects || IsHandlingSupportCardEffects)
        {
            CompletedAttackingEffect();
        }
    }

    public void AttackAllOtherPlayersStatusEffect(string statusType, int statusDuration)
    {
        foreach (Player player in GameplayManagerRef.Players)
        {
            if (statusType == "curse")
            {
                if (!player.IsPoisoned && !player.IsCursed && player != this)
                {
                    player.CursePlayer(statusDuration);
					player.WasAfflictedWithStatusThisTurn = false;
                }
            }
            else
            {
                if (!player.IsPoisoned && !player.IsCursed && player != this)
                {
                    player.PoisonPlayer(statusDuration);
                    player.WasAfflictedWithStatusThisTurn = false;
                }
            }
        }
        //Put some dialogue box here for now to showcase that we've attacked all other players. Will need a log entry + animation here.

		//MAY HAVE TO CHANGE THIS BECAUSE IT'S AN ATTACK BUT KINDA BUT NOT RLLY.
        if (IsHandlingSpaceEffects || IsHandlingSupportCardEffects)
        {
            CompletedAttackingEffect();
        }
    }
    #endregion

    #region AttackDamage
    public void ActivatePlayerToAttackDamageSelectionPopup(int numPlayersToChoose, int damageToGive, bool isElemental = false)
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
				insertedParams.Add(Tuple.Create<Sprite, string, object, List<object>>(player.ClassData.defaultPortraitImage, nameof(SelectPlayerToAttackDamage), this, paramsList));
			}
		}

		DialogueBoxPopup.instance.ActivatePopupWithImageChoices("Select the Player you wish to attack.", insertedParams, 1, "Attack");
	}

	public void ActivatePlayerWithTargetSelectionToAttackDamageSelectionPopup(List<Player> validTargets, int damageToGive)
	{
        List<Tuple<Sprite, string, object, List<object>>> insertedParams = new();

        foreach (Player player in validTargets)
		{
            List<object> paramsList = new();
            paramsList.Add(player);
            paramsList.Add(damageToGive);
            insertedParams.Add(Tuple.Create<Sprite, string, object, List<object>>(player.ClassData.defaultPortraitImage, nameof(SelectPlayerToAttackDamageNoElement), this, paramsList));
        }

        DialogueBoxPopup.instance.ActivatePopupWithImageChoices("Select the Player you wish to attack.", insertedParams, 1, "Attack");
    }

    #region BLOCK EFFECT POPUPS
    public void ActivatePlayerBlockElementalDamageSelectionPopup(Player targetedPlayer, int damageToPotentiallytake, List<SupportCard> elementalBlockSupportCards)
    {
		//Move the camera to the targeted Player. Once they select a choice, move the camera back to the current Player. Moving the camera back will be in the coroutine most likely.

		//Camera code
		//alksdfja

        List<Tuple<string, string, object, List<object>>> insertedParams = new();

        List<object> paramsList = new();
        paramsList.Add(targetedPlayer);
        paramsList.Add(damageToPotentiallytake);
        paramsList.Add(elementalBlockSupportCards);
        insertedParams.Add(Tuple.Create<string, string, object, List<object>>("Yes", nameof(UseSupportCardToBlockElementalDamage), this, paramsList));
        insertedParams.Add(Tuple.Create<string, string, object, List<object>>("No", nameof(DontUseSupportCardToBlockElementalDamage), this, paramsList));

        DialogueBoxPopup.instance.ActivatePopupWithButtonChoices($"Player {targetedPlayer.playerIDIntVal} you have a support card that can block {damageToPotentiallytake} incoming elemental damage. Do you wish to use it?", insertedParams, 1, "Reaction");
    }

	public void ActivatePlayerBlockPoisonSelectionPopup(Player targetedPlayer, List<SupportCard> poisonBlockSupportCards, int turnsToBePoisoned)
	{

        // Move the camera to the targeted Player. Once they select a choice, move the camera back to the current Player. Moving the camera back will be in the coroutine most likely.

        //Camera code
        //alksdfja

        List<Tuple<string, string, object, List<object>>> insertedParams = new();

        List<object> paramsList = new();
        paramsList.Add(targetedPlayer);
        paramsList.Add(poisonBlockSupportCards);
        paramsList.Add(turnsToBePoisoned);
        insertedParams.Add(Tuple.Create<string, string, object, List<object>>("Yes", nameof(UseSupportCardToBlockPoison), this, paramsList));
		//NEED TO BE CAREFUL IF THIS IS A SPACE WE DON'T WANT THIS TO TRIGGER THE ANIMATION!!!!
        insertedParams.Add(Tuple.Create<string, string, object, List<object>>("No", nameof(DontUseSupportCardToBlockPoison), this, paramsList));

        DialogueBoxPopup.instance.ActivatePopupWithButtonChoices($"Player {targetedPlayer.playerIDIntVal} you have a support card that can prevent you from being poisoned for {turnsToBePoisoned} turn(s). Do you wish to use it?", insertedParams, 1, "Reaction");
    }

	public void ActivatePlayerNegateSupportCardPopup(Player targetedPlayer, List<SupportCard> negateSupportCards, SupportCard supportCardTryingToBeUsed, Player playerAttemptingToUseSupportCard)
	{
        // Move the camera to the targeted Player. Once they select a choice, move the camera back to the current Player. Moving the camera back will be in the coroutine most likely.

        //Camera code
        //alksdfja

        List<Tuple<string, string, object, List<object>>> insertedParams = new();

        List<object> paramsList = new();
        paramsList.Add(targetedPlayer);
        paramsList.Add(negateSupportCards);
        paramsList.Add(supportCardTryingToBeUsed);
		paramsList.Add(playerAttemptingToUseSupportCard);
        insertedParams.Add(Tuple.Create<string, string, object, List<object>>("Yes", nameof(UseSupportCardToNegateSupportCard), this, paramsList));
        insertedParams.Add(Tuple.Create<string, string, object, List<object>>("No", nameof(DontUseSupportCardToNegateSupportCard), this, paramsList));

        DialogueBoxPopup.instance.ActivatePopupWithButtonChoices($"Player {targetedPlayer.playerIDIntVal} you have a support card that can negate the {supportCardTryingToBeUsed.SupportCardData.CardTitle} support card. Do you wish to negate it?", insertedParams, 1, "Reaction");
    }

    public void ActivatePlayerStealSupportCardPopup(Player targetedPlayer, List<SupportCard> stealSupportCards, SupportCard supportCardTryingToBeUsed, Player playerAttemptingToUseSupportCard)
    {
        // Move the camera to the targeted Player. Once they select a choice, move the camera back to the current Player. Moving the camera back will be in the coroutine most likely.

        //Camera code
        //alksdfja

        List<Tuple<string, string, object, List<object>>> insertedParams = new();

        List<object> paramsList = new();
        paramsList.Add(targetedPlayer);
        paramsList.Add(stealSupportCards);
        paramsList.Add(supportCardTryingToBeUsed);
        paramsList.Add(playerAttemptingToUseSupportCard);
        insertedParams.Add(Tuple.Create<string, string, object, List<object>>("Yes", nameof(UseSupportCardToStealSupportCard), this, paramsList));
        insertedParams.Add(Tuple.Create<string, string, object, List<object>>("No", nameof(DontUseSupportCardToStealSupportCard), this, paramsList));

        DialogueBoxPopup.instance.ActivatePopupWithButtonChoices($"Player {targetedPlayer.playerIDIntVal} you have a {stealSupportCards[0].SupportCardData.CardTitle} card that can steal the {supportCardTryingToBeUsed.SupportCardData.CardTitle} support card. Do you wish to take it?", insertedParams, 1, "Reaction");
    }

    /// <summary>
    /// Takes in a list of objects in this order: Player targetedPlayer, List<SupportCard> stealSupportCards, SupportCard supportCardTryingToBeUsed, Player playerAttemptingToUseSupportCard
    /// </summary>
    /// <param name="objects"></param>
    /// <returns></returns>
    public IEnumerator UseSupportCardToStealSupportCard(List<object> objects)
    {
        yield return null;
        Player targetedPlayer = (Player)objects[0];
        List<SupportCard> stealSupportCards = (List<SupportCard>)objects[1];
        SupportCard supportCardTryingToBeUsed = (SupportCard)objects[2];
        Player playerAttemptingToUseSupportCard = (Player)objects[3];

        //Use the first card in the list.
        if (stealSupportCards.Count > 0)
        {
            stealSupportCards[0].AttemptToUseSupportCard(targetedPlayer, false);
          //  GameplayManagerRef.CurrentSupportCardBeingUsed = null;
        }

    }

    /// <summary>
    /// Takes in a list of objects in this order: Player targetedPlayer, List<SupportCard> stealSupportCards, SupportCard supportCardTryingToBeUsed, Player playerAttemptingToUseSupportCard
    /// </summary>
    /// <param name="objects"></param>
    /// <returns></returns>
    public IEnumerator DontUseSupportCardToStealSupportCard(List<object> objects)
    {
        yield return null;
        Player targetedPlayer = (Player)objects[0];
        List<SupportCard> stealSupportCards = (List<SupportCard>)objects[1];
        SupportCard supportCardTryingToBeUsed = (SupportCard)objects[2];
        Player playerAttemptingToUseSupportCard = (Player)objects[3];
    }



    /// <summary>
    /// Takes in a list of objects in this order: Player targetedPlayer, List<SupportCard> negateSupportCards, SupportCard supportCardTryingToBeUsed, Player playerAttemptingToUseSupportCard
    /// </summary>
    /// <param name="objects"></param>
    /// <returns></returns>
    public IEnumerator UseSupportCardToNegateSupportCard(List<object> objects)
	{
		yield return null;
        Player playerThatCanNegate = (Player)objects[0];
        List<SupportCard> negateSupportCards = (List<SupportCard>)objects[1];
		SupportCard supportCardTryingToBeUsed = (SupportCard)objects[2];
		Player playerAttemptingToUseSupportCard = (Player)objects[3];

        //Use the first card in the list.
        if (negateSupportCards.Count > 0)
        {
            playerAttemptingToUseSupportCard.UseSupportCard();
          //  playerAttemptingToUseSupportCard.DiscardFromHand(Card.CardType.Support, supportCardTryingToBeUsed);
			GameplayManagerRef.OnPlayerUsedASupportCard(supportCardTryingToBeUsed);
            //In here, we pass in the support card that is BEING NEGATED (supportcardtryingtobeused).
            negateSupportCards[0].AttemptToUseSupportCard(playerThatCanNegate, false);
			GameplayManagerRef.CurrentSupportCardBeingUsed = null;
        }

		//This needs to be something more generic to essentially say whatever is the CURRENT support card being used is done being used. Maybe call it's completed effect?
        if (IsHandlingSpaceEffects || IsHandlingSupportCardEffects)
        {
            CompletedAttackingEffect();
        }
    }

    /// <summary>
    /// Takes in a list of objects in this order: Player targetedPlayer, List<SupportCard> negateSupportCards, SupportCard supportCardTryingToBeUsed, Player playerAttemptingToUseSupportCard
    /// </summary>
    /// <param name="objects"></param>
    /// <returns></returns>
    public IEnumerator DontUseSupportCardToNegateSupportCard(List<object> objects)
    {
        yield return null;
        Player targetedPlayer = (Player)objects[0];
        List<SupportCard> negateBlockSupportCards = (List<SupportCard>)objects[1];
        SupportCard supportCardTryingToBeUsed = (SupportCard)objects[2];
        Player playerAttemptingToUseSupportCard = (Player)objects[3];

		TriedToNegateCurrentSupportCard = true;

        supportCardTryingToBeUsed.AttemptToUseSupportCard(playerAttemptingToUseSupportCard, false);
    }

    /// <summary>
    /// Takes in a list of objects in this order: Player targetedPlayer, int DamageToPotentiallyTake, List<SupportCard> elementalBlockSupportCards
    /// </summary>
    /// <param name="objects"></param>
    /// <returns></returns>
    public IEnumerator UseSupportCardToBlockElementalDamage(List<object> objects)
	{
		yield return null;
		Player targetedPlayer = (Player)objects[0];
		int damageToPotentiallytake = (int)objects[1];
        List<SupportCard> elementalBlockSupportCards = (List<SupportCard>)objects[2];

		//Use the first card in the list.
		if(elementalBlockSupportCards.Count > 0)
		{
			elementalBlockSupportCards[0].AttemptToUseSupportCard(targetedPlayer, false);
		}


        if (IsHandlingSpaceEffects || IsHandlingSupportCardEffects)
        {
            CompletedAttackingEffect();
        }

    }

    /// <summary>
    /// Takes in a list of objects in this order: Player targetedPlayer, int DamageToPotentiallyTake, List<SupportCard> elementalBlockSupportCards
    /// </summary>
    /// <param name="objects"></param>
    /// <returns></returns>
    public IEnumerator DontUseSupportCardToBlockElementalDamage(List<object> objects)
	{
		yield return null;

        Player targetedPlayer = (Player)objects[0];
        int damageToPotentiallytake = (int)objects[1];
        List<SupportCard> elementalBlockSupportCards = (List<SupportCard>)objects[2];

		StartCoroutine(GameplayManagerRef.GameplayCameraManagerRef.DamageOpponentCutInPopup(this, targetedPlayer, damageToPotentiallytake));
    }

    /// <summary>
    /// Takes in a list of objects in this order: Player targetedPlayer, List<SupportCard> poisonBlockSupportCards, int turnsToBePoisoned
    /// </summary>
    /// <param name="objects"></param>
    /// <returns></returns>
    public IEnumerator UseSupportCardToBlockPoison(List<object> objects) 
	{
        yield return null;

        Player targetedPlayer = (Player)objects[0];
        List<SupportCard> elementalBlockSupportCards = (List<SupportCard>)objects[1];
        int turnsToBePoisoned = (int)objects[2];

        if (IsHandlingSpaceEffects || IsHandlingSupportCardEffects)
        {
            CompletedAttackingEffect();
        }
    }

    /// <summary>
    /// Takes in a list of objects in this order: Player targetedPlayer, List<SupportCard> poisonBlockSupportCards, int turnsToBePoisoned
    /// </summary>
    /// <param name="objects"></param>
    /// <returns></returns>
    public IEnumerator DontUseSupportCardToBlockPoison(List<object> objects)
    {
        yield return null;

        Player targetedPlayer = (Player)objects[0];
        List<SupportCard> elementalBlockSupportCards = (List<SupportCard>)objects[1];
        int turnsToBePoisoned = (int)objects[2];

        StartCoroutine(GameplayManagerRef.GameplayCameraManagerRef.StatusEffectOpponentCutInPopup(this, targetedPlayer, "poison", turnsToBePoisoned));
    }

    #endregion

    /// <summary>
    /// Takes in a list of objects in this order: Player playerToAttack, int DamageToInflict, bool isElemental
    /// </summary>
    /// <returns></returns>
    public IEnumerator SelectPlayerToAttackDamage(List<object> objects)
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
					if(CheckIfOtherPlayersCanReact().Count > 0)
					{
						List<SupportCard> supportCardsToBlockWith = GetSupportCardsPlayerCanBlockElementalDamageWith(playerTarget);
						if(supportCardsToBlockWith.Count > 0 && playerTarget.NumSupportCardsUsedThisTurn < playerTarget.MaxSupportCardsToUse)
						{
							ActivatePlayerBlockElementalDamageSelectionPopup(playerTarget, damageToTake, supportCardsToBlockWith);
                            yield break;
                        }

                        List<SupportCard> supportCardsToNegateWith = GetSupportCardsPlayersCanNegateSupportCardEffectsWith(playerTarget);
                        if (supportCardsToNegateWith.Count > 0 && playerTarget.NumSupportCardsUsedThisTurn < playerTarget.MaxSupportCardsToUse && !TriedToNegateCurrentSupportCard)
                        {
							TriedToNegateCurrentSupportCard = true;
                            ActivatePlayerNegateSupportCardPopup(playerTarget, supportCardsToNegateWith, GameplayManagerRef.CurrentSupportCardBeingUsed, GameplayManagerRef.GetCurrentPlayer());
                            yield break;
                        }

                        StartCoroutine(GameplayManagerRef.GameplayCameraManagerRef.DamageOpponentCutInPopup(this, playerTarget, damageToTake));
						yield break;
                    }
					else
					{
                        StartCoroutine(GameplayManagerRef.GameplayCameraManagerRef.DamageOpponentCutInPopup(this, playerTarget, damageToTake));
                        yield break;
                    }
				}
				else if(CheckIfOtherPlayersCanReact().Count > 0)
				{
                    List<SupportCard> supportCardsToNegateWith = GetSupportCardsPlayersCanNegateSupportCardEffectsWith(playerTarget);
                    if (supportCardsToNegateWith.Count > 0 && playerTarget.NumSupportCardsUsedThisTurn < playerTarget.MaxSupportCardsToUse && !TriedToNegateCurrentSupportCard)
                    {
                        TriedToNegateCurrentSupportCard = true;
                        ActivatePlayerNegateSupportCardPopup(playerTarget, supportCardsToNegateWith, GameplayManagerRef.CurrentSupportCardBeingUsed, GameplayManagerRef.GetCurrentPlayer());
                        yield break;
                    }

                    StartCoroutine(GameplayManagerRef.GameplayCameraManagerRef.DamageOpponentCutInPopup(this, playerTarget, damageToTake));
                    yield break;
                }
				else
				{
                    StartCoroutine(GameplayManagerRef.GameplayCameraManagerRef.DamageOpponentCutInPopup(this, playerTarget, damageToTake));
                    yield break;
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

    /// <summary>
    /// Takes in a list of objects in this order: Player playerToAttack, int DamageToInflict
    /// </summary>
    /// <returns></returns>
    public IEnumerator SelectPlayerToAttackDamageNoElement(List<object> objects)
    {
        yield return null;
        Player playerTarget = objects[0] as Player;
        int damageToTake = (int)objects[1];

        if (playerTarget != null)
        {
            if (damageToTake > 0)
            {
                StartCoroutine(GameplayManagerRef.GameplayCameraManagerRef.DamageOpponentCutInPopup(this, playerTarget, damageToTake));
				yield break;
            }
        }
        else
        {
            Debug.LogWarning($"Calling the method to attack the player but don't have a target...");
        }

        CompletedAttackingEffect();
    }

    public void AttackAllOtherPlayersDamage(int damageToGive, bool isElemental = false)
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

	public void AttackAllValidTargetsNoElement(List<Player> targetsToAttack, int damageToGive)
	{
		foreach(Player player in targetsToAttack)
		{
            //Will need a way to queue up death scene if a player in this chain of events is dealt a killing blow from this attack.
            player.TakeDamage(damageToGive);
		}

		//Put some dialogue box here for now to showcase that we've attacked all other players. Will need a log entry + animation here.
		CompletedAttackingEffect();

    }
    #endregion

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
					if (player.MovementCardsInHandCount < numCardsToTakeFromOpponent)
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

					if (player.SupportCardsInHandCount < numCardsToTakeFromOpponent)
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
				int cardToTakeIndex = Random.Range(0, playerToAttack.MovementCardsInHandCount);
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
				int cardToTakeIndex = Random.Range(0, playerToAttack.SupportCardsInHandCount);
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
				int cardToTakeIndex = Random.Range(0, playerToAttack.MovementCardsInHandCount);
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
				int cardToTakeIndex = Random.Range(0, playerToAttack.SupportCardsInHandCount);
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

    #region TakeCardsFromOpponentsSupportEffect

    public void ActivateChoosePlayerToTakeCardsFromSupportSelectionPopup(int numCardsToDiscard, Card.CardType cardTypeToDiscard, int numCardsToTakeFromOpponent, Card.CardType cardTypeToTakeFromOpponent)
    {
        List<Tuple<Sprite, string, object, List<object>>> insertedParams = new();


        foreach (Player player in GameplayManagerRef.Players)
        {
            if (!player.IsDefeated && player != this)
            {
                if(cardTypeToTakeFromOpponent == CardType.Movement && player.MovementCardsInHandCount < numCardsToTakeFromOpponent)
				{
					continue;
				}
                else if (cardTypeToTakeFromOpponent == CardType.Support && player.SupportCardsInHandCount < numCardsToTakeFromOpponent)
                {
                    continue;
                }
				else if(cardTypeToTakeFromOpponent == CardType.Both && player.CardsInhand.Count < numCardsToTakeFromOpponent)
				{
					continue;
				}
                List<object> paramsList = new();
                paramsList.Add(player);
                paramsList.Add(numCardsToDiscard);
                paramsList.Add(cardTypeToDiscard);
                paramsList.Add(numCardsToTakeFromOpponent);
                paramsList.Add(cardTypeToTakeFromOpponent);

                insertedParams.Add(Tuple.Create<Sprite, string, object, List<object>>(player.ClassData.defaultPortraitImage, nameof(AfterSelectingPlayerToTakeCardsFromSupport), this, paramsList));
            }
        }

        DialogueBoxPopup.instance.ActivatePopupWithImageChoices("Select the Player you wish to take cards from.", insertedParams, 1, "Attack");
    }

	/// <summary>
	/// Activate this popup to discard cards before taking them. This popup should display each of the Player's valid cards they can discard.
	/// </summary>
	/// <param name="numCardsToDiscard"></param>
	/// <param name="cardTypeToDiscard"></param>
	/// <param name="numCardsToTakeFromOpponent"></param>
	/// <param name="cardTypeToTakeFromOpponent"></param>
    public void ActivateChooseCardsToDiscardFromSupportSelectionPopup(Player playerTarget, int numCardsToDiscard, Card.CardType cardTypeToDiscard, int numCardsToTakeFromOpponent, Card.CardType cardTypeToTakeFromOpponent)
	{
        List<Tuple<Sprite, string, object, List<object>>> insertedParams = new();


        foreach (Player player in GameplayManagerRef.Players)
        {
            if (!player.IsDefeated && player == this)
            {
				if(cardTypeToDiscard == CardType.Movement && player.MovementCardsInHandCount >= numCardsToDiscard)
				{
					foreach(MovementCard movementCard in player.GetMovementCardsInHand())
					{
                        List<object> paramsList = new();
                        paramsList.Add(playerTarget);
                        paramsList.Add(numCardsToDiscard);
                        paramsList.Add(cardTypeToDiscard);
                        paramsList.Add(numCardsToTakeFromOpponent);
                        paramsList.Add(cardTypeToTakeFromOpponent);
						paramsList.Add(movementCard);

                        insertedParams.Add(Tuple.Create<Sprite, string, object, List<object>>(movementCard.CardArtworkImage.sprite, nameof(SelectCardsToDiscardBeforeTaking), this, paramsList));
                    }
				}
                else if (cardTypeToDiscard == CardType.Support && player.SupportCardsInHandCount >= numCardsToDiscard)
                {
                    foreach (SupportCard supportCard in player.GetSupportCardsInHand())
                    {
                        List<object> paramsList = new();
                        paramsList.Add(playerTarget);
                        paramsList.Add(numCardsToDiscard);
                        paramsList.Add(cardTypeToDiscard);
                        paramsList.Add(numCardsToTakeFromOpponent);
                        paramsList.Add(cardTypeToTakeFromOpponent);
						paramsList.Add(supportCard);

                        insertedParams.Add(Tuple.Create<Sprite, string, object, List<object>>(supportCard.CardArtworkImage.sprite, nameof(SelectCardsToDiscardBeforeTaking), this, paramsList));
                    }
                }
				else
				{
                    foreach (Card card in player.CardsInhand)
                    {
                        List<object> paramsList = new();
                        paramsList.Add(playerTarget);
                        paramsList.Add(numCardsToDiscard);
                        paramsList.Add(cardTypeToDiscard);
                        paramsList.Add(numCardsToTakeFromOpponent);
                        paramsList.Add(cardTypeToTakeFromOpponent);
						paramsList.Add(card);

                        insertedParams.Add(Tuple.Create<Sprite, string, object, List<object>>(card.CardArtworkImage.sprite, nameof(SelectCardsToDiscardBeforeTaking), this, paramsList));
                    }
                }
            }
        }

        DialogueBoxPopup.instance.ActivatePopupWithImageChoices("Select the card(s) you wish to discard.", insertedParams, numCardsToDiscard, "Discard");
    }

    /// <summary>
    /// Takes in a list of objects in this order: Player playerToAttack, int numCardsToDiscard, Card.CardType cardTypeToDiscard, int numCardsToTakeFromOpponent, Card.CardType cardTypeToTakeFromOpponent, Card cardSelected
    /// </summary>
    /// <returns></returns>
    IEnumerator AfterSelectingPlayerToTakeCardsFromSupport(List<object> objects)
	{
        yield return null;
        Player playerToAttack = (Player)objects[0];
        int numCardsToDiscard = (int)objects[1];
        CardType cardTypeToDiscard = (CardType)objects[2];
        int numCardsToTakeFromOpponent = (int)objects[3];
        CardType cardTypeToTakeFromOpponent = (CardType)objects[4];

		ActivateChooseCardsToDiscardFromSupportSelectionPopup(playerToAttack, numCardsToDiscard, cardTypeToDiscard, numCardsToTakeFromOpponent, cardTypeToTakeFromOpponent);
    }

    /// <summary>
    /// Activate this popup to take cards from the opponent's hand. This popup should display each of the Player's valid cards they can discard.
    /// </summary>
    /// <param name="numCardsToTakeFromOpponent"></param>
    /// <param name="cardTypeToTakeFromOpponent"></param>
    public void ActivateChooseCardsToTakeFromSupportSelectionPopup(Player playerTarget, int numCardsToTakeFromOpponent, Card.CardType cardTypeToTakeFromOpponent)
    {
        List<Tuple<Sprite, string, object, List<object>>> insertedParams = new();


        foreach (Player player in GameplayManagerRef.Players)
        {
            if (!player.IsDefeated && player == playerTarget)
            {
                if (cardTypeToTakeFromOpponent == CardType.Movement && player.MovementCardsInHandCount >= numCardsToTakeFromOpponent)
                {
                    foreach (MovementCard movementCard in player.GetMovementCardsInHand())
                    {
                        List<object> paramsList = new();
                        paramsList.Add(player);
                        paramsList.Add(numCardsToTakeFromOpponent);
                        paramsList.Add(cardTypeToTakeFromOpponent);
                        paramsList.Add(movementCard);

                        insertedParams.Add(Tuple.Create<Sprite, string, object, List<object>>(movementCard.CardArtworkImage.sprite, nameof(SelectCardsToTakeFromOpponentSupport), this, paramsList));
                    }
                }
                else if (cardTypeToTakeFromOpponent == CardType.Support && player.SupportCardsInHandCount >= numCardsToTakeFromOpponent)
                {
                    foreach (SupportCard supportCard in player.GetSupportCardsInHand())
                    {
                        List<object> paramsList = new();
                        paramsList.Add(player);
                        paramsList.Add(numCardsToTakeFromOpponent);
                        paramsList.Add(cardTypeToTakeFromOpponent);
                        paramsList.Add(supportCard);

                        insertedParams.Add(Tuple.Create<Sprite, string, object, List<object>>(supportCard.CardArtworkImage.sprite, nameof(SelectCardsToTakeFromOpponentSupport), this, paramsList));
                    }
                }
                else
                {
                    foreach (Card card in player.CardsInhand)
                    {
                        List<object> paramsList = new();
                        paramsList.Add(player);
                        paramsList.Add(numCardsToTakeFromOpponent);
                        paramsList.Add(cardTypeToTakeFromOpponent);
                        paramsList.Add(card);

                        insertedParams.Add(Tuple.Create<Sprite, string, object, List<object>>(card.CardArtworkImage.sprite, nameof(SelectCardsToTakeFromOpponentSupport), this, paramsList));
                    }
                }
            }
        }

        DialogueBoxPopup.instance.ActivatePopupWithImageChoices($"Select the card(s) you wish to take from player {playerTarget.playerIDIntVal}.", insertedParams, numCardsToTakeFromOpponent, "Attack");
    }



    /// <summary>
    /// Takes in a list of objects in this order: Player playerToAttack, int numCardsToDiscard, Card.CardType cardTypeToDiscard, int numCardsToTakeFromOpponent, Card.CardType cardTypeToTakeFromOpponent, Card cardSelected
    /// </summary>
    /// <returns></returns>
    public IEnumerator SelectCardsToDiscardBeforeTaking(List<object> objects)
    {
        yield return null;
        Player playerToAttack = (Player)objects[0];
        int numCardsToDiscard = (int)objects[1];
        Card.CardType cardTypeToDiscard = (Card.CardType)objects[2];
        int numCardsToTakeFromOpponent = (int)objects[3];
        Card.CardType cardTypeToTakeFromOpponent = (Card.CardType)objects[4];
		Card cardToDiscard = (Card)objects[5];

		//Discard the card that is needed for cost.
        if(cardToDiscard.ThisCardType == CardType.Movement)
		{
			MovementCard movementCard = cardToDiscard as MovementCard; 
			if(movementCard != null)
			{
				DiscardFromHand(CardType.Movement, movementCard);
			}
		}
		else if(cardToDiscard.ThisCardType == CardType.Support)
		{
            SupportCard supportCard = cardToDiscard as SupportCard;
            if (supportCard != null)
            {
                DiscardFromHand(CardType.Support, supportCard);
            }
        }

		//Call popup method for Player to now take card from opponent.
		ActivateChooseCardsToTakeFromSupportSelectionPopup(playerToAttack, numCardsToTakeFromOpponent, cardTypeToTakeFromOpponent);

    }

    /// <summary>
    /// Takes in a list of objects in this order: Player playerToAttack, int numCardsToTakeFromOpponent, Card.CardType cardTypeToTakeFromOpponent, Card cardSelectedToTake
    /// </summary>
    /// <returns></returns>
    public IEnumerator SelectCardsToTakeFromOpponentSupport(List<object> objects)
    {
        yield return null;
        Player playerToAttack = (Player)objects[0];
		int numCardsToTakeFromOpponent = (int)objects[1];
		CardType cardTypeToTake = (CardType)objects[2];
		Card cardSelectedToTake = (Card)objects[3];

        //Take the card that was selected by the attacking player.
        if (cardSelectedToTake.ThisCardType == CardType.Movement)
        {
            MovementCard movementCard = cardSelectedToTake as MovementCard;
            if (movementCard != null)
            {
                playerToAttack.DiscardFromHand(CardType.Movement, movementCard);
				DrawCard(movementCard);
				StartCoroutine(movementCard.LeaveCardEffect());
            }
        }
        else if (cardSelectedToTake.ThisCardType == CardType.Support)
        {
            SupportCard supportCard = cardSelectedToTake as SupportCard;
            if (supportCard != null)
            {
                playerToAttack.DiscardFromHand(CardType.Support, supportCard);
				DrawCard(supportCard);
                StartCoroutine(supportCard.LeaveCardEffect());
            }
        }

        if (IsHandlingSpaceEffects || IsHandlingSupportCardEffects)
        {
            CompletedAttackingEffect();
        }
    }


    #endregion

    #endregion

    #region Check for special support card negation

    /// <summary>
    /// For negation effects like smoke bomb.
    /// </summary>
    /// <returns></returns>
    public List<Player> CheckIfOtherPlayersCanNegateWithoutSingleTarget()
	{
		List<Player> playersThatCanNegate = new();
		foreach(Player player in GameplayManagerRef.Players)
		{
			if(player != this)
			{
				foreach(SupportCard supportCard in player.GetSupportCardsInHand())
				{
					foreach(SupportCardData.SupportCardEffect effect in supportCard.SupportCardData.supportCardEffects)
					{
						if(effect.supportCardEffectData.GetType() == typeof(NegateSupportCardEffect))
						{
							NegateSupportCardEffect negateSupportCardEffect = (NegateSupportCardEffect)effect.supportCardEffectData;
							if(!negateSupportCardEffect.RequiresSingleTarget)
							{
                                playersThatCanNegate.Add(player);
                                break;
                            }
						}
					}
					if(playersThatCanNegate.Contains(player))
					{
						break;
					}
				}
			}
		}

		return playersThatCanNegate;
    }

	/// <summary>
	/// For grappling hoook effect.
	/// </summary>
	/// <returns></returns>
	public List<Player> CheckIfOtherPlayersCanSteal()
	{
        List<Player> playersThatCanNegate = new();
        foreach (Player player in GameplayManagerRef.Players)
        {
            if (player != this)
            {
                foreach (SupportCard supportCard in player.GetSupportCardsInHand())
                {
                    foreach (SupportCardData.SupportCardEffect effect in supportCard.SupportCardData.supportCardEffects)
                    {
                        if (effect.supportCardEffectData.GetType() == typeof(GrapplingHookEffect))
                        {
                            playersThatCanNegate.Add(player);
                            break;
                        }
                    }
                    if (playersThatCanNegate.Contains(player))
                    {
                        break;
                    }
                }
            }
        }

        return playersThatCanNegate;
    }

	/// <summary>
	/// Check if Player that is targeted can react with a support card that blocks elemental damage.
	/// </summary>
	/// <param name="player"></param>
	/// <returns></returns>
	public List<SupportCard> GetSupportCardsPlayerCanBlockElementalDamageWith(Player player)
	{
		List<SupportCard> supportCardsToBlockWith = new();

        foreach (SupportCard supportCard in player.GetSupportCardsInHand())
        {
            foreach (SupportCardData.SupportCardEffect supportCardEffect in supportCard.SupportCardData.supportCardEffects)
            {
                if (supportCardEffect.supportCardEffectData.GetType() == typeof(BlockElementalEffect))
                {
					supportCardsToBlockWith.Add(supportCard);
                    break;
                }	
            }
        }

		return supportCardsToBlockWith;
    }

	/// <summary>
	/// Check if Player that is targeted can react with a support card that blocks being poisoned.
	/// </summary>
	/// <param name="player"></param>
	/// <returns></returns>
	public List<SupportCard> GetSupportCardsPlayerCanBlockPoisonWith(Player player)
	{
        List<SupportCard> supportCardsToBlockWith = new();

        foreach (SupportCard supportCard in player.GetSupportCardsInHand())
        {
            foreach (SupportCardData.SupportCardEffect supportCardEffect in supportCard.SupportCardData.supportCardEffects)
            {
                if (supportCardEffect.supportCardEffectData.GetType() == typeof(BlockPoisonEffect))
                {
                    supportCardsToBlockWith.Add(supportCard);
                    break;
                }
            }
        }

        return supportCardsToBlockWith;
    }

    /// <summary>
    /// Check if Players can react with a support card that negates another support card.
    /// </summary>
    /// <param name="player"></param>
    /// <returns></returns>
    public List<SupportCard> GetSupportCardsPlayersCanNegateSupportCardEffectsWith(Player player)
	{
        List<SupportCard> supportCardsToNegateWith = new();

        foreach (SupportCard supportCard in player.GetSupportCardsInHand())
        {
            foreach (SupportCardData.SupportCardEffect supportCardEffect in supportCard.SupportCardData.supportCardEffects)
            {
                if (supportCardEffect.supportCardEffectData.GetType() == typeof(NegateSupportCardEffect))
                {
                    supportCardsToNegateWith.Add(supportCard);
                    break;
                }
            }
        }

        return supportCardsToNegateWith;
    }

    /// <summary>
    /// Check if Players can react with a support card that steals another support card.
    /// </summary>
    /// <param name="player"></param>
    /// <returns></returns>
    public List<SupportCard> GetSupportCardsPlayersCanStealSupportCardsWith(Player player)
	{
        List<SupportCard> supportCardsToStealWith = new();

        foreach (SupportCard supportCard in player.GetSupportCardsInHand())
        {
            foreach (SupportCardData.SupportCardEffect supportCardEffect in supportCard.SupportCardData.supportCardEffects)
            {
                if (supportCardEffect.supportCardEffectData.GetType() == typeof(GrapplingHookEffect))
                {
                    supportCardsToStealWith.Add(supportCard);
                    break;
                }
            }
        }

        return supportCardsToStealWith;
    }

	public List<Player> CheckIfOtherPlayersCanReact()
	{
        List<Player> playersThatCanBlock = new();
        foreach (Player player in GameplayManagerRef.Players)
        {
            if (player != this)
            {
                foreach (SupportCard supportCard in player.GetSupportCardsInHand())
                {
					foreach(SupportCardData.SupportCardEffect supportCardEffect in supportCard.SupportCardData.supportCardEffects)
					{
						if(supportCardEffect.supportCardEffectData.IsReaction)
						{
							playersThatCanBlock.Add(player);
							break;
						}
						if(playersThatCanBlock.Contains(player))
						{
							break;
						}
					}
                    if (playersThatCanBlock.Contains(player))
                    {
                        break;
                    }
                }
            }
        }

        return playersThatCanBlock;
    }

	public void TargetThisPlayerForAttack()
	{

	}

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
				tempMovementCard.ManipulateMovementValue(true);
				CurseCardAnimationEffect();
			}
		}
	}

	public void DrawThenUseMovementCardImmediatelyMovement()
	{
		gameplayManagerRef.ThisDeckManager.DrawCard(CardType.Movement, this);

		//Might hafta turn this into another method and wait till an event is triggered to signify the draw being completed.
		foreach (Card card in CardsInhand)
		{
			if(card is MovementCard)
			{
                MovementCard movementCard = (MovementCard)card;
                movementCard.AttemptToMove(this);
				NoMovementCardsInHandButton.gameObject.SetActive(false);
				break;
			}
		}
	}

    public void DrawThenUseMovementCardImmediatelyDuel()
    {
        gameplayManagerRef.ThisDeckManager.DrawCard(CardType.Movement, this);

        //Might hafta turn this into another method and wait till an event is triggered to signify the draw being completed.
        foreach (Card card in CardsInhand)
        {
            if (card is MovementCard)
            {
				//Dunno if we should curse this card if the Player is cursed since technically it's not a card from their hand...
                MovementCard movementCard = (MovementCard)card;
				movementCard.AddCardToSelectedCardsDuel(true);
				Debug.Log($"Didn't have a movement card so we will use the newly drawn one from the deck with a value of: {movementCard.MovementCardValue}");
                break;
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
						tempMovementCard.ManipulateMovementValue(true);
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
		if(!NoMovementCardsInHandButton.IsActive() && MovementCardsInHandCount < 1 && GameplayManagerRef.GameplayPhaseStatemachineRef.GetCurrentState() == GameplayManagerRef.GameplayPhaseStatemachineRef.gameplayMovementPhaseState)
		{
			NoMovementCardsInHandButton.gameObject.SetActive(true);
		}

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
		if(NoMovementCardsInHandButton.IsActive())
		{
			NoMovementCardsInHandButton.gameObject.SetActive(false);
		}
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
					if(!(MovementCardsInHandCount < numToDiscard))
					{
						hasEnough = true;
					}
					break;
				}
			case CardType.Support:
				{
					if (!(SupportCardsInHandCount < numToDiscard))
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

		if(gameplayManagerRef.GameplayPhaseStatemachineRef.GetCurrentState() != gameplayManagerRef.GameplayPhaseStatemachineRef.gameplayDuelPhaseState && !GameplayManagerRef.UseSelectedCardsPanel.activeInHierarchy)
		{
			GameplayManagerRef.UseSelectedCardsPanel.SetActive(true);
		}

		GameplayManagerRef.UseSelectedCardsText.text = $"Selected cards: {numSelected}/{maxNumPlayerCanSelect}";
		GameplayManagerRef.UseSelectedCardsButton.onClick.RemoveAllListeners();
		GameplayManagerRef.UseSelectedCardsButton.onClick.AddListener(UseMultipleCards);
		List<Tuple<string, string, object>> insertedParams = new();
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
		GameplayManagerRef.SpacesPlayerWillLandOnParent.TurnOffDisplay();
		GameplayManagerRef.HandDisplayPanel.ShrinkHand();
		GameplayManagerRef.StartMove(totalToUse);
		CurrentSumOfSpacesToMove = 0;
		
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
	}


	public void SetMovementCardsInHand()
	{
		MovementCardsInHandCount = 0;
        List<MovementCard> movementCardsInHand = new();
        movementCardsInHand = GetMovementCardsInHand();
        if (movementCardsInHand.Count != 0)
        {
            foreach (MovementCard card in movementCardsInHand)
            {
                card.gameObject.transform.SetParent(MovementCardsInHandHolderPanel.transform);
            }
        }

        MovementCardsInHandCount = movementCardsInHand.Count;

        if (GameplayManagerRef is not null)
        {
            GameplayManagerRef.UpdatePlayerInfoUICardCount(this);
        }

    }

	public List<MovementCard> GetMovementCardsInHand()
	{
		List<MovementCard> movementCardsInHand = new();

        foreach (Card card in CardsInhand)
        {
            MovementCard tempMovementCard = card as MovementCard;
            if (tempMovementCard is MovementCard)
            {
				movementCardsInHand.Add(tempMovementCard);
            }
        }

		return movementCardsInHand;
    }

	public void SetSupportCardsInHand()
	{
		SupportCardsInHandCount = 0;
		List<SupportCard> supportCardsInHand = new();
		supportCardsInHand = GetSupportCardsInHand();
	    if(supportCardsInHand.Count != 0)
		{
			foreach(SupportCard card in supportCardsInHand)
			{
                card.gameObject.transform.SetParent(SupportCardsInHandHolderPanel.transform);
            }
		}

		SupportCardsInHandCount = supportCardsInHand.Count;

		if (GameplayManagerRef is not null)
		{
			GameplayManagerRef.UpdatePlayerInfoUICardCount(this);
		}
	}

	public List<SupportCard> GetSupportCardsInHand()
	{
        List<SupportCard> supportCardsInHand = new();

        foreach (Card card in CardsInhand)
        {
            SupportCard tempSupportCard = card as SupportCard;
            if (tempSupportCard is SupportCard)
            {
                supportCardsInHand.Add(tempSupportCard);
            }
        }

        return supportCardsInHand;
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
				return;
			}
            GameplayManagerRef.UpdatePlayerInfoUIStatusEffect(this);
        }

		if (IsPoisoned)
		{
			PoisonEffect();
			PoisonDuration -= 1;
			if (PoisonDuration == 0)
			{
                GameplayManagerRef.UpdatePlayerInfoUIStatusEffect(this);
                IsPoisoned = false;
				return;
			}
            GameplayManagerRef.UpdatePlayerInfoUIStatusEffect(this);
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
		if(CurrentHealth > 1)
		{
            TakeDamage(1);
        }
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

		CompletedRecoveringHealthForEffect();

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
		HasBeenDefeatedEvent();

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

		if (gameplayManagerRef.GameplayPhaseStatemachineRef.GetCurrentState().GetType() == typeof(GameplayStartResolveSpacePhaseState))
		{
			CompletedHandlingSpaceEffects();
            return;
		}

		CompletedHandlingSpaceEffects();
	}

	private void ExecuteNextSpaceEffect()
	{
		if(spaceEffectsToHandle.Count > 0)
		{
			currentSpaceEffectDataToHandle = spaceEffectsToHandle.Dequeue();

			//Change this check to be based on state of the game rather than movement cards.
			if(gameplayManagerRef.GameplayPhaseStatemachineRef.GetCurrentState().GetType() == typeof(GameplayStartResolveSpacePhaseState))
			{
				currentSpaceEffectDataToHandle.StartOfTurnEffect(this);
			}
			else if(gameplayManagerRef.GameplayPhaseStatemachineRef.GetCurrentState().GetType() == typeof(GameplayEndPhaseState))
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

    #region Manipulate card values in hand
    //This can stack. Example: Cursed + also on a space that halves movement card values.
    public void HalveAllMovementCardsInHand()
	{
		foreach (Card card in CardsInhand)
		{
			MovementCard tempMovementCard = card as MovementCard;

			if (tempMovementCard != null)
			{
				tempMovementCard.ManipulateMovementValue(true);
				tempMovementCard.ActivateCurseEffect();
			}
		}
	}

    public void BoostAllMovementCardValuesInHand(int valueToBoostBy)
    {
        List<MovementCard> movementCardsInHand = new();
        movementCardsInHand = GetMovementCardsInHand();

        foreach (MovementCard movementCard in movementCardsInHand)
        {
			movementCard.ManipulateMovementValue(false, true, valueToBoostBy);
        }
    }


    public void RevertAllBoostedMovementCardValuesInHand()
	{
        List<MovementCard> movementCardsInHand = new();
        movementCardsInHand = GetMovementCardsInHand();

		foreach(MovementCard movementCard in movementCardsInHand)
		{
			movementCard.RevertBoostedCardValue();
		}
    }

    #endregion

    #endregion

    #region SupportCardEffectHandlers
    public void StartHandlingSupportCardEffects(SupportCard supportCardBeingUsed)
	{
		IsHandlingSupportCardEffects = true;

		foreach (SupportCardEffectData supportCardEffect in supportCardEffectsToHandle)
		{
			supportCardEffect.SupportCardEffectCompleted += ExecuteNextSupportCardEffect;
			tempSupportCardEffectsToHandle.Add(supportCardEffect);
		}
		ExecuteNextSupportCardEffect(supportCardBeingUsed);
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
		TriedToNegateCurrentSupportCard = false;

        List<Player> playersThatCanSteal = new();
        playersThatCanSteal = CheckIfOtherPlayersCanSteal();

        if (playersThatCanSteal.Count > 0)
        {
            List<SupportCard> supportCards = GetSupportCardsPlayersCanStealSupportCardsWith(playersThatCanSteal[0]);
			bool wasCardUsedAGrapplingHook = false;

			if(GameplayManagerRef.CurrentSupportCardBeingUsed != null)
			{
                foreach (SupportCardData.SupportCardEffect effect in GameplayManagerRef.CurrentSupportCardBeingUsed.SupportCardData.supportCardEffects)
                {
                    if (effect.GetType() == typeof(GrapplingHookEffect))
                    {
                        wasCardUsedAGrapplingHook = true;
                        break;
                    }
                }

                if (wasCardUsedAGrapplingHook)
                {
                    return;
                }
                //Need this to be dynamic so that if Player 1 says no, Player 2 has a chance to respond etc.
                ActivatePlayerStealSupportCardPopup(playersThatCanSteal[0], supportCards, GameplayManagerRef.CurrentSupportCardBeingUsed, this);
            }
			
        }

		//We'll need to up the counter of the amount of Support cards the user has used this turn here.
	}

	private void ExecuteNextSupportCardEffect(SupportCard supportCardBeingUsed)
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
	public void ApplyCurrentSpaceEffects(Player player)
	{
		if (GameplayManagerRef.playerCharacter.GetComponent<Player>() == this)
		{
			//We'll check states for the phases of the game here to determine whether or not to use the landed on effect/passing effect/start of turn effect.
			if (gameplayManagerRef.GameplayPhaseStatemachineRef.GetCurrentState().GetType() == typeof(GameplayStartResolveSpacePhaseState))
			{
				CurrentSpacePlayerIsOn.ApplyStartOfTurnSpaceEffects(this);
			}
			else if(gameplayManagerRef.GameplayPhaseStatemachineRef.GetCurrentState().GetType() == typeof(GameplayEndPhaseState))
			{
				CurrentSpacePlayerIsOn.ApplyEndOfTurnSpaceEffects(this);
			}
			else
			{
				CurrentSpacePlayerIsOn.ApplyLandedOnSpaceEffects(this);
			}
		}
	}

	public void ApplyCurrentStartSpaceEffects()
	{

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
			if(ClassData.eliteAbilityData.HasACost && !ClassData.eliteAbilityData.CanCostBePaid(this))
			{
				return;
			}

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

    #region Audio/Sound Related

	//Most of these methods are going to be played via an animation event.
	public void PlayStepSound(int numSoundToPlay)
	{
		if(ClassData.ClassAudioDataHolder.defaultWalkingSoundData == null)
		{
			Debug.LogWarning("No step sound audio data on the class data scriptable!");
			return;
		}

		if(numSoundToPlay > ClassData.ClassAudioDataHolder.defaultWalkingSoundData.SfxClips.Count || numSoundToPlay <= 0)
		{
			Debug.LogError($"You're trying to play a sound that exceeds the index of the array. There are {ClassData.ClassAudioDataHolder.defaultWalkingSoundData.SfxClips.Count} sounds in the audio data but you're trying to play sound # {numSoundToPlay}");
			return;
		}

		int indexToPlay = 0;
        indexToPlay = numSoundToPlay - 1;

        Audio_Manager.Instance.PlaySFX(ClassData.ClassAudioDataHolder.defaultWalkingSoundData.SfxClips[indexToPlay].Clip);
    }

	public void PlayStepSoundRandom()
	{
        if (ClassData.ClassAudioDataHolder.defaultWalkingSoundData == null)
        {
            Debug.LogWarning("No step sound audio data on the class data scriptable!");
            return;
        }

        int indexToPlay = 0;
        indexToPlay = Random.Range(0, ClassData.ClassAudioDataHolder.defaultWalkingSoundData.SfxClips.Count);

        Audio_Manager.Instance.PlaySFX(ClassData.ClassAudioDataHolder.defaultWalkingSoundData.SfxClips[indexToPlay].Clip);
    }

    #endregion

    #region Event Triggers
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

	public void CompletedRecoveringHealthForEffect()
	{
		DoneRecoveringHealthEffect?.Invoke(this);
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

	public void CompletedHandlingSpaceEffects()
	{
		FinishedHandlingCurrentSpaceEffects?.Invoke(this);

    }

	public void HasBeenDefeatedEvent()
	{
		HasBeenDefeated?.Invoke(this);
    }

	public void BillboardLookAtCameraEvent()
	{
		BillboardLookAtCamera?.Invoke(this);
    }

	public void BillboardForwardCameraEvent()
	{
		BillboardLookAtCamera?.Invoke(this);
	}
    #endregion

    #endregion
}
