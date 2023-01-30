//Code written by Mohamed Riaz Khan of BukuGames.
//All code is written by me (Above name) unless otherwise stated via comments below.
//Not authorized for use outside of the Github repository of this game developed by BukuGames.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This support card effect is used to discard as a cost in order to take a card from an opponent.
/// </summary>

[CreateAssetMenu(fileName = "DiscardToTakeCardEffect", menuName = "Card Data/Support Card Effect Data/Discard To Take Card Effect", order = 0)]
public class DiscardToTakeCardEffect : SupportCardEffectData, ISupportEffect
{
    [SerializeField] private Card.CardType cardTypeToDiscard;
    [SerializeField] [Range(1, 10)] private int numCardsToDiscard = 1;
    [SerializeField] private Card.CardType cardToTakeFromOpponent;
    [SerializeField] [Range(1, 10)] private int numCardsToTake = 1;

    public Card.CardType CardTypeToDiscard { get => cardTypeToDiscard; set => cardTypeToDiscard = value; }
    public int NumCardsToDiscard { get => numCardsToDiscard; set => numCardsToDiscard = value; }
    public Card.CardType CardToTakeFromOpponent { get => cardToTakeFromOpponent; set => cardToTakeFromOpponent = value; }
    public int NumCardsToTake { get => numCardsToTake; set => numCardsToTake = value; }

    public override void EffectOfCard(Player playerReference, Card cardPlayed = null)
    {
        
    }
}

