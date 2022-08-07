//Code written by Mohamed Riaz Khan of BukuGames.
//All code is written by me (Above name) unless otherwise stated via comments below.
//Not authorized for use outside of the Github repository of this game developed by BukuGames.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Space Data", menuName = "Board Layout/Space Data", order = 0)]
public class SpaceData : ScriptableObject
{
    //Determines icon to be used. Should be moved to whatever script we'll have on the space prefabs.
    public enum SpaceType
    {
        DrawMovementCard,
        DrawSupportCard,
        Poison,
        Curse,
        RecoverHealth,
        LoseHealth,
        Barricade,
        NonDuel,
        SpecialAttack,
        LevelUp,
        ArrowSpace,
        Attack,
        StartEffect,
        AfterDuel,
        Misc,
    }

    #region Custom Editor
    [Range(1, 10)]
    [HideInInspector] public int drawMovementCardAmount;

    [Range(1, 10)]
    [HideInInspector] public int drawSupportCardAmount;

    [Range(1,10)]
    [HideInInspector] public int turnsPoisoned;

    [Range(1, 10)]
    [HideInInspector] public int turnsCursed;

    [Range(1, 10)]
    [HideInInspector] public int healthRecoveryAmount;

    [Range(1, 10)]
    [HideInInspector] public int damageTakenAmount;

    [Range(1, 10)]
    [HideInInspector] public int levelToPassBarricade;

    [Range(1, 10)]
    [HideInInspector] public int damageGivenAmount;
    #endregion


    public List<SpaceType> thisSpaceTypes;
    public string spaceName = "Space";
    [TextArea(3,10)]
    public string spaceDescription = "Please input a space description...";
    public Sprite spaceSprite;
    public Material spaceMaterial;
}
