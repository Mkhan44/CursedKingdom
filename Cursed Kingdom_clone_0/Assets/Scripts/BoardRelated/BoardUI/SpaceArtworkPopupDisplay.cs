//Code written by Mohamed Riaz Khan of BukuGames.
//All code is written by me (Above name) unless otherwise stated via comments below.
//Not authorized for use outside of the Github repository of this game developed by BukuGames.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class SpaceArtworkPopupDisplay : MonoBehaviour
{
    //Events
    public event Action<Player> SpaceArtworkDisplayTurnOff;

    private const string ISACTIVE = "isActive";

    public GameObject iconPrefab;
    [SerializeField] private TextMeshProUGUI spaceTitle;
    [SerializeField] private TextMeshProUGUI spaceDescription;
    [SerializeField] private Space currentSpacePlayerIsOn;
    [SerializeField] private Player playerOnSpace;
    [SerializeField] private Image spaceArtwork;
    [SerializeField] private GameObject iconParent;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Coroutine currentCoroutine;
    [SerializeField] private Animator canvasAnimator;
    [SerializeField] [Range(0,30)] private float timeToKeepPanelActive = 5f;

    public TextMeshProUGUI SpaceTitle { get => spaceTitle; set => spaceTitle = value; }
    public TextMeshProUGUI SpaceDescription { get => spaceDescription; set => spaceDescription = value; }
    public Space CurrentSpacePlayerIsOn { get => currentSpacePlayerIsOn; set => currentSpacePlayerIsOn = value; }
    public Player PlayerOnSpace { get => playerOnSpace; set => playerOnSpace = value; }
    public Image SpaceArtwork { get => spaceArtwork; set => spaceArtwork = value; }
    public GameObject IconParent { get => iconParent; set => iconParent = value; }
    public CanvasGroup CanvasGroup { get => canvasGroup; set => canvasGroup = value; }
    public Coroutine CurrentCoroutine { get => currentCoroutine; set => currentCoroutine = value; }
    public float TimeToKeepPanelActive { get => timeToKeepPanelActive; set => timeToKeepPanelActive = value; }

    public void TurnOnDisplay(Space spaceInfo, Player playerOnSpace)
    {
        Space cachedSpace = spaceInfo;
        PlayerOnSpace = playerOnSpace;

        if (DebugModeSingleton.instance.IsDebugActive)
        {
            Space tempSpace = DebugModeSingleton.instance.OverrideSpaceLandEffect();

            if(tempSpace != null)
            {
                spaceInfo = tempSpace;
            }
        }

        CurrentSpacePlayerIsOn = spaceInfo;
        canvasAnimator.SetBool(ISACTIVE, true);
        SpaceTitle.text = CurrentSpacePlayerIsOn.spaceData.spaceName;
        SpaceDescription.text = CurrentSpacePlayerIsOn.spaceData.spaceDescription;
        SpaceArtwork.sprite = CurrentSpacePlayerIsOn.spaceData.spaceSprite;

        int count = 0;
        foreach(SpaceData.SpaceEffect spaceEffect in CurrentSpacePlayerIsOn.spaceData.spaceEffects)
        {
            GameObject newIcon = Instantiate(iconPrefab, IconParent.transform);
            Image iconImage = newIcon.transform.GetChild(0).GetComponent<Image>();
            TextMeshProUGUI iconText = newIcon.transform.GetChild(1).GetComponent<TextMeshProUGUI>();
            if(CurrentSpacePlayerIsOn.iconHolderParent.transform.childCount == CurrentSpacePlayerIsOn.spaceData.spaceEffects.Count)
            {
                iconImage.sprite = CurrentSpacePlayerIsOn.iconHolderParent.transform.GetChild(count).GetChild(0).GetComponent<Image>().sprite;
            }
            iconText.text = CurrentSpacePlayerIsOn.spaceData.spaceEffects[count].spaceEffectData.EffectDescription;
            count++;
        }

        CurrentCoroutine = StartCoroutine(WaitTillTurnOff(CurrentSpacePlayerIsOn));

        if(DebugModeSingleton.instance.IsDebugActive)
        {
            CurrentSpacePlayerIsOn = cachedSpace;
        }
    }

    public void TurnOffDisplay(Space spaceInfo)
    {

        foreach (Transform child in IconParent.transform)
        {
            Destroy(child.gameObject);
        }
        canvasAnimator.SetBool(ISACTIVE, false);
        SpaceArtworkDisplayTurnOff?.Invoke(PlayerOnSpace);

        if (PlayerOnSpace is not null)
        {
            //PlayerOnSpace.Animator.SetBool(Player.NEGATIVEEFFECT, false);
            //PlayerOnSpace.Animator.SetBool(Player.POSITIVEEFFECT, false);
            PlayerOnSpace = null;
        }

        
    }

    public void ExitCoroutineEarly()
    {
        if(CurrentCoroutine != null && CurrentSpacePlayerIsOn != null)
        {
            StopCoroutine(CurrentCoroutine);
            TurnOffDisplay(CurrentSpacePlayerIsOn);
        }
    }

    public IEnumerator WaitTillTurnOff(Space spaceInfo)
    {
        yield return new WaitForSeconds(TimeToKeepPanelActive);
        TurnOffDisplay(spaceInfo);
    }
}
