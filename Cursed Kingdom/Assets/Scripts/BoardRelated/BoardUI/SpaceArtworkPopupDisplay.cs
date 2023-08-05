//Code written by Mohamed Riaz Khan of BukuGames.
//All code is written by me (Above name) unless otherwise stated via comments below.
//Not authorized for use outside of the Github repository of this game developed by BukuGames.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SpaceArtworkPopupDisplay : MonoBehaviour
{
    public GameObject iconPrefab;
    [SerializeField] private TextMeshProUGUI spaceTitle;
    [SerializeField] private TextMeshProUGUI spaceDescription;
    [SerializeField] private Space currentSpacePlayerIsOn;
    [SerializeField] private Image spaceArtwork;
    [SerializeField] private GameObject iconParent;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Coroutine currentCoroutine;
    [SerializeField] [Range(0,30)] private float timeToKeepPanelActive = 5f;

    public TextMeshProUGUI SpaceTitle { get => spaceTitle; set => spaceTitle = value; }
    public TextMeshProUGUI SpaceDescription { get => spaceDescription; set => spaceDescription = value; }
    public Space CurrentSpacePlayerIsOn { get => currentSpacePlayerIsOn; set => currentSpacePlayerIsOn = value; }
    public Image SpaceArtwork { get => spaceArtwork; set => spaceArtwork = value; }
    public GameObject IconParent { get => iconParent; set => iconParent = value; }
    public CanvasGroup CanvasGroup { get => canvasGroup; set => canvasGroup = value; }
    public Coroutine CurrentCoroutine { get => currentCoroutine; set => currentCoroutine = value; }
    public float TimeToKeepPanelActive { get => timeToKeepPanelActive; set => timeToKeepPanelActive = value; }

    public void TurnOnDisplay(Space spaceInfo)
    {
        CurrentSpacePlayerIsOn = spaceInfo;
        CanvasGroup.blocksRaycasts = true;
        CanvasGroup.alpha = 1f;
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

        int indexOfCurrentPlayer = CurrentSpacePlayerIsOn.gameplayManagerRef.Players.IndexOf(CurrentSpacePlayerIsOn.gameplayManagerRef.playerCharacter.GetComponent<Player>());
      //  CurrentSpacePlayerIsOn.gameplayManagerRef.Players[indexOfCurrentPlayer].HideHand();

        CurrentCoroutine = StartCoroutine(WaitTillTurnOff(CurrentSpacePlayerIsOn));
    }

    public void TurnOffDisplay(Space spaceInfo)
    {
        int indexOfCurrentPlayer = CurrentSpacePlayerIsOn.gameplayManagerRef.Players.IndexOf(CurrentSpacePlayerIsOn.gameplayManagerRef.playerCharacter.GetComponent<Player>());

        foreach (Transform child in IconParent.transform)
        {
            Destroy(child.gameObject);
        }
        CanvasGroup.blocksRaycasts = false;
        CanvasGroup.alpha = 0f;
        //spaceInfo.gameplayManagerRef.Players[indexOfCurrentPlayer].ShowHand();
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
