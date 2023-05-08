//Code written by Mohamed Riaz Khan of BukuGames.
//All code is written by me (Above name) unless otherwise stated via comments below.
//Not authorized for use outside of the Github repository of this game developed by BukuGames.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CurseSpaceEffect", menuName = "Space Effect Data/Curse Space", order = 0)]
public class CurseSpace : SpaceEffectData, ISpaceEffect
{
    [Range(1, 10)] [SerializeField] private int numTurnsToBeCursed = 1;

    public int NumTurnsToBeCursed { get => numTurnsToBeCursed; set => numTurnsToBeCursed = value; }

    public override void LandedOnEffect(Player playerReference)
    {
        base.LandedOnEffect(playerReference);
        playerReference.CursePlayer(numTurnsToBeCursed);
        Debug.Log($"Landed on: {this.name} space and should be cursed for: {NumTurnsToBeCursed} turn(s).");
    }

    public override void StartOfTurnEffect(Player playerReference)
    {
        base.StartOfTurnEffect(playerReference);
        playerReference.CursePlayer(numTurnsToBeCursed);
    }

    public override void EndOfTurnEffect(Player playerReference)
    {
        base.EndOfTurnEffect(playerReference);
        playerReference.CursePlayer(numTurnsToBeCursed);
    }
    protected override void UpdateEffectDescription()
    {
        if (!OverrideAutoDescription)
        {
            EffectDescription = $"Will be cursed for: {NumTurnsToBeCursed} turn(s).";
        }
    }

    private void OnEnable()
    {
        UpdateEffectDescription();
    }
}
