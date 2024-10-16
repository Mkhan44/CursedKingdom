//Code written by Mohamed Riaz Khan of BukuGames.
//All code is written by me (Above name) unless otherwise stated via comments below.
//Not authorized for use outside of the Github repository of this game developed by BukuGames.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LoseHealthSpaceEffect", menuName = "Space Effect Data/Lose Health", order = 0)]
public class LoseHealthSpace : SpaceEffectData, ISpaceEffect
{
    [Range(1, 10)] [SerializeField] private int healthToLose = 1;

    public int HealthToLose { get => healthToLose; set => healthToLose = value; }

    public override void LandedOnEffect(Player playerReference)
    {
        base.LandedOnEffect(playerReference);
        playerReference.TakeDamage(HealthToLose);
        Debug.Log($"Landed on: {this.name} space and should lose: {HealthToLose} health.");
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
            EffectDescription = $"Lose {HealthToLose} health.";
        }
    }

    private void OnEnable()
    {
        UpdateEffectDescription();
    }
}
