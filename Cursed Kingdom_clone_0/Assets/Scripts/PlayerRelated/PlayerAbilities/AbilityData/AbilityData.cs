//Code written by Mohamed Riaz Khan of BukuGames.
//All code is written by me (Above name) unless otherwise stated via comments below.
//Not authorized for use outside of the Github repository of this game developed by BukuGames.
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilityData : ScriptableObject, IAbility
{
    public event Action AbilityCompletedEffect;

    [SerializeField] [TextArea(3,10)] private string effectDescription;
    [Tooltip("Check this box if you want to override the auto-description setup by the code.")]
    [SerializeField] private bool overrideAutoDescription;
    [SerializeField] private bool canBeManuallyActivated = false;
    [SerializeField] private bool putsCharacterInCooldown = true;

    public string EffectDescription { get => effectDescription; set => effectDescription = value; }
    public bool OverrideAutoDescription { get => overrideAutoDescription; set => overrideAutoDescription = value; }
    public bool CanBeManuallyActivated { get => canBeManuallyActivated; set => canBeManuallyActivated = value; }
    public bool PutsCharacterInCooldown { get => putsCharacterInCooldown; set => putsCharacterInCooldown = value; }

    public virtual void ActivateEffect(Player playerReference)
    {
        if(playerReference.IsOnCooldown)
        {
            foreach(ClassData.NegativeCooldownEffects negativeCooldownEffect in playerReference.NegativeCooldownEffects)
            {
                if(negativeCooldownEffect == ClassData.NegativeCooldownEffects.CannotUseAbility)
                {
                    return;
                }
            }
        }

    }

    public virtual void CompletedEffect(Player playerReference)
    {
        AbilityCompletedEffect?.Invoke();
    }

    protected virtual void UpdateEffectDescription()
    {

    }
}
