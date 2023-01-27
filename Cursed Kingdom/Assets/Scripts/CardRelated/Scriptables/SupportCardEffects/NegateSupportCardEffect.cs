//Code written by Mohamed Riaz Khan of BukuGames.
//All code is written by me (Above name) unless otherwise stated via comments below.
//Not authorized for use outside of the Github repository of this game developed by BukuGames.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This support card effect is used to negate the effect of a support card that was played.
/// </summary>

[CreateAssetMenu(fileName = "MoveBackwardsEffect", menuName = "Card Data/Support Card Effect Data/Move Backwards Effect", order = 0)]
public class NegateSupportCardEffect : SupportCardEffectData, ISupportEffect
{
    [SerializeField] private bool requiresSingleTarget;

    public bool RequiresSingleTarget { get => requiresSingleTarget; set => requiresSingleTarget = value; }

    public override void EffectOfCard(Player playerReference, Card cardPlayed = null)
    {
        //If RequiresSingleTarget is true, then only negate the effect if it targets 1 player. Otherwise, negate the effect regardless.
    }

}




