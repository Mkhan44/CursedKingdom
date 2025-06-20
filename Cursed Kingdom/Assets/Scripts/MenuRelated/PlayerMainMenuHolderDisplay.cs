using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PlayerMainMenuHolderDisplay : MonoBehaviour
{
    public event Action<PlayerMainMenuHolderDisplay> SelectedMainMenuHolderDisplay;

    [SerializeField] private Animator animator;
    [SerializeField] private TextMeshProUGUI classText;
    [SerializeField] private TextMeshProUGUI DescriptionText;
    public TextMeshProUGUI ClassText { get => classText; set => classText = value; }
    public TextMeshProUGUI DescriptionText1 { get => DescriptionText; set => DescriptionText = value; }
    public Animator Animator { get => animator; set => animator = value; }

    public ClassData classDataRef;

    public void PopulateData(ClassData classData)
    {
        classDataRef = classData;
        Animator.runtimeAnimatorController = classData.menuAnimatorController;
        ClassText.text = classData.classType.ToString();
        DescriptionText.text = classData.description;
        DescriptionText.text += $"\n Starting health: {classData.startingHealth.ToString()} \n Special Ability: {classData.abilityData.EffectDescription} \n Elite Ability: {classData.eliteAbilityData.EffectDescription}";
    }

    public void ChangeCharacter(ClassData classData)
    {
        SelectedThisMainMenuHolder();
        PopulateData(classData);
    }

    public virtual void OnPointerDown(PointerEventData eventData)
    {
        SelectedThisMainMenuHolder();
    }

    public virtual void OnPointerEnter(PointerEventData eventData)
    {

    }

    public virtual void OnPointerExit(PointerEventData eventData)
    {

    }

    public void SelectedThisMainMenuHolder()
	{
		SelectedMainMenuHolderDisplay?.Invoke(this);
	}
}
