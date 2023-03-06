//Code written by Mohamed Riaz Khan of BukuGames.
//All code is written by me (Above name) unless otherwise stated via comments below.
//Not authorized for use outside of the Github repository of this game developed by BukuGames.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RecoverHealthSpaceEffect", menuName = "Space Effect Data/Recover Health Space", order = 0)]
public class RecoverHealthSpace : SpaceEffectData, ISpaceEffect
{
    [Range(1,10)] [SerializeField] private int healthToRecover = 1;

    public int HealthToRecover { get => healthToRecover; set => healthToRecover = value; }

    public override void LandedOnEffect(Player playerReference)
    {
        base.LandedOnEffect(playerReference);
        Debug.Log($"Landed on: {this.name} space and should recover: {HealthToRecover} health.");
       // playerReference.CurrentHealth += HealthToRecover;
    }

    public override void StartOfTurnEffect(Player playerReference)
    {
        base.StartOfTurnEffect(playerReference);
    }

    public override void EndOfTurnEffect(Player playerReference)
    {
        base.EndOfTurnEffect(playerReference);
    }
}
