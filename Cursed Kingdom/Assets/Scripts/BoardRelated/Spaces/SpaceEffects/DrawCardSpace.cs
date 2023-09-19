//Code written by Mohamed Riaz Khan of BukuGames.
//All code is written by me (Above name) unless otherwise stated via comments below.
//Not authorized for use outside of the Github repository of this game developed by BukuGames.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DrawCardSpaceEffect", menuName = "Space Effect Data/Draw Card Space", order = 0)]
public class DrawCardSpace : SpaceEffectData, ISpaceEffect
{
    [SerializeField] private Card.CardType cardTypeToDraw;
    [SerializeField] [Range(1,10)] private int numToDraw = 1;
    [Tooltip("If the user can pick any card type, this should be true. This overrides the cardTypeToDraw field.")]
    [SerializeField] private bool canBeEitherCard;
    [Tooltip("If the user needs to draw from the discard pile instead of the respective deck, this should be true.")]
    [SerializeField] private bool drawFromDiscardPile;

    public Card.CardType CardTypeToDraw { get => cardTypeToDraw; set => cardTypeToDraw = value; }
    public int NumToDraw { get => numToDraw; set => numToDraw = value; }
    public bool CanBeEitherCard { get => canBeEitherCard; set => canBeEitherCard = value; }
    public bool DrawFromDiscardPile { get => drawFromDiscardPile; set => drawFromDiscardPile = value; }

    public override void LandedOnEffect(Player playerReference)
    {
        if(CardTypeToDraw == Card.CardType.Movement)
        {
            playerReference.GameplayManagerRef.ThisDeckManager.DrawCards(Card.CardType.Movement, playerReference, NumToDraw);
            CompletedEffect(playerReference);
        }
        else if(CardTypeToDraw == Card.CardType.Support)
        {
            playerReference.GameplayManagerRef.ThisDeckManager.DrawCards(Card.CardType.Support, playerReference, NumToDraw);
            CompletedEffect(playerReference);
        }
        else if(CardTypeToDraw == Card.CardType.Both)
        {
            Debug.Log("TRYING TO DRAW 2 CARDS");
            playerReference.DoneDrawingCard += CompletedEffect;
            playerReference.SelectCardTypeToDrawPopup(NumToDraw);
        }


        Debug.Log($"Landed on: {this.name} space and should draw: {NumToDraw} {CardTypeToDraw} card(s)");
    }

    public override void StartOfTurnEffect(Player playerReference)
    {
        base.StartOfTurnEffect(playerReference);
    }

    public override void EndOfTurnEffect(Player playerReference)
    {
        base.EndOfTurnEffect(playerReference);
    }

    public override void CompletedEffect(Player playerReference)
    {
        playerReference.DoneDrawingCard -= CompletedEffect;
        base.CompletedEffect(playerReference);
    }

    protected override void UpdateEffectDescription()
    {
        if (!OverrideAutoDescription)
        {
            if(!CanBeEitherCard)
            {
                EffectDescription = $"Draw: {NumToDraw} {CardTypeToDraw} card(s)";
            }
            else
            {
                EffectDescription = $"Draw: {NumToDraw} Movement or Support card(s)";
            }
            
        }
    }

    private void OnEnable()
    {
        UpdateEffectDescription();
    }

}
