//Code written by Mohamed Riaz Khan of BukuGames.
//All code is written by me (Above name) unless otherwise stated via comments below.
//Not authorized for use outside of the Github repository of this game developed by BukuGames.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ExcaliburTheGreatSword", menuName = "Space Effect Data/ExcaliburTheGreatSword", order = 0)]
public class ExcaliburTheGreatSword : SpecialAttackSpace, ISpaceEffect
{
    [Range(1, 10)] [SerializeField] private int numCardsToDiscard;
    [SerializeField] private CardType cardTypeToDiscard;
    [Range(1, 10)] [SerializeField] private int damageToDeal;

    public int NumCardsToDiscard { get => numCardsToDiscard; set => numCardsToDiscard = value; }
    public CardType CardTypeToDiscard { get => cardTypeToDiscard; set => cardTypeToDiscard = value; }
    public int DamageToDeal { get => damageToDeal; set => damageToDeal = value; }

    public override void EffectOfSpace(Player playerReference)
    {
        base.EffectOfSpace(playerReference);
        //Need a reference to another player.
        Debug.Log($"Landed on: {this.name} special attack space. Discard {NumCardsToDiscard} {CardTypeToDiscard}(s) to deal {DamageToDeal} damage to an opponent of your choice.");
    }
}
