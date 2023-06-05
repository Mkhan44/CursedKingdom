using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpaceEffectData : ScriptableObject , ISpaceEffect
{
    [SerializeField] private const string NEGATIVEEFFECT = "NegativeEffect";
    [SerializeField] private const string POSITIVEEFFECT = "PositiveEffect";
    [SerializeField] private bool isACost;
    [SerializeField] private bool isPositiveEffect;

    [SerializeField] [TextArea(3,10)] private string effectDescription;
    [Tooltip("Check this box if you want to override the auto-description setup by the code.")]
    [SerializeField] private bool overrideAutoDescription;

    public bool IsACost { get => isACost; set => isACost = value; }
    public bool IsPositiveEffect { get => isPositiveEffect; set => isPositiveEffect = value; }
    public string EffectDescription { get => effectDescription; set => effectDescription = value; }
    public bool OverrideAutoDescription { get => overrideAutoDescription; set => overrideAutoDescription = value; }


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
    public virtual bool CanCostBePaid(Player playerReference)
    {
        return true;
    }

    protected virtual void UpdateEffectDescription()
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
