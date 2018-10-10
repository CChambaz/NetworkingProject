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

        // Prépare le positionement de l'ennemi
        NavMeshHit closestHit;

        // Récupère la hauteur de l'ennemi
        Vector3 mobHeight = new Vector3(0, navigationAgent.height, 0);

        // Détermine et récupère la position valide la plus proche sur le Navmesh 
        if (NavMesh.SamplePosition(transform.position, out closestHit, 20, NavMesh.AllAreas))
            // Défini la position initial de l'ennemi
            transform.position = closestHit.position + mobHeight;

        // Activation de l'agent de navigation
        navigationAgent.enabled = true;

        // Défini la cible de base
        if (isServer)
            SetDestination();
    }

    // Update is called once per frame
    void Update() {
        // Si la cible a changée
        if (previousTargetID != targetID)
            // Met à jour la cible
            UpdateDestinationTransform();

        // Si la cible existe
        if (targetTransform != null)
            // Défini la déstination
            navigationAgent.SetDestination(targetTransform.position);

        if (!isServer)
            return;

        // Incrémente le compteur de l'attaque
        hasNotAttackSince += Time.deltaTime;

        // Vérifie que le mob peut attaquer et que la cible éxiste
        if (hasNotAttackSince > attackCoolDown && targetTransform != null)
            Attack();

        // Vérifie qu'aucun joueur n'est plus près que le joueur actuellement pris pour cible
        SetDestination();
    }

    [Server]
    void Attack()
    {
        // Vérifie que le mob est à porté de sa cible
        if (Vector3.Distance(transform.position, targetTransform.position) > attackRange)
            return;

        // Réinitialise le timer de l'attque
        hasNotAttackSince = 0;

        // Récupère le compsant Health de la cible
        Health target = targetTransform.GetComponent<Health>();

        // Si ce composant éxiste
        if (target != null)
            // Lui inflige des dégats
            target.TakeDamage(attackDamage);
    }

    [Server]
    public void Die()
    {
        // Ajoute les points correspondant au score globale
        scoreManager.AddPoints(pointValue);

        // Retire l'ennemis de la liste globale des ennemis actifs 
        spawnManager.mobsActive.Remove(gameObject.GetInstanceID());

        // Détruit l'ennemi
        NetworkServer.Destroy(gameObject);
    }

    void SetDestination()
    {
        // Passe sur tout les joueurs de la liste globale des joueurs
        for(int i = 0; i < waveValues.players.Count; i++)
        {
            // Récupère le transform du joueur
            Transform playerTransform = waveValues.players[i].playerNetID.transform;

            // Vérifie qu'une cible est assigné
            if (targetTransform != null)
            {
                // Si la distance entre l'ennemi et la cible est plus grande que la distance entre l'ennemi et le joueur
                if (Vector3.Distance(transform.position, targetTransform.position) > Vector3.Distance(transform.position, playerTransform.position))
                {
                    // Définit la nouvelle cible
                    targetID = waveValues.players[i].playerNetID.netId.Value;
                }
            }
            else
            {
                // Assigne la cible initiale
                targetID = waveValues.players[i].playerNetID.netId.Value;
            }
        }
    }

    void UpdateDestinationTransform()
    {
        // Défini l'ID réseau de l'objet recherché
        NetworkInstanceId netInstanceId = new NetworkInstanceId(targetID);

        // Prépare l'identité réseau trouvée
        NetworkIdentity foundNetworkIdentity = null;

        // Défini si il s'agit du serveur ou d'un client puis cherche et récupère l'objet désiré
        if (isServer)
            NetworkServer.objects.TryGetValue(netInstanceId, out foundNetworkIdentity);
        else
            ClientScene.objects.TryGetValue(netInstanceId, out foundNetworkIdentity);

        // Si la recherche est concluante
        if (foundNetworkIdentity)
            // Récupère le transform de l'objet trouvé
            targetTransform = foundNetworkIdentity.GetComponent<Transform>();

        // Actualise la cible précédente à la cible actuel
        previousTargetID = targetID;
    }

    void OnTargetChanged(uint newTarget)
    {
        targetID = newTarget;
    }
}
