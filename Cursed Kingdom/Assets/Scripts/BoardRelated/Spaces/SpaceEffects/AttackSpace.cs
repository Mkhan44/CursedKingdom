//Code written by Mohamed Riaz Khan of BukuGames.
//All code is written by me (Above name) unless otherwise stated via comments below.
//Not authorized for use outside of the Github repository of this game developed by BukuGames.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Attack SpaceEffect", menuName = "Space Effect Data/Attack Space", order = 0)]
public class AttackSpace : SpaceEffectData, ISpaceEffect
{
    [Range(1, 10)] [SerializeField] private int damageToGive = 1;
    [Range(1, 3)] [SerializeField] private int numPlayersToAttack = 1;
    [Tooltip("Player can choose the opponent's they want to attack. This will prompt the game to give a choice box.")]
    [SerializeField] private bool opponentsCanBeChosen = true;
    [Tooltip("Attacks all other players. This overrides 'numPlayersToAttack' and 'opponentsCanBeChosen' will be irrelevant.")]
    [SerializeField] private bool attackAllPlayers;

    public int DamageToGive { get => damageToGive; set => damageToGive = value; }
    public int NumPlayersToAttack { get => numPlayersToAttack; set => numPlayersToAttack = value; }
    public bool OpponentsCanBeChosen { get => opponentsCanBeChosen; set => opponentsCanBeChosen = value; }
    public bool AttackAllPlayers { get => attackAllPlayers; set => attackAllPlayers = value; }

    //We'll need a way to get the reference to the Player or Players that is going to be attacked.
    public override void EffectOfSpace(Player playerReference)
    {
        Debug.Log($"Landed on: {this.name} space and should give: {DamageToGive} damage out to {NumPlayersToAttack} players each.");
    }
}