//Code written by Mohamed Riaz Khan of BukuGames.
//All code is written by me (Above name) unless otherwise stated via comments below.
//Not authorized for use outside of the Github repository of this game developed by BukuGames.
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SupportCardDuel : CardDuel
{
    //References
	[SerializeField] private SupportCard supportCardReference;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private Image supportCardTypeImage;

    public SupportCard SupportCardReference { get => supportCardReference; set => supportCardReference = value; }
    public TextMeshProUGUI DescriptionText { get => descriptionText; set => descriptionText = value; }
    public Image SupportCardTypeImage { get => supportCardTypeImage; set => supportCardTypeImage = value; }

    public SupportCardDuel(SupportCard suppCardRef)
	{
		SupportCardReference = suppCardRef;
        SetupCard(suppCardRef);
	}

	public override void SetupCard(Card cardReference)
	{
        base.SetupCard(cardReference);
        SupportCard suppCardRef = cardReference as SupportCard;
        if (suppCardRef != null)
        {
            SupportCardReference = suppCardRef;
            TitleText.text = suppCardRef.TitleText.text;
            DescriptionText.text = suppCardRef.DescriptionText.text;
            CardArtworkImage.sprite = suppCardRef.CardArtworkImage.sprite;
            SupportCardTypeImage.sprite = suppCardRef.SupportCardTypeImage.sprite;
            SupportCardTypeImage.color = suppCardRef.SupportCardTypeImage.color;
        }
    }

    public override void OnPointerClick(PointerEventData eventData)
    {
        if (DuelPhaseSMReference != null)
        {
            DuelPhaseSMReference.duelSelectCardsToUsePhaseState.DeselectSupportCard(SupportCardReference);
        }
    }
}
