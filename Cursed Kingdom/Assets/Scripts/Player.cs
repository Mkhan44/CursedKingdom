//Code written by Mohamed Riaz Khan of BukuGames.
//All code is written by me (Above name) unless otherwise stated via comments below.
//Not authorized for use outside of the Github repository of this game developed by BukuGames.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public int playerIDIntVal;

    //Properties
    private int maxHealth;
    private int currentHealth;
    private int currentLevel;
    private int spacesLeftToMove;
    private bool isMoving;
    private bool isOnCooldown;
    private bool isPoisoned;
    private bool isCursed;
    private int poisonDuration;
    private int curseDuration;
    private bool ableToLevelUp;
    private ClassData classData;
    private Space currentSpacePlayerIsOn;

    public int MaxHealth { get => maxHealth; set => maxHealth = value; }
    public int CurrentHealth { get => currentHealth; set => currentHealth = value; }
    public int CurrentLevel { get => currentLevel; set => currentLevel = value; }
    public int SpacesLeftToMove { get => spacesLeftToMove; set => spacesLeftToMove = value; }
    public bool IsMoving { get => isMoving; set => isMoving = value; }
    public bool IsOnCooldown { get => isOnCooldown; set => isOnCooldown = value; }
    public bool IsPoisoned { get => isPoisoned; set => isPoisoned = value; }
    public bool IsCursed { get => isCursed; set => isCursed = value; }
    public int PoisonDuration { get => poisonDuration; set => poisonDuration = value; }
    public int CurseDuration { get => curseDuration; set => curseDuration = value; }
    public bool AbleToLevelUp { get => ableToLevelUp; set => ableToLevelUp = value; }
    public ClassData ClassData { get => classData; set => classData = value; }
    public Space CurrentSpacePlayerIsOn { get => currentSpacePlayerIsOn; set => currentSpacePlayerIsOn = value; }

    public void InitializePlayer(ClassData data)
    {
        ClassData = data;

        MaxHealth = data.startingHealth;
        currentHealth = maxHealth;
        CurrentLevel = 1;
        AbleToLevelUp = false;
        SpacesLeftToMove = 0;
       // Debug.Log($"Player info: \n health = {CurrentHealth}, level = {CurrentLevel}, \n description: {data.description}");
    }

    public void LevelUp()
    {
        CurrentLevel += 1;
        AbleToLevelUp = false;
    }

    public void HandleLevelUp(int level)
    {
        switch(level)
        {
            case 2:
                {
                    Debug.Log("lol");
                    break;
                }
            default:
                {
                    Debug.Log("Def.");
                    break;
                }
        }
    }
}
