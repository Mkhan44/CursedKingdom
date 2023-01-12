//Code written by Mohamed Riaz Khan of BukuGames.
//All code is written by me (Above name) unless otherwise stated via comments below.
//Not authorized for use outside of the Github repository of this game developed by BukuGames.

using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SupportCard : Card
{
    //References
    [SerializeField] private TextMeshProUGUI descriptionText;


    //Data
    [SerializeField] private SupportCardData supportCardData;

    public TextMeshProUGUI DescriptionText { get => descriptionText; set => descriptionText = value; }
    public SupportCardData SupportCardData { get => supportCardData; set => supportCardData = value; }


    public SupportCard()
    {
        SetCardType(CardType.Support);
    }
}
