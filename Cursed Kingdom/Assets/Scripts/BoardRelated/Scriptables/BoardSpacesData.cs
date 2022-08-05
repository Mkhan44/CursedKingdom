//Code written by Mohamed Riaz Khan of BukuGames.
//All code is written by me (Above name) unless otherwise stated via comments below.
//Not authorized for use outside of the Github repository of this game developed by BukuGames.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Board Spaces Data", menuName = "Board Layout/Board Spaces Data", order = 0)]
public class BoardSpacesData : ScriptableObject
{
    [Header("Row Prefab to go off of")]
    public GameObject rowPrefab;

    [Header("Spaces in row")]
    public List<SpaceData> perimeterSpaces;
    public List<SpaceData> insideSpaces;

    public void OnValidate()
    {
        if(rowPrefab != null)
        {
            int numPerimeterSpacesInPrefab = 0;
            int numInsideSpacesInPrefab = 0;

            //Validate that we have the same amount in our lists that correspond to what the RowPrefab has. (Will need + 1 for the center space of the board)
            foreach (Transform space in rowPrefab.transform)
            {
                if(space.gameObject.activeSelf == true)
                {
                    numPerimeterSpacesInPrefab += 1;
                }
                else
                {
                    numInsideSpacesInPrefab = (rowPrefab.transform.childCount - (numPerimeterSpacesInPrefab + 1) );
                    break;
                }
            }

            if(numPerimeterSpacesInPrefab != perimeterSpaces.Count)
            {
                Debug.LogWarning($"Your perimeter space count does not match the amount in the prefab on the {name} scriptable. The prefab has: {numPerimeterSpacesInPrefab} and your perimeterSpaces list count is:  {perimeterSpaces.Count}");
                return;
            }

            if (numInsideSpacesInPrefab != insideSpaces.Count)
            {
                Debug.LogWarning($"Your inside space count does not match the amount in the prefab on the {name} scriptable. The prefab has: {numInsideSpacesInPrefab} and your insideSpaces list count is:  {insideSpaces.Count}");
                return;
            }
        }
    }
}
