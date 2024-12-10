//Code written by Mohamed Riaz Khan of BukuGames.
//All code is written by me (Above name) unless otherwise stated via comments below.
//Not authorized for use outside of the Github repository of this game developed by BukuGames.
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EliteAbilityData : ScriptableObject, IEliteAbility
{
    public event Action EliteAbilityCompletedEffect;

    [SerializeField][TextArea(3, 10)] private string effectDescription;
    [Tooltip("Check this box if you want to override the auto-description setup by the code.")]
    [SerializeField] private bool overrideAutoDescription;
    [SerializeField] private bool canBeManuallyActivated = false;
    [SerializeField] private bool isAfterDuelEffect;
    [SerializeField] private bool isPassive = false;
    [SerializeField] private bool hasACost;

    public string EffectDescription { get => effectDescription; set => effectDescription = value; }
    public bool OverrideAutoDescription { get => overrideAutoDescription; set => overrideAutoDescription = value; }
    public bool CanBeManuallyActivated { get => canBeManuallyActivated; set => canBeManuallyActivated = value; }
    public bool IsAfterDuelEffect { get => isAfterDuelEffect; set => isAfterDuelEffect = value; }
    public bool IsPassive { get => isPassive; set => isPassive = value; }
    public bool HasACost { get => hasACost; set => hasACost = value; }

    public virtual void ActivateEffect(Player playerReference)
    {
        //If there is a cost to pay and player can't pay the cost, early exit.
    }

    public virtual void ActivateEffect(DuelPlayerInformation duelPlayerInformation)
    {
        //If there is a cost to pay and palyer can't pay the cost, early exit.
    }

    public virtual void CompletedEffect(Player playerReference)
    {
        EliteAbilityCompletedEffect?.Invoke();
    }

    public virtual bool CanCostBePaid(Player playerReference, bool justChecking = false)
    {
        return false;
    }

    public virtual bool CanCostBePaid(DuelPlayerInformation duelPlayerInformation, bool justChecking = false)
    {
        return false;
    }

    /// <summary>
    /// This should play an animation and then afterwards activate the effect. 
    /// Will need to hook into some event that gets triggered after the animation is finished so we can then do the effect afterwards.
    /// </summary>
    /// <param name="playerReference"></param>
    /// <returns></returns>
    public virtual void PlayEliteAbilityUseAnimation(Player playerReference)
    {
        //Play animation...Invoke an event afterwards.
    }

    protected virtual void UpdateEffectDescription()
    {

    }
}
