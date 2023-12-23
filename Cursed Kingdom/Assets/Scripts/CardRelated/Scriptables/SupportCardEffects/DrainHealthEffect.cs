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
    [Range(1, 3)][SerializeField] private int numPlayersToAttack = 1;
    [Tooltip("Player can choose the opponent's they want to attack. This will prompt the game to give a choice box.")]
    [SerializeField] private bool opponentsCanBeChosen = true;
    [Tooltip("Attacks all other players. This overrides 'numPlayersToAttack' and 'opponentsCanBeChosen' will be irrelevant.")]
    [SerializeField] private bool attackAllPlayers = false;

    public int AmountOfHealthToDrainFromTarget { get => amountOfHealthToDrainFromTarget; set => amountOfHealthToDrainFromTarget = value; }
    public int NumPlayersToAttack { get => numPlayersToAttack; set => numPlayersToAttack = value; }
    public bool AttackAllPlayers { get => attackAllPlayers; set => attackAllPlayers = value; }
    public bool OpponentsCanBeChosen { get => opponentsCanBeChosen; set => opponentsCanBeChosen = value; }

    public override void EffectOfCard(Player playerReference, Card cardPlayed = null)
    {
        //If AttackAllPlayers = true, player gains the health from all targets and they all lose that same amount.

        if (AttackAllPlayers)
        {
            playerReference.DoneAttackingForEffect += CompletedEffect;
            playerReference.AttackAllOtherPlayersDamage(AmountOfHealthToDrainFromTarget, IsElemental);
            return;
        }

        if (OpponentsCanBeChosen)
        {
            playerReference.DoneAttackingForEffect += CompletedEffect;
            playerReference.ActivatePlayerToAttackDamageSelectionPopup(NumPlayersToAttack, AmountOfHealthToDrainFromTarget, IsElemental);
        }
    }

    public override void CompletedEffect(Player playerReference)
    {
        if(AttackAllPlayers)
        {
            playerReference.RecoverHealth(AmountOfHealthToDrainFromTarget * (playerReference.GameplayManagerRef.Players.Count - 1));
        }
        else if(OpponentsCanBeChosen)
        {
            playerReference.RecoverHealth(AmountOfHealthToDrainFromTarget * NumPlayersToAttack);
        }
        playerReference.DoneAttackingForEffect -= CompletedEffect;
        base.CompletedEffect(playerReference);
    }
}


