//Code written by Mohamed Riaz Khan of BukuGames.
//All code is written by me (Above name) unless otherwise stated via comments below.
//Not authorized for use outside of the Github repository of this game developed by BukuGames.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using PurrNet;

public class TopDownMapDisplay : NetworkBehaviour
{
    [SerializeField] private GameObject topDownSpaceInfoPanel;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private GameObject iconsLayoutObj;
    [SerializeField] private GameObject iconDescriptionPrefab;

    public GameObject TopDownSpaceInfoPanel { get => topDownSpaceInfoPanel; set => topDownSpaceInfoPanel = value; }
    public TextMeshProUGUI TitleText { get => titleText; set => titleText = value; }
    public TextMeshProUGUI DescriptionText { get => descriptionText; set => descriptionText = value; }
    public GameObject IconsLayoutObj { get => iconsLayoutObj; set => iconsLayoutObj = value; }
    public GameObject IconDescriptionPrefab { get => iconDescriptionPrefab; set => iconDescriptionPrefab = value; }

    public void UpdateInformation(Space space)
    {
        if (space == null)
        {
            return;
        }

        this.gameObject.SetActive(true);
        UpdateInformationPriv(space);
    }

    private void UpdateInformationPriv(Space space)
    {
        PopulateIconLayoutChildren(space);
        TitleText.text = space.spaceData.spaceName;
        DescriptionText.text = space.spaceData.spaceDescription;
    }

    public void CloseMapDisplayPanel()
    {
        this.gameObject.SetActive(false);
    }

    private void PopulateIconLayoutChildren(Space space)
    {
        ClearIconLayoutChildren();
        int count = 0;
        foreach(SpaceData.SpaceEffect spaceEffect in space.spaceData.spaceEffects)
        {
            GameObject newIcon = Instantiate(IconDescriptionPrefab, IconsLayoutObj.transform);
            newIcon.transform.SetParent(IconsLayoutObj.transform);
            Image iconImage = newIcon.transform.GetChild(0).GetComponent<Image>();
            TextMeshProUGUI iconDescription = newIcon.transform.GetChild(1).GetComponent<TextMeshProUGUI>();
            iconImage.sprite = space.iconHolderParent.transform.GetChild(count).GetChild(0).GetComponent<Image>().sprite;
            count += 1;
            iconDescription.text = spaceEffect.spaceEffectData.EffectDescription;
        }
    }

    private void ClearIconLayoutChildren()
    {
        foreach (Transform child in IconsLayoutObj.transform)
        {
            Destroy(child.gameObject);
        }
    }
}
