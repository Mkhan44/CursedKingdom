//Code written by Mohamed Riaz Khan of BukuGames.
//All code is written by me (Above name) unless otherwise stated via comments below.
//Not authorized for use outside of the Github repository of this game developed by BukuGames.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DuelPlayerInformation
{
	[SerializeField] private Player playerInDuel;
	[SerializeField] List<MovementCard> selectedMovementCards;
	[SerializeField] List<SupportCard> selectedSupportCards;

    public Player PlayerInDuel { get => playerInDuel; set => playerInDuel = value; }
    public List<MovementCard> SelectedMovementCards { get => selectedMovementCards; set => selectedMovementCards = value; }
    public List<SupportCard> SelectedSupportCards { get => selectedSupportCards; set => selectedSupportCards = value; }

    public DuelPlayerInformation(Player player)
    {
        PlayerInDuel = player;
        SelectedMovementCards = new List<MovementCard>();
        SelectedSupportCards = new List<SupportCard>();
    }
}
