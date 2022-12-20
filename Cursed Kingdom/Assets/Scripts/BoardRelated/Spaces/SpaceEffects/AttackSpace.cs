//Code written by Mohamed Riaz Khan of BukuGames.
//All code is written by me (Above name) unless otherwise stated via comments below.
//Not authorized for use outside of the Github repository of this game developed by BukuGames.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Attack Space", menuName = "Space Effect Data/Attack Space", order = 0)]
public class AttackSpace : SpaceEffectData, ISpaceEffect
{
    [Range(1, 10)] [SerializeField] private int damageToGive;
    [Range(1, 3)] [SerializeField] private int numPlayersToAttack;

    public int DamageToGive { get => damageToGive; set => damageToGive = value; }
    public int NumPlayersToAttack { get => numPlayersToAttack; set => numPlayersToAttack = value; }

    //We'll need a way to get the reference to the Player or Players that is going to be attacked.
    public void EffectOfSpace(Player playerReference)
    {
        Debug.Log($"Landed on: {this.name} space and should give: {DamageToGive} damage out to {NumPlayersToAttack} players each.");
    }
}
