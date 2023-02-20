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

    public override void EffectOfSpace(Player playerReference)
    {
        base.EffectOfSpace(playerReference);
        base.EffectOfSpace(playerReference);
        //Need a reference to another player that has at least 2 cards in their hand. Otherwise just don't do this effect.
        Debug.Log($"Landed on: {this.name} special attack space. Discard {NumCardsToDiscard} {CardTypeToDiscard}(s) to look at opponent's hand and discard 1 card then take 1 card.");
    }
}
