using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
public class SceneLoader : MonoBehaviour
{
    public TextMeshProUGUI versionText;
    public TextMeshProUGUI mainText;

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

            SceneManager.LoadScene("BoardGameplay");
            mainText.text = "Loading...";

        }

    }
}
