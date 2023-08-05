using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueBoxPopup : MonoBehaviour
{
    public static DialogueBoxPopup instance;

    public static Action optionChosen;
    public delegate void CalledDelegate();

    [SerializeField] private TextMeshProUGUI displayText;
    [SerializeField] private GameObject blockerPanel;
    [SerializeField] private GameObject buttonLayoutParent;
    [SerializeField] private GameObject imageLayoutParent;
    [SerializeField] private GameObject buttonPrefab;
    [SerializeField] private GameObject imagePrefab;
    [SerializeField] private List<Button> buttonOptions;

    public GameObject ButtonLayoutParent { get => buttonLayoutParent; set => buttonLayoutParent = value; }
    public GameObject ImageLayoutParent { get => imageLayoutParent; set => imageLayoutParent = value; }

    private void Awake()
    {
        blockerPanel.SetActive(true);
        blockerPanel.SetActive(false);
        instance = this;
        buttonOptions = new();
    }

    /// <summary>
    /// A popup with just text that stays up until the user completes the action needed.
    /// </summary>
    /// <param name="textToDisplay"></param>
    public void ActivatePopupWithJustText(string textToDisplay)
    {
        displayText.text = textToDisplay;
        ImageLayoutParent.SetActive(false);
        ButtonLayoutParent.SetActive(false);

        blockerPanel.SetActive(true);
        //Need a way to call Deactivate and also the action afterwards.
    }

    /// <summary>
    /// A popup with just a message and a button to close it.
    /// </summary>
    /// <param name="textToDisplay"></param>
    public void ActivatePopupWithConfirmation(string textToDisplay, string closeButtonText = "Okay")
    {
        displayText.text = textToDisplay;
        ImageLayoutParent.SetActive(false);

        GameObject buttonInstanceFinal = Instantiate(buttonPrefab, ButtonLayoutParent.transform);
        buttonInstanceFinal.GetComponent<Button>().onClick.AddListener(DeactivatePopup);
        buttonInstanceFinal.GetComponent<Button>().onClick.AddListener(chooseOption);
        buttonInstanceFinal.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = closeButtonText;

        blockerPanel.SetActive(true);
    }

    public void ActivatePopupWithImageChoices(string textToDisplay, List<Image> imagesToSpawn)
    {
        displayText.text = textToDisplay;
        ButtonLayoutParent.SetActive(false);

        //if (buttonsToSpawn > 0) 
        //{
        //    for (int i = 0; i < buttonsToSpawn; i++)
        //    {
        //        GameObject buttonInstance = Instantiate(buttonPrefab, ButtonLayoutParent.transform);
        //        Button theButton = buttonInstance.GetComponent<Button>();
        //    }
        //}
        //else
        //{
        //    Add debug close button.
        //    GameObject buttonInstanceFinal = Instantiate(buttonPrefab, ButtonLayoutParent.transform);
        //    buttonInstanceFinal.GetComponent<Button>().onClick.AddListener(DeactivatePopup);
        //    buttonInstanceFinal.GetComponent<Button>().onClick.AddListener(chooseOption);
        //    buttonInstanceFinal.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Close DEBUG";
        //}
        

        blockerPanel.SetActive(true);
    }
    /// <summary>
    /// A method that displays a popup with button options. For the button setup each item in the list: 
    /// The TEXT on the button, METHOD NAME you want to call, and OBJECT you are calling this from need to be populated respectively.
    /// </summary>
    /// <param name="textToDisplay"></param>
    /// <param name="buttonSetupParams">A tuple where for each item in the list: T1 = The text on the button, T2 = the method name you want to call, and T3 is the object from which this popup is being called from.</param>
    public void ActivatePopupWithButtonChoices(string textToDisplay, List<Tuple<string, string, object>> buttonSetupParams )
    {
        displayText.text = textToDisplay;
        ImageLayoutParent.SetActive(false);

        for (int i = 0; i < buttonSetupParams.Count; i++)
        {
            int index = i;
            GameObject buttonInstance = Instantiate(buttonPrefab, ButtonLayoutParent.transform);
            Button theButton = buttonInstance.GetComponent<Button>();
            TextMeshProUGUI buttonText = (TextMeshProUGUI)buttonInstance.GetComponentInChildren(typeof(TextMeshProUGUI));
            buttonText.text = buttonSetupParams[index].Item1;
            Type type = buttonSetupParams[index].Item3.GetType();

            if(type.GetMethod("Invoke") != null)
            {
                Debug.Log($"Invoke exists! Attempting to add {buttonSetupParams[index].Item2} as an on click method to the {i}th(st) button.");
                theButton.onClick.AddListener(() => ((MonoBehaviour)buttonSetupParams[index].Item3).Invoke(buttonSetupParams[index].Item2, 0));
            }
            else
            {
                Debug.LogWarning("Hey, invoke didn't exist so we couldn't add the method!!!");
            }

            theButton.onClick.AddListener(DeactivatePopup);
            theButton.onClick.AddListener(chooseOption);
        }

        blockerPanel.SetActive(true);
    }



    public void DeactivatePopup()
    {
        ImageLayoutParent.SetActive(true);
        ButtonLayoutParent.SetActive(true);
        blockerPanel.SetActive(false);
        DestroyChildrenOfParent(ButtonLayoutParent.transform);
        DestroyChildrenOfParent(ImageLayoutParent.transform);

        buttonOptions.Clear();
    }

    private void DestroyChildrenOfParent(Transform parent)
    {
        foreach(Transform child in parent.transform)
        {
            Destroy(child.gameObject);
        }
    }

    #region Events

    private void chooseOption()
    {
        optionChosen?.Invoke();
    }

    #endregion
}
