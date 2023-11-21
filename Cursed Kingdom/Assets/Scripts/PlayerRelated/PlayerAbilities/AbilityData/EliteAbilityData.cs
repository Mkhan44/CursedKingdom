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
    [SerializeField] private bool isPassive = false;

    public string EffectDescription { get => effectDescription; set => effectDescription = value; }
    public bool OverrideAutoDescription { get => overrideAutoDescription; set => overrideAutoDescription = value; }
    public bool CanBeManuallyActivated { get => canBeManuallyActivated; set => canBeManuallyActivated = value; }
    public virtual void ActivateEffect(Player playerReference)
    {
        //If there is a cost to pay and player can't pay the cost, early exit.
    }

    public virtual void CompletedEffect(Player playerReference)
    {
        EliteAbilityCompletedEffect?.Invoke();
    }

    protected virtual void UpdateEffectDescription()
    {

    }
}
