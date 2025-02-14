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
    [Tooltip("The child number of the gameobject (starting at 0) that the space artwork image is located in.")]
    [SerializeField] public int spaceArtworkChildNumber;

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
            TextMeshProUGUI spaceName = newElement.transform.GetComponentInChildren<TextMeshProUGUI>();
            Image spaceImage = newElement.transform.GetChild(spaceArtworkChildNumber).GetComponent<Image>();
            Button spaceButton = newElement.transform.GetChild(spaceArtworkChildNumber).GetComponent<Button>();
            spaceButton.onClick.AddListener(() => ChangeCameraFocus(space));
            spaceName.text = space.name;
            spaceImage.sprite = space.spaceData.spaceSprite;
        }
    }

    public void ChangeCameraFocus(Space spaceToFocusOn)
    {
        if (!spaceToFocusOn.gameplayManagerRef.cinemachineVirtualCameras[2].enabled)
        {
            spaceToFocusOn.gameplayManagerRef.ToggleGroundMapCamera();
        }

        spaceToFocusOn.gameplayManagerRef.mapManager.ChangeCurrentHighlightedSpaceGroundView(spaceToFocusOn.gameplayManagerRef.cinemachineVirtualCameras[2], spaceToFocusOn);
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
