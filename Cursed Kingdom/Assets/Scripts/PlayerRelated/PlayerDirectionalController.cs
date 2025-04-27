// Determines what direction the Player sprite should be facing depending on camera angle & what space they're on
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDirectionalController : MonoBehaviour
{
    [SerializeField] Animator animator;

    private bool isOnMageSpace = false;
    private bool isOnWarriorSpace = false;
    private bool isOnThiefSpace = false;
    private bool isOnArcherSpace = false;


    // Start is called before the first frame update
    void Start()
    {

    }

    private void OnTriggerStay(Collider other) // Checking what type of space the player is on
{
    Transform parentTransform = other.transform.parent;
    if (parentTransform == null) return;

    string tag = parentTransform.tag;

    isOnMageSpace = tag == "MageSpace";
    isOnWarriorSpace = tag == "WarriorSpace";
    isOnThiefSpace = tag == "ThiefSpace";
    isOnArcherSpace = tag == "ArcherSpace";
}

    private void OnTriggerExit(Collider other)
{
    Transform parentTransform = other.transform.parent;
    if (parentTransform == null) return;

    string tag = parentTransform.tag;

        if (tag == "MageSpace") isOnMageSpace = false;
        if (tag == "WarriorSpace") isOnWarriorSpace = false;
        if (tag == "ThiefSpace") isOnThiefSpace = false;
        if (tag == "ArcherSpace") isOnArcherSpace = false;
    }
    // Update is called once per frame
    private void LateUpdate()
    {
        Vector2 animationDirection = new Vector2(-1f, 0f); // Player faces left by default
        float yRot = Camera.main.transform.eulerAngles.y;

        if (Mathf.DeltaAngle(0f, yRot) >= -45f && Mathf.DeltaAngle(0f, yRot) <= 45f) // Checking Mage Side
        {
            if(isOnMageSpace)
            {
                animationDirection = new Vector2(-1f, 0f); // Facing Left
            }
            else if(isOnWarriorSpace)
            {
                animationDirection = new Vector2(0f, -1f); // Facing Back
            }
            else if(isOnThiefSpace) 
            {
                animationDirection = new Vector2(1f, 0f); // Facing Right
            }
            else if (isOnArcherSpace)
            {
                animationDirection = new Vector2(0f, 1f); // Facing Front
            }
            
        }
        else if (Mathf.DeltaAngle(0f, yRot) >= 45f && Mathf.DeltaAngle(0f, yRot) <= 135f) // Checking Warrior Side
        {
            if (isOnWarriorSpace)
            {
                animationDirection = new Vector2(-1f, 0f); // Facing Left
            }
            else if (isOnThiefSpace)
            {
                animationDirection = new Vector2(0f, -1f); // Facing Back
            }
            else if (isOnArcherSpace)
            {
                animationDirection = new Vector2(1f, 0f); // Facing Right
            }
            else if (isOnMageSpace)
            {
                animationDirection = new Vector2(0f, 1f); // Facing Front
            }

        }
        
        else if (Mathf.DeltaAngle(0f, yRot) >= -135f && Mathf.DeltaAngle(0f, yRot) <= -45f) // Checking Archer Side
        {
            if (isOnArcherSpace)
            {
                animationDirection = new Vector2(-1f, 0f); // Facing Left
            }
            else if (isOnMageSpace)
            {
                animationDirection = new Vector2(0f, -1f); // Facing Back
            }
            else if (isOnWarriorSpace)
            {
                animationDirection = new Vector2(1f, 0f); // Facing Right
            }
            else if (isOnThiefSpace)
            {
                animationDirection = new Vector2(0f, 1f); // Facing Front
            }

        }
        else  // Checking Thief Side
        {
            if (isOnThiefSpace)
            {
                animationDirection = new Vector2(-1f, 0f); // Facing Left
            }
            else if (isOnArcherSpace)
            {
                animationDirection = new Vector2(0f, -1f); // Facing Back
            }
            else if (isOnMageSpace)
            {
                animationDirection = new Vector2(1f, 0f); // Facing Right
            }
            else if (isOnWarriorSpace)
            {
                animationDirection = new Vector2(0f, 1f); // Facing Front
            }

        }

        // Set values to animation BlendTree
        animator.SetFloat("moveX", animationDirection.x);
        animator.SetFloat("moveY", animationDirection.y);

         //Debug.Log("Camera Y Rotation: " + yRot);
    }

}
