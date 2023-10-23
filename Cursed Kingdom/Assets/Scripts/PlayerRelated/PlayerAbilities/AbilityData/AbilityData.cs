//Code written by Mohamed Riaz Khan of BukuGames.
//All code is written by me (Above name) unless otherwise stated via comments below.
//Not authorized for use outside of the Github repository of this game developed by BukuGames.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilityData : ScriptableObject, IAbility
{
    [SerializeField] [TextArea(3,10)] private string effectDescription;
    [Tooltip("Check this box if you want to override the auto-description setup by the code.")]
    [SerializeField] private bool overrideAutoDescription;
    [SerializeField] private bool needsToBeOffCooldown = false;

    public string EffectDescription { get => effectDescription; set => effectDescription = value; }
    public bool OverrideAutoDescription { get => overrideAutoDescription; set => overrideAutoDescription = value; }
    public bool NeedsToBeOffCooldown { get => needsToBeOffCooldown; set => needsToBeOffCooldown = value; }

    public virtual void ActivateEffect(Player playerReference)
    {
        
    }

    protected virtual void UpdateEffectDescription()
    {

    }
}
