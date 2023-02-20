//Code written by Mohamed Riaz Khan of BukuGames.
//All code is written by me (Above name) unless otherwise stated via comments below.
//Not authorized for use outside of the Github repository of this game developed by BukuGames.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DiscardCardSpaceEffect", menuName = "Space Effect Data/Discard Card Space", order = 0)]
public class DiscardCardSpace : SpaceEffectData, ISpaceEffect
{

    [SerializeField] private CardType cardTypeToDiscard;
    [SerializeField] [Range(1, 10)] private int numToDiscard = 1;

    public CardType CardTypeToDiscard { get => cardTypeToDiscard; set => cardTypeToDiscard = value; }
    public int NumToDiscard { get => numToDiscard; set => numToDiscard = value; }

    //Check if the player can discard before activating any other space effects. This will have to be determined by whatever is queueing up the space effects to trigger.
    public override void EffectOfSpace(Player playerReference)
    {
        base.EffectOfSpace(playerReference);
        //CAN PLAYER DISCARD? IF NO -- SKIP THIS EFFECT AND ANYTHING RELYING ON THE DISCARD.

        Debug.Log($"Landed on: {this.name} space and should discard: {NumToDiscard} {CardTypeToDiscard} card(s)");
    }
}
