//Code written by Mohamed Riaz Khan of BukuGames.
//All code is written by me (Above name) unless otherwise stated via comments below.
//Not authorized for use outside of the Github repository of this game developed by BukuGames.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.TextCore;
using System.Threading.Tasks;
using System;
using UnityEngine.SceneManagement;

public class Space : MonoBehaviour
{
    public enum Direction
    {
        North,
        South,
        East,
        West,
    }

    

    public Transform spawnPoint;
    public GameObject selectedBorder;
    public GameObject highlightAnimationObject;
    public SpaceData spaceData;
    public GameplayManager gameplayManagerRef;
    public MeshRenderer meshRenderer;
    public SpriteRenderer spriteRenderer;
    public TextMeshPro spaceTitleTextMesh;
    public GameObject iconHolderPrefab;
    public GameObject iconHolderParent;
    public GameObject multiplePlayersSameSpacePositionsParent;
    public List<Player> playersOnThisSpace;

    public bool haveSeparatedPlayersAlready;

    [Header("Space Icons")]
    public Sprite drawMovementCardIcon;

    [Header("Space neighbors")]
    [Tooltip("ONLY TRUE IF IT'S THE CENTER SPACE LIKE CONFERENCE ROOM. OTHERWISE THIS BREAKS STUFFS.")]
    public bool isCenterSpace;
    [SerializeField] private Space eastNeighbor;
    [SerializeField] private Space westNeighbor;
    [SerializeField] private Space northNeighbor;
    [SerializeField] private Space southNeighbor;
    [Tooltip("Used to determine how far we cast rays.")]
    [SerializeField] private float spaceBetweenSpaces = 2f;

    [SerializeField] private HashSet<Direction> validDirectionsFromThisSpace;

    //Debug
    [SerializeField] private List<Direction> directionsHashsetVisual;
    public Space EastNeighbor { get => eastNeighbor; set => eastNeighbor = value; }
    public Space WestNeighbor { get => westNeighbor; set => westNeighbor = value; }
    public Space NorthNeighbor { get => northNeighbor; set => northNeighbor = value; }
    public Space SouthNeighbor { get => southNeighbor; set => southNeighbor = value; }
    public HashSet<Direction> ValidDirectionsFromThisSpace { get => validDirectionsFromThisSpace; set => validDirectionsFromThisSpace = value; }

    //Debug
    public List<Direction> DirectionsHashsetVisual { get => directionsHashsetVisual; set => directionsHashsetVisual = value; }

    private void Awake()
    {
        ValidDirectionsFromThisSpace = new();
        DirectionsHashsetVisual = new();
        playersOnThisSpace = new();
        //playersAlreadyMoved = new();
        haveSeparatedPlayersAlready = false;
        if(SceneManager.GetActiveScene().name != "BoardGameplay")
        {
            return;
        }
        gameplayManagerRef = GameObject.Find("Game Manager").GetComponent<GameplayManager>(); 
    }

    
    public void TriggerEnter(Collider collider)
    {
        Player playerReference = collider.gameObject.GetComponent<Player>();

        if (playerReference is not null)
        {
            if (playerReference.PreviousSpacePlayerWasOn is null && playerReference.IsMoving && spaceData.DecreasesSpacesToMove)
            {
                playerReference.PreviousSpacePlayerWasOn = this;
            }
            playerReference.CurrentSpacePlayerIsOn = this;
            if(this.spaceData.PassingOverSpaceEffect && playerReference.IsMoving)
            {
                if (playerReference.AbleToLevelUp)
                {
                    foreach (SpaceData.SpaceEffect spaceEffect in spaceData.spaceEffects)
                    {
                        if (spaceEffect.spaceEffectData.GetType() == typeof(LevelUpSpace))
                        {
                            //Should only be 1 but this is still hardcoded so no.
                            spaceEffect.spaceEffectData.LandedOnEffect(playerReference);
                        }
                    }
                }
            }
        }
        else
        {
            Debug.LogWarning("playerReference is null!");
        }
    }
    public void TriggerStay(Collider collider)
    {
        Player playerReference = collider.gameObject.GetComponent<Player>();

        if (playerReference != null)
        {
            if (playerReference.SpacesLeftToMove < 1 && playerReference.IsMoving && gameplayManagerRef.playerCharacter.GetComponent<Player>() == playerReference && !haveSeparatedPlayersAlready)
            {
                playerReference.IsMoving = false;
                playerReference.CurrentSpacePlayerIsOn = this;
            }
        }
        else
        {
            Debug.LogWarning("playerReference is null STAY!");
        }
    }

    public void TriggerExit(Collider collider)
    {
        if(playersOnThisSpace.Count > 0)
        {
            Player playerLeaving = collider.gameObject.GetComponent<Player>();

            if(!playerLeaving.AbleToLevelUp)
            {
                foreach (SpaceData.SpaceEffect spaceEffect in spaceData.spaceEffects)
                {
                    if (spaceEffect.spaceEffectData.GetType() == typeof(LevelUpSpace))
                    {
                        playerLeaving.AbleToLevelUp = true;
                    }
                }
            }
            
            haveSeparatedPlayersAlready = true;
            if (playersOnThisSpace.Contains(playerLeaving))
            {
                playersOnThisSpace.Remove(playerLeaving);
            }

            //MoveMultiplePlayersOnSpace();
            haveSeparatedPlayersAlready = false;
        }
        
    }

    public void MoveMultiplePlayersOnSpace()
    {
        List<Vector3> positionsToMoveTowards = new();
        if (playersOnThisSpace.Count > 1 && playersOnThisSpace.Count < 5)
        {
            haveSeparatedPlayersAlready = true;
            
            for (int i = 0; i < playersOnThisSpace.Count; i++)
            {
                Vector3 positionToMoveTowards = multiplePlayersSameSpacePositionsParent.transform.GetChild(i).position;
                positionsToMoveTowards.Add(positionToMoveTowards);
               // playersOnThisSpace[i].transform.position = positionToMoveTowards;
                
            }
            //StartCoroutine(playersOnThisSpace[0].GameplayManagerRef.playerMovementManager.MoveTowardsMultiSpace(positionsToMoveTowards, playersOnThisSpace));
            haveSeparatedPlayersAlready = false;
        }
        else
        {
            if(playersOnThisSpace.Count == 1 && playersOnThisSpace[0].transform.position != spawnPoint.position)
            {
                positionsToMoveTowards.Add(spawnPoint.position);
                // playersOnThisSpace[0].transform.position = spawnPoint.position;
              //  StartCoroutine(playersOnThisSpace[0].GameplayManagerRef.playerMovementManager.MoveTowardsMultiSpace(positionsToMoveTowards, playersOnThisSpace));
            }
        }
    }

    public void SetupSpace()
    {
        gameObject.name = spaceData.spaceName;
        SetSpaceMat();
        SetSpaceSprite();
        SetSpaceTitle();
        SetSpaceIcons();
    }

    public void SpaceTravelSetup()
    {
        SetSpaceNeighbors();
        SetValidDirections();
    }


    //Apply all space effects at the beginning of the Player's turn. Anything that is a landed on effect/after duel effect will not be activated here.
    public void ApplyStartOfTurnSpaceEffects(Player player)
    {
        //For debug mode.
        SpaceData cachedSpaceData = spaceData;

        if (DebugModeSingleton.instance.IsDebugActive)
        {
            Space tempSpace = DebugModeSingleton.instance.OverrideSpaceLandEffect();

            if (tempSpace != null)
            {
                spaceData = tempSpace.spaceData;
            }

        }

        if (spaceData.spaceEffects.Count < 1)
        {
            Debug.LogWarning("Hey, no space effects on this space currently!");
        }

        bool canAllCostsBePaid = false;
        for (int i = 0; i < spaceData.spaceEffects.Count; i++)
        {
            if (spaceData.spaceEffects[i].spaceEffectData.IsACost && spaceData.spaceEffects[i].spaceEffectData.OnSpaceTurnStartEffect)
            {
                if (!spaceData.spaceEffects[i].spaceEffectData.CanCostBePaid(player))
                {
                    Debug.LogWarning($"Cost of {spaceData.spaceEffects[i].spaceEffectData.name} can't be paid. Can't execute space effects.");
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
            Queue<SpaceEffectData> spaceEffectsForPlayerToHandle = new();
            int numSpaceEffectData;
            for (int j = 0; j < spaceData.spaceEffects.Count; j++)
            {
                //Do the space effect in sequence. We'll check for any external triggers here as well.
                if (player.IsDefeated)
                {
                    break;
                }
                //This is too fast. Need a way to wait for each effect then go to the next.
                numSpaceEffectData = j;
                if (spaceData.spaceEffects[numSpaceEffectData].spaceEffectData.OnSpaceTurnStartEffect && !spaceData.spaceEffects[numSpaceEffectData].spaceEffectData.AfterDuelEffect)
                {
                    spaceEffectsForPlayerToHandle.Enqueue(spaceData.spaceEffects[numSpaceEffectData].spaceEffectData);
                }
            }

            //If there are none, exit early.
            if (spaceEffectsForPlayerToHandle.Count < 1)
            {
                return;
            }

            //Whatever we already had from passing over effects + the new effects.
            player.SpaceEffectsToHandle = spaceEffectsForPlayerToHandle;
        }

        if (!player.IsMoving)
        {
            if (spaceData.IsNegative)
            {
                player.Animator.SetBool(Player.NEGATIVEEFFECT, true);
            }
            else
            {
                player.Animator.SetBool(Player.POSITIVEEFFECT, true);
            }
        }

        if (!player.IsDefeated)
        {
            //StartCoroutine(WaitASec(player));
            StartCoroutine(GoBackToIdle(player));
        }
        //Player is defeated so their turn ends immediately.
        else
        {
            //gameplayManagerRef.EndOfTurn(player);
            gameplayManagerRef.GameplayPhaseStatemachineRef.ChangeState(gameplayManagerRef.GameplayPhaseStatemachineRef.gameplayEndPhaseState);
        }

        //Revert the space data back if we used debug to change it.
        if (DebugModeSingleton.instance.IsDebugActive)
        {
            spaceData = cachedSpaceData;
        }
    }

    //Apply all space effects after the Player has landed. Anything that is after duel/start of turn will not be activated here.
    public void ApplyLandedOnSpaceEffects(Player player)
    {
        //For debug mode.
        SpaceData cachedSpaceData = spaceData;

        if (DebugModeSingleton.instance.IsDebugActive)
        {
            Space tempSpace = DebugModeSingleton.instance.OverrideSpaceLandEffect();
            
            if (tempSpace != null) 
            {
                spaceData = tempSpace.spaceData;
            }
        }

        if(spaceData.spaceEffects.Count < 1)
        {
            Debug.LogWarning("Hey, no space effects on this space currently!");
        }

        bool canAllCostsBePaid = false;
        for (int i = 0; i < spaceData.spaceEffects.Count; i++)
        {
            if (spaceData.spaceEffects[i].spaceEffectData.IsACost && !spaceData.spaceEffects[i].spaceEffectData.OnSpaceTurnStartEffect 
            && !spaceData.spaceEffects[i].spaceEffectData.AfterDuelEffect && !spaceData.spaceEffects[i].spaceEffectData.IsAfterDuelEffectAndMustWin)
            {
                if (!spaceData.spaceEffects[i].spaceEffectData.CanCostBePaid(player))
                {
                    Debug.LogWarning($"Cost of {spaceData.spaceEffects[i].spaceEffectData.name} can't be paid. Can't execute space effects.");
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

        if(canAllCostsBePaid)
        {
            Queue<SpaceEffectData> spaceEffectsForPlayerToHandle = new();
            int numSpaceEffectData;
            for (int j = 0; j < spaceData.spaceEffects.Count; j++)
            {
                //Do the space effect in sequence. We'll check for any external triggers here as well.
                if(player.IsDefeated)
                {
                    break;
                }
                //This is too fast. Need a way to wait for each effect then go to the next.
                numSpaceEffectData = j;
                if(!spaceData.spaceEffects[j].spaceEffectData.OnSpaceTurnStartEffect 
                && !spaceData.spaceEffects[j].spaceEffectData.AfterDuelEffect && !spaceData.spaceEffects[j].spaceEffectData.IsAfterDuelEffectAndMustWin)
                {
                    spaceEffectsForPlayerToHandle.Enqueue(spaceData.spaceEffects[numSpaceEffectData].spaceEffectData);
                }
            }
            //Whatever we already had from passing over effects + the new effects.
            player.SpaceEffectsToHandle = spaceEffectsForPlayerToHandle;
        }

        if (!player.IsMoving)
        {
            if (spaceData.IsNegative)
            {
                player.Animator.SetBool(Player.NEGATIVEEFFECT, true);
            }
            else
            {
                player.Animator.SetBool(Player.POSITIVEEFFECT, true);
            }
        }

        if (!player.IsMoving && !player.IsDefeated)
        {
            //StartCoroutine(WaitASec(player));
            StartCoroutine(GoBackToIdle(player));
        }
        //Player is defeated so their turn ends immediately.
        else if(!player.IsMoving && player.IsDefeated) 
        {
            //gameplayManagerRef.EndOfTurn(player);
            gameplayManagerRef.GameplayPhaseStatemachineRef.ChangeState(gameplayManagerRef.GameplayPhaseStatemachineRef.gameplayEndPhaseState);
        }

        //Revert the space data back if we used debug to change it.
        if (DebugModeSingleton.instance.IsDebugActive)
        {
            spaceData = cachedSpaceData;
        }
    }

    //Apply space effects when the Player passes over the space.
    public void ApplyPassingOverSpaceEffects(Player player)
    {

    }

    //Apply all space effects at the end of the Player's turn. Before Status effects are applied.
    public void ApplyEndOfTurnSpaceEffects(Player player)
    {
        //For debug mode.
        SpaceData cachedSpaceData = spaceData;

        if (DebugModeSingleton.instance.IsDebugActive)
        {
            Space tempSpace = DebugModeSingleton.instance.OverrideSpaceLandEffect();

            if (tempSpace != null)
            {
                spaceData = tempSpace.spaceData;
            }

        }

        if (spaceData.spaceEffects.Count < 1)
        {
            Debug.LogWarning("Hey, no space effects on this space currently!");
        }



        Queue<SpaceEffectData> spaceEffectsForPlayerToHandle = new();
        int numSpaceEffectData;
        for (int j = 0; j < spaceData.spaceEffects.Count; j++)
        {
            //Do the space effect in sequence. We'll check for any external triggers here as well.
            if (player.IsDefeated)
            {
                break;
            }
            //This is too fast. Need a way to wait for each effect then go to the next.
            numSpaceEffectData = j;
            if (spaceData.spaceEffects[numSpaceEffectData].spaceEffectData.OnSpaceTurnStartEffect && !spaceData.spaceEffects[numSpaceEffectData].spaceEffectData.AfterDuelEffect && !spaceData.spaceEffects[numSpaceEffectData].spaceEffectData.IsAfterDuelEffectAndMustWin)
            {
                spaceEffectsForPlayerToHandle.Enqueue(spaceData.spaceEffects[numSpaceEffectData].spaceEffectData);
            }
        }

        //If there are none, exit early.
        if (spaceEffectsForPlayerToHandle.Count < 1)
        {
            return;
        }

        //Whatever we already had from passing over effects + the new effects.
        player.SpaceEffectsToHandle = spaceEffectsForPlayerToHandle;

        if (!player.IsMoving)
        {
            if (spaceData.IsNegative)
            {
                player.Animator.SetBool(Player.NEGATIVEEFFECT, true);
            }
            else
            {
                player.Animator.SetBool(Player.POSITIVEEFFECT, true);
            }
        }

        if (!player.IsDefeated)
        {
            //StartCoroutine(WaitASec(player));
            StartCoroutine(GoBackToIdle(player));
        }
        

        //Revert the space data back if we used debug to change it.
        if (DebugModeSingleton.instance.IsDebugActive)
        {
            spaceData = cachedSpaceData;
        }
    }

    public void ApplyEndOfDuelGeneralSpaceEffects(Player player)
    {
        //For debug mode.
        SpaceData cachedSpaceData = spaceData;

        if (DebugModeSingleton.instance.IsDebugActive)
        {
            Space tempSpace = DebugModeSingleton.instance.OverrideSpaceLandEffect();

            if (tempSpace != null)
            {
                spaceData = tempSpace.spaceData;
            }

        }

        if (spaceData.spaceEffects.Count < 1)
        {
            Debug.LogWarning("Hey, no space effects on this space currently!");
        }

        Queue<SpaceEffectData> spaceEffectsForPlayerToHandle = new();
        int numSpaceEffectData;
        for (int j = 0; j < spaceData.spaceEffects.Count; j++)
        {
            //Do the space effect in sequence. We'll check for any external triggers here as well.
            if (player.IsDefeated)
            {
                break;
            }
            //This is too fast. Need a way to wait for each effect then go to the next.
            numSpaceEffectData = j;
            if (spaceData.spaceEffects[numSpaceEffectData].spaceEffectData.AfterDuelEffect || spaceData.spaceEffects[numSpaceEffectData].spaceEffectData.IsAfterDuelEffectAndMustWin)
            {
                if(spaceData.spaceEffects[numSpaceEffectData].spaceEffectData.IsAfterDuelEffectAndMustWin)
                {
                    if(gameplayManagerRef.DuelPhaseSMRef.CurrentWinners.Count == 1 && gameplayManagerRef.DuelPhaseSMRef.CurrentWinners[0].PlayerInDuel == player)
                    {
                        spaceEffectsForPlayerToHandle.Enqueue(spaceData.spaceEffects[numSpaceEffectData].spaceEffectData);
                        continue;
                    }
                }
                else
                {
                    spaceEffectsForPlayerToHandle.Enqueue(spaceData.spaceEffects[numSpaceEffectData].spaceEffectData);
                }
            }
        }

        //If there are none, exit early.
        if (spaceEffectsForPlayerToHandle.Count < 1)
        {
            return;
        }

        //Whatever we already had from passing over effects + the new effects.
        player.SpaceEffectsToHandle = spaceEffectsForPlayerToHandle;

        if (!player.IsMoving)
        {
            if (spaceData.IsNegative)
            {
                player.Animator.SetBool(Player.NEGATIVEEFFECT, true);
            }
            else
            {
                player.Animator.SetBool(Player.POSITIVEEFFECT, true);
            }
        }

        if (!player.IsDefeated)
        {
            //StartCoroutine(WaitASec(player));
            StartCoroutine(GoBackToIdle(player));
        }

        //Revert the space data back if we used debug to change it.
        if (DebugModeSingleton.instance.IsDebugActive)
        {
            spaceData = cachedSpaceData;
        }
    }

    private IEnumerator GoBackToIdle(Player player)
    {
        SpaceData spaceDataToUse = spaceData;

        yield return new WaitForSeconds(1.3f);
        if (spaceDataToUse.IsNegative)
        {
            player.Animator.SetBool(Player.NEGATIVEEFFECT, false);
        }
        else
        {
            player.Animator.SetBool(Player.POSITIVEEFFECT, false);
        }

        if(gameplayManagerRef.GetCurrentPlayer() == player)
        {
            player.ShowHand();
        }
        player.StartHandlingSpaceEffects();
    }

    public IEnumerator PlaySpaceInfoDisplayAnimationUI(Player player)
    {
        //Space info display panel set active. Populate information for the space.
        //Get animation clip length, Yield for that long.


        yield return new WaitForSeconds(0.0f);

    }


    private void SetSpaceMat()
    {
        Material[] currentMats = (meshRenderer.materials);

        for (int i = 0; i < currentMats.Length; i++)
        {
            currentMats[i] = spaceData.spaceMaterials[i];
        }

        meshRenderer.materials = currentMats;

        //Only do in play mode.
        if(Application.isPlaying)
        {
            Material[] theMats = (meshRenderer.materials);
            for (int i = 0; i < currentMats.Length; i++)
            {
                Material instancedMat = new Material(meshRenderer.materials[i]);
                theMats[i] = instancedMat;
            }
        }
    }

    private void SetSpaceSprite()
    {
        spriteRenderer.sprite = spaceData.spaceSprite;
    }

    private void SetSpaceTitle()
    {
        spaceTitleTextMesh.text = spaceData.spaceName;
    }

    private void SetSpaceIcons()
    {
        foreach(SpaceData.SpaceEffect spaceEffect in spaceData.spaceEffects) 
        {
            GameObject tempIcon = Instantiate(iconHolderPrefab, iconHolderParent.transform);
            Image iconImage = tempIcon.transform.GetChild(0).GetComponentInChildren<Image>();
            SpaceIconPreset.SpaceIconElement.SpaceIconType typeToCheck = default;
            switch(spaceEffect.spaceEffectData.GetType().Name)
            {
                case nameof(ArrowSpace):
                    {
                        typeToCheck = SpaceIconPreset.SpaceIconElement.SpaceIconType.arrow;
                        // Debug.Log("Arrow space has no sprite. Keeping it as default");
                        break;
                    }
                case nameof(AttackSpace):
                    {
                        typeToCheck = SpaceIconPreset.SpaceIconElement.SpaceIconType.attack;
                        //  Debug.Log("Attack space has no sprite. Keeping it as default");
                        break;
                    }
                case nameof(BarricadeSpace):
                    {
                        typeToCheck = SpaceIconPreset.SpaceIconElement.SpaceIconType.barricade;
                        break;
                    }
                case nameof(ConferenceRoomSpace):
                    {
                        //  Debug.Log("ConferenceRoom space has no sprite. Keeping it as default");
                        typeToCheck = SpaceIconPreset.SpaceIconElement.SpaceIconType.conferenceRoom;
                        break;
                    }
                case nameof(CurseSpace):
                    {
                        typeToCheck = SpaceIconPreset.SpaceIconElement.SpaceIconType.curse;
                        break;
                    }
                case nameof(DiscardCardSpace):
                    {
                        //  Debug.Log("Discard Card space has no sprite. Keeping it as default");
                        typeToCheck = SpaceIconPreset.SpaceIconElement.SpaceIconType.discard;
                        break;
                    }
                case nameof(HalveMovementCardSpace):
                    {
                        //  Debug.Log("Halve movement card space has no sprite. Keeping it as default");
                        typeToCheck = SpaceIconPreset.SpaceIconElement.SpaceIconType.halveMovementCard;
                        break;
                    }
                case nameof(LevelUpSpace):
                    {
                      //  Debug.Log("Level up space has no sprite. Keeping it as default");
                        typeToCheck = SpaceIconPreset.SpaceIconElement.SpaceIconType.levelUp;
                        break;
                    }
                case nameof(LoseHealthSpace):
                    {
                        typeToCheck = SpaceIconPreset.SpaceIconElement.SpaceIconType.loseHealth;
                        break;
                    }
                case nameof(PoisonSpace):
                    {
                        typeToCheck = SpaceIconPreset.SpaceIconElement.SpaceIconType.poison;
                        break;
                    }
                case nameof(RecoverHealthSpace):
                    {
                        typeToCheck = SpaceIconPreset.SpaceIconElement.SpaceIconType.recoverHealth;
                        break;
                    }
                case nameof(UseExtraCardSpace):
                    {
                        // Debug.Log("Use extra card space has no sprite. Keeping it as default");
                        typeToCheck = SpaceIconPreset.SpaceIconElement.SpaceIconType.useExtraCard;
                        break;
                    }
                case nameof(DrawCardSpace):
                    {
                        DrawCardSpace drawCardSpace = spaceEffect.spaceEffectData as DrawCardSpace;
                        if (drawCardSpace.CardTypeToDraw == Card.CardType.Movement)
                        {
                            typeToCheck = SpaceIconPreset.SpaceIconElement.SpaceIconType.drawMovementCard;
                        }
                        else if(drawCardSpace.CardTypeToDraw == Card.CardType.Support)
                        {
                            typeToCheck = SpaceIconPreset.SpaceIconElement.SpaceIconType.drawSupportCard;
                        }
                        else
                        {
                            typeToCheck = SpaceIconPreset.SpaceIconElement.SpaceIconType.drawMovementCard;
                        }
                        break;
                    }
                case nameof(ExcaliburTheGreatSword):
                    {
                        typeToCheck = SpaceIconPreset.SpaceIconElement.SpaceIconType.specialAttack;
                        break;
                    }
                case nameof(SpecialAttackSpace):
                    {
                        typeToCheck = SpaceIconPreset.SpaceIconElement.SpaceIconType.specialAttack;
                        break;
                    }

                default:
                    {
                        Debug.LogWarning("Couldn't find space type!");
                        break;
                    }
            }
            SetIconImage(typeToCheck, iconImage);
        }

        if(spaceData.IsNonDuelSpace)
        {
            GameObject tempIcon = Instantiate(iconHolderPrefab, iconHolderParent.transform);
            Image iconImage = tempIcon.transform.GetChild(0).GetComponentInChildren<Image>();
            SpaceIconPreset.SpaceIconElement.SpaceIconType typeToCheck = SpaceIconPreset.SpaceIconElement.SpaceIconType.nonDuel;
            SetIconImage(typeToCheck, iconImage);
        }
       
    }

    private void SetIconImage(SpaceIconPreset.SpaceIconElement.SpaceIconType typeToCheck, Image iconImage)
    {
        foreach (SpaceIconPreset.SpaceIconElement spaceIconElement in IconPresetsSingleton.instance.SpaceIconPreset.SpaceIconElements)
        {
            if (spaceIconElement.SpaceIconType1 == typeToCheck)
            {
                iconImage.sprite = spaceIconElement.Sprite;
                iconImage.color = spaceIconElement.Color;
            }
        }
    }

    private void SetSpaceNeighbors()
    {
        Space nextSpace = default;

        //Raycast. We need to do all 4 directions and see what spaces are around the player.
        GameObject lastHitObject;
        List<Ray> raysToCast = new();

        //North, South, East, West
        raysToCast.Add(new Ray(transform.position, transform.forward));
        raysToCast.Add(new Ray(transform.position, -transform.forward));
        raysToCast.Add(new Ray(transform.position, transform.right));
        raysToCast.Add(new Ray(transform.position, -transform.right));

        //North, South, East, West
        for (int i = 0; i < raysToCast.Count; i++)
        {
            RaycastHit hit;
            float rayCastLength = 0f;
            if(!isCenterSpace)
            {
                rayCastLength = spaceBetweenSpaces;
            }
            else
            {
                rayCastLength = 5f;
            }

            if (Physics.Raycast(raysToCast[i], out hit, rayCastLength))
            {
                lastHitObject = hit.transform.gameObject;
                nextSpace = lastHitObject.transform.parent.gameObject.GetComponent<Space>();

                if(i == 0)
                {
                    NorthNeighbor = nextSpace;
                }
                else if(i == 1)
                {
                    SouthNeighbor = nextSpace;
                }
                else if (i == 2)
                {
                    EastNeighbor = nextSpace;
                }
                else if (i == 3)
                {
                    WestNeighbor = nextSpace;
                }
                // Debug.Log($"We hit: {lastHitObject.transform.parent.gameObject.name} , nextSpace is now: {nextSpace.spaceData.spaceName}");
            }
            else
            {
            //    Debug.LogWarning($"{raysToCast[i]} Didn't hit anything.");

            }
        }
    }

    //Which directions should the player be able to travel from this space?
    private void SetValidDirections()
    {
        Direction direction;

        //All perimeter spaces.
        if (SouthNeighbor is null)
        {
            direction = Direction.West;
            ValidDirectionsFromThisSpace.Add(direction);
        }

        //Space next to 2nd barricade.
        if(NorthNeighbor is null && EastNeighbor is null)
        {
            direction = Direction.West;
            ValidDirectionsFromThisSpace.Add(direction);
        }
        //If the space above is is a barricade space and we have spaces all around us except below us.
        //**THIS IS TOO SPECIFIC FOR BARRICADE...NEED A BETTER WAY TO DO THIS.**
        if (NorthNeighbor != null && EastNeighbor != null && WestNeighbor != null && SouthNeighbor == null && NorthNeighbor.spaceData.spaceEffects[0].spaceEffectData.GetType() == typeof(BarricadeSpace))
        {
            direction = Direction.North;
            ValidDirectionsFromThisSpace.Add(direction);
        }

        //All inner spaces going up.
        if (westNeighbor is null && EastNeighbor is null)
        {
            direction = Direction.North;
            ValidDirectionsFromThisSpace.Add(direction);


            //Last barricade and special attack spaces.
            if(SouthNeighbor.SouthNeighbor is null || NorthNeighbor.isCenterSpace)
            {
                direction = Direction.South;
                ValidDirectionsFromThisSpace.Add(direction);
            }

            //CHANGE THIS IT'S SUPER BAD OMG...
            if(spaceData.spaceEffects[0].spaceEffectData.GetType() == typeof(BarricadeSpace))
            {
                BarricadeSpace barricadeSpace = spaceData.spaceEffects[0].spaceEffectData as BarricadeSpace;
                if (barricadeSpace.LevelNeededToPass == 2)
                {
                    ValidDirectionsFromThisSpace.Remove(Direction.South);
                }
            }

        }

        //TOO SPECIFIC. This is for the left side of inside spaces where there are not barricade spaces. Need to clean this up.
        if(WestNeighbor is null &&  spaceData.spaceEffects[0].spaceEffectData.GetType() != typeof(BarricadeSpace))
        {
            if(SouthNeighbor != null && SouthNeighbor.spaceData.spaceEffects[0].spaceEffectData.GetType() != typeof(BarricadeSpace))
            {
                ValidDirectionsFromThisSpace.Remove(Direction.North);
                direction = Direction.South;
                ValidDirectionsFromThisSpace.Add(direction);
            }
        }



        //Center space. (Conference room)
        if(isCenterSpace)
        {
            ValidDirectionsFromThisSpace.Clear();

            ValidDirectionsFromThisSpace.Add(Direction.North);
            ValidDirectionsFromThisSpace.Add(Direction.South);
            ValidDirectionsFromThisSpace.Add(Direction.West);
            ValidDirectionsFromThisSpace.Add(Direction.East);
        }

        foreach(Direction validDirection in ValidDirectionsFromThisSpace)
        {
            DirectionsHashsetVisual.Add(validDirection);
        }
    }

    public void EnableHighlight()
    {
        highlightAnimationObject.SetActive(true);
    }

    public void DisableHighlight()
    {
        highlightAnimationObject.SetActive(false);
    }
}
