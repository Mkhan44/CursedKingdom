//Code written by Mohamed Riaz Khan of BukuGames.
//All code is written by me (Above name) unless otherwise stated via comments below.
//Not authorized for use outside of the Github repository of this game developed by BukuGames.
using System.Collections;
using System.Collections.Generic;
using PurrNet;
using UnityEngine;

public class BukuStateMachine : NetworkBehaviour
{
    private BaseState currentState;

    private void Start()
    {
        Invoke("Started" , 0.1f);
    }

    private void Started()
    {
        currentState = GetInitialState();
        if(currentState != null )
        {
            currentState.Enter();
        }
    }

    private void Update()
    {
        if (currentState != null)
        {
            currentState.UpdateLogic();
        }
    }

    private void LateUpdate()
    {
        if (currentState != null)
        {
            currentState.UpdatePhysics();
        }
    }

    public void ChangeState(BaseState newState)
    {
        currentState.Exit();

        currentState = newState;
        currentState.Enter();
    }

    protected virtual BaseState GetInitialState()
    {
        return null;
    }

    public virtual BaseState GetCurrentState()
    {
        return currentState;
    }



}
