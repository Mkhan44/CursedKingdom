//Code written by Mohamed Riaz Khan of BukuGames.
//All code is written by me (Above name) unless otherwise stated via comments below.
//Not authorized for use outside of the Github repository of this game developed by BukuGames.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This support card effect is used when there is a suppport card that recovers the player's health.
/// </summary>

[CreateAssetMenu(fileName = "RecoverHealthEffect", menuName = "Card Data/Support Card Effect Data/Recover Health Effect", order = 0)]
public class RecoverHealthEffect : SupportCardEffectData, ISupportEffect
{
    [SerializeField] [Range(1, 10)] private int healthToRecover = 1;

    public int HealthToRecover { get => healthToRecover; set => healthToRecover = value; }

    public override void EffectOfCard(Player playerReference, Card cardPlayed = null)
    {
        
    }
}

