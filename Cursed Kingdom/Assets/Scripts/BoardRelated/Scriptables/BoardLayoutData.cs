//Code written by Mohamed Riaz Khan of BukuGames.
//All code is written by me (Above name) unless otherwise stated via comments below.
//Not authorized for use outside of the Github repository of this game developed by BukuGames.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Board layout Data", menuName = "Board Layout/Board layout Data", order = 0)]
public class BoardLayoutData : ScriptableObject
{
    public GameObject regularSpacePrefab;
    public GameObject smallSpacePrefab;

    public Vector3 boardCenter;

    //Prolly not needed, we'll do calculations to figure this out.
    public Vector3 firstSpaceSpawnPoint;

    [Range(1,20)]
    public int numSpacesPerRow;
}
