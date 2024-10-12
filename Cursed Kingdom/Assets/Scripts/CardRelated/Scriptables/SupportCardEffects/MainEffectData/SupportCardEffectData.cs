//Code written by Mohamed Riaz Khan of BukuGames.
//All code is written by me (Above name) unless otherwise stated via comments below.
//Not authorized for use outside of the Github repository of this game developed by BukuGames.
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SupportCardEffectData : ScriptableObject , ISupportEffect
{
    public event Action<SupportCard> SupportCardEffectCompleted;

    [SerializeField] private bool isACost;
    [SerializeField] private bool isElemental;
    [SerializeField] private bool isReaction;
    [SerializeField] private bool isAfterDuelEffectAndNeedsToWin;
    [SerializeField] private bool isAfterDuelEffect;
    [SerializeField] private bool isDuringDuelDamageCalc;
    //Used when we want this to always activate first before anything else.
    [SerializeField] private bool hasPriorityInDuel;
    protected SupportCard supportCardThatWasJustUsed;

    public bool IsACost { get => isACost; set => isACost = value; }
    public bool IsElemental { get => isElemental; set => isElemental = value; }
    public bool IsReaction { get => isReaction; set => isReaction = value; }
    public bool IsAfterDuelEffectAndNeedsToWin { get => isAfterDuelEffectAndNeedsToWin; set => isAfterDuelEffectAndNeedsToWin = value; }
    public bool IsAfterDuelEffect { get => isAfterDuelEffect; set => isAfterDuelEffect = value; }
    public bool IsDuringDuelDamageCalc { get => isDuringDuelDamageCalc; set => isDuringDuelDamageCalc = value; }
    public bool HasPriorityInDuel { get => hasPriorityInDuel; set => hasPriorityInDuel = value; }

    public virtual void EffectOfCard(Player playerReference, Card cardPlayed = null)
    {
        SupportCard cardUsed = (SupportCard)cardPlayed;
        if (cardUsed != null) 
        {
            supportCardThatWasJustUsed = cardUsed;
        }
        //Usually called after the effect has been completed.
        CompletedEffect(playerReference);
    }

    public virtual void EffectOfCard(DuelPlayerInformation playerDuelInfo, Card cardPlayed = null)
    {
        SupportCard cardUsed = (SupportCard)cardPlayed;
        if (cardUsed != null)
        {
            supportCardThatWasJustUsed = cardUsed;
        }
        //Usually called after the effect has been completed.
        CompletedEffect(playerDuelInfo.PlayerInDuel);
    }
    public virtual void StartOfTurnEffect(Player playerReference)
    {

    }

    public virtual void EndOfTurnEffect(Player playerReference)
    {

    }
    public virtual bool CanCostBePaid(Player playerReference, bool justChecking = false)
    {
        return true;
    }

    public virtual bool CanCostBePaid(DuelPlayerInformation playerDuelInfo, Card cardPlayer = null, bool justChecking = false)
    {
        return true;
    }

    public virtual void CompletedEffect(Player playerReference)
    {
        SupportCardEffectCompleted?.Invoke(supportCardThatWasJustUsed);
    }

    protected virtual void UpdateEffectDescription()
    {

    }

}
