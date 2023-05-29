//Code written by Mohamed Riaz Khan of BukuGames.
//All code is written by me (Above name) unless otherwise stated via comments below.
//Not authorized for use outside of the Github repository of this game developed by BukuGames.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ExcaliburTheGreatSwordEffect", menuName = "Space Effect Data/ExcaliburTheGreatSword", order = 0)]
public class ExcaliburTheGreatSword : SpecialAttackSpace, ISpaceEffect
{
    [Range(1, 10)] [SerializeField] private int numCardsToDiscard;
    [SerializeField] private Card.CardType cardTypeToDiscard;
    [Range(1, 10)] [SerializeField] private int damageToDeal;

    public int NumCardsToDiscard { get => numCardsToDiscard; set => numCardsToDiscard = value; }
    public Card.CardType CardTypeToDiscard { get => cardTypeToDiscard; set => cardTypeToDiscard = value; }
    public int DamageToDeal { get => damageToDeal; set => damageToDeal = value; }

    public override void LandedOnEffect(Player playerReference)
    {
        base.LandedOnEffect(playerReference);
        //Need a reference to another player.
        Debug.Log($"Landed on: {this.name} special attack space. Discard {NumCardsToDiscard} {CardTypeToDiscard}(s) to deal {DamageToDeal} damage to an opponent of your choice.");
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
            EffectDescription = $"Discard {NumCardsToDiscard} {CardTypeToDiscard}(s) to deal {DamageToDeal} damage to an opponent of your choice. Only the 'Warrior' class may activate this effect.";
        }
    }

    private void OnEnable()
    {
        UpdateEffectDescription();
    }
}
