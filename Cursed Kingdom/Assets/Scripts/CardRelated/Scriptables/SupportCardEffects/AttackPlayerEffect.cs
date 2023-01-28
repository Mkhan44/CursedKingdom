//Code written by Mohamed Riaz Khan of BukuGames.
//All code is written by me (Above name) unless otherwise stated via comments below.
//Not authorized for use outside of the Github repository of this game developed by BukuGames.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This support card effect is used when there is a target or targets for the player to attack and deal damage to.
/// </summary>

[CreateAssetMenu(fileName = "AttackPlayerEffect", menuName = "Card Data/Support Card Effect Data/Attack Player Effect", order = 0)]
public class AttackPlayerEffect : SupportCardEffectData, ISupportEffect
{
    [SerializeField] [Range(1, 10)] private int damageToDeal = 1;
    [SerializeField] private bool attackAllPlayers = false;

    public int DamageToDeal { get => damageToDeal; set => damageToDeal = value; }
    public bool AttackAllPlayers { get => attackAllPlayers; set => attackAllPlayers = value; }

    public override void EffectOfCard(Player playerReference, Card cardPlayed = null)
    {
        //if AttackAllPlayers is false, let the player choose which other player to attack. Otherwise deal damage to all players.
    }
}
