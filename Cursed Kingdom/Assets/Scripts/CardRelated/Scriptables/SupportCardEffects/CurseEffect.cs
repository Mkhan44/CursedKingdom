//Code written by Mohamed Riaz Khan of BukuGames.
//All code is written by me (Above name) unless otherwise stated via comments below.
//Not authorized for use outside of the Github repository of this game developed by BukuGames.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This support card effect is used to curse a target. If the target is already cursed then this cannot be used.
/// </summary>

[CreateAssetMenu(fileName = "CurseEffect", menuName = "Card Data/Support Card Effect Data/Curse Effect", order = 0)]
public class CurseEffect : SupportCardEffectData, ISupportEffect
{
    [SerializeField] [Range(1, 10)] private int numTurnsToCurse = 1;
    [SerializeField] private bool curseImmediately;
    [Range(1, 3)][SerializeField] private int numPlayersToAttack = 0;
    [Tooltip("Player can choose the opponent's they want to attack. This will prompt the game to give a choice box.")]
    [SerializeField] private bool opponentsCanBeChosen = true;
    [Tooltip("Attacks all other players. This overrides 'numPlayersToAttack' and 'opponentsCanBeChosen' will be irrelevant.")]
    [SerializeField] private bool attackAllPlayers = false;
    [SerializeField] private bool curseUser;

    public int NumTurnsToCurse { get => numTurnsToCurse; set => numTurnsToCurse = value; }
    public bool CurseImmediately { get => curseImmediately; set => curseImmediately = value; }
    public int NumPlayersToAttack { get => numPlayersToAttack; set => numPlayersToAttack = value; }
    public bool OpponentsCanBeChosen { get => opponentsCanBeChosen; set => opponentsCanBeChosen = value; }
    public bool AttackAllPlayers { get => attackAllPlayers; set => attackAllPlayers = value; }
    public bool CurseUser { get => curseUser; set => curseUser = value; }

    public override void EffectOfCard(Player playerReference, Card cardPlayed = null)
    {
        //If player is already cursed do nothing. Otherwise, add to their curse turn count.
        //If 'CurseImmediately' is true, curse as soon as the card is used. Otherwise don't curse until after the current turn ends. this usually isn't true for duel related effects.

        //Right now, no way to handle cursing self + enemy.
        if(CurseUser)
        {
            playerReference.StatusEffectUpdateCompleted += CompletedEffect;
            playerReference.CursePlayer(NumTurnsToCurse);
            return;
        }

        if(AttackAllPlayers)
        {
            playerReference.DoneAttackingForEffect += CompletedEffect;
            playerReference.AttackAllOtherPlayersStatusEffect("curse", NumTurnsToCurse);
            return;
        }

        if (OpponentsCanBeChosen)
        {
            playerReference.DoneAttackingForEffect += CompletedEffect;
            playerReference.ActivatePlayerToAttackStatusEffectSelectionPopup(NumPlayersToAttack, "curse", NumTurnsToCurse);
        }
    }

    public override void EffectOfCard(DuelPlayerInformation playerDuelInfo, Card cardPlayed = null)
    {
        //If player is already cursed do nothing. Otherwise, add to their curse turn count.
        //If 'CurseImmediately' is true, curse as soon as the card is used. Otherwise don't curse until after the current turn ends. this usually isn't true for duel related effects.

        //Right now, no way to handle cursing self + enemy.
        if(CurseUser)
        {
            playerDuelInfo.PlayerInDuel.StatusEffectUpdateCompleted += CompletedEffect;
            playerDuelInfo.PlayerInDuel.CursePlayer(NumTurnsToCurse);
            if (playerDuelInfo.PlayerInDuel.IsCursed)
            {
                playerDuelInfo.PlayerInDuel.CurseEffect();
            }

            //Make sure that for the current player they're cursed for their entire next turn essentially.
            if(playerDuelInfo.PlayerInDuel != playerDuelInfo.PlayerInDuel.GameplayManagerRef.GetCurrentPlayer() && playerDuelInfo.PlayerInDuel.IsCursed)
            {
                playerDuelInfo.PlayerInDuel.WasAfflictedWithStatusThisTurn = false;
            }
            return;
        }

        if(AttackAllPlayers)
        {
            playerDuelInfo.PlayerInDuel.DoneAttackingForEffect += CompletedEffect;
            playerDuelInfo.PlayerInDuel.AttackAllOtherPlayersStatusEffect("curse", NumTurnsToCurse);
            return;
        }

        if (OpponentsCanBeChosen)
        {
            playerDuelInfo.PlayerInDuel.DoneAttackingForEffect += CompletedEffect;
            playerDuelInfo.PlayerInDuel.ActivatePlayerToAttackStatusEffectSelectionPopup(NumPlayersToAttack, "curse", NumTurnsToCurse);
        }
    }

    public override bool CanCostBePaid(Player playerReference)
    {
        bool canCostBePaid = false;

        if(CurseUser)
        {
            if(!(playerReference.IsPoisoned && playerReference.IsCursed))
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

                
        if (!canCostBePaid)
        {
            DialogueBoxPopup.instance.ActivatePopupWithJustText("No valid targets to Curse.", 1.5f);
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
