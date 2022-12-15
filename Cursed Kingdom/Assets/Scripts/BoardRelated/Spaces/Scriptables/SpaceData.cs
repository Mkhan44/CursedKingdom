//Code written by Mohamed Riaz Khan of BukuGames.
//All code is written by me (Above name) unless otherwise stated via comments below.
//Not authorized for use outside of the Github repository of this game developed by BukuGames.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "Space Data", menuName = "Board Layout/Space Data", order = 0)]
public class SpaceData : ScriptableObject
{
    [Serializable]
    public class SpaceTypeWrapper
    {
        [SerializeField] public SpaceEffectData spaceEffectData;
        [HideInInspector] public string spaceTypeStringHidden;
    }

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

    public List<SpaceTypeWrapper> spaceTypeList;

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

    public List<SpaceTypeWrapper> spaceTypeWrappers;
    public List<SpaceType> thisSpaceTypes;
    public string spaceName = "Space";
    [TextArea(3,10)]
    public string spaceDescription = "Please input a space description...";
    public Sprite spaceSprite;
    [Header("Material related")]
    public List<Material> spaceMaterials;

    //Test for finding all classes that implement the interface for spaceEffects. We need to compile a list above for the user to select from in a dropdown.
    
    private void OnValidate()
    {
        //Type type = typeof(ISpaceEffect);
        //IEnumerable<Type> types = AppDomain.CurrentDomain.GetAssemblies()
        //                .SelectMany(x => x.GetTypes())
        //                .Where(x => x.IsClass && type.IsAssignableFrom(x));

        //foreach (Type t in types)
        //{
        //    ISpaceEffect obj = Activator.CreateInstance(t) as ISpaceEffect;
        //   // Debug.Log(t.Name);
        //}
    }

}
