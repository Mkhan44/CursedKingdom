//Code written by Mohamed Riaz Khan of BukuGames.
//All code is written by me (Above name) unless otherwise stated via comments below.
//Not authorized for use outside of the Github repository of this game developed by BukuGames.

using System.Collections;
using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;

[CreateAssetMenu(fileName = "Class Data", menuName = "Player /Class Data", order = 0)]
public class ClassData : ScriptableObject
{
    public enum ClassType
    {
        Magician,
        Thief,
        Warrior,
        Archer,
    }

    public ClassType classType;
    public int startingHealth;
    [TextArea(3, 5)]
    public string description;
    public AnimatorController animatorController;

}
