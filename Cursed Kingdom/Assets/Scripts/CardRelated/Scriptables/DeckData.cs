//Code written by Mohamed Riaz Khan of BukuGames.
//All code is written by me (Above name) unless otherwise stated via comments below.
//Not authorized for use outside of the Github repository of this game developed by BukuGames.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Deck", menuName = "Card Data/Deck Data", order = 0)]
public class DeckData : ScriptableObject
{
    [SerializeField] private List<MovementCardData> movementCardDatas;
    [SerializeField] private List<SupportCardData> supportCardDatas;

    public List<MovementCardData> MovementCardDatas { get => movementCardDatas; set => movementCardDatas = value; }
    public List<SupportCardData> SupportCardDatas { get => supportCardDatas; set => supportCardDatas = value; }
}