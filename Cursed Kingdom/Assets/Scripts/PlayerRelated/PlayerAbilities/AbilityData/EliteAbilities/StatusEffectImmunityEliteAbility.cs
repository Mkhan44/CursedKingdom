//Code written by Mohamed Riaz Khan of BukuGames.
//All code is written by me (Above name) unless otherwise stated via comments below.
//Not authorized for use outside of the Github repository of this game developed by BukuGames.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// If a character would be affected by a status effect: They are not affected by that status effect.
/// </summary>
 

[CreateAssetMenu(fileName = "StatusEffectImmunityEliteAbility", menuName = "Player/EliteAbility/StatusEffectImmunity", order = 0)]
public class StatusEffectImmunityEliteAbility : EliteAbilityData, IEliteAbility
{
    public enum StatusEffects
    {
        Poison,
        Curse,
    }

    [SerializeField] private List<StatusEffects> statusEffectsToBeImmuneTo;

    public List<StatusEffects> StatusEffectsToBeImmuneTo { get => statusEffectsToBeImmuneTo; set => statusEffectsToBeImmuneTo = value; }

    public override void ActivateEffect(Player playerReference)
    {
        playerReference.DoneActivatingEliteAbilityEffect += CompletedEffect;
        playerReference.ActivateEliteAbilityEffects();
        playerReference.SetStatusImmunities(statusEffectsToBeImmuneTo);
        foreach(StatusEffects statusImmunity in StatusEffectsToBeImmuneTo)
        {
            if(statusImmunity == StatusEffects.Poison && playerReference.IsPoisoned)
            {
                playerReference.CurePoison();
            }
            else if(statusImmunity == StatusEffects.Curse && playerReference.IsCursed)
            {
                playerReference.CureCurse();
            }
        }
    }

    public override void CompletedEffect(Player playerReference)
    {
        playerReference.DoneActivatingEliteAbilityEffect -= CompletedEffect;
        base.CompletedEffect(playerReference);
    }

    protected override void UpdateEffectDescription()
    {
        if (!OverrideAutoDescription)
        {
            EffectDescription = $"Immune to status effects.";
        }
    }

    private void OnEnable()
    {
        UpdateEffectDescription();
    }
}
