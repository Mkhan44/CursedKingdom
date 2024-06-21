//Code written by Mohamed Riaz Khan of BukuGames.
//All code is written by me (Above name) unless otherwise stated via comments below.
//Not authorized for use outside of the Github repository of this game developed by BukuGames.

using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// If the opponent is inflicted with a status effect: They take extra damage equal to the amount of turns left on their status effect.
/// ex: Poisoned for 2 turns = 2 extra damage taken. This maxes out at 3 extra damage.
/// </summary>

[CreateAssetMenu(fileName = "ExtraDamageWithStatusEffect", menuName = "Card Data/Support Card Effect Data/Extra Damage With Status Effect", order = 0)]
public class ExtraDamageWithStatusEffect : SupportCardEffectData, ISupportEffect
{
    [SerializeField][Range(0, 10)] private int numExtraDamageBasedOnStatusEffect;
    [HideInInspector] public int NumExtraDamageBasedOnStatusEffect { get => numExtraDamageBasedOnStatusEffect; set => numExtraDamageBasedOnStatusEffect = value; }

    public override void EffectOfCard(Player playerReference, Card cardPlayed = null)
    {
        SupportCard cardUsed = (SupportCard)cardPlayed;
        if (cardUsed != null)
        {
            supportCardThatWasJustUsed = cardUsed;
        }
    }

    public override void EffectOfCard(DuelPlayerInformation duelPlayerInformation, Card cardPlayed = null)
    {
        //Start at 0 since this is a scriptable who's value we don't want to stay affected incorrectly.
        NumExtraDamageBasedOnStatusEffect = 0;

        if (duelPlayerInformation.PlayerInDuel.IsPoisoned)
        {
            NumExtraDamageBasedOnStatusEffect = duelPlayerInformation.PlayerInDuel.PoisonDuration;
        }
        else if (duelPlayerInformation.PlayerInDuel.IsCursed)
        {
            NumExtraDamageBasedOnStatusEffect = duelPlayerInformation.PlayerInDuel.CurseDuration;
        }
        else
        {
            NumExtraDamageBasedOnStatusEffect = 0;
        }

        //Cannot exceed 3.
        if(NumExtraDamageBasedOnStatusEffect > 3)
        {
            NumExtraDamageBasedOnStatusEffect = 3;
        }

        duelPlayerInformation.DamageToTake += NumExtraDamageBasedOnStatusEffect;


        base.EffectOfCard(duelPlayerInformation, cardPlayed);
    }

    public override void CompletedEffect(Player playerReference)
    {
        //Reset it cause this is a scriptable...Prolly not the best way of doing this.
        NumExtraDamageBasedOnStatusEffect = 0;
        base.CompletedEffect(playerReference);
    }
}
