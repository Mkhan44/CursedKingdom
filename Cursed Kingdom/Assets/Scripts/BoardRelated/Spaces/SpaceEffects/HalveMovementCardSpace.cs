//Code written by Mohamed Riaz Khan of BukuGames.
//All code is written by me (Above name) unless otherwise stated via comments below.
//Not authorized for use outside of the Github repository of this game developed by BukuGames.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "HalveMovementCardSpaceEffect", menuName = "Space Effect Data/Halve Movement Card Space", order = 0)]
public class HalveMovementCardSpace : SpaceEffectData, ISpaceEffect
{

    //As of right now: This DOES stack with Curse halving. Round the numbers up.
    public override void LandedOnEffect(Player playerReference)
    {
        base.LandedOnEffect(playerReference);
        Debug.Log($"Landed on: {this.name} space and all movement cards in their hand have their values halved.");
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
            EffectDescription = $"All movement cards in your hand have their values halved this turn if your turn starts on this space.";
        }
    }

    private void OnEnable()
    {
        UpdateEffectDescription();
    }
}
