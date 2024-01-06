//Code written by Mohamed Riaz Khan of BukuGames.
//All code is written by me (Above name) unless otherwise stated via comments below.
//Not authorized for use outside of the Github repository of this game developed by BukuGames.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A player can deal damage to an opponent after meeting certain conditions such as discarding cards. The distance can also be determined for how far opponents can be.
/// </summary>


[CreateAssetMenu(fileName = "AttackEnemiesEliteAbility", menuName = "Player/EliteAbility/AttackEnemies", order = 0)]
public class AttackEnemiesEliteAbility : EliteAbilityData, IEliteAbility
{
    [Range(1,10)] [SerializeField] private int damageToGive = 1;
    [Range(0, 10)][SerializeField] private int rangeOfSpacesBetweenPlayerAndEnemies;
    [SerializeField] private bool attackAllEnemiesInRange;
    [SerializeField] private Card.CardType cardTypeToDiscard;
    [SerializeField] [Range(0, 10)] private int numCardsToDiscard;
    [Range(1, 3)][SerializeField] private int numPlayersToAttack = 1;
    [Tooltip("Player can choose the opponents they want to attack. This will prompt the game to give a choice box.")]
    [SerializeField] private bool opponentsCanBeChosen = true;
    [Tooltip("Attacks all other players. This overrides 'numPlayersToAttack' and 'opponentsCanBeChosen' will be irrelevant.")]
    [SerializeField] private bool attackAllPlayers;

    public int DamageToGive { get => damageToGive; set => damageToGive = value; }
    public int RangeOfSpacesBetweenPlayerAndEnemies { get => rangeOfSpacesBetweenPlayerAndEnemies; set => rangeOfSpacesBetweenPlayerAndEnemies = value; }
    public bool AttackAllEnemiesInRange { get => attackAllEnemiesInRange; set => attackAllEnemiesInRange = value; }
    public Card.CardType CardTypeToDiscard { get => cardTypeToDiscard; set => cardTypeToDiscard = value; }
    public int NumCardsToDiscard { get => numCardsToDiscard; set => numCardsToDiscard = value; }
    public int NumPlayersToAttack { get => numPlayersToAttack; set => numPlayersToAttack = value; }
    public bool OpponentsCanBeChosen { get => opponentsCanBeChosen; set => opponentsCanBeChosen = value; }
    public bool AttackAllPlayers { get => attackAllPlayers; set => attackAllPlayers = value; }

    public override void ActivateEffect(Player playerReference)
    {
        playerReference.DoneDiscardingForEffect += AttackPortionOfEffect;
        playerReference.DoneAttackingForEffect += CompletedEffect;
        DiscardPortionOfEffect(playerReference);
    }
    public override bool CanCostBePaid(Player playerReference)
    {
        bool canCostBePaid = false;
        string messageToPopupIfCostCantBePaid = string.Empty;
        if(NumCardsToDiscard > 0)
        {
            if(CardTypeToDiscard == Card.CardType.Movement)
            {
                if(playerReference.MovementCardsInHandCount >= NumCardsToDiscard)
                {
                    canCostBePaid = true;
                }
                else
                {
                    messageToPopupIfCostCantBePaid = $"You need to be able to discard at least {numCardsToDiscard} movement card(s) to activate this elite ability!";
                }
            }
            else if(CardTypeToDiscard == Card.CardType.Support)
            {
                if(playerReference.SupportCardsInHandCount >= NumCardsToDiscard)
                {
                    canCostBePaid = true;
                }
                else
                {
                    messageToPopupIfCostCantBePaid = $"You need to be able to discard at least {numCardsToDiscard} support card(s) to activate this elite ability!";
                }
            }
            else if(CardTypeToDiscard == Card.CardType.Both)
            {
                if(playerReference.CardsInhand.Count >= NumCardsToDiscard)
                {
                    canCostBePaid = true;
                }
                else
                {
                    messageToPopupIfCostCantBePaid = $"You need to be able to discard at least {numCardsToDiscard} card(s) to activate this elite ability!";
                }
            }
        }
        if(!canCostBePaid)
        {
            DialogueBoxPopup.instance.ActivatePopupWithJustText(messageToPopupIfCostCantBePaid, 2.5f);
        }

        return canCostBePaid;
    }

    public override void CompletedEffect(Player playerReference)
    {
        base.CompletedEffect(playerReference);
        playerReference.DoneDiscardingForEffect -= AttackPortionOfEffect;
        playerReference.DoneAttackingForEffect -= CompletedEffect;
    }

    protected override void UpdateEffectDescription()
    {
        if (!OverrideAutoDescription)
        {
            EffectDescription = $"Attack a player by discarding a card.";
        }
    }

    private void OnEnable()
    {
        UpdateEffectDescription();
    }

    private void DiscardPortionOfEffect(Player playerReference)
    {
        playerReference.SetCardsToDiscard(CardTypeToDiscard, NumCardsToDiscard);
        if (CardTypeToDiscard != Card.CardType.Both)
        {
            DialogueBoxPopup.instance.ActivatePopupWithJustText($"Please select {NumCardsToDiscard} {CardTypeToDiscard} card(s) to discard.");
        }
        else
        {
            DialogueBoxPopup.instance.ActivatePopupWithJustText($"Please select {NumCardsToDiscard} Movement and/or Support cards to discard.");
        }
    }

    /// <summary>
    /// Does math to figure out which opponents are valid targets. Sends that information to Player script.
    /// </summary>
    /// <param name="playerReference"></param>
    private void AttackPortionOfEffect(Player playerReference)
    {
        if (AttackAllPlayers)
        {
            playerReference.AttackAllOtherPlayersDamage(DamageToGive);
            return;
        }

        if (OpponentsCanBeChosen && RangeOfSpacesBetweenPlayerAndEnemies > 0)
        {
            List<Player> validTargets = new();
            validTargets = FindValidTargets(playerReference);
            playerReference.ActivatePlayerWithTargetSelectionToAttackDamageSelectionPopup(validTargets, DamageToGive);
            return;
        }

        if(RangeOfSpacesBetweenPlayerAndEnemies > 0 && AttackAllEnemiesInRange)
        {
            List<Player> validTargets = new();
            validTargets = FindValidTargets(playerReference);
            playerReference.AttackAllValidTargetsNoElement(validTargets, DamageToGive);
            return;
        }

        if(OpponentsCanBeChosen)
        {
            playerReference.ActivatePlayerToAttackDamageSelectionPopup(NumPlayersToAttack, DamageToGive);
            return;
        }
    }

    private List<Player> FindValidTargets(Player playerReference)
    {
        List<Player> validTargets = new();

        //Loop through all spaces up to the range in every direction and see if there is a Player on that space.
        if (RangeOfSpacesBetweenPlayerAndEnemies > 0)
        {
            List<Space> spacesToCheckNext = new();
            List<Space> spacesAlreadyChecked = new();
            for (int i = 0; i < RangeOfSpacesBetweenPlayerAndEnemies; i++)
            {
                //We have obtained all other players in the game. No need to continue looping.
                if ((validTargets.Count == numPlayersToAttack && !AttackAllEnemiesInRange) || validTargets.Count >= playerReference.GameplayManagerRef.Players.Count -1)
                {
                    break;
                }

                if(i == 0)
                {
                    //Loop through all players on the current player's space. If we find any BESIDES the player, add them as a target.
                    foreach(Player player in playerReference.CurrentSpacePlayerIsOn.playersOnThisSpace)
                    {
                        if(player != playerReference && !validTargets.Contains(player))
                        {
                            validTargets.Add(player);
                        }
                        spacesAlreadyChecked.Add(playerReference.CurrentSpacePlayerIsOn);
                        spacesToCheckNext = GetSpacesToCheckFromCurrentSpace(playerReference.CurrentSpacePlayerIsOn, spacesAlreadyChecked);
                    }
                }

                //This temporary list needs to be used because we can't change 'spacesToCheckNext' while in the foreach. We will change it after the foreach ends.
                List<Space> tempSpacesToCheckNext = new();

                foreach (Space space in spacesToCheckNext)
                {
                    foreach(Player player in space.playersOnThisSpace)
                    {
                        if(player != playerReference && ! validTargets.Contains(player))
                        {
                            validTargets.Add(player);
                        }
                    }

                    if(!spacesAlreadyChecked.Contains(space))
                    {
                        spacesAlreadyChecked.Add(space);
                    }

                    List<Space> superTempSpacesToCheckNext = GetSpacesToCheckFromCurrentSpace(space, spacesAlreadyChecked);

                    foreach(Space superSpace in superTempSpacesToCheckNext)
                    {
                        if(!tempSpacesToCheckNext.Contains(superSpace))
                        {
                            tempSpacesToCheckNext.Add(superSpace);
                        }
                    }
                }

                spacesToCheckNext = tempSpacesToCheckNext;
            }
        }
        return validTargets;
    }

    private List<Space> GetSpacesToCheckFromCurrentSpace(Space spaceToGetNeighborsFrom, List<Space> spacesAlreadyChecked)
    {
        List<Space> spacesToCheckNext = new();

        //Check all 4 neighbors.
        if(spaceToGetNeighborsFrom.NorthNeighbor != null && !spacesAlreadyChecked.Contains(spaceToGetNeighborsFrom.NorthNeighbor))
        {
            spacesToCheckNext.Add(spaceToGetNeighborsFrom.NorthNeighbor);
        }

        if (spaceToGetNeighborsFrom.SouthNeighbor != null && !spacesAlreadyChecked.Contains(spaceToGetNeighborsFrom.SouthNeighbor))
        {
            spacesToCheckNext.Add(spaceToGetNeighborsFrom.SouthNeighbor);
        }

        if (spaceToGetNeighborsFrom.EastNeighbor != null && !spacesAlreadyChecked.Contains(spaceToGetNeighborsFrom.EastNeighbor))
        {
            spacesToCheckNext.Add(spaceToGetNeighborsFrom.EastNeighbor);
        }

        if (spaceToGetNeighborsFrom.WestNeighbor != null && !spacesAlreadyChecked.Contains(spaceToGetNeighborsFrom.WestNeighbor))
        {
            spacesToCheckNext.Add(spaceToGetNeighborsFrom.WestNeighbor);
        }

        return spacesToCheckNext;
    }
}
