//Code written by Mohamed Riaz Khan of BukuGames.
//All code is written by me (Above name) unless otherwise stated via comments below.
//Not authorized for use outside of the Github repository of this game developed by BukuGames.
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
    public Action<Player> dialogueBoxClosed;
    public delegate void CalledDelegate();

    private const string dialogueAnimBool = "IsOpen";

    [SerializeField] private TextMeshProUGUI displayText;
    [SerializeField] private TextMeshProUGUI headerNoticeText;
    [SerializeField] private GameObject blockerPanel;
    [SerializeField] private Image blockerPanelImage;
    [SerializeField] private int numOptionsSelected;
    [SerializeField] private int maxNumOptionsToSelect;
    [SerializeField] private GameObject buttonLayoutParent;
    [SerializeField] private GameObject imageLayoutParent;
    [SerializeField] private GameObject buttonPrefab;
    [SerializeField] private GameObject imagePrefab;
    [SerializeField] private Animator dialogueAnimator;
    [SerializeField] private bool isActive;
    [SerializeField] private Coroutine delayCoroutine;
    [SerializeField] private List<Button> buttonOptions;

    public GameObject ButtonLayoutParent { get => buttonLayoutParent; set => buttonLayoutParent = value; }
    public GameObject ImageLayoutParent { get => imageLayoutParent; set => imageLayoutParent = value; }
    public bool IsActive { get => isActive; }

    private void Awake()
    {
        
    }

    private void Start()
    {
        Invoke("Started", 0.2f);
        
    }

    private void Started()
    {
        blockerPanel.SetActive(true);
        blockerPanel.GetComponent<Image>().raycastTarget = false;
        //blockerPanel.SetActive(true);
        blockerPanelImage = this.GetComponent<Image>();
        isActive = false;
        buttonOptions = new();
        headerNoticeText.text = "Notice";
        numOptionsSelected = 0;
        maxNumOptionsToSelect = 0;
        delayCoroutine = null;
        instance = this;
    }

    /// <summary>
    /// A popup with just text that stays up until the user completes the action needed.
    /// </summary>
    /// <param name="textToDisplay">The text to display in the body of the box.</param>
    /// <param name="numSecondsToStayActive">If this is 0, the popup stays indefinitely. Otherwise it will stay active for numSecondsToStayActive seconds.</param>
    /// <param name="headerText">Text that appears at the top of the textbox. If no text is given it will just say "Notice"</param>
    public void ActivatePopupWithJustText(string textToDisplay, float numSecondsToStayActive = 0, string headerText = "Notice")
    {
        if (isActive)
        {
            DeactivatePopup();
        }

        displayText.text = textToDisplay;
        headerNoticeText.text = headerText;
        ImageLayoutParent.SetActive(false);
        ButtonLayoutParent.SetActive(false);

        dialogueAnimator.SetBool(dialogueAnimBool, true);
        //blockerPanel.SetActive(true);
        blockerPanelImage.raycastTarget = false;
        isActive = true;
        if(numSecondsToStayActive > 0)
        {
            delayCoroutine = StartCoroutine(DeactivateAfterWaiting(numSecondsToStayActive));
        }
        //Need a way to call Deactivate and also the action afterwards.
    }

    public IEnumerator DeactivateAfterWaiting(float numSecondsToWait)
    {
        yield return new WaitForSeconds(numSecondsToWait);

        if(delayCoroutine != null)
        {
            DeactivatePopup();
        }
    }

    public List<Button> GetCurrentPopupChoices()
    {
        List<Button> choices = new List<Button>();

        if(ButtonLayoutParent.transform.childCount > 0)
        {
            foreach(Transform child in ButtonLayoutParent.transform)
            {
                choices.Add(child.GetComponent<Button>());
            }

        }
        else if(ImageLayoutParent.transform.childCount > 0)
        {
            foreach(Transform child in ImageLayoutParent.transform)
            {
                choices.Add(child.GetComponent<Button>());
            }

        }
        else
        {
            Debug.LogWarning("We did not find any choices for the player to choose.");
        }


        return choices;
    }

    /// <summary>
    /// A popup with just a message and a button to close it.
    /// </summary>
    /// <param name="textToDisplay">The text to display in the body of the box.</param>
    /// <param name="closeButtonText">The text to display on the button. Defaults to "Okay" if not specified.</param>
    /// <param name="headerText">Text that appears at the top of the textbox. If no text is given it will just say "Notice"</param>

    public void ActivatePopupWithConfirmation(string textToDisplay, string closeButtonText = "Okay", string headerText = "Notice", bool isBlockerPanelOn = true)
    {
        if(isActive)
        {
            DeactivatePopup();
        }

        displayText.text = textToDisplay;
        headerNoticeText.text = headerText;
        ImageLayoutParent.SetActive(false);
        blockerPanelImage.raycastTarget = isBlockerPanelOn;

        GameObject buttonInstanceFinal = Instantiate(buttonPrefab, ButtonLayoutParent.transform);
        buttonInstanceFinal.GetComponent<Button>().onClick.AddListener(DeactivatePopup);
        buttonInstanceFinal.GetComponent<Button>().onClick.AddListener(chooseOption);
        buttonInstanceFinal.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = closeButtonText;

        dialogueAnimator.SetBool(dialogueAnimBool, true);
        isActive = true;
    }

    /// <summary>
    /// A method that displays a popup with image options. For the image setup each item in the list: 
    /// The SPRITE used for the image, METHOD NAME you want to call which *MUST BE A COROUTINE* and OBJECT you are calling this from need to be populated respectively.
    /// </summary>
    /// <param name="textToDisplay">The text to display in the body of the box.</param>
    /// <param name="imageSetupParams">A tuple where for each item in the list: T1 = The image sprite to click on, T2 = the method name you want to call, and T3 is the object from which this popup is being called from
    /// T4 is a list of method parameters to be used within the method that is called. This can be empty if the method has no parameters.</param>
    /// <param name="numChoicesThatCanBeSelected">The number of choices that can be selected before all methods are executed.</param>
    /// <param name="headerText">Text that appears at the top of the textbox. If no text is given it will just say "Notice"</param>
    public void ActivatePopupWithImageChoices(string textToDisplay, List<Tuple<Sprite, string, object, List<object>>> imageSetupParams, int numChoicesThatCanBeSelected = 1, string headerText = "Notice", bool isBlockerPanelOn = true)
    {
        if (isActive)
        {
            DeactivatePopup();
        }

        maxNumOptionsToSelect = numChoicesThatCanBeSelected;
        displayText.text = textToDisplay;
        headerNoticeText.text = headerText;
        ButtonLayoutParent.SetActive(false);
        blockerPanelImage.raycastTarget = isBlockerPanelOn;

        for(int i = 0; i < imageSetupParams.Count; i++)
        {
            int index = i;
            GameObject imageInstance = Instantiate(imagePrefab, ImageLayoutParent.transform);
            Button theImageButton = imageInstance.GetComponent<Button>();
            Image theImageSprite = imageInstance.transform.GetChild(1).GetComponent<Image>();
            theImageSprite.sprite = imageSetupParams[index].Item1;
            Type type = imageSetupParams[index].Item3.GetType();

            if (imageSetupParams[index].Item4.Count > 0)
            {
                theImageButton.onClick.AddListener(() => ((MonoBehaviour)imageSetupParams[index].Item3).StartCoroutine(imageSetupParams[index].Item2, imageSetupParams[index].Item4));
            }
            else
            {
                theImageButton.onClick.AddListener(() => ((MonoBehaviour)imageSetupParams[index].Item3).StartCoroutine(imageSetupParams[index].Item2));
            }
            
            theImageButton.onClick.AddListener(DeactivatePopup);
            theImageButton.onClick.AddListener(() => chooseOption());
        }


        dialogueAnimator.SetBool(dialogueAnimBool, true);
        isActive = true;
    }
    /// <summary>
    /// A method that displays a popup with button options. For the button setup each item in the list: 
    /// The TEXT on the button, METHOD NAME you want to call which *MUST BE A COROUTINE* and OBJECT you are calling this from need to be populated respectively.
    /// </summary>
    /// <param name="textToDisplay">The text to display in the body of the box.</param>
    /// <param name="buttonSetupParams">A tuple where for each item in the list: T1 = The text on the button, T2 = the method name you want to call, and T3 is the object from which this popup is being called from.</param>
    /// <param name="methodParams">A list of objects that are used as parameters for the method in the Tuple. These MUST match valid parameters and order does matter.</param>
    /// <param name="numChoicesThatCanBeSelected">The number of choices that can be selected before all methods are executed.</param>
    /// <param name="headerText">Text that appears at the top of the textbox. If no text is given it will just say "Notice"</param>
    public void ActivatePopupWithButtonChoices(string textToDisplay, List<Tuple<string, string, object, List<object> >> buttonSetupParams, int numChoicesThatCanBeSelected = 1, string headerText = "Notice", bool isBlockerPanelOn = true)
    {
        if (isActive)
        {
            DeactivatePopup();
        }

        maxNumOptionsToSelect = numChoicesThatCanBeSelected;
        displayText.text = textToDisplay;
        headerNoticeText.text = headerText;
        ImageLayoutParent.SetActive(false);
        blockerPanelImage.raycastTarget = isBlockerPanelOn;

        for (int i = 0; i < buttonSetupParams.Count; i++)
        {
            int index = i;
            GameObject buttonInstance = Instantiate(buttonPrefab, ButtonLayoutParent.transform);
            Button theButton = buttonInstance.GetComponent<Button>();
            TextMeshProUGUI buttonText = (TextMeshProUGUI)buttonInstance.GetComponentInChildren(typeof(TextMeshProUGUI));
            buttonText.text = buttonSetupParams[index].Item1;
            Type type = buttonSetupParams[index].Item3.GetType();

            if (buttonSetupParams[index].Item4.Count > 0)
            {
                theButton.onClick.AddListener(() => ((MonoBehaviour)buttonSetupParams[index].Item3).StartCoroutine(buttonSetupParams[index].Item2, buttonSetupParams[index].Item4));
            }
            else
            {
                theButton.onClick.AddListener(() => ((MonoBehaviour)buttonSetupParams[index].Item3).StartCoroutine(buttonSetupParams[index].Item2));
            }
           
            theButton.onClick.AddListener(DeactivatePopup);
            theButton.onClick.AddListener(chooseOption);
        }

        dialogueAnimator.SetBool(dialogueAnimBool, true);
        isActive = true;
    }

    public void OptionSelected(Button buttonClicked)
    {

    }

    public IEnumerator OptionDeselected(int num, int num2, int num3)
    {
        yield return null;
    }



    public void DeactivatePopup()
    {
        delayCoroutine = null;
        numOptionsSelected = 0;
        maxNumOptionsToSelect = 0;
        ImageLayoutParent.SetActive(true);
        ButtonLayoutParent.SetActive(true);
        blockerPanelImage.raycastTarget = true;
        blockerPanel.GetComponent<Image>().raycastTarget = false;
        dialogueAnimator.SetBool(dialogueAnimBool, false);
        //blockerPanel.SetActive(false);
        DestroyChildrenOfParent(ButtonLayoutParent.transform);
        DestroyChildrenOfParent(ImageLayoutParent.transform);
        isActive = false;
        dialogueBoxClosed?.Invoke(null);
    }

    private void DestroyChildrenOfParent(Transform parent)
    {
        foreach(Transform child in parent.transform)
        {
            child.GetComponent<Button>().onClick.RemoveAllListeners();
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
