using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Networking;

public class Mobs : NetworkBehaviour
{
    [SerializeField] int pointValue;

    [Header("Attack parameters")]
    [SerializeField] float attackRange;
    [SerializeField] int attackDamage;
    [SerializeField] float attackCoolDown;

    float hasNotAttackSince = 0f;

    private NavMeshAgent navigationAgent;
    [SyncVar(hook = "OnTargetChanged")] uint targetID = 1;
    private uint previousTargetID = 0;
    private Transform targetTransform;

    WaveValues waveValues;
    SpawnManager spawnManager;
    ScoreManager scoreManager;

    // Use this for initialization
    void Start() {
        waveValues = FindObjectOfType<WaveValues>();
        spawnManager = FindObjectOfType<SpawnManager>();
        scoreManager = FindObjectOfType<ScoreManager>();

        navigationAgent = GetComponent<NavMeshAgent>();

        // Prepare the initial position
        NavMeshHit closestHit;

        // Get the height of the agent
        Vector3 mobHeight = new Vector3(0, navigationAgent.height, 0);

        // Get the nearest valid position on the NavMesh
        if (NavMesh.SamplePosition(transform.position, out closestHit, 20, NavMesh.AllAreas))
            // Define the initial position
            transform.position = closestHit.position + mobHeight;

        // Enabled the navigation agent
        navigationAgent.enabled = true;

        // Define the base target
        if (isServer)
            SetDestination();
    }

    // Update is called once per frame
    void Update() {
        // If the target has changed
        if (previousTargetID != targetID)
            // Update the target
            UpdateDestinationTransform();

        // If the target exists
        if (targetTransform != null)
            // Difine the destination
            navigationAgent.SetDestination(targetTransform.position);

        if (!isServer)
            return;

        hasNotAttackSince += Time.deltaTime;

        // Check if it can attack and if the target exists
        if (hasNotAttackSince > attackCoolDown && targetTransform != null)
            Attack();

        // Check if there is any other closer player  
        SetDestination();
    }

    [Server]
    void Attack()
    {
        // Check if it is at range
        if (Vector3.Distance(transform.position, targetTransform.position) > attackRange)
            return;

        hasNotAttackSince = 0;

        // Get the health component of the target
        Health target = targetTransform.GetComponent<Health>();

        // If the component exist
        if (target != null)
            // Deal damage
            target.TakeDamage(attackDamage);
    }

    [Server]
    public void Die()
    {
        // Add points to the global score
        scoreManager.AddPoints(pointValue);
        
        // Remove the enemies form the active enemies list
        spawnManager.mobsActive.Remove(gameObject.GetInstanceID());

        // Destroy the object accros the network
        NetworkServer.Destroy(gameObject);
    }

    void SetDestination()
    {
        // Check every player
        for(int i = 0; i < waveValues.players.Count; i++)
        {
            // Get the player transform
            Transform playerTransform = waveValues.players[i].playerNetID.transform;

            // Check if a target is assigned
            if (targetTransform != null)
            {
                // Check if the distance beetween the enemie and the player is smaller than the distance beetween the enemies and the actual target
                if (Vector3.Distance(transform.position, targetTransform.position) > Vector3.Distance(transform.position, playerTransform.position))
                {
                    // Define the new target
                    targetID = waveValues.players[i].playerNetID.netId.Value;
                }
            }
            else
            {
                // Assign base target
                targetID = waveValues.players[i].playerNetID.netId.Value;
            }
        }
    }

    void UpdateDestinationTransform()
    {
        // Define the NetID of the searched object
        NetworkInstanceId netInstanceId = new NetworkInstanceId(targetID);

        // Prepare the NetID found
        NetworkIdentity foundNetworkIdentity = null;

        // Define if it is a client or a server and search the object
        if (isServer)
            NetworkServer.objects.TryGetValue(netInstanceId, out foundNetworkIdentity);
        else
            ClientScene.objects.TryGetValue(netInstanceId, out foundNetworkIdentity);

        // If the search found something
        if (foundNetworkIdentity)
            // Get the transform of the target
            targetTransform = foundNetworkIdentity.GetComponent<Transform>();

        // Actualise the previous target to the current
        previousTargetID = targetID;
    }

    void OnTargetChanged(uint newTarget)
    {
        targetID = newTarget;
    }
}
