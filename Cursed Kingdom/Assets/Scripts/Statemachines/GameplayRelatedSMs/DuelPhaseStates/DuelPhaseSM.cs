//Code written by Mohamed Riaz Khan of BukuGames.
//All code is written by me (Above name) unless otherwise stated via comments below.
//Not authorized for use outside of the Github repository of this game developed by BukuGames.
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DuelPhaseSM : BukuStateMachine
{

    //Events

    public event Action FadePanelCompletedFadingDuel;
    //States

    //Default
    public DuelNotDuelingPhaseState duelNotDuelingPhaseState;

	public DuelStartPhaseState duelStartPhaseState;
	public DuelSelectCardsToUsePhaseState duelSelectCardsToUsePhaseState;
	public DuelSupportResolutionPhaseState duelSupportResolutionPhaseState;
	public DuelMovementResolutionPhaseState duelMovementResolutionPhaseState;
	public DuelResultPhaseState DuelResultPhaseState;

	public GameplayManager gameplayManager;

	//Players in duel
	
	//THIS LIST should be in the order of: Player who initiated the duel followed by turn order of other players. If someone comes in via unwillful warp, 
	//they become the last player in the list.

	private List<DuelPlayerInformation> playersInCurrentDuel;
	private DuelPlayerInformation currentPlayerBeingHandled;
    private List<DuelPlayerInformation> currentWinners;

	//Testing button references. We should NOT have these here.
	public GameObject duelUIHolder;
	public Button movementCardsDeselectButton;
	public Button supportCardsDeselectButton;
	public Button confirmChoicesButton;
	public Animator duelFadePanelAnimator;

    public List<DuelPlayerInformation> PlayersInCurrentDuel { get => playersInCurrentDuel; set => playersInCurrentDuel = value; }
	
	public DuelPlayerInformation CurrentPlayerBeingHandled { get => currentPlayerBeingHandled; set => currentPlayerBeingHandled = value; }
    public List<DuelPlayerInformation> CurrentWinners { get => currentWinners; set => currentWinners = value; }

    private void Awake()
	{
		duelNotDuelingPhaseState = new DuelNotDuelingPhaseState(this);
		duelStartPhaseState = new DuelStartPhaseState(this);
		duelSelectCardsToUsePhaseState = new DuelSelectCardsToUsePhaseState(this);
		duelSupportResolutionPhaseState = new DuelSupportResolutionPhaseState(this);
		duelMovementResolutionPhaseState = new DuelMovementResolutionPhaseState(this);
		DuelResultPhaseState = new DuelResultPhaseState(this);

		gameplayManager = GetComponent<GameplayManager>();
		CurrentWinners = new();
		duelUIHolder.SetActive(false);
    }

	protected override BaseState GetInitialState()
	{
		PlayersInCurrentDuel = new();
		return duelNotDuelingPhaseState;
	}

	//Cleanup should happen here post-duel.
	public void ResetDuelParameters()
	{
		foreach(DuelPlayerInformation duelPlayerInformation in PlayersInCurrentDuel)
		{
            Destroy(duelPlayerInformation.PlayerDuelPrefabInstance);
			duelPlayerInformation.PlayerDuelTransform = null;
			duelPlayerInformation.PlayerDuelAnimator = null;
		}

		PlayersInCurrentDuel.Clear();
		CurrentWinners.Clear();
		CurrentPlayerBeingHandled = null;
		DialogueBoxPopup.instance.DeactivatePopup();
    }

	public IEnumerator FadePanelActivate()
	{
		duelFadePanelAnimator.SetBool(GameplayManager.ISFADING, value: true);
		duelFadePanelAnimator.gameObject.GetComponent<Image>().raycastTarget = true;

        float animationTime = 0f;
        //FadePanelAnim
        foreach (AnimationClip animationClip in duelFadePanelAnimator.runtimeAnimatorController.animationClips)
        {
            if (animationClip.name.ToLower() == "fadeoutpanelanim")
            {
                animationTime = animationClip.length;
                break;
            }
        }
        yield return new WaitForSeconds(animationTime);
        duelFadePanelAnimator.SetBool(GameplayManager.ISFADING, false);
        duelFadePanelAnimator.gameObject.GetComponent<Image>().raycastTarget = false;
        FadePanelCompletedFadingDuel?.Invoke();
    }

	//Entering duel.
	public IEnumerator CharacterDuelAnimationTransition(DuelPlayerInformation duelPlayerInformation)
	{
		foreach(DuelPlayerInformation playerInfo in PlayersInCurrentDuel)
		{
            playerInfo.PlayerInDuel.Animator.SetBool(Player.ISTRANSITIONINGTODUEL, true);
        }
        
		float animationTime = 0f;

		//This only works for 1 char atm. Need to do it for all players...
		
		foreach(AnimationClip animationClip in duelPlayerInformation.PlayerInDuel.Animator.runtimeAnimatorController.animationClips)
		{
			if(animationClip.name.ToLower() == duelPlayerInformation.PlayerInDuel.Animator.GetCurrentAnimatorClipInfo(0)[0].clip.name)
			{
				animationTime = animationClip.length;
				break;
			}
		}
        
		yield return new WaitForSeconds(animationTime);

        foreach (DuelPlayerInformation playerInfo in PlayersInCurrentDuel)
        {
            playerInfo.PlayerInDuel.Animator.SetBool(Player.ISDUELINGIDLE, true);
			yield return null;
            playerInfo.PlayerInDuel.Animator.SetBool(Player.ISTRANSITIONINGTODUEL, false);
        }

        //IsDuelingIdle
        foreach (AnimationClip animationClip in duelPlayerInformation.PlayerInDuel.Animator.runtimeAnimatorController.animationClips)
        {
            if (animationClip.name.ToLower() == duelPlayerInformation.PlayerInDuel.Animator.GetCurrentAnimatorClipInfo(0)[0].clip.name)
            {
                animationTime = animationClip.length;
                break;
            }
        }

        yield return new WaitForSeconds(animationTime);


        StartCoroutine(FadePanelActivate());
    }

	public IEnumerator ChooseNoSupportCardToUseInDuel()
	{
		yield return null;
	}

	public IEnumerator TestingTimeBetweenPopupsSupportCardResolution()
	{
		yield return new WaitForSeconds(2.5f);

		DialogueBoxPopup.instance.DeactivatePopup();

		int indexOfCurrentPlayerBeingHandled = PlayersInCurrentDuel.IndexOf(CurrentPlayerBeingHandled);


		if (indexOfCurrentPlayerBeingHandled != PlayersInCurrentDuel.Count - 1)
		{
			//Shouldn't ever go out of bounds.
			CurrentPlayerBeingHandled = PlayersInCurrentDuel[indexOfCurrentPlayerBeingHandled + 1];
			duelSupportResolutionPhaseState.Logic();
		}
		//We're at the final player. Reset back to the first player who initiated the duel and move onto Movementcard resolution phase.
		else
		{
			CurrentPlayerBeingHandled = PlayersInCurrentDuel[0];
			ChangeState(duelMovementResolutionPhaseState);
		}
	}

	public IEnumerator TestingTimeBetweenPopupsMovementCardResolution()
	{
		yield return new WaitForSeconds(2.5f);

		DialogueBoxPopup.instance.DeactivatePopup();

		int indexOfCurrentPlayerBeingHandled = PlayersInCurrentDuel.IndexOf(CurrentPlayerBeingHandled);


		if (indexOfCurrentPlayerBeingHandled != PlayersInCurrentDuel.Count - 1)
		{
			//Shouldn't ever go out of bounds.
			CurrentPlayerBeingHandled = PlayersInCurrentDuel[indexOfCurrentPlayerBeingHandled + 1];
			duelMovementResolutionPhaseState.Logic();
		}
		//We're at the final player.
		else
		{
			CurrentPlayerBeingHandled = PlayersInCurrentDuel[0];
			if(CurrentWinners.Count > 1)
			{
                DialogueBoxPopup.instance.ActivatePopupWithJustText($"The duel is a tie. All players take 1 damage.");
            }
			else
			{
				DialogueBoxPopup.instance.ActivatePopupWithJustText($"Player {CurrentWinners[0].PlayerInDuel.playerIDIntVal} wins the duel with a value of: {CurrentWinners[0].SelectedMovementCards[0].GetCurrentCardValue()}!");
			}

            ChangeState(DuelResultPhaseState);
		}
	}

}
