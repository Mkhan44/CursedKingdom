//Code written by Mohamed Riaz Khan of BukuGames.
//All code is written by me (Above name) unless otherwise stated via comments below.
//Not authorized for use outside of the Github repository of this game developed by BukuGames.

using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class PlayerAI : MonoBehaviour
{
    [SerializeField] private Player playerReference;
    public int defaultAIDelaySpeedInMiliseconds = 1500;

    public Player PlayerReference { get => playerReference; set => playerReference = value; }

    #region Movement phase methods

    public async void ChooseMovementCardToUseMovementPhase()
    {
        await Task.Delay(defaultAIDelaySpeedInMiliseconds);
        playerReference.GameplayManagerRef.HandDisplayPanel.ExpandHand(Card.CardType.Movement);
        await Task.Delay(defaultAIDelaySpeedInMiliseconds);

        int movementCardToUseIndex = UnityEngine.Random.Range(0,PlayerReference.MovementCardsInHandCount);
        MovementCard movementCardToUse = PlayerReference.GetMovementCardsInHand()[movementCardToUseIndex];
        await Task.Delay(defaultAIDelaySpeedInMiliseconds);
        movementCardToUse.OnPointerClick(null);
        await Task.Delay(defaultAIDelaySpeedInMiliseconds);
        movementCardToUse.OnPointerClick(null);
        //Debug.Log($"Player is an AI and we are attempting to use a random movement card witht he value of {movementCardToUse.MovementCardValue}");
    }

    public async void DrawAndUseMovementCardMovementPhase()
    {
        await Task.Delay(defaultAIDelaySpeedInMiliseconds);
        playerReference.GameplayManagerRef.HandDisplayPanel.ExpandHand(Card.CardType.Movement);
        await Task.Delay(defaultAIDelaySpeedInMiliseconds);

        PlayerReference.NoMovementCardsInHandButton.onClick.RemoveAllListeners();
        PlayerReference.DrawThenUseMovementCardImmediatelyMovement();
    }


    #endregion

    #region Dialogue Box Selection methods

    public async void SelectFirstOptionDialogueBoxChoice()
    {
        await Task.Delay(defaultAIDelaySpeedInMiliseconds);
        List<Button> buttons = DialogueBoxPopup.instance.GetCurrentPopupChoices();
        buttons[0].onClick.Invoke();
    }

    public async void SelectLastOptionDialogueBoxChoice()
    {
        await Task.Delay(defaultAIDelaySpeedInMiliseconds);
        List<Button> buttons = DialogueBoxPopup.instance.GetCurrentPopupChoices();
        buttons[buttons.Count-1].onClick.Invoke();
    }

    public async void SelectRandomOptionDialogueBoxChoice()
    {
        await Task.Delay(defaultAIDelaySpeedInMiliseconds);

        List<Button> buttons = DialogueBoxPopup.instance.GetCurrentPopupChoices();

        int randomIndex = Random.Range(0, buttons.Count);
       // Debug.Log($"Player is an AI and we are attempting to select a random dialogue box option the value of {randomIndex}");
        buttons[randomIndex].onClick.Invoke();
    }

    #endregion
}
