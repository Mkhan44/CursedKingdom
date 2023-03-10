//Code written by Mohamed Riaz Khan of BukuGames.
//All code is written by me (Above name) unless otherwise stated via comments below.
//Not authorized for use outside of the Github repository of this game developed by BukuGames.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PlayerInfoDisplay : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TextMeshProUGUI playerClassText;
    [SerializeField] private TextMeshProUGUI playerNameText;
    [SerializeField] private TextMeshProUGUI playerLevelText;
    [SerializeField] private TextMeshProUGUI playerMovementCardsInHandText;
    [SerializeField] private TextMeshProUGUI playerSupportCardsInHandText;
    [SerializeField] private Image playerPortraitImage;
    [SerializeField] private GameObject heartsHolder;
    [SerializeField] private List<Image> hearts;
    [SerializeField] private Image curseImage;
    [SerializeField] private Image poisonImage;
    [SerializeField] private TextMeshProUGUI cooldownText;


    //TEST
    public Sprite filledHeartSpriteTest;

    private Player playerReference;

    public TextMeshProUGUI PlayerNameText { get => playerNameText; set => playerNameText = value; }
    public TextMeshProUGUI PlayerClassText { get => playerClassText; set => playerClassText = value; }
    public TextMeshProUGUI PlayerLevelText { get => playerLevelText; set => playerLevelText = value; }
    public TextMeshProUGUI PlayerMovementCardsInHandText { get => playerMovementCardsInHandText; set => playerMovementCardsInHandText = value; }
    public TextMeshProUGUI PlayerSupportCardsInHandText { get => playerSupportCardsInHandText; set => playerSupportCardsInHandText = value; }
    public Image PlayerPortraitImage { get => playerPortraitImage; set => playerPortraitImage = value; }
    public GameObject HeartsHolder { get => heartsHolder; set => heartsHolder = value; }
    public List<Image> Hearts { get => hearts; set => hearts = value; }
    public Player PlayerReference { get => playerReference; set => playerReference = value; }
    public Image CurseImage { get => curseImage; set => curseImage = value; }
    public Image PoisonImage { get => poisonImage; set => poisonImage = value; }
    public TextMeshProUGUI CooldownText { get => cooldownText; set => cooldownText = value; }

    public void SetupPlayerInfo(Player playerRef)
    {
        PlayerReference = playerRef;
        PlayerNameText.text = "TestUsername";
        PlayerClassText.text = playerRef.ClassData.classType.ToString();
        PlayerLevelText.text = "Level: " + playerRef.CurrentLevel.ToString();
        PlayerMovementCardsInHandText.text = playerRef.MovementCardsInHand.ToString();
        PlayerSupportCardsInHandText.text = playerRef.SupportCardsInHand.ToString();
        PlayerPortraitImage.sprite = playerRef.ClassData.defaultPortraitImage;
        CooldownText.text = string.Empty;
        CurseImage.color = new Color(CurseImage.color.r, CurseImage.color.g, CurseImage.color.b, 0f);
        PoisonImage.color = new Color(PoisonImage.color.r, PoisonImage.color.g, PoisonImage.color.b, 0f);

        foreach (Transform child in HeartsHolder.transform)
        {
            hearts.Add(child.GetComponent<Image>());
        }

        if(playerRef.CurrentHealth <= hearts.Count)
        {
            for (int i = 0; i < playerRef.CurrentHealth; i++)
            {
                hearts[i].sprite = filledHeartSpriteTest;
            }
        }
    }

    public void UpdateCardTotals()
    {
        PlayerMovementCardsInHandText.text = PlayerReference.MovementCardsInHand.ToString();
        PlayerSupportCardsInHandText.text = PlayerReference.SupportCardsInHand.ToString();
    }

    public void UpdateLevel()
    {
        PlayerLevelText.text = "Level: " + PlayerReference.CurrentLevel.ToString();
    }
}
