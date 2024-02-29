using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.Events;
using System;
using System.Linq;
using TMPro;
using UnityEngine.VFX;
using Unity.VisualScripting;
using JetBrains.Annotations;

public enum TurnState { FREEZE, BURN }
public enum HeatState
{
    Green,
    Yellow,
    Red
}

public class BattleController : MonoBehaviour
{
    //battle start and battle references
    public PartyManager partyManager;
    public EncounterManager encounterManager;
    public CameraController cameraController;
    public SoundManager soundManager;
    //battle start and battle references
    public AudioSource audioSource;
    private AudioSource hitSource;
    public GameData gameData;
    public CharacterBase activeCharacter = null;
    public List<GameObject> playerParty;
    public List<GameObject> alivePlayers;
    public List<GameObject> aliveEnemies;
    public List<GameObject> defeatedEnemies;
    public Dictionary<CharacterBase, List<CharacterBase>> playerAdjacencyList = new Dictionary<CharacterBase, List<CharacterBase>>();
    public Dictionary<CharacterBase, List<CharacterBase>> enemyAdjacencyList = new Dictionary<CharacterBase, List<CharacterBase>>();
    [SerializeField]
    private AnimationCurve hopCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(0.5f, 1), new Keyframe(1, 0));
    public AnimationClip freeze;
    public AnimationClip burn;
    
    public Transform freezeEffect; // The prefab of the object you want to spawn
    public Transform burnEffect; // The prefab of the object you want to spawn
    //heat system
    public float maxHeat = 100f;
    public float playerHeat;
    public float enemyHeat;
    public bool playerHasOverHeated = false;
    public bool enemyHasOverHeated = false;
    public TurnState turnState;
    public TurnLockout turnLockout;
    public Vulnerability vulnerability;
    //fuel
    public float fuel = 1f;
    public float maxFuel = 5f;
    public float usedFuel;
    //turn initizilations variables
    public Queue<CharacterBase> turnQueue = new Queue<CharacterBase>();
    public const float raceDistance = 500f;  // the arbitrary distance
    //damage effects
    public delegate void DamageDealtEventHandler(CharacterBase attacker, Act act, float damage);
    public static event DamageDealtEventHandler OnDamageDealt;
    public delegate void PlayerSpellUsageHandler(Act act, CharacterBase caster);
    public static event PlayerSpellUsageHandler OnPlayerSpellUsed;
    public Queue<Act> actionQueue = new Queue<Act>(); // Queue for actions
    public Queue<float> damageQueue = new Queue<float>(); // Queue for damage values

    private float damage;
    private bool isWeakOverride = false;
    private float weaknessHeatGain = 15f;     
    private float attackPower;
    private float weaknessMultiplier;
    private float AOEHeatScaling = 1f;
    private float attackManaRegen = 5f;
    public BattleUI battleUI;  // Reference to the BattleUI component/script.
    public VisualEffect hitEffectPrefab; // Drag your hit effect prefab here in the Inspector
    public GameObject damageTextPrefab;
    public GameObject critDamageTextPrefab;
    public GameObject healTextPrefab;
    public GameObject critHealTextPrefab; 
    private Transform damageNumbersCanvas;
    public UnityEvent battleStartEvent; // battle has started
    
    private void Start()
    {
        damageNumbersCanvas = GameObject.FindGameObjectWithTag("DamageNumbers").transform;
    }
    private IEnumerator MoveToPosition(Transform target, Vector3 targetPosition, float duration)
    {
        // Get all colliders and the rigidbody on the target object
        Collider[] colliders = target.GetComponents<Collider>();
        Rigidbody rb = target.GetComponent<Rigidbody>();
        
        // Disable all colliders
        foreach (var collider in colliders)
        {
            collider.enabled = false;
        }

        // If a Rigidbody is attached, make it kinematic
        bool wasKinematic = false;
        if (rb != null)
        {
            rb.useGravity = false;
            rb.velocity = Vector3.zero;
            wasKinematic = rb.isKinematic;
            rb.isKinematic = true;
        }

        Vector3 startPosition = target.position;
        float timeElapsed = 0f;

        while (timeElapsed < duration)
        {
            target.position = Vector3.Lerp(startPosition, targetPosition, timeElapsed / duration);
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        target.position = targetPosition;

        // Re-enable all colliders
        foreach (var collider in colliders)
        {
            collider.enabled = true;
        }

        // Restore the Rigidbody's original kinematic state
        if (rb != null)
        {
            rb.isKinematic = wasKinematic;
            
        }
    }




    private void CreateAdjacencyList(Encounter encounter, List<GameObject> spawnedEnemies)
    {
        // For player characters
        for (int i = 0; i < playerParty.Count; i++)
        {
            CharacterBase currentPlayer = playerParty[i].GetComponent<CharacterBase>();
            List<CharacterBase> adjacentPlayers = new List<CharacterBase>();
            
            foreach (int index in encounter.playerAdjacencyDefinitions[i].adjacentIndices)
            {
                if (index < playerParty.Count) 
                {
                    adjacentPlayers.Add(playerParty[index].GetComponent<CharacterBase>());
                }
            }

            playerAdjacencyList[currentPlayer] = adjacentPlayers;
        }

        // For enemy characters
        for (int i = 0; i < spawnedEnemies.Count; i++)
        {
            CharacterBase currentEnemy = spawnedEnemies[i].GetComponent<CharacterBase>();
            List<CharacterBase> adjacentEnemies = new List<CharacterBase>();

            try
            {
                foreach (int index in encounter.enemyAdjacencyDefinitions[i].adjacentIndices)
                {
                    if (index < spawnedEnemies.Count)
                    {
                        adjacentEnemies.Add(spawnedEnemies[index].GetComponent<CharacterBase>());
                    }
                }
            } catch { enemyAdjacencyList[currentEnemy] = adjacentEnemies; }

            enemyAdjacencyList[currentEnemy] = adjacentEnemies;
        }
    }
    private void UpdateEnemyAdjacencyList(GameObject spawnedEnemy, int enemyIndex)
    {
        Encounter encounter = encounterManager.currentEncounter;
        
        // Ensure the spawnedEnemy has a CharacterBase component
        CharacterBase newEnemyCharacter = spawnedEnemy.GetComponent<CharacterBase>();
        if (newEnemyCharacter == null) return;

        List<CharacterBase> adjacentEnemies = new List<CharacterBase>();

        // Update the adjacency list for the new enemy
        foreach (int index in encounter.enemyAdjacencyDefinitions[enemyIndex].adjacentIndices)
        {
            if (index < aliveEnemies.Count)
            {
                adjacentEnemies.Add(aliveEnemies[index].GetComponent<CharacterBase>());
            }
        }

        // Set the adjacency list of the new enemy
        enemyAdjacencyList[newEnemyCharacter] = adjacentEnemies;

        // Update the adjacency lists of other enemies defined as adjacent
        for (int i = 0; i < aliveEnemies.Count; i++)
        {
            if (i != enemyIndex && encounter.enemyAdjacencyDefinitions[i].adjacentIndices.Contains(enemyIndex))
            {
                CharacterBase adjacentEnemy = aliveEnemies[i].GetComponent<CharacterBase>();
                List<CharacterBase> existingAdjacentEnemies = enemyAdjacencyList.ContainsKey(adjacentEnemy) ? enemyAdjacencyList[adjacentEnemy] : new List<CharacterBase>();
                if (!existingAdjacentEnemies.Contains(newEnemyCharacter))
                {
                    existingAdjacentEnemies.Add(newEnemyCharacter);
                    enemyAdjacencyList[adjacentEnemy] = existingAdjacentEnemies;
                }
            }
        }
    }


    public IEnumerator StartBattle(Encounter encounter, List<GameObject> spawnedEnemies, PlayerController playerController)
    {

        GameObject player = GameObject.FindGameObjectWithTag("Player");

        // Switch camera to battle mode
        cameraController.SwitchToBattleMode();

        // Fetch the positions
        List<Vector3> battlePositions = encounter.PrepareEntitiesBattlePositions(player, spawnedEnemies);

        // Start all the move actions
        //player
        List<GameObject> allEntities = new List<GameObject> { player };
        playerParty = new List<GameObject>
        {
            player
        };
        alivePlayers = new List<GameObject>();
        alivePlayers.Add(player);
        //enemies
        aliveEnemies = spawnedEnemies;
        allEntities.AddRange(aliveEnemies);

        player.GetComponentInChildren<Animator>().SetBool("IsRunning", false);
        player.GetComponent<PlayerController>().isRunning = false;
        player.transform.Find("dustps").gameObject.SetActive(false);
        player.GetComponentInChildren<SpriteRenderer>().flipX = false;
        player.GetComponent<PlayerController>().enabled = false;
        player.GetComponent<PlayerController>().audioSource.Stop();
        for (int i = 0; i < allEntities.Count; i++)
        {
            if(i < allEntities.Count-1)
            {
                StartCoroutine(MoveToPosition(allEntities[i].transform, battlePositions[i], .5f));
            }else{
                yield return StartCoroutine(MoveToPosition(allEntities[i].transform, battlePositions[i], .5f));
            }
        }

        //Now spawn active party members
        for (int i = 0; i < partyManager.activePartyMembersPrefabs.Count; i++)
        {
            GameObject memberInstance = Instantiate(
                partyManager.activePartyMembersPrefabs[i], 
                encounterManager.currentEncounter.partyBattlePositions[i + 1], // Start at index 1 because index 0 is for the player
                Quaternion.identity
            );
            memberInstance.GetComponent<PlayerCharacter>().characterStats = partyManager.GetStat(partyManager.activePartyMembersPrefabs[i]);
            allEntities.Add(memberInstance);
            playerParty.Add(memberInstance);
            if(memberInstance.GetComponent<CharacterBase>().IsAlive )// make isAlive private
            {
                alivePlayers.Add(memberInstance);
            }

        }

        //define which characters are adjacent to each other
        CreateAdjacencyList(encounter,spawnedEnemies);//currently doesn't do so player party
        // Logic after all entities are in position.
        UIManager ui = FindObjectOfType<UIManager>();
        ui.StartBattleUI();
        InitializeTurnOrder();
        // set heat values
        BattleUI battleUI = ui.battleUI.GetComponent<BattleUI>();
        yield return StartCoroutine(battleUI.UpdateTurnWheel(turnQueue));
        playerHeat = 0;
        enemyHeat = 0;
        yield return StartCoroutine(battleUI.UpdateBothHeatsSequentially(playerHeat,enemyHeat));
        
        //set fuel
        fuel = 1f;
        battleUI.UpdateFuelUI();
        //turnstate
        turnState = TurnState.FREEZE;
        StartCoroutine(battleUI.SetTurnStateIndicator(turnState));
        //party status bars
        battleUI.UpdatePartyConditionUI();

        //stop the area music

        if(soundManager.currentAreaAudioSource)
        {
            soundManager.currentAreaAudioSource.Pause();
        }
        
        //start the music
        audioSource = GetComponents<AudioSource>()[0];
        hitSource = GetComponents<AudioSource>()[1];  
        audioSource.clip = encounterManager.currentEncounter.battleMusic;
        audioSource.volume = soundManager.GetVolume();
        audioSource.Play();

        // remove any lingering buffs or debuffs
        foreach(GameObject obj in allEntities){
            obj.GetComponent<CharacterBase>().characterStats.activeStatusEffects = new List<StatusEffect>();
        }

        try
        {
            StartCoroutine(MainBattleLoop());
            yield break;
        } catch (Exception e)
        {
            EndBattle(true);
        }
        
    }
    private IEnumerator hide(SpriteRenderer sprite)
    {
        yield return new WaitForSeconds(5f);
        if (sprite) sprite.enabled = false;
    }
    public void removeDead()
    {
        for (int i = aliveEnemies.Count - 1; i >= 0; i--)
        {
            CharacterBase character = aliveEnemies[i].GetComponent<CharacterBase>();
            if (!character.IsAlive)
            {
                // Turn off the SpriteRenderer for the dead enemy
                 SpriteRenderer sr = aliveEnemies[i].GetComponentInChildren<SpriteRenderer>();
                StartCoroutine(hide(sr));
                // Disable all child objects of the dead enemy
                foreach (Transform child in aliveEnemies[i].transform)
                {
                    if(child.name != "sprite")
                    {
                        child.gameObject.SetActive(false);
                    }
                }

                defeatedEnemies.Add(aliveEnemies[i]);
                aliveEnemies.RemoveAt(i);
            }
        }
        for (int i = alivePlayers.Count - 1; i >= 0; i--)
        {
            CharacterBase character = alivePlayers[i].GetComponent<CharacterBase>();
            if (!character.IsAlive)
            {
                // Turn off the SpriteRenderer for the dead enemy

                alivePlayers.RemoveAt(i);
            }
        }
    }
    public IEnumerator MainBattleLoop()
    {
        while (true) // Ensure you have some method to determine if the game is over.
        {
            while (turnQueue.Count > 0)
            {
                yield return StartCoroutine(AdvanceTurn());
                usedFuel = 0;
                // Remove dead characters from the queue
                turnQueue = new Queue<CharacterBase>(turnQueue.Where(character => character.IsAlive));
                    
                // Check for dead enemies and transfer them to defeatedEnemies list.
                removeDead();

                // Check other logic like win/loss conditions.
                // If the game is over, break out of the loop.
                // Victory Condition
                if (aliveEnemies.Count == 0)
                {
                    EndBattle(true);
                    yield break;
                }

                bool noPlayersAlive = true;
                foreach(GameObject player in playerParty){
                    //if a player is found to be alive
                    if(player.GetComponent<CharacterBase>().IsAlive){
                        noPlayersAlive = false;
                        break;
                    }
                }    
                if(noPlayersAlive){
                    EndBattle(false); //end loss condition
                    yield break;
                }

                //party status bars
                battleUI.UpdatePartyConditionUI();

                yield return(StartCoroutine(battleUI.UpdateTurnWheel(turnQueue)));
            }


            Debug.Log("new turn");

            if(enemyHeat >= 100){
                enemyHeat = 0;
                enemyHasOverHeated = false;
            }
            if(playerHeat >= 100){
                playerHeat = 0;
                playerHasOverHeated = false;
            }
            battleUI.UpdateBothHeatsSequentially(playerHeat,enemyHeat);
            InitializeTurnOrder();
            yield return(StartCoroutine(battleUI.UpdateTurnWheel(turnQueue)));
            ToggleTurnState();
            yield return (StartCoroutine(battleUI.SetTurnStateIndicator(turnState)));
        }

    }


    public void EndBattle(bool victory)
    {
        if (victory)
        {
            int totalExperienceGained = 0;

            foreach (var enemy in defeatedEnemies)
            {
                EnemyStats es = enemy.GetComponent<EnemyCharacter>().characterStats as EnemyStats;
                totalExperienceGained += es.GetExpGiven();
            }

            foreach (var player in playerParty)
            {
                PlayerCharacter ps = player.GetComponent<PlayerCharacter>();
                ((PlayerStats)ps.characterStats).GainExp(totalExperienceGained);
                ps.IsAlive = true;
                if(ps.IsAlive){
                    
                    ps.characterStats.Heal(ps.characterStats.GetEffectiveStat(StatType.HEALTH)/2); // Heal to full health
                }else{
                    ps.characterStats.Heal(ps.characterStats.GetEffectiveStat(StatType.HEALTH)/4); // Heal to full health
                }

            }
            gameData.partyManager.playerStats.GainExp(totalExperienceGained);
            foreach(PlayerStats clonestats in gameData.partyManager.cloneStats)
            {
                clonestats.GainExp(totalExperienceGained);
            }   


            encounterManager.defeatedEncounters.Add(encounterManager.currentEncounter.encounterID, encounterManager.respawnTime);
            encounterManager.ClearCurrentEncounter();

            Debug.Log("Battle is over. Victory!");
        }
        else
        {



            //respawn encounter here
            encounterManager.RespawnEncounter(encounterManager.currentEncounter, true);
            //clear current encounter
            encounterManager.ClearCurrentEncounter();
            // Handle the defeat logic here. For example:
            //TODO:play transition animation
            // Teleport player to respawn point
            Vector3 respawnPosition = gameData.respawnLocation;
            playerParty[0].GetComponent<Transform>().position = respawnPosition;


            // Heal the party
            foreach (var player in playerParty)
            {
                PlayerStats ps = player.GetComponent<PlayerCharacter>().characterStats as PlayerStats;
                
                ps.Heal(ps.GetEffectiveStat(StatType.HEALTH)); // Heal to full health
            }
            Debug.Log("Battle is over. Defeat!");
            // Restart the battle, go back to a checkpoint, show game over screen, etc.
        }

        //remove status effects
        foreach (var enemy in defeatedEnemies)
        {
            EnemyStats es = enemy.GetComponent<EnemyCharacter>().characterStats as EnemyStats;
            es.RemoveAllActiveStatusEffects();
        }
        foreach (var enemy in defeatedEnemies)
        {
            Destroy(enemy);
        }
        foreach (var enemy in aliveEnemies)
        {
            Destroy(enemy);
        }
        defeatedEnemies.Clear();
        foreach (var player in playerParty)
        {
            PlayerStats ps = player.GetComponent<PlayerCharacter>().characterStats as PlayerStats;
            ps.RemoveAllActiveStatusEffects();
        }

        foreach (GameObject clone in playerParty)
        {
            PlayerStats ps = clone.GetComponent<CharacterBase>().characterStats as PlayerStats;
            if(ps.isClone){
                Destroy(clone);
            }
        }
        // refill mana
        foreach (var player in playerParty)
        {
            PlayerStats ps = player.GetComponent<PlayerCharacter>().characterStats as PlayerStats;

            ps.RegenerateMana(ps.GetEffectiveStat(StatType.MANA)); // Refill mana to max
        }

        //stop battle music
        audioSource.Stop();
        //reset music
        if(soundManager.currentAreaAudioSource)
        {
            soundManager.currentAreaAudioSource.UnPause();
        }
        
        //general battle removal behaviour
        cameraController.SwitchToTraversalMode();
        // Toggle UI to overworld state
        UIManager uiManager = FindObjectOfType<UIManager>();
        if (uiManager != null)
        {
            uiManager.ToggleUIState(UIManager.PageState.OVERWORLD);
        }
    }



    public void DebugPrintTurnOrder()
    {
        int i = 0;
        Debug.Log("Turn Order:");
        foreach (var character in turnQueue)
        {
            Debug.Log(character.characterName + " " + i.ToString());
            i++;
        }
    }
    public IEnumerator AdvanceTurn()
    {
        // Set the current active character.
        activeCharacter = turnQueue.Dequeue();
        battleUI.activeCharacterAnimator = activeCharacter.GetComponentInChildren<Animator>();
        StartCoroutine(cameraController.ShiftCameraForCharacterTurn(activeCharacter,.25f));
        Debug.Log($"actor: {activeCharacter.characterName} remain queue: {turnQueue.Count}");
        // Resolve status effects and check for lockout.
        bool lockout = false;

        foreach (var effect in activeCharacter.characterStats.activeStatusEffects.ToList()) 
        {
            if (effect is TurnLockout) // Check if the effect is of type TurnLockout.
            {
                lockout = true;
            }

            effect.DecreaseDuration(activeCharacter.characterStats);
        }

        // If lockout is true, skip the turn.
        if (lockout)
        {
            Debug.Log($"{activeCharacter.characterName} is locked out and skips their turn.");
            yield break; // Exit the coroutine early.
        }

        // Initialize with a default action.
        Act decidedAction = null; // Example default action.

        if (activeCharacter is PlayerCharacter player)
        {
            //brings up battle hud
            battleUI.ShowActionMenu(activeCharacter);
            //party status bars
            battleUI.UpdatePartyConditionUI();
            //turn on animation to show whose active(it's the glowing circle on the ground)
            VisualEffect groundEffect = player.GameObject().GetComponentInChildren<VisualEffect>();
            groundEffect.enabled = true;
            
            groundEffect.SetVector4("OuterColor",(Vector4)player.GetFireTypeColor(100f));
            bool actionDecided = false;

            player.OnActionDecided += action =>
            {
                decidedAction = action;
                actionDecided = true;
            };

            player.Act();
            yield return new WaitUntil(() => actionDecided);
            //turn off animation to show whose active(it's the glowing circle on the ground)
            player.GameObject().GetComponentInChildren<VisualEffect>().enabled = false;
            //party status bars
            battleUI.UpdatePartyConditionUI();
            if (decidedAction.actionType != ActionType.USE_ITEM)
            {
                yield return StartCoroutine(ExecuteAction(decidedAction));
            }
            //turn off ignite effect
            Animator charAnimator = activeCharacter.GameObject().GetComponentInChildren<Animator>();
            AnimatorStateInfo stateInfo = charAnimator.GetCurrentAnimatorStateInfo(1); // Assuming "Ignite" is the second layer.
            if (stateInfo.IsName("Ignite Loop Red") || stateInfo.IsName("Ignite Loop White") || stateInfo.IsName("Ignite Loop Blue"))
            {
                charAnimator.SetBool("endIgnite", true);
            }
        }
        else
        {
            battleUI.HideActionMenu();
            // For non-player characters (like enemies).
            decidedAction = activeCharacter.Act();
            yield return StartCoroutine(ExecuteAction(decidedAction));
        }

        cameraController.SwitchToBattleMode();
        foreach (var effect in activeCharacter.characterStats.activeStatusEffects.ToList()) 
        {
            effect.actionEffect(activeCharacter, decidedAction);
        }
        Debug.Log("ended action");
    }



    public IEnumerator ApplyOverheatEffects(CharacterBase target)
    {
        yield return new WaitForSeconds(.3f);
        Vector3 targetPosition = target.GameObject().transform.position;
        CharacterStats stats = target.characterStats;
    
        if (turnState == TurnState.FREEZE)
        {

            StartCoroutine(SpawnAndSpinEffectObject(targetPosition, freezeEffect));
            StartCoroutine(cameraController.HandleOverheatEffects(targetPosition));
            yield return new WaitForSeconds(.5f);

            
            Time.timeScale = 0.5f;
            battleUI.GetComponent<Animator>().SetTrigger("Freeze");
            yield return new WaitForSeconds(freeze.length*.8f);

            Time.timeScale = 1;
            turnLockout.duration = 2; // Locked out for 2 turns
            turnLockout.currentDuration = 2; // Locked out for 2 turns

            // Apply the freeze effect to the target
            turnLockout.ApplyEffect(stats);
        }
        else // Assuming the other state is BURN
        { 
            float XOffset = 1f;
            if(enemyHeat >= 100){
                XOffset = -1f;
            }
            targetPosition = targetPosition + new Vector3(XOffset,0,- 1.5f);//explosion is in front 
            StartCoroutine(SpawnAndSpinEffectObject(targetPosition, burnEffect));
            
  
            StartCoroutine(cameraController.HandleOverheatEffects(targetPosition));

            Time.timeScale = 0.5f;
            battleUI.GetComponent<Animator>().SetTrigger("Burn");
            yield return new WaitForSeconds(burn.length*.8f);
            Time.timeScale = 1;

            turnLockout.duration = 1; // Locked out for 2 turns
            turnLockout.currentDuration = 1; // Locked out for 1 turn


            // Apply the burn and vulnerability effects to the target
            turnLockout.ApplyEffect(stats);
            vulnerability.ApplyEffect(stats);
        }
        
        yield break;
    }
    private IEnumerator SpawnAndSpinEffectObject(Vector3 targetPosition, Transform effect)
    {
        // Spawn the object with a slight positive Z offset
        Transform effectObject = Instantiate(effect, targetPosition + new Vector3(0, 0, .05f), Quaternion.identity);
        effectObject.rotation = Quaternion.identity;
        float elapsedTime = 0f;
        float duration = 1.5f;  // adjust for desired movement speed
        while (elapsedTime < duration)
        {
            effectObject.Rotate(0, .5f, 0); // "5f" is the rotation speed. Adjust as necessary.
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        yield return new WaitForSeconds(.5f);
        Destroy(effectObject.GameObject());
        yield break;

    }

    private CharacterBase DetermineAOECenter()
    {
        // Start with assuming the first enemy has the most adjacents
        CharacterBase centermostEnemy = enemyAdjacencyList.Keys.FirstOrDefault();
        int maxAdjacentCount = 0;

        foreach (var enemy in enemyAdjacencyList.Keys)
        {
            // Count how many adjacent enemies this enemy has
            int adjacentCount = enemyAdjacencyList[enemy]?.Count ?? 0;

            // If this enemy has more adjacents than the current max, it becomes the new centermost
            if (adjacentCount > maxAdjacentCount)
            {
                maxAdjacentCount = adjacentCount;
                centermostEnemy = enemy;
            }
        }

        if (centermostEnemy == null)
        {
            return aliveEnemies[0].GetComponent<CharacterBase>();
        }

        return centermostEnemy; // This will be null if the enemyAdjacencyList is empty
    }
    private CharacterBase DeterminePlayerAOECenter()
    {
        // Start with assuming the first player has the most adjacents
        CharacterBase centermostPlayer = playerAdjacencyList.Keys.FirstOrDefault();
        int maxAdjacentCount = 0;

        foreach (var player in playerAdjacencyList.Keys)
        {
            // Count how many adjacent players this player has
            int adjacentCount = playerAdjacencyList[player]?.Count ?? 0;

            // If this player has more adjacents than the current max, it becomes the new centermost
            if (adjacentCount > maxAdjacentCount)
            {
                maxAdjacentCount = adjacentCount;
                centermostPlayer = player;
            }
        }

        return centermostPlayer; // This will be null if the playerAdjacencyList is empty
    }

    // Method to add an action and corresponding damage to the queue
    public void QueueAction(Act act, float damage)
    {
        Act actCopy = act.Copy(act);// Create a copy of the act
        actionQueue.Enqueue(actCopy);
        damageQueue.Enqueue(damage);
    }
    public void ProcessActions()
    {
        while (actionQueue.Count > 0 && damageQueue.Count > 0)
        {
            
            Act act = actionQueue.Dequeue();
            float damage = damageQueue.Dequeue();

            // Apply damage and any other effects associated with the act
            act.target.TakeDamage(damage, act.isCritical, act.isWeak, act.isBlock);

            if(playerHeat > 100 && playerHasOverHeated != true){
                playerHasOverHeated = true;
                StartCoroutine(ApplyOverheatEffects(activeCharacter));
            }
            if(enemyHeat > 100 && enemyHasOverHeated != true){
                enemyHasOverHeated = true;
                StartCoroutine(ApplyOverheatEffects(act.target));
            }
            
        }
        cameraController.SwitchToBattleMode();
    }
    public IEnumerator ExecuteAction(Act act)
    {
        damage = 0;
        float healAmount = 0;
        switch (act.actionType)
        {
            case ActionType.ATTACK:
                attackManaRegen = 5f;
                if (activeCharacter is PlayerCharacter player)
                {
                    damage = CalculateDamage(player, act);

                    // Movement & animation
                    MoveCharacterToTarget(activeCharacter, act.target, damage, act);
                    // Regenerate mana
                    if (player.characterStats is PlayerStats stats)
                    {
                        Debug.Log("regenerate mana");
                        stats.RegenerateMana(attackManaRegen);
                    }
                    //TODO: add fuel
                    if(usedFuel == 0f){
                        AccumulateFuel();
                    }
                }
                else
                {
                    if (act.target is PlayerCharacter targetPlayer)
                    {
                        damage = CalculateDamage(activeCharacter, act);
                        MoveCharacterToTarget(activeCharacter, targetPlayer, damage, act);
                    }
                }
                break;
            case ActionType.USE_ITEM:
                    //partyManager.UseHealingItem();
                break;
            case ActionType.SPELL:
                // Logic for casting a spell
                EffectType effectType = act.spell.effectType;

                CharacterBase aoeCenter;
                //turn spell text
                battleUI.SpellLabel.SetActive(true);
                battleUI.SpellLabel.GetComponentInChildren<TextMeshProUGUI>().text = act.spell.spellName;

                if (activeCharacter is PlayerCharacter playerS)// S stands for spell
                {     
                    //needs to handle spell effect types
                    switch (effectType)
                    {
                        //TODO: Heal text,block text,weakness hit reaction, health bar tick down animation, sprite flash white on hit, spell on hit particle
                        //down dead characters, make spells move faster/variable, speed up attack animation
                        //camera changing angle or position for:activecharacter, opening spell menu, targeting, using spells
                        case EffectType.HEAL:
                            Debug.Log("heal");
                            healAmount = CalculateDamage(playerS,act);//does the normal damage calc just ignores defense, might add healing scalar to damge calc
                            Debug.Log("heal amount:"+ healAmount);
                             ((PlayerCharacter)act.target).Heal(healAmount,act.isCritical);

                            // For non-AOE spells, play the effect and then apply the effect to the target
                            StartCoroutine(act.spell.PlaySpellEffect(activeCharacter, act.target));
                            yield return new WaitForSeconds(1.5f);
                            if(playerHeat > 100 && playerHasOverHeated != true){
                                playerHasOverHeated = true;
                                yield return StartCoroutine(ApplyOverheatEffects(activeCharacter));//TODO:once spell animations exist we don't need to yeild return
                            }
                            //spell buff effects, some spells have buffs but arent of type buff
                            if(act.spell.applySelf){
                                act.spell.applySelf.ApplyEffect(act.target.characterStats);
                            }
                            break;
                        case EffectType.AOE_HEAL:
                            foreach (GameObject target in alivePlayers)
                            {
                                act.target = target.GetComponent<CharacterBase>();
                                healAmount = CalculateDamage(playerS,act);//does the normal damage calc just ignores defense, might add healing scalar to damge calc
                                Debug.Log("heal amount:"+ healAmount);
                                ((PlayerCharacter)act.target).Heal(healAmount,act.isCritical);

                                foreach(GameObject ally in alivePlayers){
                                    StartCoroutine(act.spell.PlaySpellEffect(activeCharacter,ally.GetComponent<CharacterBase>()));
                                }
                                yield return new WaitForSeconds(1.5f);
                                if(playerHeat > 100 && playerHasOverHeated != true){
                                    playerHasOverHeated = true;
                                    yield return StartCoroutine(ApplyOverheatEffects(activeCharacter));//TODO:once spell animations exist we don't need to yeild return
                                }
                                //spell buff effects, some spells have buffs but arent of type buff
                                if(act.spell.applySelf){
                                    act.spell.applySelf.ApplyEffect(act.target.characterStats);
                                }
                            }
                            break;
                        case EffectType.SINGLE_TARGET_DAMAGE:
                            damage = CalculateDamage(playerS,act);
                            QueueAction(act,damage);
                            // For non-AOE spells, play the effect and then apply the effect to the target
                            yield return StartCoroutine(act.spell.PlaySpellEffect(activeCharacter, act.target));
                            

                            //spell buff effects, some spells have buffs but arent of type buff
                            if(act.spell.applySelf){
                                act.spell.applySelf.ApplyEffect(activeCharacter.characterStats);
                            }
                            if(act.spell.applyTarget){
                                act.spell.applyTarget.ApplyEffect(act.target.characterStats);
                            }
                            break;
                         case EffectType.MULTIHIT_TARGET_DAMAGE:
                            AOEHeatScaling = 2f;
                            for(int i = 0; i < act.spell.numHits; i++){
                                damage = CalculateDamage(playerS,act);
                                QueueAction(act,damage);
                                // For non-AOE spells, play the effect and then apply the effect to the target
                                yield return StartCoroutine(act.spell.PlaySpellEffect(activeCharacter, act.target));

                            }

                            //spell buff effects, some spells have buffs but arent of type buff
                            if(act.spell.applySelf){
                                act.spell.applySelf.ApplyEffect(activeCharacter.characterStats);
                            }
                            if(act.spell.applyTarget){
                                act.spell.applyTarget.ApplyEffect(act.target.characterStats);
                            }
                            break;                           
                        case EffectType.AOE_DAMAGE:
                            foreach (var character in aliveEnemies){
                                if(character == null) { break;}
                                act.target = character.GetComponent<CharacterBase>();
                                damage = CalculateDamage(playerS,act);
                                QueueAction(act,damage);

                            }
                            // Determine the central point of the AOE, could be based on the player position or predefined.
                            aoeCenter = DetermineAOECenter();
                            yield return StartCoroutine(act.spell.PlaySpellEffect(activeCharacter, aoeCenter));

                            if(act.spell.applyTarget){
                                act.spell.applyTarget.ApplyEffect(act.target.characterStats);
                            }
                            //spell buff effects, some spells have buffs but arent of type buff
                            if(act.spell.applySelf){
                                act.spell.applySelf.ApplyEffect(activeCharacter.characterStats);
                            }

                            break;
                        case EffectType.ADJACENT_AOE_DAMAGE:
                            // Get adjacent enemies
                            List<CharacterBase> adjacentEnemies = enemyAdjacencyList[act.target];
                        
                            damage = CalculateDamage(playerS, act); // This might need adjustment if 'CalculateDamage' depends on 'act.target' 
                            QueueAction(act,damage);

                            // Apply damage to each adjacent enemy
                            foreach (CharacterBase adjacentEnemy in adjacentEnemies)
                            {
                                act.target = adjacentEnemy;
                                damage = CalculateDamage(playerS, act); // This might need adjustment if 'CalculateDamage' depends on 'act.target' 
                                QueueAction(act,damage);
                                if(act.spell.applyTarget){
                                    act.spell.applyTarget.ApplyEffect(act.target.characterStats);
                                }
                            }
                            aoeCenter = DetermineAOECenter();
                            yield return StartCoroutine(act.spell.PlaySpellEffect(activeCharacter, aoeCenter));
                            //spell buff effects, some spells have buffs but arent of type buff
                            if(act.spell.applySelf){
                                act.spell.applySelf.ApplyEffect(activeCharacter.characterStats);
                            }
                            break;
                        case EffectType.BUFF:
                            playerHeat += SpellHeat(act.spell);
                            act.spell.applySelf.ApplyEffect(act.target.characterStats);
                            StartCoroutine(act.spell.PlaySpellEffect(activeCharacter,act.target.GetComponent<CharacterBase>()));
                            yield return new WaitForSeconds(1f);
                            if(playerHeat > 100 && playerHasOverHeated != true){
                                playerHasOverHeated = true;
                                yield return StartCoroutine(ApplyOverheatEffects(activeCharacter));//TODO:once spell animations exist we don't need to yeild return
                            }
                            yield return new WaitForSeconds(1f);
                            //play animation here
                            break;
                        case EffectType.AOE_BUFF:
                            foreach (GameObject target in alivePlayers)
                            {
                                act.target = target.GetComponent<CharacterBase>();
                                playerHeat += SpellHeat(act.spell);
                                act.spell.applySelf.ApplyEffect(act.target.characterStats);
                                

                            }
                            foreach(GameObject ally in alivePlayers){
                                StartCoroutine(act.spell.PlaySpellEffect(activeCharacter,ally.GetComponent<CharacterBase>()));
                            }
                                                        // For non-AOE spells, play the effect and then apply the effect to the target

                            yield return new WaitForSeconds(1f);
                            if(playerHeat > 100 && playerHasOverHeated != true){
                                playerHasOverHeated = true;
                                yield return StartCoroutine(ApplyOverheatEffects(activeCharacter));//TODO:once spell animations exist we don't need to yeild return
                            }
                            yield return new WaitForSeconds(1f);

                            break;
                        case EffectType.DEBUFF:
                            playerHeat += SpellHeat(act.spell);
                            AOEHeatScaling = 1f;
                            enemyHeat += getGeatHeatGain(act.target);
                            act.spell.applyTarget.ApplyEffect(act.target.characterStats);
                            yield return new WaitForSeconds(1f);
                            if(playerHeat > 100 && playerHasOverHeated != true){
                                playerHasOverHeated = true;
                                yield return StartCoroutine(ApplyOverheatEffects(activeCharacter));//TODO:once spell animations exist we don't need to yeild return
                            }
                            if(enemyHeat > 100 && enemyHasOverHeated != true){
                                enemyHasOverHeated = true;
                                yield return StartCoroutine(ApplyOverheatEffects(act.target));
                            }
                            yield return new WaitForSeconds(1f);
                            break;
                            // Continue with other case logic based on your game mechanics...
                        default:
                        Debug.Log("effecttype of spell does not exist");
                        break;
                    }
                    

                    if(act.spell.postDamageConditions){
                        Debug.Log("post condition");
                        yield return StartCoroutine(act.spell.postDamageConditions.ApplyPostDamageEffect(activeCharacter,act));
                    }
                    //overheatCase
                    //if we have overheated aka heat has exceeded 100 depending on the turn state freeze or burn
                    //lockout for 1 turn for burn or 2 for freeze
                    //burn also applies vulnerability
                    //you can change duration it is a public variable
                    //also in the case that enemy has overheat apply the same

                    
                    
                    //put down wings of fire
                    yield return StartCoroutine(battleUI.UpdateBothHeatsSequentially(playerHeat, enemyHeat));
                    playerS.ToggleWings();
                    yield return null;
                    //exit spell
                    act.MarkAsFinished();
                }else{
                    // Enemy spells
                    EnemyCharacter enemyCharacter = (EnemyCharacter)activeCharacter;
                    // Handle different types of enemy spells
                    switch (effectType)
                    {
                        case EffectType.HEAL:
                            Debug.Log("heal");
                            healAmount = CalculateDamage(enemyCharacter,act);//does the normal damage calc just ignores defense, might add healing scalar to damge calc
                            Debug.Log("heal amount:"+ healAmount);
                             act.target.Heal(healAmount,false);

                            // For non-AOE spells, play the effect and then apply the effect to the target
                            StartCoroutine(act.spell.PlaySpellEffect(activeCharacter, act.target));
                            yield return new WaitForSeconds(1.5f);

                            //spell buff effects, some spells have buffs but arent of type buff
                            if(act.spell.applySelf){
                                act.spell.applySelf.ApplyEffect(act.target.characterStats);
                            }
                            break;
                        case EffectType.AOE_HEAL:
                            foreach (GameObject target in alivePlayers)
                            {
                                act.target = target.GetComponent<CharacterBase>();
                                healAmount = CalculateDamage(enemyCharacter,act);//does the normal damage calc just ignores defense, might add healing scalar to damge calc
                                Debug.Log("heal amount:"+ healAmount);
                                ((PlayerCharacter)act.target).Heal(healAmount,act.isCritical);

                                foreach(GameObject ally in alivePlayers){
                                    StartCoroutine(act.spell.PlaySpellEffect(activeCharacter,ally.GetComponent<CharacterBase>()));
                                }
                                yield return new WaitForSeconds(1.5f);
                                //spell buff effects, some spells have buffs but arent of type buff
                                if(act.spell.applySelf){
                                    act.spell.applySelf.ApplyEffect(act.target.characterStats);
                                }
                            }
                            break;
                        case EffectType.SINGLE_TARGET_DAMAGE:
                                damage = CalculateDamage(enemyCharacter,act);
                                QueueAction(act,damage);
                                Debug.Log($"activeCharacter: {activeCharacter.characterName}");
                                Debug.Log($"target: {act.target.characterName}");
                                // For non-AOE spells, play the effect and then apply the effect to the target
                                yield return StartCoroutine(act.spell.PlaySpellEffect(activeCharacter, act.target));
                                

                                //spell buff effects, some spells have buffs but arent of type buff
                                if(act.spell.applySelf){
                                    act.spell.applySelf.ApplyEffect(activeCharacter.characterStats);
                                }
                                if(act.spell.applyTarget){
                                    act.spell.applyTarget.ApplyEffect(act.target.characterStats);
                                }
                            break;
                        case EffectType.MULTIHIT_TARGET_DAMAGE:
                            // Enemy multi-hit target damage logic
                            break;
                        case EffectType.AOE_DAMAGE:
                            foreach (var playerCharacter in alivePlayers){
                                act.target = playerCharacter.GetComponent<CharacterBase>();
                                damage = CalculateDamage(enemyCharacter,act);
                                QueueAction(act,damage);

                            }
                            // Determine the central point of the AOE, could be based on the player position or predefined.
                            aoeCenter = DeterminePlayerAOECenter();
                            yield return StartCoroutine(act.spell.PlaySpellEffect(activeCharacter, aoeCenter));

                            if(act.spell.applyTarget){
                                act.spell.applyTarget.ApplyEffect(act.target.characterStats);
                            }
                            //spell buff effects, some spells have buffs but arent of type buff
                            if(act.spell.applySelf){
                                act.spell.applySelf.ApplyEffect(activeCharacter.characterStats);
                            }

                            break;
                        case EffectType.BUFF:
                            act.spell.applySelf.ApplyEffect(act.target.characterStats);
                            StartCoroutine(act.spell.PlaySpellEffect(activeCharacter,act.target.GetComponent<CharacterBase>()));
                            yield return new WaitForSeconds(1.5f);
                            //play animation here
                            break;
                        case EffectType.DEBUFF:
                            // Enemy debuff logic
                            break;
                        case EffectType.AOE_BUFF:
                            // Enemy AOE buff logic
                            break;
                        case EffectType.ADJACENT_AOE_DAMAGE:
                            // Get adjacent enemies
                            List<CharacterBase> adjacentEnemies = enemyAdjacencyList[act.target];
                        
                            damage = CalculateDamage(enemyCharacter, act); // This might need adjustment if 'CalculateDamage' depends on 'act.target' 
                            QueueAction(act,damage);

                            // Apply damage to each adjacent enemy
                            foreach (CharacterBase adjacentEnemy in adjacentEnemies)
                            {
                                act.target = adjacentEnemy;
                                damage = CalculateDamage(enemyCharacter, act); // This might need adjustment if 'CalculateDamage' depends on 'act.target' 
                                QueueAction(act,damage);
                                if(act.spell.applyTarget){
                                    act.spell.applyTarget.ApplyEffect(act.target.characterStats);
                                }
                            }
                            // Determine the central point of the AOE, could be based on the player position or predefined.
                            aoeCenter = DeterminePlayerAOECenter();
                            yield return StartCoroutine(act.spell.PlaySpellEffect(activeCharacter, aoeCenter));

                            //spell buff effects, some spells have buffs but arent of type buff
                            if(act.spell.applySelf){
                                act.spell.applySelf.ApplyEffect(activeCharacter.characterStats);
                            }
                            break;
                        case EffectType.SPAWN:
                            // Determine the number of enemies to spawn based on the spell's power
                            int numToSpawn = Mathf.FloorToInt(act.spell.basePower);
                            Encounter encounter = encounterManager.currentEncounter;

                            for (int i = 0; i < numToSpawn; i++)
                            {
                                bool hasOpenSpace = false;
                                int spawnIndex = 0;

                                // Search for an unoccupied position

                                foreach (Vector3 position in encounter.enemyBattlePositions)
                                {

                                    if (!IsPositionOccupied(position))
                                    {
                                        hasOpenSpace = true;
                                        break; // Found an unoccupied position
                                    }else{}
                                    spawnIndex+=1;
                                }

                                if (hasOpenSpace)
                                {
                                    // Call the Spawn method of the spell
                                    GameObject spawn = ((EnemySpell)act.spell).Spawn(encounter.enemyBattlePositions[spawnIndex], i);
                                    if (spawn)
                                    {
                                        aliveEnemies.Add(spawn);
                                        UpdateEnemyAdjacencyList(spawn, spawnIndex);
                                    }
                                }
                                else
                                {
                                    Debug.LogWarning("No unoccupied positions available for spawning.");
                                    break; // No more available positions
                                }
                            }
                            // For non-AOE spells, play the effect and then apply the effect to the target
                            yield return StartCoroutine(act.spell.PlaySpellEffect(activeCharacter, act.target));
                            
                            //spell buff effects, some spells have buffs but arent of type buff
                            if(act.spell.applySelf){
                                act.spell.applySelf.ApplyEffect(activeCharacter.characterStats);
                            }
                            if(act.spell.applyTarget){
                                act.spell.applyTarget.ApplyEffect(act.target.characterStats);
                            }
                            break;
                        // Add other cases as needed for different spell types
                        default:
                            Debug.LogError("Unhandled spell effect type");
                            break;
                    }

                    if (act.spell.postDamageConditions)
                    {
                        yield return StartCoroutine(act.spell.postDamageConditions.ApplyPostDamageEffect(enemyCharacter, act));
                    }
                    act.MarkAsFinished();
                }

                
                break;
            case ActionType.EMPTY:
                Debug.Log("action was called from character base rather that playercharacter/enemycharacter");
                break;
        }
        battleUI.SpellLabel.SetActive(false);
        battleUI.SpellLabel.GetComponentInChildren<TextMeshProUGUI>().text = "";
        yield return new WaitUntil(() => act.IsFinished);
    }
    private bool IsPositionOccupied(Vector3 position, float tolerance = .5f)
    {
        foreach (GameObject enemy in aliveEnemies)
        {
            Debug.Log("enemy position"+enemy.transform.position);
            Debug.Log("position"+position);
            if (Vector3.Distance(enemy.transform.position, position) < tolerance)
            {
                return true;
            }
        }
        return false;
    }

    public void AddEnemyToBattle(GameObject enemy)
    {
        aliveEnemies.Add(enemy);
        // Additional logic to integrate the new enemy into the battle system
    }
    private void MoveCharacterToTarget(CharacterBase character, CharacterBase target, float damage, Act act)
    {
        Vector3 startPosition = character.transform.position;
        Vector3 targetPosition = target.transform.position;
        StartCoroutine(MoveCharacterCoroutine(startPosition, targetPosition, character, damage, target, act));
    }
    public void DealDamage(bool isCritical, bool isWeaknessHit, bool isBlock, Vector3 position, int damageAmount)
    {
        GameObject textPrefab = isCritical ? critDamageTextPrefab : damageTextPrefab;
        GameObject spawnedText = Instantiate(textPrefab, position + new Vector3(0, 0, -0.5f), Quaternion.identity, damageNumbersCanvas);
        TextMeshProUGUI textComponent = spawnedText.GetComponent<TextMeshProUGUI>();
        textComponent.text = damageAmount.ToString();

        DamageTextBehavior damageTextBehavior = spawnedText.GetComponent<DamageTextBehavior>();

        if (isWeaknessHit)
        {
            damageTextBehavior.InstantiateWeaknessText();
        }

        if (isBlock)
        {
            damageTextBehavior.InstantiateBlockText();
        }

        StartCoroutine(damageTextBehavior.ScaleSequence());
        hitSource.volume = soundManager.GetVolume();
        hitSource.Play();
    }



        public void HealDamage(bool isCritical, Vector3 position, int healAmount)
    {
        GameObject textPrefab = isCritical ? critHealTextPrefab : healTextPrefab;
        GameObject spawnedText = Instantiate(textPrefab, position, Quaternion.identity, damageNumbersCanvas);
        TextMeshProUGUI textComponent = spawnedText.GetComponent<TextMeshProUGUI>();
        textComponent.text = healAmount.ToString();
            Debug.Log("preheal");
         StartCoroutine(spawnedText.GetComponent<DamageTextBehavior>().ScaleSequence());
        // The DamageTextBehavior script attached to the prefab will handle the fading and rising behavior.
    }

    private IEnumerator MoveCharacterCoroutine(Vector3 start, Vector3 target, CharacterBase character, float damage, CharacterBase targetCharacter, Act act)
    {
        // Calculate the offset position using bounds
        Vector3 directionToTarget = (target - start).normalized;
        float attackerExtent = character.GetComponentInChildren<SpriteRenderer>().bounds.extents.magnitude; 
        float targetExtent = targetCharacter.GetComponentInChildren<SpriteRenderer>().bounds.extents.magnitude;
        float totalOffset = attackerExtent + targetExtent;
        Vector3 offsetPosition = target - directionToTarget * totalOffset;

        float elapsed = 0f;
        float duration = hopCurve.keys[hopCurve.length - 1].time; // Get the end time of the curve. This determines the total duration.
        //Here we'll look for the animator in children, trigger "Attack". wait for the animation to finish and then carry on
        Animator characterAnimator = character.GetComponentInChildren<Animator>();

        if(characterAnimator){
            characterAnimator.SetBool("IsJumping", true);
        }  
        while (elapsed < duration)
        {
            float curveValue = hopCurve.Evaluate(elapsed / duration);
            
            // Compute the position based on Lerp and adjust Y position based on the curve
            Vector3 nextPosition = Vector3.Lerp(start, offsetPosition, elapsed / duration);
            nextPosition.y += curveValue;

            character.transform.position = nextPosition;

            elapsed += Time.deltaTime;
            yield return null;
        }
        character.transform.position = offsetPosition; // Ensure character reaches the offset position
        if(characterAnimator){
            characterAnimator.SetBool("IsJumping", false);
        }  
    

        if (characterAnimator != null)
        {
            characterAnimator.SetTrigger("Attack");
            yield return new WaitForSeconds(.05f);
            yield return new WaitForSeconds(Mathf.Min(characterAnimator.GetCurrentAnimatorStateInfo(0).length,.6f));
        }else{
            Debug.Log("attack animator not found");
        }
        VisualEffect instantiatedEffect = Instantiate(hitEffectPrefab, targetCharacter.transform.position, Quaternion.identity);
        instantiatedEffect.Play();

        Destroy(instantiatedEffect.gameObject, 1f);

        // Deal damage to the target
        targetCharacter.TakeDamage(damage, act.isCritical,act.isWeak,act.isBlock);


        elapsed = 0f;

        //returns to original position
        while (elapsed < duration)
        {
            float curveValue = hopCurve.Evaluate(1 - (elapsed / duration)); // Reverse the curve for moving back
                
            // Compute the position based on Lerp and adjust Y position based on the curve
            Vector3 nextPosition = Vector3.Lerp(offsetPosition, start, elapsed / duration);
            nextPosition.y += curveValue;

            character.transform.position = nextPosition;

            elapsed += Time.deltaTime;
            yield return null;
        }
        character.transform.position = start; // Ensure character returns to the exact start position
        Debug.Log("ended attack");
        act.MarkAsFinished();
    }

    bool IsCriticalHit(float critRate, Act act)
    {
        float randomValue = UnityEngine.Random.Range(0f, 1f);
        return randomValue <= critRate + act.target.characterStats.GetEffectiveStat(StatType.CRIT_VULNERABILITY);
    }
    bool IsBlockHit(float blockRate)
    {
        float randomValue = UnityEngine.Random.Range(0f, 1f);
        Debug.Log( randomValue <= blockRate);
        return randomValue <= blockRate;
    }
    public float SpellHeat(Spell spell){
        return spell.manaCost/2f/AOEHeatScaling;
    }

    public float getGeatHeatGain(CharacterBase target){
        float heat = 0f;
        PlayerCharacter pc = activeCharacter as PlayerCharacter;
        Spell spell = pc.GetSelectedSpell();

        if(spell){
            if(spell.effectType != EffectType.HEAL && spell.effectType != EffectType.AOE_HEAL)
            {
                if(target.characterStats.weakness == spell.fireType || isWeakOverride)
                {
                    heat += weaknessHeatGain/AOEHeatScaling;
                }
            }
            
            heat += weaknessHeatGain/2/AOEHeatScaling;
            return heat;
        }

        return 0f;
    }
    public HeatState GetHeatColor(float heatValue)
    {
        if (heatValue <= 40f)
        {
            return HeatState.Yellow;
        }
        else if (heatValue > 40f && heatValue < 80f)
        {
            return HeatState.Green;
        }
        else
        {
            return HeatState.Red;
        }
    }


    public void SetWeaknessMultiplier(float newMultiplier){
        weaknessMultiplier = newMultiplier;
    }
    public void SetIsWeakOverride(bool isWeak){
        isWeakOverride = isWeak;
    }
    public void SetAttackManaRegen(float regen){
        attackManaRegen = regen;
    }
    public void AddAttackPower(float newMultiplier){
        attackPower += newMultiplier;
    }
    public void SetDamage(float newDamage){
        damage = newDamage;
    }
    public float GetDamage(){
        return damage;
    }
    //DamageEquation(attackStat * (randVal(0.8,1,2)) * attackPower * CritMultiplier * BlockMultiplier 
    //* DefenseMultiplier * WeaknessMultiplier * Heat Multiplier * IgniteMultiplier)
    //attackPower is 0.6 for normal attacks different for spells
    //block is ignored for player attacks, and crit is ignored for enemy attacks
    // reminder that defence represents a percentage but is stored as an noraml number (divide by 100)
    public float CalculateDamage(CharacterBase attacker, Act act)
    {
        float critMultiplier = 1f;
        float blockMultiplier = 1f;
        attackPower = 0.6f; // Default for normal attacks
        weaknessMultiplier = 1f;
        float heatMultiplier = 1f;
        float heatDamageBoost = 0;
        float heatDefenceDrop = 0;
        float IgniteMultipler = 1f;
        float noDefense = 1;// when it is 0 treats the calc as not having defense
        // Shared damage components
        damage = attacker.characterStats.GetEffectiveStat(StatType.ATTACK)
                * UnityEngine.Random.Range(0.8f, 1.2f);

        if (attacker is PlayerCharacter playerAttacker)
        {
            critMultiplier = IsCriticalHit(playerAttacker.characterStats.GetEffectiveStat(StatType.CRIT_RATE), act) ? 1.25f : 1f;
            if (critMultiplier == 1.25f) 
            {
                act.isCritical = true;
                Debug.Log($"{playerAttacker.characterName} crit");
            }
                
            if (act.spell != null)
            {
                AOEHeatScaling = 1f;
                attackPower = act.spell.basePower;
                EffectType spellEffect = act.spell.effectType;
                switch(spellEffect)
                {
                    case EffectType.AOE_DAMAGE:
                        AOEHeatScaling = aliveEnemies.Count;
                    break;
                    case EffectType.ADJACENT_AOE_DAMAGE:
                        AOEHeatScaling = 2;
                    break;
                    case EffectType.AOE_HEAL:
                        AOEHeatScaling = alivePlayers.Count;
                        break;
                    default:
                    break;
                }
                //event for when player uses spells

                 OnPlayerSpellUsed?.Invoke(act, attacker);
                if(act.spell.effectType != EffectType.HEAL && act.spell.effectType != EffectType.AOE_HEAL)
                {
                    if (act.target.characterStats.weakness == act.spell.fireType)
                    {
                        weaknessMultiplier = 1.3f;
                        act.isWeak = true;
                    }
                }
                enemyHeat += getGeatHeatGain(act.target);
                playerHeat += SpellHeat(act.spell);
                
                if (act.spell.preDamageConditions != null)
                {
                    Debug.Log("spell");
                    float changeDamage = act.spell.preDamageConditions.AdjustDamage(activeCharacter, act, damage);
                    if(changeDamage != 0){
                        damage = changeDamage;
                    }
                }
            }

            if (act.isCritical)
            {
                critMultiplier = 1.25f;
            }
            // Heat effects
            if (GetHeatColor(playerHeat) ==  HeatState.Green)
            {
                heatDamageBoost = 0.2f;
            }

            if (GetHeatColor(enemyHeat) ==  HeatState.Red)
            {
                heatDefenceDrop = 0.2f;
            }
            
            IgniteMultipler = 1 + usedFuel * 0.4f;
            playerHeat -= 10 * usedFuel;
            if (act.spell != null && (act.spell.effectType == EffectType.HEAL || act.spell.effectType == EffectType.AOE_HEAL))
            {
                damage *= 1 + attacker.characterStats.GetEffectiveStat(StatType.MANA)/100f;//scales attack stat with mana
                weaknessMultiplier = 1f;
                noDefense = 0;
                heatDefenceDrop = 0;
            }
        }
        else // Enemy logic
        {
            blockMultiplier = IsBlockHit(act.target.characterStats.GetEffectiveStat(StatType.BLOCK_RATE)) ? 0.5f : 1f;
            if(blockMultiplier == .5f){
                Debug.Log("isblock");
                act.isBlock = true;
            }
            // Heat effects for enemies
            if (GetHeatColor(enemyHeat) ==  HeatState.Green)
            {
                heatDamageBoost = 0.2f;
            }

            if (GetHeatColor(playerHeat) ==  HeatState.Red)
            {
                heatDefenceDrop = 0.2f;
            }

            if (act.spell != null && (act.spell.effectType == EffectType.HEAL || act.spell.effectType == EffectType.AOE_HEAL))
            {
                damage *= 1.2f;
                weaknessMultiplier = 1f;
                noDefense = 0;
                heatDefenceDrop = 0;
            }
        }

        heatMultiplier = 1 + heatDamageBoost + heatDefenceDrop;

        // Final damage multiplier components
        damage *= attackPower * critMultiplier * blockMultiplier
            * (1 - (act.target.characterStats.GetEffectiveStat(StatType.DEFENSE) *noDefense / 100f))
            * weaknessMultiplier * heatMultiplier * IgniteMultipler;
        //informs others that damage was dealt
        if (OnDamageDealt != null)
        {
           OnDamageDealt(attacker,act, damage);
        }

        return damage;
    }


    public void InitializeTurnOrder()
    {
        List<CharacterBase> allCharacters = new List<CharacterBase>();

        // Add other characters from the party manager.
        foreach(GameObject obj in alivePlayers){
            if(obj.GetComponent<CharacterBase>().IsAlive)
            {
                allCharacters.Add(obj.GetComponent<CharacterBase>());
            }
        }

        // Add enemy characters from the current encounter.
        if (encounterManager.currentEncounter.GetSpawnedEnemies() != null && encounterManager.currentEncounter.GetSpawnedEnemies().Count > 0)
        {
            allCharacters.AddRange(encounterManager.currentEncounter.GetSpawnedEnemies().Where(e => e.GetComponent<EnemyCharacter>().IsAlive).Select(e => e.GetComponent<EnemyCharacter>()).ToList());
        }
        else
        {
            Debug.LogError("Enemies list is null or empty!");
        }
        // A list to store the times it will take for each character to finish the race
        List<Tuple<CharacterBase, float, bool>> finishEvents = new List<Tuple<CharacterBase, float, bool>>();
        //Debug.Log(allCharacters.Count);
        foreach (var character in allCharacters)
        {
            float speed = character.characterStats.GetEffectiveStat(StatType.SPEED);
            float timeToFinish = raceDistance / (speed * UnityEngine.Random.Range(0.8f, 1.2f)); //makes distance vary
            float timeToFinishTwice = 2 * timeToFinish;

            finishEvents.Add(new Tuple<CharacterBase, float, bool>(character, timeToFinish, false)); // First finish
            finishEvents.Add(new Tuple<CharacterBase, float, bool>(character, timeToFinishTwice, true)); // Second finish (double)
        }

        // Order the events by their finish times, then by random (to handle ties)
        List<Tuple<CharacterBase, float, bool>> orderedEvents = finishEvents.OrderBy(t => t.Item2).ThenBy(t => Guid.NewGuid()).ToList();

        Queue<CharacterBase> tempQueue = new Queue<CharacterBase>();
        HashSet<CharacterBase> addedCharacters = new HashSet<CharacterBase>();

        float lastFinishTime = orderedEvents.Where(t => !t.Item3).Max(t => t.Item2); // Maximum time for the first finishes

        foreach (var entry in orderedEvents)
        {
            if (entry.Item3 == false)// if its their first action
            {
                tempQueue.Enqueue(entry.Item1);
                addedCharacters.Add(entry.Item1);
            }
            else
            {// if it's their second action
                //characters will not be enqued a second time if all characters have already gone at least once
                //this includes if their second run is tied for last with the last place first run
                if (entry.Item2 != lastFinishTime && addedCharacters.Count < allCharacters.Count)
                {
                    //  Debug.Log($"this:{entry.Item2} last:{lastFinishTime}");
                    tempQueue.Enqueue(entry.Item1);
                }
            }
        }

        turnQueue = new Queue<CharacterBase>(tempQueue);
    }


    public void ToggleTurnState()
    {
        if(turnState == TurnState.FREEZE)
        {
            turnState = TurnState.BURN;
        }
        else
        {
            turnState = TurnState.FREEZE;
        }
    }

    public float ConsumeFuel()
    {   
        //can't use more than three per action
        if(usedFuel < 3){
            if( fuel > 0)
            {
                fuel--;
                usedFuel++;
            }

            battleUI = FindObjectOfType<BattleUI>();
            battleUI.UpdateFuelUI();
        }

        return usedFuel;
    }

    public void AccumulateFuel()
    {
        if( fuel < maxFuel)
        {
            fuel += 1;
        }
        battleUI = FindObjectOfType<BattleUI>();
        battleUI.UpdateFuelUI();
    }

    // Add nested enums/classes as needed
}
