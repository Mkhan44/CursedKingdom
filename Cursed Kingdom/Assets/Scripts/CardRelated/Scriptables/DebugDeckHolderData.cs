//Code written by Mohamed Riaz Khan of BukuGames.
//All code is written by me (Above name) unless otherwise stated via comments below.
//Not authorized for use outside of the Github repository of this game developed by BukuGames.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Use this scriptable for the debug start menu. The 2 lists will be populated with scriptables.
/// Once we go into playmode if the user selects to use the selected deck for either movement, support or both; we swap in the DeckManager of the GameplayManager
/// the scriptable that we want to use instead from these lists.
/// </summary>

[CreateAssetMenu(fileName = "DebugDeckHolder", menuName = "Debug/Debug Deck Holder Data", order = 0)]
public class DebugDeckHolderData : ScriptableObject
{
    [SerializeField] private List<DeckData> movementCardDeckDatas;
    [SerializeField] private List<DeckData> supportCardDeckDatas;

    public List<DeckData> MovementCardDeckDatas { get => movementCardDeckDatas; set => movementCardDeckDatas = value; }
    public List<DeckData> SupportCardDeckDatas { get => supportCardDeckDatas; set => supportCardDeckDatas = value; }
}
