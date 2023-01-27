//Code written by Mohamed Riaz Khan of BukuGames.
//All code is written by me (Above name) unless otherwise stated via comments below.
//Not authorized for use outside of the Github repository of this game developed by BukuGames.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This support card effect is used to drain health from a player or players. The lost health from the targets go to the current player's health.
/// </summary>

[CreateAssetMenu(fileName = "DrainHealthEffect", menuName = "Card Data/Support Card Effect Data/Drain Health Effect", order = 0)]
public class DrainHealthEffect : SupportCardEffectData, ISupportEffect
{
    [SerializeField] [Range(1, 10)] private int amountOfHealthToDrainFromTarget = 1;
    [SerializeField] private bool attackAllTargets = false;

    public int AmountOfHealthToDrainFromTarget { get => amountOfHealthToDrainFromTarget; set => amountOfHealthToDrainFromTarget = value; }
    public bool AttackAllTargets { get => attackAllTargets; set => attackAllTargets = value; }

    public override void EffectOfCard(Player playerReference, Card cardPlayed = null)
    {
        //If attackAllTargets = true, player gains the health from all targets and they all lose that same amount.
    }
}


