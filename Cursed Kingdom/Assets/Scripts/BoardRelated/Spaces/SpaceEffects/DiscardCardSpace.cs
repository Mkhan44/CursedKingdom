//Code written by Mohamed Riaz Khan of BukuGames.
//All code is written by me (Above name) unless otherwise stated via comments below.
//Not authorized for use outside of the Github repository of this game developed by BukuGames.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DiscardCardSpaceEffect", menuName = "Space Effect Data/Discard Card Space", order = 0)]
public class DiscardCardSpace : SpaceEffectData, ISpaceEffect
{

    [SerializeField] private Card.CardType cardTypeToDiscard;
    [SerializeField] [Range(1, 10)] private int numToDiscard = 1;

    public Card.CardType CardTypeToDiscard { get => cardTypeToDiscard; set => cardTypeToDiscard = value; }
    public int NumToDiscard { get => numToDiscard; set => numToDiscard = value; }

    //Check if the player can discard before activating any other space effects. This will have to be determined by whatever is queueing up the space effects to trigger.
    public override void LandedOnEffect(Player playerReference)
    {
        base.LandedOnEffect(playerReference);

        if (playerReference.CanDiscard(CardTypeToDiscard, NumToDiscard))
        {
            playerReference.ChooseCardsToDiscard(CardTypeToDiscard, NumToDiscard);
        }
        Debug.Log($"Landed on: {this.name} space and should discard: {NumToDiscard} {CardTypeToDiscard} card(s)");
    }

    public override void StartOfTurnEffect(Player playerReference)
    {
        base.StartOfTurnEffect(playerReference);
    }

    public override void EndOfTurnEffect(Player playerReference)
    {
        base.EndOfTurnEffect(playerReference);
    }

    public override bool CanCostBePaid(Player playerReference)
    {
        //test.
        if (playerReference.CanDiscard(CardTypeToDiscard, NumToDiscard))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    protected override void UpdateEffectDescription()
    {
        if (!OverrideAutoDescription)
        {
            EffectDescription = $"Discard: {NumToDiscard} {CardTypeToDiscard} card(s).";
        }
    }

    private void OnEnable()
    {
        UpdateEffectDescription();
    }
}
