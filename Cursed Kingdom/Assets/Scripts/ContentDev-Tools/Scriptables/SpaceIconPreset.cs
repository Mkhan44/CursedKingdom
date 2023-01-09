//Code written by Mohamed Riaz Khan of BukuGames.
//All code is written by me (Above name) unless otherwise stated via comments below.
//Not authorized for use outside of the Github repository of this game developed by BukuGames.

using UnityEngine;

[CreateAssetMenu(fileName = "SpaceIconPreset", menuName = "Content Tools/Space Icon Preset data", order = 0)]
public class SpaceIconPreset : ScriptableObject
{
    [SerializeField] private Sprite drawMovementCardSprite;
    [SerializeField] private Sprite drawSupportCardSprite;
    [SerializeField] private Sprite poisonSprite;
    [SerializeField] private Sprite curseSprite;
    [SerializeField] private Sprite recoverHealthSprite;
    [SerializeField] private Sprite loseHealthSprite;
    [SerializeField] private Sprite nonDuelSprite;
    [SerializeField] private Sprite specialAttackSprite;
    [SerializeField] private Sprite barricadeSprite;
    public Sprite DrawMovementCardSprite { get => drawMovementCardSprite; set => drawMovementCardSprite = value; }
    public Sprite DrawSupportCardSprite { get => drawSupportCardSprite; set => drawSupportCardSprite = value; }
    public Sprite PoisonSprite { get => poisonSprite; set => poisonSprite = value; }
    public Sprite CurseSprite { get => curseSprite; set => curseSprite = value; }
    public Sprite RecoverHealthSprite { get => recoverHealthSprite; set => recoverHealthSprite = value; }
    public Sprite LoseHealthSprite { get => loseHealthSprite; set => loseHealthSprite = value; }
    public Sprite NonDuelSprite { get => nonDuelSprite; set => nonDuelSprite = value; }
    public Sprite SpecialAttackSprite { get => specialAttackSprite; set => specialAttackSprite = value; }
    public Sprite BarricadeSprite { get => barricadeSprite; set => barricadeSprite = value; }
}
