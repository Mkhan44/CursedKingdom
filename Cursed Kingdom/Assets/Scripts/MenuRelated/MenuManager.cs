using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
public class MenuManager : MonoBehaviour
{
    public TextMeshProUGUI versionText;
    public TextMeshProUGUI versionTextMobile;
    public TextMeshProUGUI mainText;
    public Animator scroll;
    public Animator title;
    public GameObject scrollMenu;
    public GameObject desktopMenu;
    public GameObject mobileMenu;
    public string fadeOut = "FadeOut";

    // Start is called before the first frame update
    void Start()
    {
        versionText.text = "ver. " + Application.version;
        versionTextMobile.text = versionText.text;
        /* mobileMenu.SetActive(false);
        desktopMenu.SetActive(false);
        scrollMenu.SetActive(false); */
    }

    // Update is called once per frame
    private void Update()

    {
        if (Application.isMobilePlatform)
        {
            mobileMenu.SetActive(true);
            desktopMenu.SetActive(false);
        }
        else
        {
            if (Input.anyKeyDown || Input.touchCount > 0)
            {
                title.SetTrigger(fadeOut);
                scrollMenu.SetActive(true);
            }
        }
    }

    public void LoadGameScene()
    {
        SceneManager.LoadScene("BoardGameplay");
    }
    
    public void ExitGame()
    { 
        Application.Quit(); 
    }

}
