//Code written by Mohamed Riaz Khan of BukuGames.
//All code is written by me (Above name) unless otherwise stated via comments below.
//Not authorized for use outside of the Github repository of this game developed by BukuGames.
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class MovementCardDuel : CardDuel
{
    //Data
    [SerializeField] private MovementCardData movementCardData;
    [SerializeField] private int movementCardValue;
    //Used for if we need to temporarily halve or increase a movement card's value while in the player's hand due to item being used or curse, etc.
    [SerializeField] private int tempCardValue;
    [SerializeField] private int tempIncreaseValue;
    [SerializeField] private int tempDecreaseValue;

    //References
    [SerializeField] private TextMeshProUGUI movementValueText;
    [SerializeField] private MovementCard movementCardReference;

    public MovementCardData MovementCardData { get => movementCardData; set => movementCardData = value; }
    public int MovementCardValue { get => movementCardValue; set => movementCardValue = value; }
    public int TempCardValue { get => tempCardValue; set => tempCardValue = value; }
    public int TempIncreaseValue { get => tempIncreaseValue; set => tempIncreaseValue = value; }
    public int TempDecreaseValue { get => tempDecreaseValue; set => tempDecreaseValue = value; }
    public MovementCard MovementCardReference { get => movementCardReference; set => movementCardReference = value; }

    public MovementCardDuel(MovementCard moveCardRef)
    {
        MovementCardReference = moveCardRef;
        SetupCard(moveCardRef);
    }

    public override void SetupCard(Card cardReference)
    {
        base.SetupCard(cardReference);
        MovementCard movementCardRef = cardReference as MovementCard;
        if (movementCardRef != null)
        {
            movementCardReference = movementCardRef;
            MovementCardValue = movementCardRef.MovementCardValue;
            TempCardValue = movementCardRef.TempCardValue;
            TitleText.text = movementCardRef.TitleText.text;
        }

        if(TempCardValue > 0)
        {
            movementValueText.text = TempCardValue.ToString();
        }
        else
        {
            movementValueText.text = MovementCardValue.ToString();
        }
        //MovementCardData = cardData as MovementCardData;
        //MovementCardValue = MovementCardData.MovementValue;
        //TempCardValue = 0;
        //TitleText.text = "Movement";
        //movementValueText.text = MovementCardValue.ToString();
    }

    public override void OnPointerClick(PointerEventData eventData)
    {
        if(DuelPhaseSMReference != null)
        {
            DuelPhaseSMReference.duelSelectCardsToUsePhaseState.DeselectMovementCard(movementCardReference);
        }
    }
}
