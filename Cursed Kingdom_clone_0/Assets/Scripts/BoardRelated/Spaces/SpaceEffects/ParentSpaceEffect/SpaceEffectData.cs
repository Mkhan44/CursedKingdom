using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpaceEffectData : ScriptableObject , ISpaceEffect
{
    [SerializeField] private const string NEGATIVEEFFECT = "NegativeEffect";
    [SerializeField] private const string POSITIVEEFFECT = "PositiveEffect";

    public event Action SpaceEffectCompleted;

    [SerializeField] private bool isACost;
    [SerializeField] private bool isPositiveEffect;

    [Tooltip("This should be true if the space effect happens when a player starts their turn on the space. NOTE: SHOULD BE THE 1ST EFFECT IN SPACE EFFECTS LIST.")]
    [SerializeField] private bool onSpaceTurnStartEffect;
    [Tooltip("This should be true if the space effect happens AFTER a duel.")]
    [SerializeField] private bool afterDuelEffect;
    [SerializeField] private bool isAfterDuelEffectAndMustWin;
    [SerializeField] [TextArea(3,10)] private string effectDescription;
    [Tooltip("Check this box if you want to override the auto-description setup by the code.")]
    [SerializeField] private bool overrideAutoDescription;

    public bool IsACost { get => isACost; set => isACost = value; }
    public bool IsPositiveEffect { get => isPositiveEffect; set => isPositiveEffect = value; }
    public bool OnSpaceTurnStartEffect { get => onSpaceTurnStartEffect; set => onSpaceTurnStartEffect = value; }
    public bool AfterDuelEffect { get => afterDuelEffect; set => afterDuelEffect = value; }
    public bool IsAfterDuelEffectAndMustWin { get => isAfterDuelEffectAndMustWin; set => isAfterDuelEffectAndMustWin = value; }
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
        //Usually called after the effect has been completed.
        CompletedEffect(playerReference);
    }

    public virtual void StartOfTurnEffect(Player playerReference)
    {
        //Usually called after the effect has been completed.
        CompletedEffect(playerReference);
    }

    public virtual void EndOfDuelEffect(DuelPlayerInformation playerInformation)
    {
        CompletedEffect(playerInformation.PlayerInDuel);
    }

    public virtual void EndOfTurnEffect(Player playerReference)
    {
        //Usually called after the effect has been completed.
        CompletedEffect(playerReference);
    }
    public virtual bool CanCostBePaid(Player playerReference)
    {
        return true;
    }

    public virtual void CompletedEffect(Player playerReference)
    {
        SpaceEffectCompleted?.Invoke();
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
