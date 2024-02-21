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
    private const string ISPOISONED = "isPoisoned";
    private const string ISCURSED = "isCursed";

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
    [SerializeField] private Animator animator;

    //Preset values
    [Header("Presets")]
    private PlayerInfoIconPreset.InfoIconElement poisonElement;
    private PlayerInfoIconPreset.InfoIconElement curseElement;
    private PlayerInfoIconPreset.InfoIconElement movementElement;
    private PlayerInfoIconPreset.InfoIconElement supportElement;
    private PlayerInfoIconPreset.InfoIconElement heartElement;


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
    public Animator Animator { get => animator; set => animator = value; }

    public void SetupPlayerInfo(Player playerRef)
    {
        PlayerReference = playerRef;
        PlayerNameText.text = "TestUsername";
        PlayerClassText.text = playerRef.ClassData.classType.ToString();
        PlayerLevelText.text = "Level: " + playerRef.CurrentLevel.ToString();
        PlayerMovementCardsInHandText.text = playerRef.MovementCardsInHandCount.ToString();
        PlayerSupportCardsInHandText.text = playerRef.SupportCardsInHandCount.ToString();
        PlayerPortraitImage.sprite = playerRef.ClassData.defaultPortraitImage;
        CooldownText.text = string.Empty;
        CurseTurnsText.text = string.Empty;
        PoisonTurnsText.text = string.Empty;

        poisonElement = GrabPresetElementFromSingleton(PlayerInfoIconPreset.InfoIconElement.InfoIconType.poisonIcon);
        curseElement = GrabPresetElementFromSingleton(PlayerInfoIconPreset.InfoIconElement.InfoIconType.curseicon);
        movementElement = GrabPresetElementFromSingleton(PlayerInfoIconPreset.InfoIconElement.InfoIconType.movementCardIcon);
        supportElement = GrabPresetElementFromSingleton(PlayerInfoIconPreset.InfoIconElement.InfoIconType.supportCardIcon);
        heartElement = GrabPresetElementFromSingleton(PlayerInfoIconPreset.InfoIconElement.InfoIconType.heartIcon);

        CurseImage.color = curseElement.InactiveColor;
        PoisonImage.color = poisonElement.InactiveColor;

        if(HeartsHolder.transform.childCount < playerRef.MaxHealth)
        {
            for(int i = 0; i < playerRef.MaxHealth; i++)
            {
                GameObject tempObj = Instantiate(HeartPrefab, HeartsHolder.transform);
                Image image = tempObj.GetComponent<Image>();
                if(heartElement != null)
                {
                    image.sprite = heartElement.ActiveSprite;
                }
            }
        }

        foreach (Transform child in HeartsHolder.transform)
        {
            Hearts.Add(child.GetComponent<Image>());
        }

        UpdatePlayerHealth();
        
    }

    private PlayerInfoIconPreset.InfoIconElement GrabPresetElementFromSingleton(PlayerInfoIconPreset.InfoIconElement.InfoIconType infoIconTypeToFind)
    {
        PlayerInfoIconPreset.InfoIconElement infoIconElementToReturn = default;

        foreach (PlayerInfoIconPreset.InfoIconElement iconElement in IconPresetsSingleton.instance.PlayerInfoIconPreset.InfoIconElements)
        {
            if (iconElement.InfoIconType1 == infoIconTypeToFind)
            {
                infoIconElementToReturn = iconElement;
                break;
            }
        }

        return infoIconElementToReturn;
    }

    public void UpdateCardTotals()
    {
        PlayerMovementCardsInHandText.text = PlayerReference.MovementCardsInHandCount.ToString();
        PlayerSupportCardsInHandText.text = PlayerReference.SupportCardsInHandCount.ToString();
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
                CurseImage.color = curseElement.ActiveColor;
                PoisonImage.color = poisonElement.InactiveColor;
                CurseTurnsText.text = PlayerReference.CurseDuration.ToString();
                animator.SetBool(ISCURSED, true);
            }
            else
            {
                CurseImage.color = curseElement.InactiveColor;
                PoisonImage.color = poisonElement.InactiveColor;
                CurseTurnsText.text = string.Empty;
                animator.SetBool(ISCURSED, false);
            }
            
            return;
        }
        
        if(PlayerReference.IsPoisoned)
        {
            if(PlayerReference.PoisonDuration > 0)
            {
                PoisonImage.color = poisonElement.ActiveColor;
                CurseImage.color = curseElement.InactiveColor;
                PoisonTurnsText.text = PlayerReference.PoisonDuration.ToString();
                animator.SetBool(ISPOISONED, true);
            }
            else
            {
                PoisonImage.color = poisonElement.InactiveColor;
                CurseImage.color = curseElement.InactiveColor;
                PoisonTurnsText.text = string.Empty;
                animator.SetBool(ISPOISONED, false);
            }
          
            return;
        }

        Debug.LogWarning("Couldn't update status effect!");
    }

    public void UpdatePlayerHealth()
    {
        int numActiveHearts = 0;
        foreach(Image image in Hearts)
        {
            numActiveHearts++;
            if (numActiveHearts <= PlayerReference.CurrentHealth)
            {
                image.color = heartElement.ActiveColor;
             //   Debug.Log($"Player health is: {PlayerReference.CurrentHealth} and numActive hearts is: {numActiveHearts}");
            }
            else
            {
                image.color = heartElement.InactiveColor;
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
