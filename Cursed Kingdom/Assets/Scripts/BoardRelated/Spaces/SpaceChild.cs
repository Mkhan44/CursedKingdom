//Code written by Mohamed Riaz Khan of BukuGames.
//All code is written by me (Above name) unless otherwise stated via comments below.
//Not authorized for use outside of the Github repository of this game developed by BukuGames.


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//This script is a workaround for the fact that we use a mesh under the parent object for the space which has the collider on it instead of the parent.
public class SpaceChild : MonoBehaviour
{
    Space parentSpaceRef;
    private void Awake()
    {
        parentSpaceRef = transform.parent.GetComponent<Space>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        parentSpaceRef.CollisionEntry(collision);
    }

    private void OnCollisionStay(Collision collision)
    {
        parentSpaceRef.CollisionStay(collision);
    }

    private void OnTriggerEnter(Collider collider)
    {
        parentSpaceRef.TriggerEnter(collider);
    }

    private void OnTriggerStay(Collider collider)
    {
        parentSpaceRef.TriggerStay(collider);
    }

}
