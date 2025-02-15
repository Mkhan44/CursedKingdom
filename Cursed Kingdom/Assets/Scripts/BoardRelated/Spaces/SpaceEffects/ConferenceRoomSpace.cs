//Code written by Mohamed Riaz Khan of BukuGames.
//All code is written by me (Above name) unless otherwise stated via comments below.
//Not authorized for use outside of the Github repository of this game developed by BukuGames.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ConferenceRoomSpaceEffect", menuName = "Space Effect Data/ConferenceRoom Space", order = 0)]
public class ConferenceRoomSpace : SpaceEffectData, ISpaceEffect
{
    [SerializeField] private List<Card.CardType> cardTypeToDraw;
    [SerializeField] [Range(1, 10)] private int numToDraw = 1;
    [SerializeField] [Range(1, 10)] private int healthToRecover = 1;

    //This needs to be AFTER duel.
    [SerializeField] [Range(1, 10)] private int damageToDeal = 1;

    public List<Card.CardType> CardTypeToDraw1 { get => cardTypeToDraw; set => cardTypeToDraw = value; }
    public int NumToDraw { get => numToDraw; set => numToDraw = value; }
    public int HealthToRecover { get => healthToRecover; set => healthToRecover = value; }
    public int DamageToDeal { get => damageToDeal; set => damageToDeal = value; }

    public override void LandedOnEffect(Player playerReference)
    {
        base.LandedOnEffect(playerReference);
        Debug.Log($"Landed on: {this.name} space...lots of stuff should happen lolol");
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
            EffectDescription = $"Do lots of stuff...Will update later lolol.";
        }
    }

    private void OnEnable()
    {
        UpdateEffectDescription();
    }
}
