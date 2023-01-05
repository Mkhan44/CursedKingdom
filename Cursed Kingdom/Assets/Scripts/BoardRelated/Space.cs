//Code written by Mohamed Riaz Khan of BukuGames.
//All code is written by me (Above name) unless otherwise stated via comments below.
//Not authorized for use outside of the Github repository of this game developed by BukuGames.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Space : MonoBehaviour
{
    public Transform spawnPoint;
    public GameObject selectedBorder;
    public GameObject highlightAnimationObject;
    public SpaceData spaceData;
    public GameplayManager gameplayManagerRef;
    public MeshRenderer meshRenderer;
    public SpriteRenderer spriteRenderer;
    public TextMeshPro spaceTitleTextMesh;

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
    private float spaceBetweenSpaces = 2f;

    public Space EastNeighbor { get => eastNeighbor; set => eastNeighbor = value; }
    public Space WestNeighbor { get => westNeighbor; set => westNeighbor = value; }
    public Space NorthNeighbor { get => northNeighbor; set => northNeighbor = value; }
    public Space SouthNeighbor { get => southNeighbor; set => southNeighbor = value; }

    private void Start()
    {
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
                Debug.Log($"Previous space was null, assigning current space as previous space.");
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
                Debug.Log($"Player landed on space: {spaceData.spaceName}");
                ApplySpaceEffects(playerReference);
                playerReference.CurrentSpacePlayerIsOn = this;
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

        if (playerReference != null)
        {
            if (playerReference.PreviousSpacePlayerWasOn is null)
            {
                playerReference.PreviousSpacePlayerWasOn = this;
                Debug.Log($"Previous space was null, assigning current space as previous space.");
            }
            playerReference.CurrentSpacePlayerIsOn = this;
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
                Debug.Log($"Player landed on space: {spaceData.spaceName}");
                ApplySpaceEffects(playerReference);
                playerReference.CurrentSpacePlayerIsOn = this;
                //playerReference.DebugTheSpace();
            }
        }
        else
        {
            Debug.LogWarning("playerReference is null STAY!");
        }
    }

    //Will probably need a way to check if it's the beginning of the turn, if the player just landed on the space, etc.
    private void ApplySpaceEffects(Player player)
    {
        if(spaceData.spaceEffects.Count < 1)
        {
            Debug.LogWarning("Hey, no space effects on this space currently!");
        }

        for (int i = 0; i < spaceData.spaceEffects.Count; i++)
        {
            //Do the space effect in sequence. We'll check for any external triggers here as well.
            spaceData.spaceEffects[i].spaceEffectData.EffectOfSpace(player);
        }
    }

    public void SetupSpace()
    {
        gameObject.name = spaceData.spaceName;
        SetSpaceMat();
        SetSpaceSprite();
        SetSpaceTitle();
    }

    public void SetSpaceMat()
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

    public void SetSpaceSprite()
    {
        spriteRenderer.sprite = spaceData.spaceSprite;
    }

    public void SetSpaceTitle()
    {
        spaceTitleTextMesh.text = spaceData.spaceName;
    }

    public void GetSpaceNeighbors()
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
                Debug.LogWarning($"{raysToCast[i]} Didn't hit anything.");

            }
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
