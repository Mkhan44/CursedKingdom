//Code written by Mohamed Riaz Khan of BukuGames.
//All code is written by me (Above name) unless otherwise stated via comments below.
//Not authorized for use outside of the Github repository of this game developed by BukuGames.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
	public GameObject duelResolveCardsHolder;
	public GameObject cardResolveHolderPrefab;
	public Button movementCardsDeselectButton;
	public Button supportCardsDeselectButton;
	public Button confirmChoicesButton;
	public Animator duelFadePanelAnimator;
	public Animator duelSelectCardsUIAnimator;
	public GameObject supportCardDuelPrefab;
	public GameObject movementCardDuelPrefab;
	public GameObject movementCardDuelHolderPrefab;
	public GameObject supportCardDuelHolderPrefab;

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
		duelResolveCardsHolder.SetActive(false);
		movementCardDuelHolderPrefab.SetActive(false);
		supportCardDuelHolderPrefab.SetActive(false);
	}

	protected override BaseState GetInitialState()
	{
		PlayersInCurrentDuel = new();
		return duelNotDuelingPhaseState;
	}

	//Cleanup should happen here post-duel.
	public void ResetDuelParameters()
	{
		foreach (DuelPlayerInformation duelPlayerInformation in PlayersInCurrentDuel)
		{
			Destroy(duelPlayerInformation.PlayerDuelPrefabInstance);
			duelPlayerInformation.PlayerDuelTransform = null;
			duelPlayerInformation.PlayerDuelAnimator = null;
			if(duelPlayerInformation.CardDuelResolveHolderObject != null)
			{
				foreach(Transform child in duelPlayerInformation.CardDuelResolveHolderObject.transform.GetChild(0))
				{
					Destroy(child.gameObject);
				}
				foreach(Transform child in duelPlayerInformation.CardDuelResolveHolderObject.transform.GetChild(1))
				{
					Destroy(child.gameObject);
				}
			}
		}

		duelResolveCardsHolder.SetActive(false);
		PlayersInCurrentDuel.Clear();
		CurrentWinners.Clear();
		CurrentPlayerBeingHandled = null;
		DialogueBoxPopup.instance.DeactivatePopup();
	}

	public void EnableAbilityButtons()
	{
        Player currentPlayer = gameplayManager.DuelPhaseSMRef.PlayersInCurrentDuel[0].PlayerInDuel;

        if (!currentPlayer.IsOnCooldown && currentPlayer.ClassData.abilityData.CanBeManuallyActivated)
        {
            gameplayManager.UseAbilityButton.transform.parent.gameObject.SetActive(true);
        }
        else
        {
            gameplayManager.UseAbilityButton.transform.parent.gameObject.SetActive(false);
        }

        if (currentPlayer.CanUseEliteAbility && currentPlayer.ClassData.eliteAbilityData.CanBeManuallyActivated)
        {
            gameplayManager.UseEliteAbilityButton.transform.parent.gameObject.SetActive(true);
        }
        else
        {
            gameplayManager.UseEliteAbilityButton.transform.parent.gameObject.SetActive(false);
        }
    }

	public IEnumerator FadePanelActivate(float delay = 0f)
	{
		yield return new WaitForSeconds(delay);
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
		duelPlayerInformation.PlayerInDuel.Animator.SetBool(Player.ISTRANSITIONINGTODUEL, true);

		float animationTime = 0f;

		foreach (AnimationClip animationClip in duelPlayerInformation.PlayerInDuel.Animator.runtimeAnimatorController.animationClips)
		{
			string animationNameToSearchFor = animationClip.name.ToLower();
			if (animationNameToSearchFor.EndsWith("battletransition"))
			{
				animationTime = animationClip.length;
				break;
			}
		}

		yield return new WaitForSeconds(animationTime);

		duelPlayerInformation.PlayerInDuel.Animator.SetBool(Player.ISDUELINGIDLE, true);
		duelPlayerInformation.PlayerInDuel.Animator.SetBool(Player.ISIDLE, false);
		yield return null;
		duelPlayerInformation.PlayerInDuel.Animator.SetBool(Player.ISTRANSITIONINGTODUEL, false);

		//IsDuelingIdle

		//This works but for mage right now the 'battle stance' is just the mageidle since it's not completed yet that's why it looks weird on the Mage.
		foreach (AnimationClip animationClip in duelPlayerInformation.PlayerInDuel.Animator.runtimeAnimatorController.animationClips)
		{
			string animationNameToSearchFor = animationClip.name.ToLower();
			if (animationNameToSearchFor.EndsWith("battlestance"))
			{
				animationTime = animationClip.length;
				break;
			}
		}

		yield return new WaitForSeconds(animationTime);

		if (PlayersInCurrentDuel.Last() == duelPlayerInformation)
		{
			StartCoroutine(FadePanelActivate());
		}
	}

	public IEnumerator ChooseNoSupportCardToUseInDuel()
	{
		yield return null;
	}

	public IEnumerator SetupCardsToResolve()
	{
		duelSupportResolutionPhaseState.SetupSupportCardsToResolve();
		duelSupportResolutionPhaseState.SetupMovementCardsToResolve();
		yield return new WaitForSeconds(0.65f);
		duelSupportResolutionPhaseState.FlipAndUseSupportCards();
	}

	/// <summary>
	/// Use the list from SupportResolutionPhaseState here because some support cards may have priority.
	/// </summary>
	/// <returns></returns>
	public IEnumerator TestingTimeBetweenPopupsSupportCardResolution()
	{
		yield return new WaitForSeconds(2.0f);

		DialogueBoxPopup.instance.DeactivatePopup();

		int indexOfCurrentPlayerBeingHandled = duelSupportResolutionPhaseState.PlayersLeftToResolveSupportCardsFor.IndexOf(duelSupportResolutionPhaseState.CurrentDuelPlayerInfoBeingHandledSupportRes);


		if (indexOfCurrentPlayerBeingHandled != duelSupportResolutionPhaseState.PlayersLeftToResolveSupportCardsFor.Count - 1)
		{
			//Shouldn't ever go out of bounds.
			duelSupportResolutionPhaseState.CurrentDuelPlayerInfoBeingHandledSupportRes = duelSupportResolutionPhaseState.PlayersLeftToResolveSupportCardsFor[indexOfCurrentPlayerBeingHandled + 1];
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
		yield return new WaitForSeconds(1.5f);

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
			//play animation of all players attacking
			foreach (DuelPlayerInformation playerInfo in PlayersInCurrentDuel)
			{
				StartCoroutine(PlayerEndOfDuelAnimations(playerInfo));
			}
		}
	}

	public IEnumerator PlayerEndOfDuelAnimations(DuelPlayerInformation duelPlayerInformation)
	{
		duelPlayerInformation.PlayerDuelAnimator.SetBool(Player.ISCASTING, true);

		float animationTime = 0f;

		foreach (AnimationClip animationClip in duelPlayerInformation.PlayerDuelAnimator.runtimeAnimatorController.animationClips)
		{
			string animationNameToSearchFor = animationClip.name.ToLower();
			if (animationNameToSearchFor.EndsWith("cast"))
			{
				animationTime = animationClip.length;
				break;
			}
		}

		yield return new WaitForSeconds(animationTime + 0.8f);

		duelPlayerInformation.PlayerDuelAnimator.SetBool(Player.ISCASTING, false);

		bool wasAWinner = false;

		//Need a condition for tie here if we're gonna have a neutral animation I think.
		if (CurrentWinners.Contains(duelPlayerInformation) && CurrentWinners.Count == 1)
		{
			wasAWinner = true;
			duelPlayerInformation.PlayerDuelAnimator.SetBool(Player.POSITIVEEFFECT, true);
		}
		else
		{
			duelPlayerInformation.PlayerDuelAnimator.SetBool(Player.NEGATIVEEFFECT, true);
		}

		foreach (AnimationClip animationClip in duelPlayerInformation.PlayerDuelAnimator.runtimeAnimatorController.animationClips)
		{
			string animationNameToSearchFor = animationClip.name.ToLower();
			if (animationNameToSearchFor.EndsWith("positive") || animationNameToSearchFor.EndsWith("negative"))
			{
				animationTime = animationClip.length;
				break;
			}
		}

		yield return new WaitForSeconds(animationTime + 0.8f);

		if (wasAWinner)
		{
			duelPlayerInformation.PlayerDuelAnimator.SetBool(Player.POSITIVEEFFECT, false);
		}
		else
		{
			duelPlayerInformation.PlayerDuelAnimator.SetBool(Player.NEGATIVEEFFECT, false);
		}

		//We need to rework this as right now there is no way to guarantee that the final player has the longest animations.
		if (PlayersInCurrentDuel.Last() == duelPlayerInformation)
		{
			gameplayManager.GameplayCameraManagerRef.DuelVirtualCameraAnimator.SetBool(GameplayCameraManager.ISRESOLVINGCAM, false);
			gameplayManager.GameplayCameraManagerRef.DuelVirtualCameraAnimator.SetBool(GameplayCameraManager.ISGOINGBACKTODEFAULT, true);
			CurrentPlayerBeingHandled = PlayersInCurrentDuel[0];
			if (CurrentWinners.Count > 1)
			{
				DialogueBoxPopup.instance.ActivatePopupWithJustText($"The duel is a tie. All players take 1 damage.");
			}
			else
			{
				DialogueBoxPopup.instance.ActivatePopupWithJustText($"Player {CurrentWinners[0].PlayerInDuel.playerIDIntVal} wins the duel with a value of: {CurrentWinners[0].SelectedMovementCards[0].GetCurrentCardValue()}!");
			}

			yield return new WaitForSeconds(2.5f);
			DialogueBoxPopup.instance.DeactivatePopup();
			duelMovementResolutionPhaseState.EnterResultPhase();
		}
	}

	public IEnumerator EndOfDuelResultAnimation(DuelPlayerInformation duelPlayerInformation)
	{
		yield return new WaitForSeconds(0.5f);

		float animationTime = 0f;

        duelPlayerInformation.PlayerInDuel.Animator.SetBool(Player.ISDUELINGIDLE, false);

        //reverse getting into duel anim.
		duelPlayerInformation.PlayerInDuel.Animator.SetBool(Player.ISTRANSITIONINGTODUEL, true);
		yield return null;

        foreach (AnimationClip animationClip in duelPlayerInformation.PlayerDuelAnimator.runtimeAnimatorController.animationClips)
        {
            string animationNameToSearchFor = animationClip.name.ToLower();
            if (animationNameToSearchFor.EndsWith("battletransitionrev"))
            {
                animationTime = animationClip.length;
                break;
            }
        }

		yield return new WaitForSeconds(animationTime + 0.7f);
        duelPlayerInformation.PlayerInDuel.Animator.SetBool(Player.ISTRANSITIONINGTODUEL, false);
        duelPlayerInformation.PlayerInDuel.Animator.SetBool(Player.ISIDLE, true);

        //Do any effects that we need to after a duel is over. Damage calc first.
        if (GetCurrentState() == DuelResultPhaseState && PlayersInCurrentDuel.Last() == duelPlayerInformation)
        {
            DuelResultPhaseState.DamageResults();
        }
    }
}
