//Code written by Mohamed Riaz Khan of BukuGames.
//All code is written by me (Above name) unless otherwise stated via comments below.
//Not authorized for use outside of the Github repository of this game developed by BukuGames.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ManipulateOpponentsHandEffect", menuName = "Space Effect Data/Manipulate Opponents Hand", order = 0)]
public class ManipulateOpponentsHand : SpaceEffectData, ISpaceEffect
{
    [Range(1, 10)][SerializeField] private int numCardsToDiscard = 1;
    [SerializeField] private Card.CardType cardTypeToDiscard;
    [Range(1, 10)][SerializeField] private int numCardsToTake = 1;
    [SerializeField] private Card.CardType cardTypeToTake;
    [Tooltip("Player can choose the opponent's they want to attack. This will prompt the game to give a choice box.")]
    [SerializeField] private bool opponentsCanBeChosen = true;


    public int NumCardsToDiscard { get => numCardsToDiscard; set => numCardsToDiscard = value; }
    public Card.CardType CardTypeToDiscard { get => cardTypeToDiscard; set => cardTypeToDiscard = value; }
    public int NumCardsToTake { get => numCardsToTake; set => numCardsToTake = value; }
    public Card.CardType CardTypeToTake { get => cardTypeToTake; set => cardTypeToTake = value; }
    public bool OpponentsCanBeChosen { get => opponentsCanBeChosen; set => opponentsCanBeChosen = value; }

    public override void LandedOnEffect(Player playerReference)
    {
        if (playerReference.CheckIfCanTakeCardsFromOtherPlayersHand(NumCardsToDiscard, CardTypeToDiscard, NumCardsToTake, CardTypeToTake))
        {
            //Call method on Player to give option to pick which player to take card from.
            playerReference.ActivatePlayerToTakeCardsFromSelectionPopup(NumCardsToDiscard, CardTypeToDiscard, NumCardsToTake, CardTypeToTake);
        }
        
        Debug.Log($"Landed on: {this.name} ManipulateOpponentsHandEffect space. Discard {NumCardsToDiscard} {CardTypeToDiscard}(s) to look at opponent's hand and discard 1 card then take 1 card.");
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
            EffectDescription = $"Discard {NumCardsToDiscard} {CardTypeToDiscard}(s) to look at opponent's hand and discard 1 card then take 1 card. Only the 'Thief' class may activate this effect.";
        }
    }

    private void OnEnable()
    {
        UpdateEffectDescription();
    }
}
