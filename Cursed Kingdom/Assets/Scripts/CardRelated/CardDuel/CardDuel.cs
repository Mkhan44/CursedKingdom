//Code written by Mohamed Riaz Khan of BukuGames.
//All code is written by me (Above name) unless otherwise stated via comments below.
//Not authorized for use outside of the Github repository of this game developed by BukuGames.
using System.Collections;
using System.Collections.Generic;
using PurrNet;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CardDuel : NetworkBehaviour , IPointerClickHandler
{
	protected const string ISHIDDEN = "IsHidden";

	//Animation string parameters
	protected const string ISCURSED = "isCursed";
	protected const string ISBOOSTED = "isBoosted";
	public enum CardType
	{
		Movement,
		Support,
		Both,
		None,
	}
	
	[SerializeField] private DuelPhaseSM duelPhaseSMReference;
	[SerializeField] private TextMeshProUGUI titleText;
	[SerializeField] private Image cardArtworkImage;
	[SerializeField] private Image cardBorderImage;
	[SerializeField] private CardType thisCardType;
	[SerializeField] private Button clickableAreaButton;
	[SerializeField] private Image clickableAreaImage;
	[SerializeField] private GameplayManager gameplayManager;
	[SerializeField] private Image backgroundSelectedGlow;
	[SerializeField] private Animator parentAnimator;
	[SerializeField] private Animator textureAnimator;

	[SerializeField] private bool cardIsActiveHovered;
	[SerializeField] private bool selectedForDiscard;
	[SerializeField] private bool selectedForUse;
	[SerializeField] private Vector3 originalSize;
	[SerializeField] private Vector3 hoveredSize;
	[SerializeField] private Color originalBackgroundGlowColor;
	[SerializeField] private Animator cardAnimator;
	[SerializeField] private CanvasGroup cardCanvasGroup;

    [SerializeField] private bool isClickable;

    public DuelPhaseSM DuelPhaseSMReference { get => duelPhaseSMReference; set => duelPhaseSMReference = value; }
	public TextMeshProUGUI TitleText { get => titleText; set => titleText = value; }
	public Image CardArtworkImage { get => cardArtworkImage; set => cardArtworkImage = value; }
	public Image CardBorderImage { get => cardBorderImage; set => cardBorderImage = value; }
	public CardType ThisCardType { get => thisCardType; }
	public Button ClickableAreaButton { get => clickableAreaButton; set => clickableAreaButton = value; }
    public Image ClickableAreaImage { get => clickableAreaImage; set => clickableAreaImage = value; }
	public Image BackgroundSelectedGlow { get => backgroundSelectedGlow; set => backgroundSelectedGlow = value; }
	public Animator ParentAnimator { get => parentAnimator; set => parentAnimator = value; }
	public Animator TextureAnimator { get => textureAnimator; set => textureAnimator = value; }
	public bool CardIsActiveHovered { get => cardIsActiveHovered; set => cardIsActiveHovered = value; }
	public bool SelectedForDiscard { get => selectedForDiscard; set => selectedForDiscard = value; }
	public bool SelectedForUse { get => selectedForUse; set => selectedForUse = value; }
	public GameplayManager GameplayManager { get => gameplayManager; set => gameplayManager = value; }
	public Vector3 OriginalSize { get => originalSize; set => originalSize = value; }
	public Vector3 HoveredSize { get => hoveredSize; set => hoveredSize = value; }
	public Color OriginalBackgroundGlowColor { get => originalBackgroundGlowColor; set => originalBackgroundGlowColor = value; }
    public bool IsClickable { get => isClickable; set => isClickable = value; }
    public Animator CardAnimator { get => cardAnimator; set => cardAnimator = value; }
    public CanvasGroup CardCanvasGroup { get => cardCanvasGroup; set => cardCanvasGroup = value; }

    public virtual void SetupCard(Card cardReference)
    {
		TitleText.text = cardReference.TitleText.text;
		CardArtworkImage.sprite = cardReference.CardArtworkImage.sprite;
		IsClickable = true;
		if(DuelPhaseSMReference.GetCurrentState() == DuelPhaseSMReference.duelSelectCardsToUsePhaseState)
		{
			CardAnimator.enabled = false;
			CardCanvasGroup.alpha = 1.0f;
        }
    }
    public virtual void OnPointerClick(PointerEventData eventData)
	{
		
	}
}
