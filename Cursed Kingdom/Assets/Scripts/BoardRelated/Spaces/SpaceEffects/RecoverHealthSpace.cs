//Code written by Mohamed Riaz Khan of BukuGames.
//All code is written by me (Above name) unless otherwise stated via comments below.
//Not authorized for use outside of the Github repository of this game developed by BukuGames.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RecoverHealthSpace", menuName = "Space Effect Data/Recover Health Space", order = 0)]
public class RecoverHealthSpace : SpaceEffectData, ISpaceEffect
{
    [Range(1,10)] [SerializeField] private int healthToRecover;

    public int HealthToRecover { get => healthToRecover; set => healthToRecover = value; }

    public void EffectOfSpace(Player playerReference)
    {
        Debug.Log($"Landed on: {this.name} space and should recover: {HealthToRecover} health.");
       // playerReference.CurrentHealth += HealthToRecover;
    }
}
