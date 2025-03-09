using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class CombatantStats
{
    public int maxBlood;
    public int currentBlood;
    public int maxAnger;
    public int currentAnger;
}

public abstract class Combatant : MonoBehaviour
{
    [SerializeField] protected string combatantName;
    [SerializeField] protected CombatantStats stats;
    [SerializeField] protected bool isPlayerControlled;
    [SerializeField] protected List<Ability> abilities = new List<Ability>();
    
    // Events for UI updates and gameplay effects
    public UnityEvent<int, int> OnBloodChanged = new UnityEvent<int, int>(); // current, max
    public UnityEvent<int, int> OnAngerChanged = new UnityEvent<int, int>(); // current, max
    public UnityEvent<Combatant> OnDefeated = new UnityEvent<Combatant>();
    public UnityEvent<Ability, Combatant, Combatant> OnAbilityUsed = new UnityEvent<Ability, Combatant, Combatant>();
    
    protected List<StatusEffect> statusEffects = new List<StatusEffect>();
    
    public string Name => combatantName;
    public bool IsPlayerControlled => isPlayerControlled;
    public List<Ability> Abilities => abilities;
    
    public virtual void Initialize()
    {
        // Reset stats to starting values
        stats.currentBlood = stats.maxBlood;
        stats.currentAnger = 0; // Start with no anger
        
        // Initialize abilities
        foreach (var ability in abilities)
        {
            ability.Initialize(this);
        }
        
        // Broadcast initial stats
        OnBloodChanged.Invoke(stats.currentBlood, stats.maxBlood);
        OnAngerChanged.Invoke(stats.currentAnger, stats.maxAnger);
    }
    
    public abstract IEnumerator TakeTurn();
    
    public virtual void TakeDamage(int damage, Combatant source)
    {
        // Apply damage reduction from status effects if applicable
        foreach (var effect in statusEffects)
        {
            damage = effect.ModifyIncomingDamage(damage, source);
        }
        
        // Ensure damage is at least 1
        damage = Mathf.Max(1, damage);
        
        // Apply damage
        stats.currentBlood -= damage;
        stats.currentBlood = Mathf.Max(0, stats.currentBlood);
        
        // Generate anger from taking damage
        GainAnger(Mathf.CeilToInt(damage * 0.5f)); // Gain 50% of damage as anger
        
        // Broadcast stat changes
        OnBloodChanged.Invoke(stats.currentBlood, stats.maxBlood);
        
        // Check if defeated
        if (stats.currentBlood <= 0)
        {
            OnDefeated.Invoke(this);
        }
    }
    
    public virtual bool GainAnger(int amount)
    {
        // Apply anger gain modifiers from status effects
        foreach (var effect in statusEffects)
        {
            amount = effect.ModifyAngerGain(amount);
        }
        
        stats.currentAnger += amount;
        stats.currentAnger = Mathf.Min(stats.currentAnger, stats.maxAnger);
        
        // Broadcast stat changes
        OnAngerChanged.Invoke(stats.currentAnger, stats.maxAnger);
        
        return true;
    }
    
    public virtual bool SpendAnger(int cost)
    {
        if (stats.currentAnger >= cost)
        {
            stats.currentAnger -= cost;
            OnAngerChanged.Invoke(stats.currentAnger, stats.maxAnger);
            return true;
        }
        return false;
    }
    
    public virtual bool CanUseAbility(Ability ability)
    {
        return stats.currentAnger >= ability.AngerCost;
    }
    
    public virtual void UseAbility(Ability ability, Combatant target)
    {
        if (CanUseAbility(ability))
        {
            SpendAnger(ability.AngerCost);
            ability.Execute(this, target);
            OnAbilityUsed.Invoke(ability, this, target);
        }
    }
    
    public bool IsDefeated()
    {
        return stats.currentBlood <= 0;
    }
    
    public virtual void AddStatusEffect(StatusEffect effect)
    {
        statusEffects.Add(effect);
        effect.Apply(this);
    }
    
    public virtual void RemoveStatusEffect(StatusEffect effect)
    {
        if (statusEffects.Contains(effect))
        {
            effect.Remove(this);
            statusEffects.Remove(effect);
        }
    }
    
    public virtual void UpdateStatusEffects()
    {
        for (int i = statusEffects.Count - 1; i >= 0; i--)
        {
            StatusEffect effect = statusEffects[i];
            effect.Tick();
            
            if (effect.IsExpired)
            {
                effect.Remove(this);
                statusEffects.RemoveAt(i);
            }
        }
    }
    
    // Helper methods for UI and game logic
    public int GetCurrentBlood() => stats.currentBlood;
    public int GetMaxBlood() => stats.maxBlood;
    public int GetCurrentAnger() => stats.currentAnger;
    public int GetMaxAnger() => stats.maxAnger;
}