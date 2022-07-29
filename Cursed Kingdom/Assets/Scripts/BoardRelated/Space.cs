//Code written by Mohamed Riaz Khan of BukuGames.
//All code is written by me (Above name) unless otherwise stated via comments below.
//Not authorized for use outside of the Github repository of this game developed by BukuGames.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Space : MonoBehaviour
{
    public Transform spawnPoint;
    public GameObject selectedBorder;
    public SpaceData spaceData;
    public GameplayManager testManagerRef;
    public MeshRenderer meshRenderer;
    public SpriteRenderer spriteRenderer;

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
                ApplySpaceEffects();
            }
        }
        else
        {
            Debug.LogWarning("playerReference is null STAY!");
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
       
    }

    private void OnCollisionStay(Collision collision)
    {
        
    }

    private void ApplySpaceEffects()
    {
        for (int i = 0; i < spaceData.thisSpaceTypes.Count; i++)
        {
          //  Debug.Log($"Applying space effect: {spaceData.thisSpaceTypes[i]}");
        }
    }

    public void SetupSpace()
    {
        SetSpaceMat();
        SetSpaceSprite();
    }

    public void SetSpaceMat()
    {
        Material[] currentMats = (meshRenderer.materials);

        for (int i = 0; i < currentMats.Length; i++)
        {
            currentMats[i] = spaceData.spaceMaterial;
        }

        meshRenderer.materials = currentMats;

    }

    public void SetSpaceSprite()
    {
        spriteRenderer.sprite = spaceData.spaceSprite;
    }

}
