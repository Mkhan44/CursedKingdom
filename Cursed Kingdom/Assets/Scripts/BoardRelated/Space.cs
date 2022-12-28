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
    public GameplayManager testManagerRef;
    public MeshRenderer meshRenderer;
    public SpriteRenderer spriteRenderer;
    public TextMeshPro spaceTitleTextMesh;

    [Header("Space Icons")]
    public Sprite drawMovementCardIcon;

    private void Start()
    {
        testManagerRef = GameObject.Find("TestManager").GetComponent<GameplayManager>();
    }

    public void CollisionEntry(Collision collision)
    {
        //Debug.Log($"The: {collision.gameObject.name} just touched the {this.name}!");

        Player playerReference = collision.gameObject.GetComponent<Player>();

        if (playerReference != null)
        {
            if(playerReference.PreviousSpacePlayerWasOn is null)
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

    public void EnableHighlight()
    {
        highlightAnimationObject.SetActive(true);
    }

    public void DisableHighlight()
    {
        highlightAnimationObject.SetActive(false);
    }
}
