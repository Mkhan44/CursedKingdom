//Code written by Mohamed Riaz Khan of BukuGames.
//All code is written by me (Above name) unless otherwise stated via comments below.
//Not authorized for use outside of the Github repository of this game developed by BukuGames.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Card : MonoBehaviour
{
    private Image cardBackImage;

    protected Image CardBackImage { get => cardBackImage; set => cardBackImage = value; }

    protected virtual void InitializeCard()
    {

    }
}
