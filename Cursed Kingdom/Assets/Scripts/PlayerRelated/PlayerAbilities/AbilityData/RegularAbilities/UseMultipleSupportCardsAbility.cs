

//Code written by Mohamed Riaz Khan of BukuGames.
//All code is written by me (Above name) unless otherwise stated via comments below.
//Not authorized for use outside of the Github repository of this game developed by BukuGames.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Allow the character that uses this ability to use x amount of extra Movement cards to move during Movement phase.
/// </summary>

[CreateAssetMenu(fileName = "UseMultipleSupportCardsAbility", menuName = "Player/Ability/UseMultipleSupportCards", order = 0)]
public class UseMultipleSupportCardsAbility : AbilityData, IAbility
{
    [SerializeField][Range(0, 10)] private int numExtraCardsToUse = 1;
    public override void ActivateEffect(Player playerReference)
    {
        base.ActivateEffect(playerReference);
        playerReference.ActivateAbilityEffects();
        playerReference.IncreaseMaxCardUses(numExtraCardsToUse, Card.CardType.Support);
    }

    public override void CompletedEffect(Player playerReference)
    {
        base.CompletedEffect(playerReference);
    }

    protected override void UpdateEffectDescription()
    {
        if (!OverrideAutoDescription)
        {
            EffectDescription = $"Able to use: {numExtraCardsToUse} extra support card(s) this turn.";
        }
    }

    private void OnEnable()
    {
        UpdateEffectDescription();
    }
}
