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

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log($"The: {collision.gameObject.name} just touched the {this.name}!");

        Player playerReference = collision.gameObject.GetComponent<Player>();

        if(playerReference != null)
        {
            playerReference.CurrentSpacePlayerIsOn = this;
        }
        else
        {
            Debug.LogWarning("playerReference is null!");
        }
    }


}
