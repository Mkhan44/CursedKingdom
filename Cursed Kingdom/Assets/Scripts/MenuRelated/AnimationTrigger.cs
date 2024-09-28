using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationTrigger : MonoBehaviour
{
    public Animator animator;
    public GameObject myGameObject;
    public string triggerName = "";

    public void TriggerAnimation()
        {
            animator.SetTrigger(triggerName);
        }
    public void Enabler()
    {
        myGameObject.SetActive(true);
    }
}
