//Code written by Mohamed Riaz Khan of BukuGames.
//All code is written by me (Above name) unless otherwise stated via comments below.
//Not authorized for use outside of the Github repository of this game developed by BukuGames.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This support card effect is used when being attacked by an opponent's elemental card effect. It negates the damage that would have been done by that effect.
/// </summary>

[CreateAssetMenu(fileName = "BlockElementalEffect", menuName = "Card Data/Support Card Effect Data/Block Elemental Effect", order = 0)]
public class BlockElementalEffect : SupportCardEffectData, ISupportEffect
{
    public override void EffectOfCard(Player playerReference, Card cardPlayed = null)
    {
        //Block the incoming damage of an elemental effect card.
    }
}