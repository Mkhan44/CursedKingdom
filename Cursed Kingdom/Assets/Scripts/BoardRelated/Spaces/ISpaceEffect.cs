//Code written by Mohamed Riaz Khan of BukuGames.
//All code is written by me (Above name) unless otherwise stated via comments below.
//Not authorized for use outside of the Github repository of this game developed by BukuGames.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Interface for the type of spaces that we can have in the game. This could be drawing cards, recovering or losing health, etc.
public interface ISpaceEffect
{
    public abstract void EffectOfSpace(Player playerReference);
}