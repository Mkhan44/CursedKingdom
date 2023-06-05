using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueBoxPopup : MonoBehaviour
{
    public static DialogueBoxPopup instance;

    [SerializeField] private TextMeshProUGUI displayText;
    [SerializeField] private GameObject blockerPanel;
    [SerializeField] private GameObject buttonLayoutParent;
    [SerializeField] private GameObject buttonPrefab;
    [SerializeField] private List<Button> buttonOptions;
    bool isActive;

    private void Awake()
    {
        blockerPanel.SetActive(true);
        blockerPanel.SetActive(false);
        instance = this;
        isActive = false;
        buttonOptions = new();
    }

    public void ActivatePopup(string textToDisplay, int buttonsToSpawn)
    {
        displayText.text = textToDisplay;

        for (int i = 0; i <buttonsToSpawn; i++)
        {
            GameObject buttonInstance = Instantiate(buttonPrefab, buttonLayoutParent.transform);
        }

        //Add debug close button.
        GameObject buttonInstanceFinal = Instantiate(buttonPrefab, buttonLayoutParent.transform);
        buttonInstanceFinal.GetComponent<Button>().onClick.AddListener(DeactivatePopup);
        buttonInstanceFinal.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Close DEBUG";
        
        isActive = true;
        blockerPanel.SetActive(true);
    }

    public void DeactivatePopup()
    {
        isActive = false;
        blockerPanel.SetActive(false);
        foreach(Transform child in buttonLayoutParent.transform)
        {
            Destroy(child.gameObject);
        }
        buttonOptions.Clear();
    }
}
