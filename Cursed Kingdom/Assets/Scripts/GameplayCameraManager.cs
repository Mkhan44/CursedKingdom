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
    [SerializeField] private CinemachineVirtualCamera duelVirtualCamera;
    [SerializeField] private Animator currentPlayerFollowVirtualCamAnimator;
    [SerializeField] private Camera cutInCamera;
    [SerializeField] private Camera duelCamera;
    [SerializeField] private Animator cutInCameraPlayerFollowVirtualCamAnimator;
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
    public CinemachineVirtualCamera DuelVirtualCamera { get => duelVirtualCamera; set => duelVirtualCamera = value; }
    public Animator CurrentPlayerFollowVirtualCamAnimator { get => currentPlayerFollowVirtualCamAnimator; set => currentPlayerFollowVirtualCamAnimator = value; }
    public Animator CutInCameraPlayerFollowVirtualCamAnimator { get => cutInCameraPlayerFollowVirtualCamAnimator; set => cutInCameraPlayerFollowVirtualCamAnimator = value; }
    public Camera CutInCamera { get => cutInCamera; set => cutInCamera = value; }
    public Camera DuelCamera { get => duelCamera; set => duelCamera = value; }
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
        CutInCameraPlayerFollowVirtualCamAnimator.SetBool("ZoomIn", true);
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

    public void TurnOnVirtualDuelCamera()
    {
        duelVirtualCamera.enabled = true;
        DuelCamera.enabled = true;
    }

    public void TurnOffVirtualDuelCamera()
    {
        duelVirtualCamera.enabled = false;
        DuelCamera.enabled = false;
    }

    public void SpawnCastParticle(Player currentPlayer, out GameObject particle)
    {
        particle = Instantiate(PlayerCastParticle, currentPlayer.transform);
    }
    public void SpawnAttackParticle(string typeOfParticle, Player targetPlayer, out GameObject particle)
    {
        particle = null;
        if(typeOfParticle.ToLower() == "attack")
        {
            particle = Instantiate(PlayerAttackedParticle, targetPlayer.transform);
        }
        else if(typeOfParticle.ToLower() == "poison")
        {
            particle = Instantiate(PlayerPoisonedParticle, targetPlayer.transform);
        }
        else if (typeOfParticle.ToLower() == "curse")
        {
            particle = Instantiate(PlayerCursedParticle, targetPlayer.transform);
        }
    }

    public IEnumerator DamageOpponentCutInPopup(Player currentPlayer, Player targetPlayer, int damageToGive)
    {
        //Player cam zoom in.
        CurrentPlayerFollowVirtualCamAnimator.SetBool("ZoomIn", true);

        currentPlayer.HideHand();
        yield return new WaitForSeconds(TimeToWaitForCastingPlayerToBeginCasting);
        GameObject castParticle;
        SpawnCastParticle(currentPlayer, out castParticle);
        

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

        GameObject attackParticle;
        SpawnAttackParticle("attack", targetPlayer, out attackParticle);


        //Play opponent hurt animation.
        targetPlayer.TakeDamage(damageToGive);

        targetPlayer.Animator.SetBool(Player.ISHURT, true);
        float hurtAnimationTime = 0f;
        foreach (AnimationClip animationClip in targetPlayer.Animator.runtimeAnimatorController.animationClips)
        {
            if (animationClip.name.ToLower() == "thiefhurt")
            {
                hurtAnimationTime = animationClip.length;
            }
        }
        //Wait for animation of virtual camera coming on.

        yield return new WaitForSeconds(hurtAnimationTime);


        //Wait for 1-2 seconds.
        yield return new WaitForSeconds(TimeToWaitForCutInCamToGoAway);
        CutInCameraPlayerFollowVirtualCamAnimator.SetBool("ZoomIn", false);

        float camAnimationTime = 0f;
        foreach (AnimationClip animationClip in CutInCameraPlayerFollowVirtualCamAnimator.runtimeAnimatorController.animationClips)
        {
            if (animationClip.name.ToLower() == "zoomout")
            {
                camAnimationTime = animationClip.length;
            }
        }

        yield return new WaitForSeconds(camAnimationTime);
        //Player cam return to original zoom.
        CurrentPlayerFollowVirtualCamAnimator.SetBool("ZoomIn", false);
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
        //Player cam zoom in.
        CurrentPlayerFollowVirtualCamAnimator.SetBool("ZoomIn", true);

        currentPlayer.HideHand();
        yield return new WaitForSeconds(TimeToWaitForCastingPlayerToBeginCasting);
        GameObject castParticle;
        SpawnCastParticle(currentPlayer, out castParticle);


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
        SpawnAttackParticle(typeOfStatusEffect, targetPlayer, out attackParticle);

        
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
        CutInCameraPlayerFollowVirtualCamAnimator.SetBool("ZoomIn", false);

        float camAnimationTime = 0f;
        foreach (AnimationClip animationClip in CutInCameraPlayerFollowVirtualCamAnimator.runtimeAnimatorController.animationClips)
        {
            if (animationClip.name.ToLower() == "zoomout")
            {
                camAnimationTime = animationClip.length;
            }
        }

        yield return new WaitForSeconds(camAnimationTime);
        //Player cam return to original zoom.
        CurrentPlayerFollowVirtualCamAnimator.SetBool("ZoomIn", false);
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
