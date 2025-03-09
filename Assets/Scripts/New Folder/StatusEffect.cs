using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum StatusEffectType
{
    Bleed,       // Lose blood over time
    Enrage,      // Generate more anger when damaged
    Protect,     // Reduce damage taken
    Weaken,      // Deal less damage
    Strengthen,  // Deal more damage
    Focus,       // Reduce anger cost of abilities
    Confusion,   // Chance to target allies
    Taunt        // Force enemies to target this character
}

[System.Serializable]
public class StatusEffectData
{
    public StatusEffectType type;
    public string name;
    public string description;
    public Sprite icon;
    public int duration;
    public float magnitude;
    
    public StatusEffect CreateEffect()
    {
        StatusEffect effect = new StatusEffect
        {
            Type = type,
            Name = name,
            Description = description,
            Icon = icon,
            Duration = duration,
            Magnitude = magnitude
        };
        
        return effect;
    }
}

[System.Serializable]
public class StatusEffect
{
    public StatusEffectType Type { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public Sprite Icon { get; set; }
    public int Duration { get; set; }
    public float Magnitude { get; set; }
    
    private Combatant target;
    
    public bool IsExpired => Duration <= 0;
    
    public void Apply(Combatant affectedTarget)
    {
        target = affectedTarget;
        
        // Apply immediate effects if needed
        switch (Type)
        {
            case StatusEffectType.Enrage:
                // Maybe trigger a visual effect
                break;
                
            // Handle other immediate effects
        }
    }
    
    public void Remove(Combatant affectedTarget)
    {
        // Clean up or apply final effects when status is removed
        switch (Type)
        {
            case StatusEffectType.Protect:
                // Maybe trigger a shield breaking effect
                break;
                
            // Handle other removal effects
        }
    }
    
    public void Tick()
    {
        // Reduce duration
        Duration--;
        
        // Apply per-turn effects
        switch (Type)
        {
            case StatusEffectType.Bleed:
                int bleedDamage = Mathf.RoundToInt(Magnitude);
                target.TakeDamage(bleedDamage, target); // Self-damage from bleeding
                break;
                
            // Handle other effects that trigger each turn
        }
    }
    
    public int ModifyIncomingDamage(int damage, Combatant source)
    {
        switch (Type)
        {
            case StatusEffectType.Protect:
                // Reduce damage by percentage based on magnitude
                return Mathf.RoundToInt(damage * (1 - Magnitude));
                
            case StatusEffectType.Weaken:
                // If the attacker has Weaken, they do less damage
                if (source == target)
                {
                    return Mathf.RoundToInt(damage * (1 - Magnitude));
                }
                break;
        }
        
        return damage;
    }
    
    public int ModifyOutgoingDamage(int damage)
    {
        switch (Type)
        {
            case StatusEffectType.Strengthen:
                // Increase damage by percentage based on magnitude
                return Mathf.RoundToInt(damage * (1 + Magnitude));
        }
        
        return damage;
    }
    
    public int ModifyAngerGain(int amount)
    {
        switch (Type)
        {
            case StatusEffectType.Enrage:
                // Increase anger gain by percentage based on magnitude
                return Mathf.RoundToInt(amount * (1 + Magnitude));
        }
        
        return amount;
    }
    
    public int ModifyAbilityCost(int cost)
    {
        switch (Type)
        {
            case StatusEffectType.Focus:
                // Reduce anger cost by percentage based on magnitude
                return Mathf.RoundToInt(cost * (1 - Magnitude));
        }
        
        return cost;
    }
    
    public bool ShouldRedirectTarget(Combatant originalTarget, out Combatant newTarget)
    {
        newTarget = originalTarget;
        
        switch (Type)
        {
            case StatusEffectType.Confusion:
                // Chance to target randomly based on magnitude
                if (Random.value < Magnitude)
                {
                    List<Combatant> possibleTargets = CombatManager.Instance.GetAllCombatants();
                    newTarget = possibleTargets[Random.Range(0, possibleTargets.Count)];
                    return true;
                }
                break;
                
            case StatusEffectType.Taunt:
                // Force attacks to this target
                newTarget = target;
                return true;
        }
        
        return false;
    }
}