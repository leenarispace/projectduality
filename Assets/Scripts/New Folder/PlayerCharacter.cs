using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCharacter : Combatant
{
    [SerializeField] private CharacterData characterData;
    
    // Reference to the character's portrait/sprite for UI
    [SerializeField] private Sprite characterPortrait;
    
    // Character-specific flavor and backstory
    [SerializeField][TextArea] private string characterDescription;
    
    // Special abilities unique to this character
    [SerializeField] private List<Ability> uniqueAbilities = new List<Ability>();
    
    // Tracks if this character is currently selected for input
    private bool isAwaitingInput = false;
    
    // Selected ability and target for current turn
    private Ability selectedAbility;
    private Combatant selectedTarget;

    public Sprite Portrait => characterPortrait;
    public string Description => characterDescription;
    
    public override void Initialize()
    {
        base.Initialize();
        
        // Load data from CharacterData if available
        if (characterData != null)
        {
            combatantName = characterData.characterName;
            stats.maxBlood = characterData.maxBlood;
            stats.maxAnger = characterData.maxAnger;
            characterPortrait = characterData.portrait;
            characterDescription = characterData.description;
            
            // Add unique abilities from character data
            foreach (var ability in characterData.startingAbilities)
            {
                Ability newAbility = Instantiate(ability);
                newAbility.Initialize(this);
                abilities.Add(newAbility);
            }
        }
        
        // Add unique abilities defined in the inspector
        foreach (var ability in uniqueAbilities)
        {
            if (!abilities.Contains(ability))
            {
                ability.Initialize(this);
                abilities.Add(ability);
            }
        }
        
        // Reset current values
        stats.currentBlood = stats.maxBlood;
        stats.currentAnger = 0;
    }
    
    public override IEnumerator TakeTurn()
    {
        // Reset selection
        selectedAbility = null;
        selectedTarget = null;
        
        // Wait for player input
        isAwaitingInput = true;
        
        // UI should be updated to show this character is active and display available abilities
        CombatUI.Instance.ShowPlayerTurn(this);
        
        // Wait until player makes selections
        while (isAwaitingInput)
        {
            yield return null;
        }
        
        // Execute the selected ability on the selected target
        if (selectedAbility != null && selectedTarget != null)
        {
            UseAbility(selectedAbility, selectedTarget);
            
            // Give time for ability animation and effects
            yield return new WaitForSeconds(1.0f);
        }
        
        // End turn
        CombatManager.Instance.EndCurrentTurn();
    }
    
    // Called by UI buttons when player selects an ability
    public void SelectAbility(Ability ability)
    {
        if (CanUseAbility(ability))
        {
            selectedAbility = ability;
            
            // Update UI to show valid targets
            CombatUI.Instance.ShowTargetSelection(ability.ValidTargets());
        }
    }
    
    // Called by UI when player selects a target
    public void SelectTarget(Combatant target)
    {
        if (selectedAbility != null && selectedAbility.IsValidTarget(target))
        {
            selectedTarget = target;
            isAwaitingInput = false; // Selection complete, continue with turn
        }
    }
    
    // Self-harm ability - unique mechanic for your "anger" concept
    public void SelfHarm(int bloodCost)
    {
        // Take damage to self to gain anger
        TakeDamage(bloodCost, this);
        
        // Gain additional anger for self-harm (more than from enemy attacks)
        GainAnger(bloodCost); // 100% conversion rate for self-harm
    }
    
    // Cancel the current ability selection
    public void CancelAbilitySelection()
    {
        selectedAbility = null;
        CombatUI.Instance.ShowPlayerTurn(this); // Reset UI to ability selection
    }
    
    // Skip the current turn
    public void SkipTurn()
    {
        // Gain a small amount of anger for skipping
        GainAnger(5);
        isAwaitingInput = false;
    }
}