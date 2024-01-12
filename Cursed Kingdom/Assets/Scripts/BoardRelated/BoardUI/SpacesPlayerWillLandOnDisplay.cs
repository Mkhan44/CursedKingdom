//Code written by Mohamed Riaz Khan of BukuGames.
//All code is written by me (Above name) unless otherwise stated via comments below.
//Not authorized for use outside of the Github repository of this game developed by BukuGames.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class SpacesPlayerWillLandOnDisplay : MonoBehaviour
{
    public GameObject elementPrefab;
    public GameObject contentArea;
    public TextMeshProUGUI mainText;

    private void Start()
    {
        TurnOffDisplay();
    }
    public void TurnOnDisplay(List<Space> spacesPlayerCanLandOn)
    {
        this.gameObject.SetActive(true);
        SpawnElements(spacesPlayerCanLandOn);
    }

    public void TurnOffDisplay()
    {
        this.gameObject.SetActive( false);
    }
    public void SpawnElements(List<Space> spacesPlayerCanLandOn)
    {
        RefreshElements();
        string textToDisplay = string.Empty;
        if (spacesPlayerCanLandOn.Count == 1)
        {
            textToDisplay = $"You will land on:";
        }
        else
        {
            textToDisplay = $"You may land on:";
        }

        ChangeMainText(textToDisplay);

        foreach (Space space in spacesPlayerCanLandOn)
        {
            GameObject newElement = Instantiate(elementPrefab, contentArea.transform);
            TextMeshProUGUI spaceName = newElement.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
            Image spaceImage = newElement.transform.GetChild(1).GetComponent<Image>();
            spaceName.text = space.name;
            spaceImage.sprite = space.spaceData.spaceSprite;
        }
    }

    public void RefreshElements()
    {
        foreach(Transform child in contentArea.transform)
        {
            Destroy(child.gameObject);
        }
    }
    private void ChangeMainText(string text = "")
    {
        mainText.text = text;
    }
}
