using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;


public class WaveManager : NetworkBehaviour
{
    [Header("Mobs parameters")]
    [SerializeField] int mobToSpawn;
    [SerializeField] float mobMultiplier;

    [Header("Wave parameters")]
    [SerializeField] int waveToSurvive;
    [SerializeField] float timeBetweenWaves;

    float timeWaitedForNextWave = 0f;

    SpawnManager spawnManager;
    WaveValues waveValues;

	// Use this for initialization
	void Start ()
    {
        if (!isServer)
            Destroy(this);

        Debug.Log("Wave manager initialized...");

        spawnManager = FindObjectOfType<SpawnManager>();
        waveValues = FindObjectOfType<WaveValues>();
    }
	
	// Update is called once per frame
    [Server]
	void Update ()
    {
        // Vérifie qu'il n'y a pas de vague active et que la partie n'est pas terminée
        if (!waveValues.isWaveActive && waveValues.gameStatus == -1)
        {
            timeWaitedForNextWave += Time.deltaTime;

            // Si le temps écoulé depuis la précédente vague est suffisant
            if(timeWaitedForNextWave >= timeBetweenWaves)
                // Lance la vague suivante
                StartWave();
        }
	}

    [Server]
    void StartWave()
    {
        // Incrémente le numéro de vague
        waveValues.waveNumber++;

        // Défini qu'une vague est en cours
        waveValues.isWaveActive = true;

        // Lance la coroutine de spawn avec le nombre d'ennemis à spawn
        StartCoroutine(spawnManager.StartSpawn(mobToSpawn));
    }

    [Server]
    public void EndWave()
    {
        // Prépare le timer pour l'attente de la prochaine vague
        timeWaitedForNextWave = 0;

        // Défini qu'aucune vague n'est active
        waveValues.isWaveActive = false;

        // Détermine le nombre d'ennemis à spawner lors de la vague suivante
        mobToSpawn = (int)(mobToSpawn * mobMultiplier);
        
        // Check si tout les joueurs sont en vie
        foreach(WaveValues.PlayerInfo player in waveValues.players)
        {
            // Si le joueur n'est pas en vie
            if (!player.playerNetID.GetComponent<HealthValues>().isAlive)
                // Défini qu'il l'est, lançant son respawn
                player.playerNetID.GetComponent<HealthValues>().isAlive = true;
        }

        // Vérifie s'il s'agit de la dernière manche et assigne le statut victoire si c'est le cas
        if (waveValues.waveNumber == waveToSurvive)
            waveValues.gameStatus = 1;
    }
}
