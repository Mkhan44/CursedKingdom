//Code written by Mohamed Riaz Khan of BukuGames.
//All code is written by me (Above name) unless otherwise stated via comments below.
//Not authorized for use outside of the Github repository of this game developed by BukuGames.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public class StartDebugPlayerElement : MonoBehaviour
{
    public TextMeshProUGUI playerNumberText;
    public TMP_Dropdown classDropdown;
    public TMP_Dropdown healthOverrideDropdown;
    public TMP_Dropdown startSpaceDropdown;
    public TMP_Dropdown levelOverrideDropdown;
    public TMP_Dropdown movementCardsInHandOverrideDropdown;
    public TMP_Dropdown supportCardsInHandOverrideDropdown;
    public Toggle isAIToggle;

    public int pageNum;

    //events

    public event Action<StartDebugPlayerElement> DropdownValueChanged;
    public event Action<StartDebugPlayerElement> ToggleValueChanged;

    private void Start()
    {
        if(pageNum == 1)
        {
            classDropdown.onValueChanged.AddListener(ChangedDropdownValue);
            healthOverrideDropdown.onValueChanged.AddListener(ChangedDropdownValue);
            startSpaceDropdown.onValueChanged.AddListener(ChangedDropdownValue);
            levelOverrideDropdown.onValueChanged.AddListener(ChangedDropdownValue);
            movementCardsInHandOverrideDropdown.onValueChanged.AddListener(ChangedDropdownValue);
            supportCardsInHandOverrideDropdown.onValueChanged.AddListener(ChangedDropdownValue);
        }
        else if(pageNum == 2)
        {
            isAIToggle.onValueChanged.AddListener(ChangeToggleValue);
        }
        
    }

    public void ChangedDropdownValue(int value)
    {
        DropdownValueChanged?.Invoke(this);
    }

    public void ChangeToggleValue(bool value)
    {
        ToggleValueChanged?.Invoke(this);
    }
}
