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
    [SerializeField] private Image spaceArtwork;
    [SerializeField] private GameObject iconParent;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] [Range(0,30)] private float timeToKeepPanelActive = 5f;

    public TextMeshProUGUI SpaceTitle { get => spaceTitle; set => spaceTitle = value; }
    public TextMeshProUGUI SpaceDescription { get => spaceDescription; set => spaceDescription = value; }
    public Image SpaceArtwork { get => spaceArtwork; set => spaceArtwork = value; }
    public GameObject IconParent { get => iconParent; set => iconParent = value; }
    public CanvasGroup CanvasGroup { get => canvasGroup; set => canvasGroup = value; }
    public float TimeToKeepPanelActive { get => timeToKeepPanelActive; set => timeToKeepPanelActive = value; }

    public void TurnOnDisplay(Space spaceInfo)
    {
        CanvasGroup.blocksRaycasts = true;
        CanvasGroup.alpha = 1f;
        SpaceTitle.text = spaceInfo.spaceData.spaceName;
        SpaceDescription.text = spaceInfo.spaceData.spaceDescription;
        SpaceArtwork.sprite = spaceInfo.spaceData.spaceSprite;

        int count = 0;
        foreach(SpaceData.SpaceEffect spaceEffect in spaceInfo.spaceData.spaceEffects)
        {
            GameObject newIcon = Instantiate(iconPrefab, IconParent.transform);
            Image iconImage = newIcon.transform.GetChild(0).GetComponent<Image>();
            TextMeshProUGUI iconText = newIcon.transform.GetChild(1).GetComponent<TextMeshProUGUI>();
            iconImage.sprite = spaceInfo.iconHolderParent.transform.GetChild(count).GetChild(0).GetComponent<Image>().sprite;
            iconText.text = spaceInfo.spaceData.spaceEffects[count].spaceEffectData.EffectDescription;
            count++;
        }
        spaceInfo.gameplayManagerRef.Players[0].HideHand();

        StartCoroutine(WaitTillTurnOff(spaceInfo));
    }

    public void TurnOffDisplay(Space spaceInfo)
    {
        foreach(Transform child in IconParent.transform)
        {
            Destroy(child.gameObject);
        }
        CanvasGroup.blocksRaycasts = false;
        CanvasGroup.alpha = 0f;
        spaceInfo.gameplayManagerRef.Players[0].ShowHand();
    }

    public IEnumerator WaitTillTurnOff(Space spaceInfo)
    {
        yield return new WaitForSeconds(TimeToKeepPanelActive);
        TurnOffDisplay(spaceInfo);
    }
}
