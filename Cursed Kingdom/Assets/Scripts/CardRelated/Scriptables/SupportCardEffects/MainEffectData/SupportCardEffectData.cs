//Code written by Mohamed Riaz Khan of BukuGames.
//All code is written by me (Above name) unless otherwise stated via comments below.
//Not authorized for use outside of the Github repository of this game developed by BukuGames.
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SupportCardEffectData : ScriptableObject , ISupportEffect
{
    public event Action SupportCardEffectCompleted;

    [SerializeField] private bool isACost;

    public bool IsACost { get => isACost; set => isACost = value; }

    public virtual void EffectOfCard(Player playerReference, Card cardPlayed = null)
    {
        //Usually called after the effect has been completed.
        CompletedEffect(playerReference);
    }

    public virtual void StartOfTurnEffect(Player playerReference)
    {

    }

    public virtual void EndOfTurnEffect(Player playerReference)
    {

    }
    public virtual bool CanCostBePaid(Player playerReference)
    {
        return true;
    }

    public virtual void CompletedEffect(Player playerReference)
    {
        SupportCardEffectCompleted?.Invoke();
    }

    protected virtual void UpdateEffectDescription()
    {

    }
}
