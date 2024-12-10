//Code written by Mohamed Riaz Khan of BukuGames.
//All code is written by me (Above name) unless otherwise stated via comments below.
//Not authorized for use outside of the Github repository of this game developed by BukuGames.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// After a character wins a duel: They may take the card(s) that were used by the opponents.
/// </summary>
 
 [CreateAssetMenu(fileName = "TakeCardsAfterDuelEliteAbility", menuName = "Player/EliteAbility/TakeCardsAfterDuelElite", order = 0)]
public class TakeCardsAfterDuelEliteAbility : EliteAbilityData, IEliteAbility
{
    [SerializeField] private bool mustBeWinner = true;
    public override void ActivateEffect(DuelPlayerInformation duelPlayerInformation)
    {
        //duelPlayerInformation.PlayerInDuel.DoneActivatingEliteAbilityEffect += CompletedEffect;
        duelPlayerInformation.PlayerInDuel.ActivateEliteAbilityEffects();

        foreach(DuelPlayerInformation playerInfo in duelPlayerInformation.PlayerInDuel.GameplayManagerRef.DuelPhaseSMRef.PlayersInCurrentDuel)
        {
            if(playerInfo != duelPlayerInformation)
            {
                playerInfo.SelectedMovementCards[0].gameObject.SetActive(true);

                //Take the card from the opponent.
                duelPlayerInformation.PlayerInDuel.DrawCard(playerInfo.SelectedMovementCards[0]);

                playerInfo.SelectedMovementCards[0].transform.localScale = duelPlayerInformation.SelectedMovementCards[0].OriginalSize;
                playerInfo.SelectedMovementCards[0].ResetMovementValue();

                //Remove the card from the selectedmovement cards list so that way we don't try to discard it later.
                playerInfo.SelectedMovementCards.Remove(duelPlayerInformation.SelectedMovementCards[0]);
            }
        }
        CompletedEffect(duelPlayerInformation.PlayerInDuel);
    }

    public override bool CanCostBePaid(DuelPlayerInformation duelPlayerInformation, bool justChecking = false)
    {
        bool canCostBePaid = false;
        if(mustBeWinner && duelPlayerInformation.PlayerInDuel.GameplayManagerRef.DuelPhaseSMRef.CurrentWinners.Contains(duelPlayerInformation))
        {
            canCostBePaid = true;
        }
        return canCostBePaid;
    }

    public override void CompletedEffect(Player playerReference)
    {
       // playerReference.DoneActivatingEliteAbilityEffect -= CompletedEffect;
        playerReference.CompletedEliteAbilityActivation();
        base.CompletedEffect(playerReference);
    }

    protected override void UpdateEffectDescription()
    {
        if (!OverrideAutoDescription)
        {
            EffectDescription = $"If you win a duel: Take the movement card that each of the losers used from that duel.";
        }
    }

    private void OnEnable()
    {
        UpdateEffectDescription();
    }

}
