//Code written by Mohamed Riaz Khan of BukuGames.
//All code is written by me (Above name) unless otherwise stated via comments below.
//Not authorized for use outside of the Github repository of this game developed by BukuGames.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This support card effect is used to move a target player backwards a certain amount of spaces.
/// </summary>

[CreateAssetMenu(fileName = "MoveBackwardsEffect", menuName = "Card Data/Support Card Effect Data/Move Backwards Effect", order = 0)]
public class DuelMoveBackwardsEffect : SupportCardEffectData, ISupportEffect
{
    [SerializeField] [Range(1, 10)] private int numSpacesToMoveBack = 1;

    public int NumSpacesToMoveBack { get => numSpacesToMoveBack; set => numSpacesToMoveBack = value; }

    public override void EffectOfCard(Player playerReference, Card cardPlayed = null)
    {
        playerReference.DoneMovingBackwards += CompletedEffect;
        SupportCard cardUsed = (SupportCard)cardPlayed;
        if (cardUsed != null)
        {
            supportCardThatWasJustUsed = cardUsed;
        }
    }
    public override void EffectOfCard(DuelPlayerInformation playerDuelInfo, Card cardPlayed = null)
    {
        playerDuelInfo.PlayerInDuel.DoneMovingBackwards += CompletedEffect;
        SupportCard cardUsed = (SupportCard)cardPlayed;
        if (cardUsed != null)
        {
            supportCardThatWasJustUsed = cardUsed;
        }

        List<Player> validTargets = new();
        foreach(DuelPlayerInformation duelPlayerInformation in playerDuelInfo.PlayerInDuel.GameplayManagerRef.DuelPhaseSMRef.PlayersInCurrentDuel)
        {
            if(duelPlayerInformation != playerDuelInfo)
            {
                validTargets.Add(duelPlayerInformation.PlayerInDuel);
            }
        }

        //Don't worry about choosing a target. just make the player choose how many spaces to move them back by.
        if(validTargets.Count == 1)
        {
            //Depends if we want this to be a 'max num' and the user can choose. If it is a max num, then we should just go right into moving the single target back.
            if(NumSpacesToMoveBack == 1)
            {

            }
            playerDuelInfo.PlayerInDuel.ActivateSelectNumSpacesToMakeTargetPlayerMoveBackPopup(playerDuelInfo.PlayerInDuel, validTargets[0], NumSpacesToMoveBack);
            return;
        }

        playerDuelInfo.PlayerInDuel.ActivateTargetPlayerToMoveBackwardsPopup(playerDuelInfo.PlayerInDuel, validTargets, NumSpacesToMoveBack);
    }

    public override void CompletedEffect(Player playerReference)
    {
        playerReference.DoneMovingBackwards -= CompletedEffect;
        base.CompletedEffect(playerReference);
    }
}



