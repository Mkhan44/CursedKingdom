//Code written by Mohamed Riaz Khan of BukuGames.
//All code is written by me (Above name) unless otherwise stated via comments below.
//Not authorized for use outside of the Github repository of this game developed by BukuGames.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This support card effect is used when we want to multiply or divide the user's attack value during a duel.
/// </summary>
[CreateAssetMenu(fileName = "DuelMultiplyDivideAttack", menuName = "Card Data/Support Card Effect Data/Duel Multiply Divide Attack Effect", order = 0)]
public class DuelMultiplyDivideAttackEffect : SupportCardEffectData, ISupportEffect
{
    [SerializeField] [Range(1, 10)] private int numMovementMultiply = 1;
    [SerializeField] [Range(1,10)] private int numMovementDivide = 1;
    [SerializeField] private bool buffAllCards = true;
    [SerializeField] private bool randomizeBetweenMultiplyAndDivide = false;

    public int NumMovementMultiply { get => numMovementMultiply; set => numMovementMultiply = value; }
    public int NumMovementDivide { get => numMovementDivide; set => numMovementDivide = value; }
    public bool BuffAllCards { get => buffAllCards; set => buffAllCards = value; }
    public bool RandomizeBetweenMultiplyAndDivide { get => randomizeBetweenMultiplyAndDivide; set => randomizeBetweenMultiplyAndDivide = value; }

    public override void EffectOfCard(Player playerReference, Card cardPlayed = null)
    {
        //If BuffAllCards != true ....Need to let player choose which card to apply the buff to.
        SupportCard cardUsed = (SupportCard)cardPlayed;
        if (cardUsed != null)
        {
            supportCardThatWasJustUsed = cardUsed;
        }
    }

    public override void EffectOfCard(DuelPlayerInformation duelPlayerInformation, Card cardPlayed = null)
    {
        if(RandomizeBetweenMultiplyAndDivide)
        {
            int randomNum = Random.Range(0,2);
            if(randomNum == 0)
            {
                
                foreach(MovementCard movementCard in duelPlayerInformation.SelectedMovementCards)
                {
                    Debug.Log($"Multiplying movement card value {movementCard.MovementCardValue} by {NumMovementMultiply}");
                    movementCard.ManipulateMovementValue(false, 0, true, NumMovementMultiply, false, 0);
                }
            }
            else
            {
                foreach(MovementCard movementCard in duelPlayerInformation.SelectedMovementCards)
                {
                    Debug.Log($"Dividng movement card value {movementCard.MovementCardValue} by {NumMovementDivide}");
                    movementCard.ManipulateMovementValue(true, NumMovementDivide, false, 0, false, 0);
                }
            }

            base.EffectOfCard(duelPlayerInformation, cardPlayed);
            return;
        }
        

        if(numMovementMultiply > 1)
        {
            foreach(MovementCard movementCard in duelPlayerInformation.SelectedMovementCards)
            {
                movementCard.ManipulateMovementValue(false, 0, true, NumMovementMultiply, false, 0);
            }

        }
        else if(numMovementDivide > 1)
        {
            foreach(MovementCard movementCard in duelPlayerInformation.SelectedMovementCards)
            {
                movementCard.ManipulateMovementValue(true, NumMovementDivide, false, 0, false, 0);
            }
        }
        
       
    }
}
