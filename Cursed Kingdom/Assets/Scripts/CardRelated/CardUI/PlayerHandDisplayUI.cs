//Code written by Mohamed Riaz Khan of BukuGames.
//All code is written by me (Above name) unless otherwise stated via comments below.
//Not authorized for use outside of the Github repository of this game developed by BukuGames.

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PlayerHandDisplayUI : MonoBehaviour , IPointerClickHandler
{
    public const string active = "IsActive";
    public const string hidden = "IsHidden";
    public const string isHovered = "IsHovered";

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
        handUITransform.MovementAnimator = movementCardHolder.transform.parent.GetComponent<Animator>();
        handUITransform.SupportAnimator = supportCardHolder.transform.parent.GetComponent<Animator>();

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
            if(CurrentActiveTransform.MovementAnimator is not null)
            {
                CurrentActiveTransform.MovementAnimator.SetBool(active, true);
            }

            if (CurrentActiveTransform.SupportAnimator is not null)
            {
                CurrentActiveTransform.SupportAnimator.SetBool(hidden, true);
            }
                
            //CurrentActiveTransform.MovementCardsHolder.anchorMin = new Vector2(0, 0);
            //CurrentActiveTransform.MovementCardsHolder.anchorMax = new Vector2(1, 1);
            //CurrentActiveTransform.SupportCardsHolder.gameObject.SetActive(false);
        }
        else
        {

            if (CurrentActiveTransform.MovementAnimator is not null)
            {
                CurrentActiveTransform.MovementAnimator.SetBool(hidden, true);
            }

            if (CurrentActiveTransform.SupportAnimator is not null)
            {
                CurrentActiveTransform.SupportAnimator.SetBool(active, true);
            }
            
            //CurrentActiveTransform.SupportCardsHolder.anchorMin = new Vector2(0, 0);
            //CurrentActiveTransform.SupportCardsHolder.anchorMax = new Vector2(1, 1);
            //CurrentActiveTransform.MovementCardsHolder.gameObject.SetActive(false);
        }

        CurrentActiveTransform.IsExpanded = true;
    }

    public void ShrinkHand(bool waitForAnim = false)
    {
        if(CurrentActiveTransform is null)
        {
            return;
        }
        float animTime = 0f;
        if (CurrentActiveTransform.MovementAnimator is not null)
        {
            //Debug.LogWarning("WE'RE TESTING AN ANIMATION NAME:  Animation 0 in layer 0's name is: " + CurrentActiveTransform.MovementAnimator.runtimeAnimatorController.animationClips[0].name + " And it's length is: " + CurrentActiveTransform.MovementAnimator.runtimeAnimatorController.animationClips[0].length);
            animTime = CurrentActiveTransform.MovementAnimator.runtimeAnimatorController.animationClips[0].length;
            CurrentActiveTransform.MovementAnimator.SetBool(hidden, false);
            CurrentActiveTransform.MovementAnimator.SetBool(active, false);
        }

        if (CurrentActiveTransform.SupportAnimator is not null)
        {
            CurrentActiveTransform.SupportAnimator.SetBool(hidden, false);
            CurrentActiveTransform.SupportAnimator.SetBool(active, false);
        }


        if(waitForAnim)
        {
            StartCoroutine(WaitForAnimation(animTime));
        }
        else
        {
            StartCoroutine(WaitForAnimation());
        }
        //Vector2 smallCardHolderAnchorMaxTemp = SmallCardHolderPanelTransform.anchorMax;
        //smallCardHolderAnchorMaxTemp.y = 0.4f;
        //SmallCardHolderPanelTransform.anchorMax = smallCardHolderAnchorMaxTemp;
        //CurrentActiveTransform.MovementCardsHolder.anchorMin = CurrentActiveTransform.MovementCardAnchorMinInitial;
        //CurrentActiveTransform.MovementCardsHolder.anchorMax = CurrentActiveTransform.MovementCardAnchorMaxInitial;

        //CurrentActiveTransform.MovementCardsHolder.gameObject.SetActive(true);
        //CurrentActiveTransform.SupportCardsHolder.anchorMin = CurrentActiveTransform.SupportCardAnchorMinInitial;
        //CurrentActiveTransform.SupportCardsHolder.anchorMax = CurrentActiveTransform.SupportCardAnchorMaxInitial;

        //CurrentActiveTransform.SupportCardsHolder.gameObject.SetActive(true);

    }

    public IEnumerator WaitForAnimation(float waitTime = 0f)
    {
        if(waitTime == 0f)
        {
            DeselectedSelectedCards();
            CurrentActiveTransform.IsExpanded = false;
            yield return null;
        }
        else
        {
            //CHANGE THIS, IT'S NOT BASED IN UNSCALED TIME!
            yield return new WaitForSeconds(waitTime);
            DeselectedSelectedCards();
            CurrentActiveTransform.IsExpanded = false;
            CurrentActiveTransform.MovementCardsHolder.gameObject.SetActive(false);
            CurrentActiveTransform.SupportCardsHolder.gameObject.SetActive(false);
        }
        
    }


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
                    theCard.DeselectForDiscard();
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
    [SerializeField] private Animator movementAnimator;
    [SerializeField] private Animator supportAnimator;

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
    public Animator MovementAnimator { get => movementAnimator; set => movementAnimator = value; }
    public Animator SupportAnimator { get => supportAnimator; set => supportAnimator = value; }
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
