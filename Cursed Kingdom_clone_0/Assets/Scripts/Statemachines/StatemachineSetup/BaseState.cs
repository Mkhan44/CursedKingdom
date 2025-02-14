//Code written by Mohamed Riaz Khan of BukuGames.
//All code is written by me (Above name) unless otherwise stated via comments below.
//Not authorized for use outside of the Github repository of this game developed by BukuGames.
using System;
using Unity.VisualScripting;
using UnityEngine;

public class BaseState
{
    public string name;
    protected BukuStateMachine stateMachine;

    public BaseState(string name, BukuStateMachine stateMachine)
    {
        this.name = name;
        this.stateMachine = stateMachine;
    }

    public virtual void Enter()
    {

    }

    public virtual void UpdateLogic()
    {

    }

    public virtual void UpdatePhysics()
    {

    }

    public virtual void Exit()
    {

    }


}
