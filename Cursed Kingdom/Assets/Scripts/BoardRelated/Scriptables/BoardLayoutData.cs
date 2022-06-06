//Code written by Mohamed Riaz Khan of BukuGames.
//All code is written by me (Above name) unless otherwise stated via comments below.
//Not authorized for use outside of the Github repository of this game developed by BukuGames.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Board layout Data", menuName = "Board Layout/Board layout Data", order = 0)]
public class BoardLayoutData : ScriptableObject
{
    public List<SpaceData> MagicianLevelOneRow;
    public List<SpaceData> ArcherLevelOneRow;
    public List<SpaceData> ThiefLevelOneRow;
    public List<SpaceData> WarriorLevelOneRow;
}
