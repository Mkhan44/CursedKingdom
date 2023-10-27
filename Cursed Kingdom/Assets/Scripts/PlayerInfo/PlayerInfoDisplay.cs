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
    [SerializeField] private TextMeshProUGUI curseTurnsText;
    [SerializeField] private TextMeshProUGUI poisonTurnsText;
    [SerializeField] private TextMeshProUGUI cooldownText;
    [SerializeField] private GameObject heartPrefab;


    //TEST
    public Sprite filledHeartSpriteTest;
    public Sprite brokenHeartSpriteTest;

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
    public TextMeshProUGUI CurseTurnsText { get => curseTurnsText; set => curseTurnsText = value; }
    public TextMeshProUGUI PoisonTurnsText { get => poisonTurnsText; set => poisonTurnsText = value; }
    public Image PoisonImage { get => poisonImage; set => poisonImage = value; }
    public TextMeshProUGUI CooldownText { get => cooldownText; set => cooldownText = value; }
    public GameObject HeartPrefab { get => heartPrefab; set => heartPrefab = value; }

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
        CurseTurnsText.text = string.Empty;
        PoisonTurnsText.text = string.Empty;
        CurseImage.color = new Color(CurseImage.color.r, CurseImage.color.g, CurseImage.color.b, 0f);
        PoisonImage.color = new Color(PoisonImage.color.r, PoisonImage.color.g, PoisonImage.color.b, 0f);

        if(HeartsHolder.transform.childCount < playerRef.MaxHealth)
        {
            for(int i = 0; i < playerRef.MaxHealth; i++)
            {
                Instantiate(HeartPrefab, HeartsHolder.transform);
            }
        }

        foreach (Transform child in HeartsHolder.transform)
        {
            Hearts.Add(child.GetComponent<Image>());
        }

        UpdatePlayerHealth();
        
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

    public void UpdateStatusEffect()
    {
        if(PlayerReference.IsCursed)
        {
            if(PlayerReference.CurseDuration > 0) 
            {
                CurseImage.color = new Color(CurseImage.color.r, CurseImage.color.g, CurseImage.color.b, 100f);
                PoisonImage.color = new Color(PoisonImage.color.r, PoisonImage.color.g, PoisonImage.color.b, 0f);
                CurseTurnsText.text = PlayerReference.CurseDuration.ToString();
            }
            else
            {
                CurseImage.color = new Color(CurseImage.color.r, CurseImage.color.g, CurseImage.color.b, 0f);
                PoisonImage.color = new Color(PoisonImage.color.r, PoisonImage.color.g, PoisonImage.color.b, 0f);
                CurseTurnsText.text = string.Empty;
            }
            
            return;
        }
        
        if(PlayerReference.IsPoisoned)
        {
            if(PlayerReference.PoisonDuration > 0)
            {
                PoisonImage.color = new Color(PoisonImage.color.r, PoisonImage.color.g, PoisonImage.color.b, 100f);
                CurseImage.color = new Color(CurseImage.color.r, CurseImage.color.g, CurseImage.color.b, 0f);
                PoisonTurnsText.text = PlayerReference.PoisonDuration.ToString();
            }
            else
            {
                PoisonImage.color = new Color(PoisonImage.color.r, PoisonImage.color.g, PoisonImage.color.b, 0f);
                CurseImage.color = new Color(CurseImage.color.r, CurseImage.color.g, CurseImage.color.b, 0f);
                PoisonTurnsText.text = string.Empty;
            }
          
            return;
        }

        Debug.LogWarning("Couldn't update status effect!");
    }

    public void UpdatePlayerHealth()
    {
        int numActiveHearts = 0;
        Color32 activeHeartColor = new Color(255f, 0f, 0f, 100f);
        Color32 inactiveHeartColor = new Color(255f, 255f, 255f, 0.28f);
        foreach(Image image in Hearts)
        {
            numActiveHearts++;
            if (numActiveHearts <= PlayerReference.CurrentHealth)
            {
                image.color = activeHeartColor;
             //   Debug.Log($"Player health is: {PlayerReference.CurrentHealth} and numActive hearts is: {numActiveHearts}");
            }
            else
            {
                image.color = inactiveHeartColor;
            }
            
        }
    }

    public void UpdateCooldownText()
    {
        if(PlayerReference.IsOnCooldown)
        {
            CooldownText.text = "Cooldown";
        }
        else
        {
            CooldownText.text = string.Empty;
        }
    }
}
