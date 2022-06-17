using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class TestCardMoveButton : MonoBehaviour
{
    public int testMoveNumber;
    public TextMeshProUGUI moveText;

    public TestManager testManagerRef;
    private void Start()
    {
        testManagerRef = GameObject.Find("TestManager").GetComponent<TestManager>();
        if(testManagerRef != null)
        {
            this.GetComponent<Button>().onClick.AddListener(() => testManagerRef.StartMove(testMoveNumber));
        }
        else
        {
            Debug.LogWarning("Hey, the testManager reference is null!");
        }
        
    }
    private void OnValidate()
    {
        if(testMoveNumber <= 0 || testMoveNumber > 10)
        {
            Debug.LogWarning("Hey, invalid movement number!", this);
            return;
        }

        moveText.text = $"Move {testMoveNumber}";
    }
}
