//Code written by Mohamed Riaz Khan of BukuGames.
//All code is written by me (Above name) unless otherwise stated via comments below.
//Not authorized for use outside of the Github repository of this game developed by BukuGames.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DuelPlayerInformation
{
	[SerializeField] private Player playerInDuel;
	[SerializeField] private List<MovementCard> selectedMovementCards;
	[SerializeField] private List<SupportCard> selectedSupportCards;
    [SerializeField] private GameObject playerDuelPrefabInstance;
    [SerializeField] private Transform playerDuelTransform;
    [SerializeField] private Animator playerDuelAnimator;
    [SerializeField] private GameObject cardDuelResolveHolderObject;
    [SerializeField] private int damageToTake = 0;

    public Player PlayerInDuel { get => playerInDuel; set => playerInDuel = value; }
    public List<MovementCard> SelectedMovementCards { get => selectedMovementCards; set => selectedMovementCards = value; }
    public List<SupportCard> SelectedSupportCards { get => selectedSupportCards; set => selectedSupportCards = value; }
    public GameObject PlayerDuelPrefabInstance { get => playerDuelPrefabInstance; set => playerDuelPrefabInstance = value; }
    public Transform PlayerDuelTransform { get => playerDuelTransform; set => playerDuelTransform = value; }
    public Animator PlayerDuelAnimator { get => playerDuelAnimator; set => playerDuelAnimator = value; }
    public GameObject CardDuelResolveHolderObject { get => cardDuelResolveHolderObject; set => cardDuelResolveHolderObject = value; }
    public int DamageToTake { get => damageToTake; set => damageToTake = value; }

    public DuelPlayerInformation(Player player)
    {
        PlayerInDuel = player;
        SelectedMovementCards = new List<MovementCard>();
        SelectedSupportCards = new List<SupportCard>();
    }

    public void SetupPlayerDuelPrefabInstance(GameObject playerDuelPrefabInstance)
    {
        PlayerDuelPrefabInstance = playerDuelPrefabInstance;
        if (PlayerDuelPrefabInstance != null)
        {
            PlayerDuelTransform = PlayerDuelPrefabInstance.transform;
            PlayerDuelAnimator = PlayerDuelPrefabInstance.GetComponent<Animator>();
            PlayerDuelAnimator.runtimeAnimatorController = PlayerInDuel.ClassData.animatorController;
            //Bad might wanna change this to be less hardcoded.
            playerDuelTransform.GetChild(0).GetComponent<Billboard>().CameraToBillboardTowards = GameObject.Find("Duel Camera").GetComponent<Camera>();
        }
    }
}
