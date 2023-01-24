using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardData : ScriptableObject
{
    [SerializeField] private string cardTitle;
    [SerializeField] private Sprite cardArtwork;

    public string CardTitle { get => cardTitle; set => cardTitle = value; }
    public Sprite CardArtwork { get => cardArtwork; set => cardArtwork = value; }
}
