//Code written by Mohamed Riaz Khan of BukuGames.
//All code is written by me (Above name) unless otherwise stated via comments below.
//Not authorized for use outside of the Github repository of this game developed by BukuGames.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TakeCardsFromOpponentsHand", menuName = "Space Effect Data/Take Cards From Opponents Hand", order = 0)]
public class TakeCardsFromOpponentsHand : SpaceEffectData, ISpaceEffect
{
    [Range(1, 10)][SerializeField] private int numCardsToDiscard = 1;
    [SerializeField] private Card.CardType cardTypeToDiscard;
    [Range(1, 10)][SerializeField] private int numCardsToTake = 1;
    [SerializeField] private Card.CardType cardTypeToTake;


    public int NumCardsToDiscard { get => numCardsToDiscard; set => numCardsToDiscard = value; }
    public Card.CardType CardTypeToDiscard { get => cardTypeToDiscard; set => cardTypeToDiscard = value; }
    public int NumCardsToTake { get => numCardsToTake; set => numCardsToTake = value; }
    public Card.CardType CardTypeToTake { get => cardTypeToTake; set => cardTypeToTake = value; }

    public override void LandedOnEffect(Player playerReference)
    {
        base.LandedOnEffect(playerReference);
        //Need a reference to another player that has at least 2 cards in their hand. Otherwise just don't do this effect.
        Debug.Log($"Landed on: {this.name} TakeCardsFromOpponentsHand space.");
    }

    public override void StartOfTurnEffect(Player playerReference)
    {
        base.StartOfTurnEffect(playerReference);
    }

    public override void EndOfTurnEffect(Player playerReference)
    {
        base.EndOfTurnEffect(playerReference);
    }

    public override void CompletedEffect(Player playerReference)
    {
        base.CompletedEffect(playerReference);
    }
    protected override void UpdateEffectDescription()
    {
        if (!OverrideAutoDescription)
        {
            EffectDescription = $"Look at an opponent's hand and discard {NumCardsToDiscard} {CardTypeToDiscard} then take {NumCardsToTake} {CardTypeToTake} card.";
        }
    }

    private void OnEnable()
    {
        UpdateEffectDescription();
    }
}
