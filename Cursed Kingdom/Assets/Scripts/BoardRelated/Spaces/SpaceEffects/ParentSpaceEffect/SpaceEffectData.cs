using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpaceEffectData : ScriptableObject
{
    [Header("Space trigger timing")]
    [Tooltip("This should be true if the space effect happens AFTER a duel.")]
    [SerializeField] private bool afterDuelEffect;
    [Tooltip("This should be true if the space effect happens when a player starts their turn on the space.")]
    [SerializeField] private bool onSpaceTurnStartEffect;
    [Tooltip("This should be true if the player can activate this space effect when passing over it.")]
    [SerializeField] private bool passingOverSpaceEffect;
    [Tooltip("If the space effect is mandatory -- the game will try and activate it. Otherwise popup will ask the player if they want to. Default = true.")]
    [SerializeField] private bool isMandatory;

    //might need another bool for barricade???

    [Header("Extra Space Stipulations")]
    [Tooltip("If a duel cannot be commenced with a player that is on this space, this should be true.")]
    [SerializeField] private bool isNonDuelSpace;
    [Tooltip("If the player needs to have something before the effects can happen this should be true. Example: Discarding a card to then draw a card. NOTE: COST SHOULD BE THE 1ST EFFECT IN SPACE EFFECTS LIST.")]
    [SerializeField] private bool hasCostToPay;

    public bool AfterDuelEffect { get => afterDuelEffect; set => afterDuelEffect = value; }
    public bool OnSpaceStartEffect { get => onSpaceTurnStartEffect; set => onSpaceTurnStartEffect = value; }
    public bool PassingOverSpaceEffect { get => passingOverSpaceEffect; set => passingOverSpaceEffect = value; }
    public bool IsMandatory { get => isMandatory; set => isMandatory = value; }

    public enum CardType
    {
        MovementCard,
        SupportCard,
        Both,
    }


    //public enum SpaceType
    //{
    //    DrawMovementCard, DONE
    //    DrawSupportCard,  DONE
    //    Poison,  DONE
    //    Curse,  DONE
    //    RecoverHealth,  DONE
    //    LevelUp,     DONE
    //    LoseHealth, DONE
    //    Attack,     DONE
    //    halve movement values, DONE
    //    use extra card, DONE
    //    Barricade, DONE

    //    SpecialAttack,
    //    ArrowSpace,
    //}
}
