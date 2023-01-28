//Code written by Mohamed Riaz Khan of BukuGames.
//All code is written by me (Above name) unless otherwise stated via comments below.
//Not authorized for use outside of the Github repository of this game developed by BukuGames.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This support card effect is used to bring another player into a duel that isn't currently dueling. It moves them to the player's current space as well.
/// </summary>

[CreateAssetMenu(fileName = "BringIntoDuelEffect", menuName = "Card Data/Support Card Effect Data/Bring Into Duel Effect", order = 0)]
public class BringIntoDuelEffect : SupportCardEffectData, ISupportEffect
{
    public override void EffectOfCard(Player playerReference, Card cardPlayed = null)
    {

    }
}

