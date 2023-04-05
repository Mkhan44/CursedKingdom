//Code written by Mohamed Riaz Khan of BukuGames.
//All code is written by me (Above name) unless otherwise stated via comments below.
//Not authorized for use outside of the Github repository of this game developed by BukuGames.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SwiftSwipeEffect", menuName = "Space Effect Data/Swift Swipe", order = 0)]
public class SwiftSwipe : SpecialAttackSpace, ISpaceEffect
{
    [Range(1, 10)] [SerializeField] private int numCardsToDiscard = 1;
    [SerializeField] private CardType cardTypeToDiscard;

    public int NumCardsToDiscard { get => numCardsToDiscard; set => numCardsToDiscard = value; }
    public CardType CardTypeToDiscard { get => cardTypeToDiscard; set => cardTypeToDiscard = value; }

    public override void LandedOnEffect(Player playerReference)
    {
        base.LandedOnEffect(playerReference);
        //Need a reference to another player that has at least 2 cards in their hand. Otherwise just don't do this effect.
        Debug.Log($"Landed on: {this.name} special attack space. Discard {NumCardsToDiscard} {CardTypeToDiscard}(s) to look at opponent's hand and discard 1 card then take 1 card.");
    }

    public override void StartOfTurnEffect(Player playerReference)
    {
        base.StartOfTurnEffect(playerReference);
    }

    public override void EndOfTurnEffect(Player playerReference)
    {
        base.EndOfTurnEffect(playerReference);
    }
    protected override void UpdateEffectDescription()
    {
        if (!OverrideAutoDescription)
        {
            EffectDescription = $"Discard {NumCardsToDiscard} {CardTypeToDiscard}(s) to look at opponent's hand and discard 1 card then take 1 card. Only the 'Thief' class may activate this effect.";
        }
    }

    private void OnEnable()
    {
        UpdateEffectDescription();
    }
}
