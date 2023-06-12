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
    [SerializeField] private List<HandUITransform> handUITransforms = new();
    [SerializeField] private RectTransform smallCardHolderPanelTransform;
    [SerializeField] private HandUITransform currentActiveTransform;

    public List<HandUITransform> HandUITransforms { get => handUITransforms; set => handUITransforms = value; }
    public RectTransform SmallCardHolderPanelTransform { get => smallCardHolderPanelTransform; set => smallCardHolderPanelTransform = value; }
    public HandUITransform CurrentActiveTransform { get => currentActiveTransform; set => currentActiveTransform = value; }

    public void AddNewHandUI(RectTransform movementCardHolder, RectTransform supportCardHolder)
    {
        HandUITransform handUITransform = new();


        handUITransform.MovementCardsHolder = movementCardHolder;
        handUITransform.SupportCardsHolder = supportCardHolder;
        handUITransform.MovementLayoutGroup = movementCardHolder.GetComponent<HorizontalLayoutGroup>();
        handUITransform.SupportLayoutGroup = supportCardHolder.GetComponent<HorizontalLayoutGroup>();


        handUITransform.IsExpanded = false;
        handUITransform.MovementCardAnchorMaxInitial = handUITransform.MovementCardsHolder.anchorMax;
        handUITransform.MovementCardAnchorMinInitial = handUITransform.MovementCardsHolder.anchorMin;
        handUITransform.SupportCardAnchorMaxInitial = handUITransform.SupportCardsHolder.anchorMax;
        handUITransform.SupportCardAnchorMinInitial = handUITransform.SupportCardsHolder.anchorMin;
        handUITransform.SmallCardHolderPanelAnchorMaxInitial = SmallCardHolderPanelTransform.anchorMax;
        handUITransform.SmallCardHolderPanelAnchorMinInitial = SmallCardHolderPanelTransform.anchorMin;
        handUITransform.MovementLayoutGroupInitialSpacing = handUITransform.MovementLayoutGroup.spacing;
        handUITransform.SupportLayoutGroupInitialSpacing = handUITransform.SupportLayoutGroup.spacing;

        HandUITransforms.Add(handUITransform);
    }

    //For now this just uses the "Players" list index. We want this in the future to use the actual playerID to identify the player.
    public void SetCurrentActiveHandUI(int playerIndex)
    {
        if(playerIndex <= HandUITransforms.Count)
        {
            if(CurrentActiveTransform != null)
            {
                ShrinkHand();
            }
            CurrentActiveTransform = HandUITransforms[playerIndex];
        }
        else
        {
            Debug.LogWarning("HEY WE COULDN'T FIND THE PLAYER INDEX! It is: " + playerIndex + " and handUITransformsCount is: " + HandUITransforms.Count);
        }
    }


    public void ExpandHand(Card.CardType cardTypeToExpand, int playerIndex)
    {
        Vector2 smallCardHolderAnchorMaxTemp = SmallCardHolderPanelTransform.anchorMax;
        smallCardHolderAnchorMaxTemp.y = 1;
        SmallCardHolderPanelTransform.anchorMax = smallCardHolderAnchorMaxTemp;

        if (cardTypeToExpand == Card.CardType.Movement)
        {
            CurrentActiveTransform.MovementCardsHolder.anchorMin = new Vector2(0, 0);
            CurrentActiveTransform.MovementCardsHolder.anchorMax = new Vector2(1, 1);
            CurrentActiveTransform.MovementLayoutGroup.spacing = -100f;
            CurrentActiveTransform.SupportCardsHolder.gameObject.SetActive(false);
        }
        else
        {
            CurrentActiveTransform.SupportCardsHolder.anchorMin = new Vector2(0, 0);
            CurrentActiveTransform.SupportCardsHolder.anchorMax = new Vector2(1, 1);
            CurrentActiveTransform.SupportLayoutGroup.spacing = -100f;
            CurrentActiveTransform.MovementCardsHolder.gameObject.SetActive(false);
        }

        CurrentActiveTransform.IsExpanded = true;
    }

    public void ShrinkHand()
    {
        Vector2 smallCardHolderAnchorMaxTemp = SmallCardHolderPanelTransform.anchorMax;
        smallCardHolderAnchorMaxTemp.y = 0.4f;
        SmallCardHolderPanelTransform.anchorMax = smallCardHolderAnchorMaxTemp;
        CurrentActiveTransform.MovementCardsHolder.anchorMin = CurrentActiveTransform.MovementCardAnchorMinInitial;
        CurrentActiveTransform.MovementCardsHolder.anchorMax = CurrentActiveTransform.MovementCardAnchorMaxInitial;
        CurrentActiveTransform.MovementLayoutGroup.spacing = CurrentActiveTransform.MovementLayoutGroupInitialSpacing;
        CurrentActiveTransform.MovementCardsHolder.gameObject.SetActive(true);
        CurrentActiveTransform.SupportCardsHolder.anchorMin = CurrentActiveTransform.SupportCardAnchorMinInitial;
        CurrentActiveTransform.SupportCardsHolder.anchorMax = CurrentActiveTransform.SupportCardAnchorMaxInitial;
        CurrentActiveTransform.SupportLayoutGroup.spacing = CurrentActiveTransform.SupportLayoutGroupInitialSpacing;
        CurrentActiveTransform.SupportCardsHolder.gameObject.SetActive(true);
        DeselectedSelectedCards();

        CurrentActiveTransform.IsExpanded = false;
    }

    //Debug tests
    public void SetLeft(RectTransform rt, float left)
    {
        rt.offsetMin = new Vector2(left, rt.offsetMin.y);
    }

    public void SetRight(RectTransform rt, float right)
    {
        rt.offsetMax = new Vector2(-right, rt.offsetMax.y);
    }

    public void SetTop(RectTransform rt, float top)
    {
        rt.offsetMax = new Vector2(rt.offsetMax.x, -top);
    }

    public void SetBottom(RectTransform rt, float bottom)
    {
        rt.offsetMin = new Vector2(rt.offsetMin.x, bottom);
    }
    //debug tests


    public void OnPointerClick(PointerEventData eventData)
    {
        if (CurrentActiveTransform.IsExpanded)
        {
            ShrinkHand();
        }
    }

    private void DeselectedSelectedCards()
    {
        Transform movementCardParentTransform = CurrentActiveTransform.MovementCardsHolder;
        Transform supportCardParentTransform = CurrentActiveTransform.SupportCardsHolder;

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

public class HandUITransform
{
    [SerializeField] private RectTransform parentCardHolder;
    [SerializeField] private RectTransform movementCardsHolder;
    [SerializeField] private RectTransform supportCardsHolder;

    [SerializeField] private Vector2 movementCardAnchorMaxInitial;
    [SerializeField] private Vector2 movementCardAnchorMinInitial;
    [SerializeField] private Vector2 supportCardAnchorMaxInitial;
    [SerializeField] private Vector2 supportCardAnchorMinInitial;
    [SerializeField] private Vector2 smallCardHolderPanelAnchorMaxInitial;
    [SerializeField] private Vector2 smallCardHolderPanelAnchorMinInitial;
    [SerializeField] private float smallPaddingSizeInitial;
    [SerializeField] private float expandedPaddingSizeInitial;
    [SerializeField] private HorizontalLayoutGroup movementLayoutGroup;
    [SerializeField] private float movementLayoutGroupInitialSpacing;
    [SerializeField] private HorizontalLayoutGroup supportLayoutGroup;
    [SerializeField] private float supportLayoutGroupInitialSpacing;

    [SerializeField] private bool isExpanded;

   
    public RectTransform ParentCardHolder { get => parentCardHolder; set => parentCardHolder = value; }
    public RectTransform MovementCardsHolder { get => movementCardsHolder; set => movementCardsHolder = value; }
    public RectTransform SupportCardsHolder { get => supportCardsHolder; set => supportCardsHolder = value; }
    public Vector2 MovementCardAnchorMaxInitial { get => movementCardAnchorMaxInitial; set => movementCardAnchorMaxInitial = value; }
    public Vector2 MovementCardAnchorMinInitial { get => movementCardAnchorMinInitial; set => movementCardAnchorMinInitial = value; }
    public Vector2 SupportCardAnchorMaxInitial { get => supportCardAnchorMaxInitial; set => supportCardAnchorMaxInitial = value; }
    public Vector2 SupportCardAnchorMinInitial { get => supportCardAnchorMinInitial; set => supportCardAnchorMinInitial = value; }
    public Vector2 SmallCardHolderPanelAnchorMaxInitial { get => smallCardHolderPanelAnchorMaxInitial; set => smallCardHolderPanelAnchorMaxInitial = value; }
    public Vector2 SmallCardHolderPanelAnchorMinInitial { get => smallCardHolderPanelAnchorMinInitial; set => smallCardHolderPanelAnchorMinInitial = value; }
    public float SmallPaddingSizeInitial { get => smallPaddingSizeInitial; set => smallPaddingSizeInitial = value; }
    public float ExpandedPaddingSizeInitial { get => expandedPaddingSizeInitial; set => expandedPaddingSizeInitial = value; }
    public HorizontalLayoutGroup MovementLayoutGroup { get => movementLayoutGroup; set => movementLayoutGroup = value; }
    public float MovementLayoutGroupInitialSpacing { get => movementLayoutGroupInitialSpacing; set => movementLayoutGroupInitialSpacing = value; }
    public HorizontalLayoutGroup SupportLayoutGroup { get => supportLayoutGroup; set => supportLayoutGroup = value; }
    public float SupportLayoutGroupInitialSpacing { get => supportLayoutGroupInitialSpacing; set => supportLayoutGroupInitialSpacing = value; }
    public bool IsExpanded { get => isExpanded; set => isExpanded = value; }
}
