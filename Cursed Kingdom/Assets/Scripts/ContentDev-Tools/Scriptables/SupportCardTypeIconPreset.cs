//Code written by Mohamed Riaz Khan of BukuGames.
//All code is written by me (Above name) unless otherwise stated via comments below.
//Not authorized for use outside of the Github repository of this game developed by BukuGames.

using UnityEngine;

[CreateAssetMenu(fileName = "SpaceIconPreset", menuName = "Content Tools/Support Card Type Icon Preset data", order = 0)]
public class SupportCardTypeIconPreset : ScriptableObject
{
    [SerializeField] private Sprite movementSprite;
    [SerializeField] private Sprite duelSprite;
    [SerializeField] private Sprite specialSprite;
    public Sprite MovementSprite { get => movementSprite;}
    public Sprite DuelSprite { get => duelSprite;}
    public Sprite SpecialSprite { get => specialSprite;}
}
