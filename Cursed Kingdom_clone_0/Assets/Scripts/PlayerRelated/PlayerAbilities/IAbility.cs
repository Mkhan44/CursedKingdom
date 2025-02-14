//Code written by Mohamed Riaz Khan of BukuGames.
//All code is written by me (Above name) unless otherwise stated via comments below.
//Not authorized for use outside of the Github repository of this game developed by BukuGames.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IAbility
{
    /// <summary>
    /// Activation of the effect that this ability will have.
    /// </summary>
    /// <param name="playerReference">The player who's ability is being used.</param>
    public abstract void ActivateEffect(Player playerReference);

    public abstract void CompletedEffect(Player playerReference);

}
