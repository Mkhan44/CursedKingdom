//Code written by Mohamed Riaz Khan of BukuGames.
//All code is written by me (Above name) unless otherwise stated via comments below.
//Not authorized for use outside of the Github repository of this game developed by BukuGames.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Interface for the type of Support Cards that we can have in the game. This could be drawing cards, recovering or losing health, etc.
public interface ISupportEffect
{
    public abstract void EffectOfCard(Player playerReference, Card cardPlayed = null);
}
