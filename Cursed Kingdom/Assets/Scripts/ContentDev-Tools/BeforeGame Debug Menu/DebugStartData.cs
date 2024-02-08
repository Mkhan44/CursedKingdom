//Code written by Mohamed Riaz Khan of BukuGames.
//All code is written by me (Above name) unless otherwise stated via comments below.
//Not authorized for use outside of the Github repository of this game developed by BukuGames.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DebugStartData", menuName = "Debug /Start Data", order = 0)]
public class DebugStartData : ScriptableObject
{

    public class PlayerDebugData
    {
        public ClassData.ClassType typeOfClass;
        [Range(0,15)] public int startingHealthOverride = 0;
        public string startingSpaceOverride = "";
        [Range(0, 5)] public int startingLevelOverride = 0;
        [Range(0, 5)] public int movementCardsToStartWithOverride = 0;
        [Range(0, 5)] public int supportCardsToStartWithOverride = 0;

        public PlayerDebugData()
        {
            typeOfClass = ClassData.ClassType.Magician;
            startingHealthOverride = 0;
            startingSpaceOverride = "none";
            startingLevelOverride = 0;
            movementCardsToStartWithOverride = 0;
            supportCardsToStartWithOverride = 0;
        }

    }

    public List<PlayerDebugData> playerDebugDatas = new();
}
