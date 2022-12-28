using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpaceEffectData : ScriptableObject , ISpaceEffect
{

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

    public virtual void EffectOfSpace(Player playerReference)
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
