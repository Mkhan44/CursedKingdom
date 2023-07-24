//Code written by Mohamed Riaz Khan of BukuGames.
//All code is written by me (Above name) unless otherwise stated via comments below.
//Not authorized for use outside of the Github repository of this game developed by BukuGames.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelUpSpaceEffect", menuName = "Space Effect Data/Level Up Space", order = 0)]
public class LevelUpSpace : SpaceEffectData, ISpaceEffect
{
    [Space(10f)]
    [Range(1, 5)] [SerializeField] private int levelsToIncrease = 1;
    [Range(1, 5)] [SerializeField] private int numHealthToRecover = 1;
    [SerializeField] private ClassData.ClassType classTypeNeeded;

    public int LevelsToIncrease { get => levelsToIncrease; set => levelsToIncrease = value; }
    public int NumHealthToRecover { get => numHealthToRecover; set => numHealthToRecover = value; }
    public ClassData.ClassType ClassTypeNeeded { get => classTypeNeeded; set => classTypeNeeded = value; }

    //NEED TO ENSURE THAT SPACES-TO-LEVELUP ARE RESET ONCE THIS EFFECT IS TRIGGERED.
    public override void LandedOnEffect(Player playerReference)
    {
        if(playerReference.AbleToLevelUp && playerReference.ClassData.classType == ClassTypeNeeded)
        {
            playerReference.LevelUp(levelsToIncrease);
            Debug.Log($"Landed on: {this.name} space and level should be increased by: {LevelsToIncrease} and health recovered by: {NumHealthToRecover}");
        }
        base.LandedOnEffect(playerReference);

        //Reset player's spaces to the level up space so they can't get the effect again until they've traveled around the board.
    }

    public override void StartOfTurnEffect(Player playerReference)
    {
        base.StartOfTurnEffect(playerReference);
    }

    public override void EndOfTurnEffect(Player playerReference)
    {
        base.EndOfTurnEffect(playerReference);
    }
    protected override void UpdateEffectDescription()
    {
        if (!OverrideAutoDescription)
        {
            EffectDescription = $"Level increased by: {LevelsToIncrease} and health recovered by: {NumHealthToRecover} if you land on/pass this space when being the correct class.";
        }
    }

    private void OnEnable()
    {
        UpdateEffectDescription();
    }
}
