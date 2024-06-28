//Code written by Mohamed Riaz Khan of BukuGames.
//All code is written by me (Above name) unless otherwise stated via comments below.
//Not authorized for use outside of the Github repository of this game developed by BukuGames.
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameplayStartPhaseState : BaseState
{
	GameplayPhaseSM gameplayPhaseSM;
	private const string stateName = "GameplayStartPhase";
    private bool popupAppeared = false;
	public GameplayStartPhaseState(GameplayPhaseSM stateMachine) : base(stateName, stateMachine)
	{
		gameplayPhaseSM = stateMachine as GameplayPhaseSM;
	}


	public override void Enter()
	{
		base.Enter();
        PhaseDisplay.instance.TurnOnDisplay("Start phase!", 1.5f);
        PhaseDisplay.instance.displayTimeCompleted += ActivateStartTurnPopup;
	}

	public override void UpdateLogic()
	{
		base.UpdateLogic();
	}

    public override void Exit()
    {
        base.Exit();
        PhaseDisplay.instance.displayTimeCompleted -= ActivateStartTurnPopup;
        popupAppeared = false;
    }

    public void CheckIfMusicNeedsToChangeBoard()
    {
        if(!StartDebugMenu.instance.turnOffPanel)
        {
            //Play menu music here.
            return;
        }

        //Check audiosources in the audiomanager. If the song we want to play is in OUR AudioData:
        //Check if it is the current one playing already. If it is, do nothing.
        //If it is NOT: (Example: Same set of songs that have lvl 1,3,5 variations; last player was level 3 and we are not at least level 3.) Swap to the correct one but keep same audio sources.
        //If the first point is NOT true. We need to swap entire set of audiosources to the ones related to our data and tell it which one to play.

        AudioData ourRowAudioData = gameplayPhaseSM.gameplayManager.GetCurrentAreaPlayerIsInAudioData();

        //Could be a special space like the conference room so basically just do what we would do if the music does not need to change.
        if(ourRowAudioData == null)
        {
            Debug.LogWarning($"Couldn't find a row that matched the space we are on. This is probably bad!");
            return;
        }


        if(Audio_Manager.Instance.CurrentlyPlayingMusicInformation.CurrentlyPlayingTrackSource == null)
        {
            //Pass in our audioData and play music from there since it's the first time we're playing music.
            Audio_Manager.Instance.SetupMusicTracks(ourRowAudioData);
        }

        bool isSameMusicAlreadyPlaying = false;
        //Checking for if the music is already playing. If it is: We do nothing. If it's not, then audio_manager will hafta take care of some lifting to change the track. We will pass in our audioData.
        //foreach(AudioSource audioSource in Audio_Manager.Instance.MusicSources)
        //{
        //    if(isSameMusicAlreadyPlaying)
        //    {
        //        break;
        //    }

        //    foreach(AudioData.MusicClip musicClip in ourRowAudioData.MusicClips)
        //    {
        //        if(musicClip.Clip == audioSource.clip)
        //        {
        //            isSameMusicAlreadyPlaying = true;
        //            break;
        //        }
        //    }
        //}

        //Method on Audio_manager that takes in a audioData scriptable and we fade to that track.
        
    }

    public void ActivateStartTurnPopup()
    {
        CheckIfMusicNeedsToChangeBoard();

        List<Tuple<string, string, object, List<object>>> insertedParams = new();

        List<object> paramsList = new List<object>();
        paramsList.Add(gameplayPhaseSM.gameplayStartResolveSpacePhaseState);

        insertedParams.Add(Tuple.Create<string, string, object, List<object>>("Start!", nameof(gameplayPhaseSM.StartTurnConfirmation), gameplayPhaseSM, paramsList));

        DialogueBoxPopup.instance.ActivatePopupWithButtonChoices($"Player {gameplayPhaseSM.gameplayManager.GetCurrentPlayer().playerIDIntVal}'s turn!", insertedParams, 1, "Turn start!");
    }
}
