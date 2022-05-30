using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardData : ScriptableObject
{
    [SerializeField] string cardName;

    public string CardName { get => cardName; set => cardName = value; }
}
