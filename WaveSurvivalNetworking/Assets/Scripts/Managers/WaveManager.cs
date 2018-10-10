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
        // Check if a wave is actually running and that the game is not over
        if (!waveValues.isWaveActive && waveValues.gameStatus == -1)
        {
            timeWaitedForNextWave += Time.deltaTime;
            
            // If the time passed since the end of the last wave is enough
            if(timeWaitedForNextWave >= timeBetweenWaves)
                // Start the next wave
                StartWave();
        }
	}

    [Server]
    void StartWave()
    {
        waveValues.waveNumber++;

        // Define that a wave is in progress
        waveValues.isWaveActive = true;
        
        // Start the spawn coroutine with the amount of enemies to spawn
        StartCoroutine(spawnManager.StartSpawn(mobToSpawn));
    }

    [Server]
    public void EndWave()
    {
        timeWaitedForNextWave = 0;

        // Define that no wave is in progress
        waveValues.isWaveActive = false;

        // Define the number of enemies to spawn on the next wave
        mobToSpawn = (int)(mobToSpawn * mobMultiplier);
        
        // Check if all the player are alive
        foreach(WaveValues.PlayerInfo player in waveValues.players)
        {
            // If not alive
            if (!player.playerNetID.GetComponent<HealthValues>().isAlive)
                // Define that he is, starting his respawn
                player.playerNetID.GetComponent<HealthValues>().isAlive = true;
        }

        // Check if it was the last wave
        if (waveValues.waveNumber == waveToSurvive)
            // Define the victory status to the game
            waveValues.gameStatus = 1;
    }
}
