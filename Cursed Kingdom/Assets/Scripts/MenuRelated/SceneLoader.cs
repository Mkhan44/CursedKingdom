using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
public class SceneLoader : MonoBehaviour
{
    public TextMeshProUGUI versionText;
    public TextMeshProUGUI mainText;
    public Animator scroll;
    public Animator title;
    public string scrollOpen = "ScrollOpen";
    public string fadeOut = "FadeOut";

    // Start is called before the first frame update
    void Start()
    {
        versionText.text = "ver. " + Application.version;
    }

    // Update is called once per frame
    private void Update()

    {

        if (Input.anyKeyDown || Input.touchCount > 0)
        {
            title.SetTrigger(fadeOut);

        }

    }
}
