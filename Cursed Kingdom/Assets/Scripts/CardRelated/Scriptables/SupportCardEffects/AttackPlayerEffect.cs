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
    [SerializeField] [Range(1, 10)] private int damageToGive = 1;
    [Range(1, 3)][SerializeField] private int numPlayersToAttack = 1;
    [Tooltip("Player can choose the opponent's they want to attack. This will prompt the game to give a choice box.")]
    [SerializeField] private bool opponentsCanBeChosen = true;
    [Tooltip("Attacks all other players. This overrides 'numPlayersToAttack' and 'opponentsCanBeChosen' will be irrelevant.")]
    [SerializeField] private bool attackAllPlayers = false;

    public int DamageToGive { get => damageToGive; set => damageToGive = value; }
    public int NumPlayersToAttack { get => numPlayersToAttack; set => numPlayersToAttack = value; }
    public bool OpponentsCanBeChosen { get => opponentsCanBeChosen; set => opponentsCanBeChosen = value; }
    public bool AttackAllPlayers { get => attackAllPlayers; set => attackAllPlayers = value; }

    public override void EffectOfCard(Player playerReference, Card cardPlayed = null)
    {
        if (AttackAllPlayers)
        {
            playerReference.DoneAttackingForEffect += CompletedEffect;
            playerReference.AttackAllOtherPlayersDamage(DamageToGive, IsElemental);
            return;
        }

        if (OpponentsCanBeChosen)
        {
            playerReference.DoneAttackingForEffect += CompletedEffect;
            playerReference.ActivatePlayerToAttackDamageSelectionPopup(NumPlayersToAttack, DamageToGive, IsElemental);
        }
    }

    public override void CompletedEffect(Player playerReference)
    {
        playerReference.DoneAttackingForEffect -= CompletedEffect;
        base.CompletedEffect(playerReference);
    }
}
