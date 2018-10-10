using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class SpawnManager : NetworkBehaviour
{
    [SerializeField] List<Area> areas;
    [SerializeField] float spawnCoolDown;
    List<Spawners> spawners = new List<Spawners>();

    public List<int> mobsActive = new List<int>();

    int mobToSpawn;
    int lastSpawnerIndex = 0;

    WaveManager waveManager;
    WaveValues waveValues;

	// Use this for initialization
	void Start () {
        if (!isServer)
            Destroy(this);

        waveManager = FindObjectOfType<WaveManager>();
        waveValues = FindObjectOfType<WaveValues>();

        // Get all the enemies spawn on the map
        foreach (Spawners spawn in FindObjectsOfType<Spawners>())
            spawners.Add(spawn);
	}

    private void Update()
    {
        if (!isServer)
            return;

        if (mobsActive.Count != waveValues.mobsLeft)
            waveValues.mobsLeft = mobsActive.Count;
    }

    [Server]
    public IEnumerator StartSpawn(int mobNumber)
    {
        // Get the number of enemies to spawn
        mobToSpawn = mobNumber;

        // While there is enemies to spawn
        while(mobToSpawn > 0)
        {
            // Start the spawn and wait for it to finish
            yield return new WaitUntil(() => Spawn());

            // Wait a moment before the next spawn
            yield return new WaitForSeconds(spawnCoolDown);
        }

        // Wait that all the enemies have been killed
        yield return new WaitUntil(() => mobsActive.Count == 0);

        // Call the end of the wave
        waveManager.EndWave();
    }

    [Server]
    bool Spawn()
    {
        // Go around all the spawns, starting by the last used
        for(int i = lastSpawnerIndex; i < spawners.Count; i++)
        {
            // If the spawn is active
            if(spawners[i].isActive)
            {
                // Spawn the enemie and add it to the list of the active enemies
                mobsActive.Add(spawners[i].Spawn(Random.Range(0, spawners[i].mobsArray.Length - 1)));

                // Reduce the number of enemies needing to be spawnes
                mobToSpawn--;
                
                // If all the spawner have been used
                if (i + 1 >= spawners.Count)
                    // Set the index to 0
                    lastSpawnerIndex = 0;
                else
                    // Otherwise, prepare the next spawn on the spawn following this one
                    lastSpawnerIndex = i + 1;

                // Informe that the spawn is over
                return true;
            }
        }

        // Set the index to 0
        lastSpawnerIndex = 0;

        // Informe that the spawn is over
        return true;
    }
}
