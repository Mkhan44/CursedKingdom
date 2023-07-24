//Code written by Mohamed Riaz Khan of BukuGames.
//All code is written by me (Above name) unless otherwise stated via comments below.
//Not authorized for use outside of the Github repository of this game developed by BukuGames.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PoisonSpaceEffect", menuName = "Space Effect Data/Poison Space", order = 0)]
public class PoisonSpace : SpaceEffectData, ISpaceEffect
{
    [Range(1, 10)] [SerializeField] private int numTurnsToBePoisoned = 1;

    public int NumTurnsToBePoisoned { get => numTurnsToBePoisoned; set => numTurnsToBePoisoned = value; }

    public override void LandedOnEffect(Player playerReference)
    {
        playerReference.PoisonPlayer(NumTurnsToBePoisoned);
        base.LandedOnEffect(playerReference);
        Debug.Log($"Landed on: {this.name} space and should be poisoned for: {NumTurnsToBePoisoned} turn(s).");
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
            EffectDescription = $"Will be poisoned for: {NumTurnsToBePoisoned} turn(s).";
        }
    }

    private void OnEnable()
    {
        UpdateEffectDescription();
    }
}