using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpaceEffectData : ScriptableObject , ISpaceEffect
{
    [SerializeField] private bool isACost;
    [SerializeField] private bool isPositiveEffect;
    [SerializeField] private const string NEGATIVEEFFECT = "NegativeEffect";
    [SerializeField] private const string POSITIVEEFFECT = "PositiveEffect";

    public bool IsACost { get => isACost; set => isACost = value; }
    public bool IsPositiveEffect { get => isPositiveEffect; set => isPositiveEffect = value; }

    public enum CardType
    {
        MovementCard,
        SupportCard,
        Both,
    }

    public enum DirectionToTravel
    {
        Up,
        Down,
        Left,
        Right,
    }

    public virtual void LandedOnEffect(Player playerReference)
    {
        
    }

    public virtual void StartOfTurnEffect(Player playerReference)
    {
        
    }

    public virtual void EndOfTurnEffect(Player playerReference)
    {
        
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
    //    SpecialAttack, DONE

    //    ArrowSpace,
    // Conference room space,
    //}
}
