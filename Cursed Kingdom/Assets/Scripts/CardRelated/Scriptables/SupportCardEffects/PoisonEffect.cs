//Code written by Mohamed Riaz Khan of BukuGames.
//All code is written by me (Above name) unless otherwise stated via comments below.
//Not authorized for use outside of the Github repository of this game developed by BukuGames.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This support card effect is used to poison a target. If the target is already poisoned then this cannot be used.
/// </summary>

[CreateAssetMenu(fileName = "PoisonEffect", menuName = "Card Data/Support Card Effect Data/Poison Effect", order = 0)]
public class PoisonEffect : SupportCardEffectData, ISupportEffect
{
    [SerializeField][Range(1, 10)] private int numTurnsToPoison = 1;
    [SerializeField] private bool poisonImmediately;
    [Range(1, 3)][SerializeField] private int numPlayersToAttack = 0;
    [Tooltip("Player can choose the opponent's they want to attack. This will prompt the game to give a choice box.")]
    [SerializeField] private bool opponentsCanBeChosen = true;
    [Tooltip("Attacks all other players. This overrides 'numPlayersToAttack' and 'opponentsCanBeChosen' will be irrelevant.")]
    [SerializeField] private bool attackAllPlayers = false;
    [SerializeField] private bool poisonUser;

    public int NumTurnsToPoison { get => numTurnsToPoison; set => numTurnsToPoison = value; }
    public bool PoisonImmediately { get => poisonImmediately; set => poisonImmediately = value; }
    public int NumPlayersToAttack { get => numPlayersToAttack; set => numPlayersToAttack = value; }
    public bool OpponentsCanBeChosen { get => opponentsCanBeChosen; set => opponentsCanBeChosen = value; }
    public bool AttackAllPlayers { get => attackAllPlayers; set => attackAllPlayers = value; }
    public bool PoisonUser { get => poisonUser; set => poisonUser = value; }

    public override void EffectOfCard(Player playerReference, Card cardPlayed = null)
    {
        SupportCard cardUsed = (SupportCard)cardPlayed;
        if (cardUsed != null)
        {
            supportCardThatWasJustUsed = cardUsed;
        }
        //If player is already cursed do nothing. Otherwise, add to their curse turn count.
        //If 'PoisonImmediately' is true, poison as soon as the card is used. Otherwise don't poison until after the current turn ends. this usually isn't true for duel related effects.

        //Right now, no way to handle poison self + enemy.
        if (PoisonUser)
        {
            playerReference.StatusEffectUpdateCompleted += CompletedEffect;
            playerReference.PoisonPlayer(NumTurnsToPoison);
            return;
        }

        if (AttackAllPlayers)
        {
            playerReference.DoneAttackingForEffect += CompletedEffect;
            playerReference.AttackAllOtherPlayersStatusEffect("poison", NumTurnsToPoison);
            return;
        }

        if (OpponentsCanBeChosen)
        {
            int numPossiblePlayersToAfflict = 0;
            foreach (Player player in playerReference.GameplayManagerRef.Players)
            {
                if (!player.IsCursed && !player.IsPoisoned && player != playerReference)
                {
                    numPossiblePlayersToAfflict++;
                }
            }
            if (numPossiblePlayersToAfflict >= numPlayersToAttack)
            {
                playerReference.DoneAttackingForEffect += CompletedEffect;
                playerReference.ActivatePlayerToAttackStatusEffectSelectionPopup(NumPlayersToAttack, "poison", NumTurnsToPoison);
            }
            else
            {
                DialogueBoxPopup.instance.ActivatePopupWithJustText("No valid targets to Poison.", 1.5f);
            }
        }
    }

    public override bool CanCostBePaid(Player playerReference, bool justChecking = false)
    {
        bool canCostBePaid = false;

        if (PoisonUser)
        {
            if (!(playerReference.IsPoisoned && playerReference.IsCursed))
            {
                canCostBePaid = true;
            }
        }

        foreach (Player player in playerReference.GameplayManagerRef.Players)
        {
            if (!player.IsCursed && !player.IsPoisoned && player != playerReference)
            {
                canCostBePaid = true;
            }
        }


        if (!canCostBePaid && !justChecking)
        {
            DialogueBoxPopup.instance.ActivatePopupWithJustText("No valid targets to Poison.", 1.5f);
        }
        return canCostBePaid;
    }

    public override void CompletedEffect(Player playerReference)
    {
        playerReference.DoneAttackingForEffect -= CompletedEffect;
        playerReference.StatusEffectUpdateCompleted -= CompletedEffect;
        base.CompletedEffect(playerReference);
    }

}
