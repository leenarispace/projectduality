using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TargetType
{
    Self,
    SingleAlly,
    AllAllies,
    SingleEnemy,
    AllEnemies,
    All
}

[CreateAssetMenu(fileName = "New Ability", menuName = "Combat/Ability")]
public class Ability : ScriptableObject
{
    [Header("Basic Info")]
    [SerializeField] private string abilityName;
    [SerializeField][TextArea] private string description;
    [SerializeField] private Sprite icon;
    
    [Header("Cost")]
    [SerializeField] private int angerCost;
    
    [Header("Targeting")]
    [SerializeField] private TargetType targetType;
    
    [Header("Effects")]
    [SerializeField] private int baseDamage;
    [SerializeField] private int bloodHealing;
    [SerializeField] private int angerGeneration;
    [SerializeField] private StatusEffectData[] statusEffects;
    
    [Header("Visual Effects")]
    [SerializeField] private GameObject visualEffectPrefab;
    [SerializeField] private AudioClip soundEffect;
    
    // Reference to the owner of this ability
    private Combatant owner;
    
    public string Name => abilityName;
    public string Description => description;
    public Sprite Icon => icon;
    public int AngerCost => angerCost;
    public TargetType TargetingType => targetType;
    
    public void Initialize(Combatant abilityOwner)
    {
        owner = abilityOwner;
    }
    
    public void Execute(Combatant user, Combatant target)
    {
        // Apply damage
        if (baseDamage > 0)
        {
            // Calculate final damage considering any modifiers
            int finalDamage = CalculateDamage(user, target);
            target.TakeDamage(finalDamage, user);
            
            // Show visual effect
            if (visualEffectPrefab != null)
            {
                GameObject effect = Instantiate(visualEffectPrefab, target.transform.position, Quaternion.identity);
                Destroy(effect, 2f);
            }
            
            // Play sound effect
            if (soundEffect != null)
            {
                AudioSource.PlayClipAtPoint(soundEffect, target.transform.position);
            }
        }
        
        // Apply healing
        if (bloodHealing > 0)
        {
            // Implement healing logic
            // Since we don't have a specific Heal method in Combatant, we would need to create one
            // or implement healing through a different mechanism
            
            // For example:
            // target.Heal(bloodHealing);
        }
        
        // Generate anger
        if (angerGeneration > 0)
        {
            target.GainAnger(angerGeneration);
        }
        
        // Apply status effects
        foreach (var effectData in statusEffects)
        {
            StatusEffect effect = effectData.CreateEffect();
            target.AddStatusEffect(effect);
        }
    }
    
    private int CalculateDamage(Combatant user, Combatant target)
    {
        // Base calculation
        int damage = baseDamage;
        
        // Add modifiers based on user's anger (for example, more anger = more damage)
        float angerMultiplier = 1.0f + (user.GetCurrentAnger() / (float)user.GetMaxAnger() * 0.5f);
        damage = Mathf.RoundToInt(damage * angerMultiplier);
        
        // Could add critical hit chance, elemental effects, etc. here
        
        return damage;
    }
    
    public bool IsValidTarget(Combatant potentialTarget)
    {
        switch (targetType)
        {
            case TargetType.Self:
                return potentialTarget == owner;
                
            case TargetType.SingleAlly:
                return potentialTarget.IsPlayerControlled;
                
            case TargetType.AllAllies:
                return potentialTarget.IsPlayerControlled;
                
            case TargetType.SingleEnemy:
                return !potentialTarget.IsPlayerControlled;
                
            case TargetType.AllEnemies:
                return !potentialTarget.IsPlayerControlled;
                
            case TargetType.All:
                return true;
                
            default:
                return false;
        }
    }
    
    public List<Combatant> ValidTargets()
    {
        List<Combatant> validTargets = new List<Combatant>();
        
        // Get all combatants from CombatManager
        List<Combatant> allCombatants = CombatManager.Instance.GetAllCombatants();
        
        // Filter based on targeting type
        foreach (var combatant in allCombatants)
        {
            if (IsValidTarget(combatant))
            {
                validTargets.Add(combatant);
            }
        }
        
        return validTargets;
    }
}