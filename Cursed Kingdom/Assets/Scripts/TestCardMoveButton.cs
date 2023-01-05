//Code written by Mohamed Riaz Khan of BukuGames.
//All code is written by me (Above name) unless otherwise stated via comments below.
//Not authorized for use outside of the Github repository of this game developed by BukuGames.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class TestCardMoveButton : MonoBehaviour
{
    public enum MoveButtonType
    {
        Movement,
        Direction,
    }

    public int testMoveNumber;
    public TextMeshProUGUI moveText;
    public MoveButtonType buttonType;
    public GameObject parentObj;

    public GameplayManager gameManagerRef;
    private void Start()
    {
        gameManagerRef = GameObject.Find("Game Manager").GetComponent<GameplayManager>();
        if(gameManagerRef != null)
        {
            if (buttonType == MoveButtonType.Movement)
            {
                parentObj = transform.parent.gameObject;
                this.GetComponent<Button>().onClick.AddListener(() => gameManagerRef.StartMove(testMoveNumber));
            }
            //If it's supposed to be an arrow.
            //else
            //{
            //    gameObject.SetActive(false);
            //}

            
        }
        else
        {
            Debug.LogWarning("Hey, the testManager reference is null!");
        }


        
    }
    private void OnValidate()
    {
        if(buttonType == MoveButtonType.Movement)
        {
            if (testMoveNumber <= 0 || testMoveNumber > 10)
            {
                Debug.LogWarning("Hey, invalid movement number!", this);
                return;
            }

            moveText.text = $"Move {testMoveNumber}";
        }
      
    }

    public void TurnOffParent()
    {
        if(parentObj != null)
        {
            parentObj.SetActive(false);
        }
    }
}
