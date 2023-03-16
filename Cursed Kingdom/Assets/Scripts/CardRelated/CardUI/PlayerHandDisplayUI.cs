//Code written by Mohamed Riaz Khan of BukuGames.
//All code is written by me (Above name) unless otherwise stated via comments below.
//Not authorized for use outside of the Github repository of this game developed by BukuGames.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PlayerHandDisplayUI : MonoBehaviour , IPointerClickHandler
{
    [SerializeField] private RectTransform smallCardHolderPanelTransform;
    [SerializeField] private RectTransform expandedCardHolderPanelTransform;
    [SerializeField] private RectTransform movementCardsHolder;
    [SerializeField] private RectTransform supportCardsHolder;

    [SerializeField] private Vector2 movementCardAnchorMaxInitial;
    [SerializeField] private Vector2 movementCardAnchorMinInitial;
    [SerializeField] private Vector2 supportCardAnchorMaxInitial;
    [SerializeField] private Vector2 supportCardAnchorMinInitial;
    [SerializeField] private Vector2 smallCardHolderPanelAnchorMaxInitial;
    [SerializeField] private Vector2 smallCardHolderPanelAnchorMinInitial;

    [SerializeField] private bool isExpanded;

    public RectTransform SmallCardHolderPanelTransform { get => smallCardHolderPanelTransform; set => smallCardHolderPanelTransform = value; }
    public RectTransform ExpandedCardHolderPanelTransform { get => expandedCardHolderPanelTransform; set => expandedCardHolderPanelTransform = value; }
    public RectTransform MovementCardsHolder { get => movementCardsHolder; set => movementCardsHolder = value; }
    public RectTransform SupportCardsHolder { get => supportCardsHolder; set => supportCardsHolder = value; }
    public bool IsExpanded { get => isExpanded; set => isExpanded = value; }

    private void Start()
    {
        IsExpanded = false;
        movementCardAnchorMaxInitial = MovementCardsHolder.anchorMax;
        movementCardAnchorMinInitial = MovementCardsHolder.anchorMin;
        supportCardAnchorMaxInitial = SupportCardsHolder.anchorMax;
        supportCardAnchorMinInitial = SupportCardsHolder.anchorMin;
        smallCardHolderPanelAnchorMaxInitial = SmallCardHolderPanelTransform.anchorMax;
        smallCardHolderPanelAnchorMinInitial = SmallCardHolderPanelTransform.anchorMin;
    }

    public void ExpandHand(Card.CardType cardTypeToExpand)
    {
        Vector2 smallCardHolderAnchorMaxTemp = SmallCardHolderPanelTransform.anchorMax;
        smallCardHolderAnchorMaxTemp.y = 1;
        SmallCardHolderPanelTransform.anchorMax = smallCardHolderAnchorMaxTemp;

        if (cardTypeToExpand == Card.CardType.Movement)
        {
            MovementCardsHolder.anchorMin = new Vector2(0, 0);
            MovementCardsHolder.anchorMax = new Vector2(1, 1);
            SupportCardsHolder.gameObject.SetActive(false);
        }
        else
        {
            SupportCardsHolder.anchorMin = new Vector2(0, 0);
            SupportCardsHolder.anchorMax = new Vector2(1, 1);
            MovementCardsHolder.gameObject.SetActive(false);
        }

        IsExpanded = true;
    }

    public void ShrinkHand()
    {
        Vector2 smallCardHolderAnchorMaxTemp = SmallCardHolderPanelTransform.anchorMax;
        smallCardHolderAnchorMaxTemp.y = 0.4f;
        SmallCardHolderPanelTransform.anchorMax = smallCardHolderAnchorMaxTemp;
        MovementCardsHolder.anchorMin = movementCardAnchorMinInitial;
        MovementCardsHolder.anchorMax = movementCardAnchorMaxInitial;
        MovementCardsHolder.gameObject.SetActive(true);
        SupportCardsHolder.anchorMin = supportCardAnchorMinInitial;
        SupportCardsHolder.anchorMax = supportCardAnchorMaxInitial;
        SupportCardsHolder.gameObject.SetActive(true);
        DeselectedSelectedCards();

        IsExpanded = false;
    }


    public void OnPointerClick(PointerEventData eventData)
    {
        if (IsExpanded)
        {
            ShrinkHand();
        }
    }

    private void DeselectedSelectedCards()
    {
        Transform movementCardParentTransform = movementCardsHolder.GetChild(0).GetChild(0).GetChild(0);
        Transform supportCardParentTransform = supportCardsHolder.GetChild(0).GetChild(0).GetChild(0);

        foreach (Transform child in movementCardParentTransform.transform)
        {
            Card theCard = child.GetComponent<Card>();

            if (theCard is not null)
            {
                if (theCard.CardIsSelected)
                {
                    theCard.DeselectCard();
                    break;
                }
            }
        }

        foreach (Transform child in supportCardParentTransform.transform)
        {
            Card theCard = child.GetComponent<Card>();

            if (theCard is not null)
            {
                if (theCard.CardIsSelected)
                {
                    theCard.DeselectCard();
                    break;
                }
            }
        }
    }

}
