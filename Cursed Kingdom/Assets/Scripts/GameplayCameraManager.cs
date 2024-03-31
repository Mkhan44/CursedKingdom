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
    [SerializeField] private float timeToWaitForCastingPlayerToBeginCasting;
    [SerializeField] private float timeToWaitForCutInCamToPopup;
    [SerializeField] private float timeToWaitAfterCutInCamPopupBeforePlayerHurtAnimationPlays;
    [SerializeField] private float timeToWaitForCutInCamToGoAway;

    [Header("Particle effects")]
    [SerializeField] private GameObject playerCastParticle;
    [SerializeField] private GameObject playerAttackedParticle;
    [SerializeField] private GameObject playerCursedParticle;
    [SerializeField] private GameObject playerPoisonedParticle;

    public CinemachineVirtualCamera CutInPlayerCam { get => cutInPlayerCam; set => cutInPlayerCam = value; }
    public CinemachineVirtualCamera CurrentPlayerFollowVirtualCam { get => currentPlayerFollowVirtualCam; set => currentPlayerFollowVirtualCam = value; }
    public Camera CutInCamera { get => cutInCamera; set => cutInCamera = value; }
    public RawImage CutInCameraDisplayRawImage { get => cutInCameraDisplayRawImage; set => cutInCameraDisplayRawImage = value; }
    public float CurrentPlayerCamStartingFOV { get => currentPlayerCamStartingFOV; set => currentPlayerCamStartingFOV = value; }
    public float CurrentPlayerCamZoomFOV { get => currentPlayerCamZoomFOV; set => currentPlayerCamZoomFOV = value; }
    public float TimeToWaitForCastingPlayerToBeginCasting { get => timeToWaitForCastingPlayerToBeginCasting; set => timeToWaitForCastingPlayerToBeginCasting = value; }
    public float TimeToWaitForCutInCamToPopup { get => timeToWaitForCutInCamToPopup; set => timeToWaitForCutInCamToPopup = value; }
    public float TimeToWaitAfterCutInCamPopupBeforePlayerHurtAnimationPlays { get => timeToWaitAfterCutInCamPopupBeforePlayerHurtAnimationPlays; set => timeToWaitAfterCutInCamPopupBeforePlayerHurtAnimationPlays = value; }
    public float TimeToWaitForCutInCamToGoAway { get => timeToWaitForCutInCamToGoAway; set => timeToWaitForCutInCamToGoAway = value; }
    public GameObject PlayerCastParticle { get => playerCastParticle; set => playerCastParticle = value; }
    public GameObject PlayerAttackedParticle { get => playerAttackedParticle; set => playerAttackedParticle = value; }
    public GameObject PlayerCursedParticle { get => playerCursedParticle; set => playerCursedParticle = value; }
    public GameObject PlayerPoisonedParticle { get => playerPoisonedParticle; set => playerPoisonedParticle = value; }

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
        currentPlayer.HideHand();
        yield return new WaitForSeconds(TimeToWaitForCastingPlayerToBeginCasting);
        GameObject castParticle = Instantiate(PlayerCastParticle, currentPlayer.transform);
        currentPlayer.Animator.SetBool(Player.ISCASTING, true);
        float castAnimationTime = 0;
        
        foreach(AnimationClip animationClip in currentPlayer.Animator.runtimeAnimatorController.animationClips)
        {
            if (animationClip.name.ToLower() == currentPlayer.Animator.GetCurrentAnimatorClipInfo(0)[0].clip.name)
            {
                castAnimationTime = animationClip.length;
            }
        }

        //Animation of player casting attack starts.
        //wait for about 5 frames.
        yield return new WaitForSeconds(timeToWaitForCutInCamToPopup);

        

        //Turn on camera.
        TurnOnVirtualCutInCamera();
        ChangeVirtualCutInCameraTarget(targetPlayer.transform);

        //Delay a bit to give some visual on the Targetplayer and establish they are being targeted.
        yield return new WaitForSeconds(TimeToWaitAfterCutInCamPopupBeforePlayerHurtAnimationPlays);

        GameObject attackParticle = Instantiate(PlayerAttackedParticle, targetPlayer.transform);
        targetPlayer.Animator.SetBool(Player.ISHURT, true);
        float hurtAnimationTime = 0f;
        foreach (AnimationClip animationClip in targetPlayer.Animator.runtimeAnimatorController.animationClips)
        {
            if (animationClip.name.ToLower() == "thiefcast")
            {
                hurtAnimationTime = animationClip.length;
            }
        }
        //Wait for animation of virtual camera coming on.

        yield return new WaitForSeconds(hurtAnimationTime);
        //Play opponent hurt animation.
        targetPlayer.TakeDamage(damageToGive);

        
        //Wait for 1-2 seconds.
        yield return new WaitForSeconds(TimeToWaitForCutInCamToGoAway);

        //Player cam return to original zoom.
        CurrentPlayerFollowVirtualCam.m_Lens.FieldOfView = CurrentPlayerCamStartingFOV;
        TurnOffVirtualCutInCamera();
        currentPlayer.ShowHand();
        currentPlayer.Animator.SetBool(Player.ISCASTING, false);
        targetPlayer.Animator.SetBool(Player.ISHURT, false);
        Destroy(castParticle);
        Destroy(attackParticle);

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
        currentPlayer.HideHand();
        yield return new WaitForSeconds(TimeToWaitForCastingPlayerToBeginCasting);
        GameObject castParticle = Instantiate(PlayerCastParticle, currentPlayer.transform);
        currentPlayer.Animator.SetBool(Player.ISCASTING, true);
        float castAnimationTime = 0;

        foreach (AnimationClip animationClip in currentPlayer.Animator.runtimeAnimatorController.animationClips)
        {
            if (animationClip.name.ToLower() == currentPlayer.Animator.GetCurrentAnimatorClipInfo(0)[0].clip.name)
            {
                castAnimationTime = animationClip.length;
            }
        }

        //Animation of player casting attack starts.
        //wait for about 5 frames.
        yield return new WaitForSeconds(timeToWaitForCutInCamToPopup);



        //Turn on camera.
        TurnOnVirtualCutInCamera();
        ChangeVirtualCutInCameraTarget(targetPlayer.transform);

        //Delay a bit to give some visual on the Targetplayer and establish they are being targeted.
        yield return new WaitForSeconds(TimeToWaitAfterCutInCamPopupBeforePlayerHurtAnimationPlays);
        GameObject attackParticle = null;

        if (typeOfStatusEffect == "curse")
        {
            attackParticle = Instantiate(PlayerCursedParticle, targetPlayer.transform);

        }
        else
        {
            attackParticle = Instantiate(PlayerPoisonedParticle, targetPlayer.transform);
        }

        
        targetPlayer.Animator.SetBool(Player.ISHURT, true);
        float hurtAnimationTime = 0f;
        foreach (AnimationClip animationClip in targetPlayer.Animator.runtimeAnimatorController.animationClips)
        {
            if (animationClip.name.ToLower() == "thiefcast")
            {
                hurtAnimationTime = animationClip.length;
            }
        }
        //Wait for animation of virtual camera coming on.

        yield return new WaitForSeconds(hurtAnimationTime);
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
        currentPlayer.ShowHand();
        currentPlayer.Animator.SetBool(Player.ISCASTING, false);
        targetPlayer.Animator.SetBool(Player.ISHURT, false);
        Destroy(castParticle);
        Destroy(attackParticle);

        //yield return new WaitForSeconds(0.3f);
        //End
        if (currentPlayer.IsHandlingSpaceEffects || currentPlayer.IsHandlingSupportCardEffects)
        {
            currentPlayer.CompletedAttackingEffect();
        }
    }
}
