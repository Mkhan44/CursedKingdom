//Code written by Mohamed Riaz Khan of BukuGames.
//All code is written by me (Above name) unless otherwise stated via comments below.
//Not authorized for use outside of the Github repository of this game developed by BukuGames.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//No create asset for this.
public class SpecialAttackSpace : SpaceEffectData, ISpaceEffect
{
    [SerializeField] ClassData.ClassType classTypeNeeded;

    public ClassData.ClassType ClassTypeNeeded { get => classTypeNeeded; set => classTypeNeeded = value; }

    public virtual void EffectOfSpace(Player playerReference)
    {
        Debug.Log($"Landed on: {this.name} special attack space and class is: {playerReference.ClassData.classType} while the class type needed is: {ClassTypeNeeded}");
    }
}
