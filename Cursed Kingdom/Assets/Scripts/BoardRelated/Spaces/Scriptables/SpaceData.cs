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
    [Tooltip("A list of effects that are on the space.")]
    [Serializable]
    public class SpaceEffect
    {
        [SerializeField] public SpaceEffectData spaceEffectData;
        [HideInInspector] public string spaceTypeStringHidden;
    }

    [Header("Space Effect trigger timing [HOVER FOR TOOLTIPS]")]
    [Space(2f)]
    [Tooltip("This should be true if the player can activate this space effect when passing over it.")]
    [SerializeField] private bool passingOverSpaceEffect;
    [Tooltip("If the space effect is mandatory -- the game will try and activate it. Otherwise popup will ask the player if they want to. Default = true.")]
    [SerializeField] private bool isMandatory = true;
    [Tooltip("When passing over this space: Does the amount of moves a player has left decrease?")]
    [SerializeField] private bool decreasesSpacesToMove = true;
    [Space(10f)]

    //might need another bool for barricade???

    [Header("Extra Space Stipulations")]
    [Space(2f)]
    [Tooltip("If a duel cannot be commenced with a player that is on this space, this should be true.")]
    [SerializeField] private bool isNonDuelSpace;
    [Tooltip("Is this a negative space the impacts the player negatively? If so: True, else: should be False.")]
    [SerializeField] private bool isNegative;

    public bool PassingOverSpaceEffect { get => passingOverSpaceEffect; set => passingOverSpaceEffect = value; }
    public bool IsMandatory { get => isMandatory; set => isMandatory = value; }
    public bool IsNonDuelSpace { get => isNonDuelSpace; set => isNonDuelSpace = value; }
    public bool IsNegative { get => isNegative; set => isNegative = value; }
    public bool DecreasesSpacesToMove { get => decreasesSpacesToMove; set => decreasesSpacesToMove = value; }

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

    public List<SpaceEffect> spaceEffects;
    public string spaceName = "Space";
    [TextArea(3,10)]
    public string spaceDescription = "Please input a space description...";
    public Sprite spaceSprite;
    [Header("Material related")]
    public List<Material> spaceMaterials;
    [Header("Audio")]
    public AudioData StepSoundClipsData;

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
