//Code written by Mohamed Riaz Khan of BukuGames.
//All code is written by me (Above name) unless otherwise stated via comments below.
//Not authorized for use outside of the Github repository of this game developed by BukuGames.

using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapManager : MonoBehaviour
{
    public GameplayManager gameplayManager;
    public Space currentHighlightedSpace;
    [SerializeField] private bool isViewingMapOverhead;
    [SerializeField] private bool isViewingMapGround;
    [SerializeField] private bool spaceIsHighlighted;

    public bool IsViewingMapOverhead { get => isViewingMapOverhead; set => isViewingMapOverhead = value; }
    public bool IsViewingMapGround { get => isViewingMapGround; set => isViewingMapGround = value; }
    public bool SpaceIsHighlighted { get => spaceIsHighlighted; set => spaceIsHighlighted = value; }

    private void Start()
    {
        gameplayManager = this.GetComponent<GameplayManager>();
        SpaceIsHighlighted = false;
        IsViewingMapOverhead = false;

        if(gameplayManager != null) 
        {
            SubscribeToEvents();
        }
    }

    private void SubscribeToEvents()
    {
        gameplayManager.MoveHighlightedSpaceIconOverheadView += MoveOverheadView;
        gameplayManager.MoveHighlightedSpaceIconGroundView += MoveGroundView;
    }


    private void UnsubscribeToEvents()
    {
        gameplayManager.MoveHighlightedSpaceIconOverheadView -= MoveOverheadView;
        gameplayManager.MoveHighlightedSpaceIconGroundView -= MoveGroundView;
    }

    #region Overhead map view
    public void ActivateHighlightOverheadView(Space spaceToHighlight)
    {
        int indexOfCurrentPlayer = gameplayManager.Players.IndexOf(gameplayManager.playerCharacter.GetComponent<Player>());

        currentHighlightedSpace = spaceToHighlight;
        currentHighlightedSpace.EnableHighlight();
        gameplayManager.Players[indexOfCurrentPlayer].HideHand();
        //Turn on Timour's panel and populate information in it with current space's info.
        gameplayManager.TopDownMapDisplay.UpdateInformation(spaceToHighlight);
        SpaceIsHighlighted = true;
        IsViewingMapOverhead = true;
    }

    public void ChangeCurrentHighlightedSpaceOverheadView(Space spaceToHighlight)
    {
        //Disable old one.
        currentHighlightedSpace.DisableHighlight();
        ActivateHighlightOverheadView(spaceToHighlight);
        gameplayManager.TopDownMapDisplay.UpdateInformation(spaceToHighlight);
        SpaceIsHighlighted = true;
        IsViewingMapOverhead = true;
    }

    public void DisableCurrentHighlightedSpaceOverheadView(Space spaceToDisableHighlight)
    {
        int indexOfCurrentPlayer = gameplayManager.Players.IndexOf(gameplayManager.playerCharacter.GetComponent<Player>());

        currentHighlightedSpace.DisableHighlight();
        SpaceIsHighlighted = false;
        gameplayManager.Players[indexOfCurrentPlayer].ShowHand();
        IsViewingMapOverhead = false;
        gameplayManager.TopDownMapDisplay.CloseMapDisplayPanel();
    }

    #endregion

    #region Ground View
    public void SetupGroundViewCamera(CinemachineVirtualCamera camera, Space spaceToFocus)
    {
        isViewingMapGround = true;
        camera.LookAt = spaceToFocus.gameObject.transform;
        camera.Follow = spaceToFocus.gameObject.transform;
        ActivateHighlightGroundView(spaceToFocus);
    }

    public void ActivateHighlightGroundView(Space spaceToHighlight)
    {
        int indexOfCurrentPlayer = gameplayManager.Players.IndexOf(gameplayManager.playerCharacter.GetComponent<Player>());

        currentHighlightedSpace = spaceToHighlight;
        currentHighlightedSpace.EnableHighlight();
        gameplayManager.Players[indexOfCurrentPlayer].HideHand();

        //will need a new type of info panel here being populated.

        SpaceIsHighlighted = true;
        isViewingMapGround = true;
    }

    public void ChangeCurrentHighlightedSpaceGroundView(CinemachineVirtualCamera camera, Space spaceToHighlight)
    {
        //Disable old one.
        currentHighlightedSpace.DisableHighlight();
        camera.LookAt = spaceToHighlight.gameObject.transform;
        camera.Follow = spaceToHighlight.gameObject.transform;
        ActivateHighlightGroundView(spaceToHighlight);
      //  gameplayManager.TopDownMapDisplay.UpdateInformation(spaceToHighlight);
        SpaceIsHighlighted = true;
        isViewingMapGround = true;
    }

    public void DisableHighlightedSpaceGroundView(Space spaceToDisableHighlight)
    {
        int indexOfCurrentPlayer = gameplayManager.Players.IndexOf(gameplayManager.playerCharacter.GetComponent<Player>());

        currentHighlightedSpace.DisableHighlight();
        SpaceIsHighlighted = false;
        gameplayManager.Players[indexOfCurrentPlayer].ShowHand();
        isViewingMapGround = false;
        //gameplayManager.TopDownMapDisplay.CloseMapDisplayPanel();
    }
    #endregion

    #region Event Handlers
    private void MoveGroundView(KeyCode keyCodePressed)
    {
        if (isViewingMapGround)
        {
            if (keyCodePressed == KeyCode.UpArrow)
            {
                if (currentHighlightedSpace.NorthNeighbor != null)
                {
                    ChangeCurrentHighlightedSpaceGroundView(gameplayManager.currentActiveCamera, currentHighlightedSpace.NorthNeighbor);
                }
            }

            if (keyCodePressed == KeyCode.DownArrow)
            {
                if (currentHighlightedSpace.SouthNeighbor != null)
                {
                    ChangeCurrentHighlightedSpaceGroundView(gameplayManager.currentActiveCamera, currentHighlightedSpace.SouthNeighbor);
                }
            }

            if (keyCodePressed == KeyCode.RightArrow)
            {
                if (currentHighlightedSpace.EastNeighbor != null)
                {
                    ChangeCurrentHighlightedSpaceGroundView(gameplayManager.currentActiveCamera, currentHighlightedSpace.EastNeighbor);
                }
            }

            if (keyCodePressed == KeyCode.LeftArrow)
            {
                if (currentHighlightedSpace.WestNeighbor != null)
                {
                    ChangeCurrentHighlightedSpaceGroundView(gameplayManager.currentActiveCamera, currentHighlightedSpace.WestNeighbor);
                }
            }
        }
    }

    private void MoveOverheadView(KeyCode keyCodePressed)
    {
        if (IsViewingMapOverhead)
        {
            if (keyCodePressed == KeyCode.UpArrow)
            {
                if (currentHighlightedSpace.NorthNeighbor != null)
                {
                    ChangeCurrentHighlightedSpaceOverheadView(currentHighlightedSpace.NorthNeighbor);
                }
            }

            if (keyCodePressed == KeyCode.DownArrow)
            {
                if (currentHighlightedSpace.SouthNeighbor != null)
                {
                    ChangeCurrentHighlightedSpaceOverheadView(currentHighlightedSpace.SouthNeighbor);
                }
            }

            if (keyCodePressed == KeyCode.RightArrow)
            {
                if (currentHighlightedSpace.EastNeighbor != null)
                {
                    ChangeCurrentHighlightedSpaceOverheadView(currentHighlightedSpace.EastNeighbor);
                }
            }

            if (keyCodePressed == KeyCode.LeftArrow)
            {
                if (currentHighlightedSpace.WestNeighbor != null)
                {
                    ChangeCurrentHighlightedSpaceOverheadView(currentHighlightedSpace.WestNeighbor);
                }
            }
        }
    }
    #endregion
}
