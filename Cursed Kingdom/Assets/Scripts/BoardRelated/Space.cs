//Code written by Mohamed Riaz Khan of BukuGames.
//All code is written by me (Above name) unless otherwise stated via comments below.
//Not authorized for use outside of the Github repository of this game developed by BukuGames.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class Space : MonoBehaviour
{
    public enum Direction
    {
        North,
        South,
        East,
        West,
    }

    [SerializeField] private const string NEGATIVEEFFECT = "NegativeEffect";
    [SerializeField] private const string POSITIVEEFFECT = "PositiveEffect";

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
        gameplayManagerRef = GameObject.Find("Game Manager").GetComponent<GameplayManager>();
    }

    public void CollisionEntry(Collision collision)
    {
        //Debug.Log($"The: {collision.gameObject.name} just touched the {this.name}!");

        Player playerReference = collision.gameObject.GetComponent<Player>();

        if (playerReference != null)
        {
            if (playerReference.PreviousSpacePlayerWasOn is null)
            {
                playerReference.PreviousSpacePlayerWasOn = this;
               // Debug.Log($"Previous space was null, assigning current space as previous space.");
            }
            playerReference.CurrentSpacePlayerIsOn = this;
        }
        else
        {
            Debug.LogWarning("playerReference is null!");
        }
    }

    public void CollisionStay(Collision collision)
    {
        Player playerReference = collision.gameObject.GetComponent<Player>();

        if (playerReference != null)
        {
            if (playerReference.SpacesLeftToMove < 1 && playerReference.IsMoving)
            {
                playerReference.IsMoving = false;
               // Debug.Log($"Player landed on space: {spaceData.spaceName}");
                ApplySpaceEffects(playerReference);
                playerReference.CurrentSpacePlayerIsOn = this;
                gameplayManagerRef.SpaceArtworkPopupDisplay.TurnOnDisplay(this);
                //playerReference.DebugTheSpace();
            }
        }
        else
        {
            Debug.LogWarning("playerReference is null STAY!");
        }
    }

    public void TriggerEnter(Collider collider)
    {
        Player playerReference = collider.gameObject.GetComponent<Player>();

        if (playerReference is not null)
        {
            if (playerReference.PreviousSpacePlayerWasOn is null)
            {
                playerReference.PreviousSpacePlayerWasOn = this;
              //  Debug.Log($"Previous space was null, assigning current space as previous space.");
            }
            playerReference.CurrentSpacePlayerIsOn = this;
            if(this.spaceData.PassingOverSpaceEffect && playerReference.IsMoving)
            {
                ApplySpaceEffects(playerReference);
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
            if (playerReference.SpacesLeftToMove < 1 && playerReference.IsMoving)
            {
                playerReference.IsMoving = false;
               // Debug.Log($"Player landed on space: {spaceData.spaceName}");
                ApplySpaceEffects(playerReference);
                playerReference.CurrentSpacePlayerIsOn = this;
                gameplayManagerRef.SpaceArtworkPopupDisplay.TurnOnDisplay(this);
                //playerReference.DebugTheSpace();
            }
        }
        else
        {
            Debug.LogWarning("playerReference is null STAY!");
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
        GetSpaceNeighbors();
        GetValidDirections();
    }



    //Will probably need a way to check if it's the beginning of the turn, if the player just landed on the space, etc.
    private void ApplySpaceEffects(Player player)
    {
        if(spaceData.spaceEffects.Count < 1)
        {
            Debug.LogWarning("Hey, no space effects on this space currently!");
        }

        StartCoroutine(PlaySpaceInfoDisplayAnimationUI(player));

        if(!player.IsMoving)
        {
            if (spaceData.IsNegative)
            {
                player.Animator.SetBool(NEGATIVEEFFECT, true);
                Debug.LogWarning("NEGATIVE SPACE EFFECT");
            }
            else
            {
                player.Animator.SetBool(POSITIVEEFFECT, true);
                Debug.LogWarning("POSITIVE SPACE EFFECT");
            }
        }

        bool canAllCostsBePaid = false;
        for (int i = 0; i < spaceData.spaceEffects.Count; i++)
        {
            if (spaceData.spaceEffects[i].spaceEffectData.IsACost)
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
                spaceEffectsForPlayerToHandle.Enqueue(spaceData.spaceEffects[numSpaceEffectData].spaceEffectData);
            }
            //Whatever we already had from passing over effects + the new effects.
            player.SpaceEffectsToHandle = spaceEffectsForPlayerToHandle;
        }
        

        if(!player.IsMoving && !player.IsDefeated)
        {
            StartCoroutine(WaitASec(player));
        }
        //Player is defeated so their turn ends immediately.
        else if(!player.IsMoving && player.IsDefeated) 
        {
            gameplayManagerRef.EndOfTurn(player);
        }
        
    }

    private IEnumerator WaitASec(Player player)
    {
        yield return new WaitForSeconds(1.3f);

        if (spaceData.IsNegative)
        {
            player.Animator.SetBool(NEGATIVEEFFECT, false);
        }
        else
        {
            player.Animator.SetBool(POSITIVEEFFECT, false);
        }

        player.ShowHand();
        player.StartHandlingSpaceEffects();

        //gameplayManagerRef.EndOfTurn(player);
    }

    private IEnumerator PlaySpaceInfoDisplayAnimationUI(Player player)
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
            Sprite iconSprite = null;
            switch(spaceEffect.spaceEffectData.GetType().Name)
            {
                case nameof(ArrowSpace):
                    {
                       // Debug.Log("Arrow space has no sprite. Keeping it as default");
                        iconSprite = gameplayManagerRef.IconPresets.BarricadeSprite;
                        break;
                    }
                case nameof(AttackSpace):
                    {
                      //  Debug.Log("Attack space has no sprite. Keeping it as default");
                        iconSprite = gameplayManagerRef.IconPresets.BarricadeSprite;
                        break;
                    }
                case nameof(BarricadeSpace):
                    {
                        iconSprite = gameplayManagerRef.IconPresets.BarricadeSprite;
                        break;
                    }
                case nameof(ConferenceRoomSpace):
                    {
                      //  Debug.Log("ConferenceRoom space has no sprite. Keeping it as default");
                        iconSprite = gameplayManagerRef.IconPresets.BarricadeSprite;
                        break;
                    }
                case nameof(CurseSpace):
                    {
                        iconSprite = gameplayManagerRef.IconPresets.CurseSprite;
                        break;
                    }
                case nameof(DiscardCardSpace):
                    {
                      //  Debug.Log("Discard Card space has no sprite. Keeping it as default");
                        iconSprite = gameplayManagerRef.IconPresets.BarricadeSprite;
                        break;
                    }
                case nameof(HalveMovementCardSpace):
                    {
                      //  Debug.Log("Halve movement card space has no sprite. Keeping it as default");
                        iconSprite = gameplayManagerRef.IconPresets.BarricadeSprite;
                        break;
                    }
                case nameof(LevelUpSpace):
                    {
                       // Debug.Log("Level up space has no sprite. Keeping it as default");
                        iconSprite = gameplayManagerRef.IconPresets.BarricadeSprite;
                        break;
                    }
                case nameof(LoseHealthSpace):
                    {
                        iconSprite = gameplayManagerRef.IconPresets.LoseHealthSprite;
                        break;
                    }
                case nameof(PoisonSpace):
                    {
                        iconSprite = gameplayManagerRef.IconPresets.PoisonSprite;
                        break;
                    }
                case nameof(RecoverHealthSpace):
                    {
                        iconSprite = gameplayManagerRef.IconPresets.RecoverHealthSprite;
                        break;
                    }
                case nameof(UseExtraCardSpace):
                    {
                       // Debug.Log("Use extra card space has no sprite. Keeping it as default");
                        iconSprite = gameplayManagerRef.IconPresets.BarricadeSprite;
                        break;
                    }
                case nameof(DrawCardSpace):
                    {
                        DrawCardSpace drawCardSpace = spaceEffect.spaceEffectData as DrawCardSpace;
                        if (drawCardSpace.CardTypeToDraw == Card.CardType.Movement)
                        {
                            iconSprite = gameplayManagerRef.IconPresets.DrawMovementCardSprite;
                        }
                        else if(drawCardSpace.CardTypeToDraw == Card.CardType.Support)
                        {
                            iconSprite = gameplayManagerRef.IconPresets.DrawSupportCardSprite;
                        }
                        else
                        {
                            iconSprite = gameplayManagerRef.IconPresets.DrawMovementCardSprite;
                        }
                        break;
                    }
                case nameof(ExcaliburTheGreatSword):
                    {
                        iconSprite = gameplayManagerRef.IconPresets.SpecialAttackSprite;
                        break;
                    }
                case nameof(SwiftSwipe):
                    {
                        iconSprite = gameplayManagerRef.IconPresets.SpecialAttackSprite;
                        break;
                    }
                case nameof(SpecialAttackSpace):
                    {
                        iconSprite = gameplayManagerRef.IconPresets.SpecialAttackSprite;
                        break;
                    }

                default:
                    {
                        Debug.LogWarning("Couldn't find space type!");
                        break;
                    }
            }
            iconImage.sprite = iconSprite;
        }
       
    }

    private void GetSpaceNeighbors()
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
    private void GetValidDirections()
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
