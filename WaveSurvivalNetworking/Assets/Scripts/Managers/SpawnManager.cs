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

        // Récupère tout les spawns d'ennemis de la carte
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
        // Récupre le nombre d'ennemis à spawn
        mobToSpawn = mobNumber;

        // Tant que tout les ennemis n'ont pas été spawn
        while(mobToSpawn > 0)
        {
            // Attend que le spawn actuel est effectué
            yield return new WaitUntil(() => Spawn());

            // Attend un instant avant de lancer le spawn suivant
            yield return new WaitForSeconds(spawnCoolDown);
        }

        // Attend que tout les ennemis ont été tués
        yield return new WaitUntil(() => mobsActive.Count == 0);

        // Indique que la vague est términée
        waveManager.EndWave();
    }

    [Server]
    bool Spawn()
    {
        // Parcours tout les spawns d'ennemis depuis le spawn suivant le dernier spawn utilisé
        for(int i = lastSpawnerIndex; i < spawners.Count; i++)
        {
            // Si le spawn est actif
            if(spawners[i].isActive)
            {
                // Ajoute l'ennemis et le spawn
                mobsActive.Add(spawners[i].Spawn(Random.Range(0, spawners[i].mobsArray.Length - 1)));

                // Décremente le nombre d'ennemis à spawner
                mobToSpawn--;
                
                // Si le tour des spawns a été fait
                if (i + 1 >= spawners.Count)
                    // Réinitialise l'index à 0
                    lastSpawnerIndex = 0;
                else
                    // Sinon, prépare le passage suivant au spawn suivant
                    lastSpawnerIndex = i + 1;

                // Indique que le spawn est terminer
                return true;
            }
        }

        // Réinitialise l'index à 0
        lastSpawnerIndex = 0;

        // Indique que le spawn est terminer
        return true;
    }
}
