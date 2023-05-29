//Code written by Mohamed Riaz Khan of BukuGames.
//All code is written by me (Above name) unless otherwise stated via comments below.
//Not authorized for use outside of the Github repository of this game developed by BukuGames.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapManager : MonoBehaviour
{
    public GameplayManager gameplayManager;
    public Space currentHighlightedSpace;
    [SerializeField] private bool isViewingMap;
    [SerializeField] private bool spaceIsHighlighted;

    public bool SpaceIsHighlighted { get => spaceIsHighlighted; set => spaceIsHighlighted = value; }
    public bool IsViewingMap { get => isViewingMap; set => isViewingMap = value; }

    private void Start()
    {
        gameplayManager = this.GetComponent<GameplayManager>();
        SpaceIsHighlighted = false;
        IsViewingMap = false;
    }

    public void ActivateHighlight(Space spaceToHighlight)
    {
        int indexOfCurrentPlayer = gameplayManager.Players.IndexOf(gameplayManager.playerCharacter.GetComponent<Player>());

        currentHighlightedSpace = spaceToHighlight;
        currentHighlightedSpace.EnableHighlight();
        gameplayManager.Players[indexOfCurrentPlayer].HideHand();
        //Turn on Timour's panel and populate information in it with current space's info.
        gameplayManager.TopDownMapDisplay.UpdateInformation(spaceToHighlight);
        SpaceIsHighlighted = true;
        IsViewingMap = true;
    }

    public void ChangeCurrentHighlightedSpace(Space spaceToHighlight)
    {
        //Disable old one.
        currentHighlightedSpace.DisableHighlight();
        ActivateHighlight(spaceToHighlight);
        gameplayManager.TopDownMapDisplay.UpdateInformation(spaceToHighlight);
        SpaceIsHighlighted = true;
        IsViewingMap = true;
    }

    public void DisableCurrentHighlightedSpace(Space spaceToHighlight)
    {
        int indexOfCurrentPlayer = gameplayManager.Players.IndexOf(gameplayManager.playerCharacter.GetComponent<Player>());

        currentHighlightedSpace.DisableHighlight();
        SpaceIsHighlighted = false;
        gameplayManager.Players[indexOfCurrentPlayer].ShowHand();
        IsViewingMap = false;
        gameplayManager.TopDownMapDisplay.CloseMapDisplayPanel();
    }
}
