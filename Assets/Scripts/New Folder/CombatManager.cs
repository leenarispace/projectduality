using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Linq;

public enum CombatState
{
    Initialize,
    StartTurn,
    PlayerTurn,
    EnemyTurn,
    EndTurn,
    Victory,
    Defeat,
    Escape
}

public class CombatManager : MonoBehaviour
{
    // Singleton instance
    public static CombatManager Instance { get; private set; }
    
    [Header("Combat Configuration")]
    [SerializeField] private Transform playerCharactersParent;
    [SerializeField] private Transform enemyCharactersParent;
    [SerializeField] private GameObject victoryPanel;
    [SerializeField] private GameObject defeatPanel;
    
    [Header("Narrative Events")]
    [SerializeField] private NarrativeEvent introEvent;
    [SerializeField] private NarrativeEvent victoryEvent;
    [SerializeField] private NarrativeEvent defeatEvent;
    
    // Lists of combatants
    private List<Combatant> allCombatants = new List<Combatant>();
    private List<PlayerCharacter> playerCharacters = new List<PlayerCharacter>();
    private List<Combatant> enemies = new List<Combatant>();
    
    // Turn management
    private Queue<Combatant> turnOrder = new Queue<Combatant>();
    private Combatant currentCombatant;
    private CombatState currentState;
    
    // Event callbacks
    public UnityEvent<CombatState> OnCombatStateChanged = new UnityEvent<CombatState>();
    public UnityEvent<Combatant> OnTurnStarted = new UnityEvent<Combatant>();
    public UnityEvent<Combatant> OnTurnEnded = new UnityEvent<Combatant>();
    public UnityEvent OnCombatVictory = new UnityEvent();
    public UnityEvent OnCombatDefeat = new UnityEvent();
    
    // Current turn coroutine
    private Coroutine currentTurnCoroutine;
    
    // Turn counter
    private int turnCount = 0;
    
    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    
    public void StartCombat()
    {
        StartCoroutine(CombatLoop());
    }
    
    private IEnumerator CombatLoop()
    {
        // Initialize combat
        ChangeState(CombatState.Initialize);
        yield return StartCoroutine(InitializeCombat());
        
        // Play intro narrative if available
        if (introEvent != null)
        {
            yield return StartCoroutine(introEvent.PlayEvent());
        }
        
        // Main combat loop
        while (currentState != CombatState.Victory && currentState != CombatState.Defeat && currentState != CombatState.Escape)
        {
            // Start next turn
            ChangeState(CombatState.StartTurn);
            yield return StartCoroutine(StartNextTurn());
            
            // Execute current turn based on whether it's a player or enemy
            if (currentCombatant.IsPlayerControlled)
            {
                ChangeState(CombatState.PlayerTurn);
                yield return StartCoroutine(currentCombatant.TakeTurn());
            }
            else
            {
                ChangeState(CombatState.EnemyTurn);
                yield return StartCoroutine(currentCombatant.TakeTurn());
            }
            
            // End turn processing
            ChangeState(CombatState.EndTurn);
            yield return StartCoroutine(EndTurn());
            
            // Check victory/defeat conditions
            if (CheckVictoryCondition())
            {
                ChangeState(CombatState.Victory);
                yield return StartCoroutine(HandleVictory());
                break;
            }
            else if (CheckDefeatCondition())
            {
                ChangeState(CombatState.Defeat);
                yield return StartCoroutine(HandleDefeat());
                break;
            }
            
            turnCount++;
        }
    }
    
    private IEnumerator InitializeCombat()
    {
        // Clear previous combat data
        allCombatants.Clear();
        playerCharacters.Clear();
        enemies.Clear();
        turnOrder.Clear();
        turnCount = 0;
        
        // Find all player characters
        foreach (Transform child in playerCharactersParent)
        {
            PlayerCharacter playerChar = child.GetComponent<PlayerCharacter>();
            if (playerChar != null)
            {
                playerCharacters.Add(playerChar);
                allCombatants.Add(playerChar);
                playerChar.Initialize();
                
                // Subscribe to defeat event
                playerChar.OnDefeated.AddListener(OnCombatantDefeated);
            }
        }
        
        // Find all enemies
        foreach (Transform child in enemyCharactersParent)
        {
            Combatant enemy = child.GetComponent<Combatant>();
            if (enemy != null && !enemy.IsPlayerControlled)
            {
                enemies.Add(enemy);
                allCombatants.Add(enemy);
                enemy.Initialize();
                
                // Subscribe to defeat event
                enemy.OnDefeated.AddListener(OnCombatantDefeated);
            }
        }
        
        // Initialize the turn order
        DetermineTurnOrder();
        
        yield return null;
    }
    
    private void DetermineTurnOrder()
    {
        // Clear existing turn order
        turnOrder.Clear();
        
        // Add all active combatants to the turn order
        foreach (var combatant in allCombatants)
        {
            if (!combatant.IsDefeated())
            {
                turnOrder.Enqueue(combatant);
            }
        }
    }    
}        // In a more complex system,