//Code written by Mohamed Riaz Khan of BukuGames.
//All code is written by me (Above name) unless otherwise stated via comments below.
//Not authorized for use outside of the Github repository of this game developed by BukuGames.
using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameplayCameraManager : MonoBehaviour
{
    [SerializeField] private CinemachineVirtualCamera cutInPlayerCam;
    [SerializeField] private CinemachineVirtualCamera currentPlayerFollowVirtualCam;
    [SerializeField] private Camera cutInCamera;
    [SerializeField] private RawImage cutInCameraDisplayRawImage;

    [Header("Values to change at runtime")]
    [SerializeField] private float currentPlayerCamStartingFOV;
    [SerializeField] private float currentPlayerCamZoomFOV;
    [SerializeField] private float timeToWaitForCutInCamToPopup;
    [SerializeField] private float timeToWaitForCutInCamToGoAway;

    public CinemachineVirtualCamera CutInPlayerCam { get => cutInPlayerCam; set => cutInPlayerCam = value; }
    public CinemachineVirtualCamera CurrentPlayerFollowVirtualCam { get => currentPlayerFollowVirtualCam; set => currentPlayerFollowVirtualCam = value; }
    public Camera CutInCamera { get => cutInCamera; set => cutInCamera = value; }
    public RawImage CutInCameraDisplayRawImage { get => cutInCameraDisplayRawImage; set => cutInCameraDisplayRawImage = value; }
    public float CurrentPlayerCamStartingFOV { get => currentPlayerCamStartingFOV; set => currentPlayerCamStartingFOV = value; }
    public float CurrentPlayerCamZoomFOV { get => currentPlayerCamZoomFOV; set => currentPlayerCamZoomFOV = value; }
    public float TimeToWaitForCutInCamToPopup { get => timeToWaitForCutInCamToPopup; set => timeToWaitForCutInCamToPopup = value; }
    public float TimeToWaitForCutInCamToGoAway { get => timeToWaitForCutInCamToGoAway; set => timeToWaitForCutInCamToGoAway = value; }

    private void Start()
    {
        TurnOffVirtualCutInCamera();
    }

    public void TurnOnVirtualCutInCamera()
    {
        CutInPlayerCam.enabled = true;
        CutInCamera.enabled = true;
        CutInCameraDisplayRawImage.enabled = true;
    }

    public void TurnOffVirtualCutInCamera()
    {
        CutInPlayerCam.enabled = false;
        CutInCamera.enabled = false;
        CutInCameraDisplayRawImage.enabled = false;
    }

    public void ChangeVirtualCutInCameraTarget(Transform lookAtAndFollowTransform)
    {
        CutInPlayerCam.LookAt = lookAtAndFollowTransform;
        CutInPlayerCam.Follow = lookAtAndFollowTransform;
    }

    public IEnumerator DamageOpponentCutInPopup(Player currentPlayer, Player targetPlayer, int damageToGive)
    {
        yield return null;

        //Player cam zoom in.
        CurrentPlayerFollowVirtualCam.m_Lens.FieldOfView = CurrentPlayerCamZoomFOV;
        //Wait for about half a second.
      //  yield return new WaitForSeconds(0.5f);

        //Animation of player casting attack starts.
        //wait for about 5 frames.
        yield return new WaitForSeconds(TimeToWaitForCutInCamToPopup);

        //Turn on camera.
        TurnOnVirtualCutInCamera();
        ChangeVirtualCutInCameraTarget(targetPlayer.transform);

        //Wait for animation of virtual camera coming on.

        yield return new WaitForSeconds(0.3f);
        //Play opponent hurt animation.

        targetPlayer.TakeDamage(damageToGive);
        //Wait for 1-2 seconds.
        yield return new WaitForSeconds(TimeToWaitForCutInCamToGoAway);

        //Player cam return to original zoom.
        CurrentPlayerFollowVirtualCam.m_Lens.FieldOfView = CurrentPlayerCamStartingFOV;
        TurnOffVirtualCutInCamera();

        //yield return new WaitForSeconds(0.3f);
        //End
        if (currentPlayer.IsHandlingSpaceEffects || currentPlayer.IsHandlingSupportCardEffects)
        {
            currentPlayer.CompletedAttackingEffect();
        }
    }

    public IEnumerator StatusEffectOpponentCutInPopup(Player currentPlayer, Player targetPlayer, string typeOfStatusEffect, int statusDuration)
    {
        yield return null;

        //Player cam zoom in.
        CurrentPlayerFollowVirtualCam.m_Lens.FieldOfView = CurrentPlayerCamZoomFOV;
        //Wait for about half a second.
        //  yield return new WaitForSeconds(0.5f);

        //Animation of player casting attack starts.
        //wait for about 5 frames.
        yield return new WaitForSeconds(TimeToWaitForCutInCamToPopup);

        //Turn on camera.
        TurnOnVirtualCutInCamera();
        ChangeVirtualCutInCameraTarget(targetPlayer.transform);

        //Wait for animation of virtual camera coming on.

        yield return new WaitForSeconds(0.3f);
        //Play opponent hurt animation.

        if (typeOfStatusEffect == "curse")
        {
            targetPlayer.CursePlayer(statusDuration);
            targetPlayer.WasAfflictedWithStatusThisTurn = false;
            
        }
        else
        {
            targetPlayer.PoisonPlayer(statusDuration);
            targetPlayer.WasAfflictedWithStatusThisTurn = false;
        }

        //Wait for 1-2 seconds.
        yield return new WaitForSeconds(TimeToWaitForCutInCamToGoAway);

        //Player cam return to original zoom.
        CurrentPlayerFollowVirtualCam.m_Lens.FieldOfView = CurrentPlayerCamStartingFOV;
        TurnOffVirtualCutInCamera();

        //yield return new WaitForSeconds(0.3f);
        //End
        if (currentPlayer.IsHandlingSpaceEffects || currentPlayer.IsHandlingSupportCardEffects)
        {
            currentPlayer.CompletedAttackingEffect();
        }
    }
}
