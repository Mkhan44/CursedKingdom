using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenModeSetting : MonoBehaviour
{
    [SerializeField] private TMPro.TMP_Dropdown ScreenModeDropDown;    //check name!
    private Resolutions resolutions;
    // Start is called before the first frame update
    void Start()
    {
        resolutions = FindObjectOfType<Resolutions>();
        int val = PlayerPrefs.GetInt("ScreenMode");    //check name!
        ScreenModeDropDown.value = val;    //check name!
    }

    public void SetScreenMode(int index)    //check name!
    {
        PlayerPrefs.SetInt("ScreenMode", index);    //check name!
        if (index == 0)
        {
            // Switch to 4k full
            Screen.SetResolution(resolutions.resolution.width, resolutions.resolution.height, true);

            //Screen.fullScreen = true;
            Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
        }
        if (index == 1)
        {
            Screen.SetResolution(resolutions.resolution.width, resolutions.resolution.height, true);
            //Screen.fullScreen = true;
            Screen.fullScreenMode = FullScreenMode.ExclusiveFullScreen;
        }
        if (index == 2)
        {
            Screen.SetResolution(resolutions.resolution.width, resolutions.resolution.height, false);
            //Screen.fullScreen = false;
            Screen.fullScreenMode = FullScreenMode.Windowed;
        }
    }
}
