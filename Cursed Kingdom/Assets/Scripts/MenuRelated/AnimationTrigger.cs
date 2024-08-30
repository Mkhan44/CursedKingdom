using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationTrigger : MonoBehaviour
{
    public Animator animator;
    public string triggerName = "";

    public void TriggerAnimation()
        {
            animator.SetTrigger(triggerName); // Assuming "StartAnimation" is a trigger parameter in Animator B
        }
    }
