using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Area : NetworkBehaviour {
    [SerializeField] public List<Spawners> dedicatedSpawn;
    List<GameObject> playersInArea = new List<GameObject>();

    private void Start()
    {
        if (!isServer)
            Destroy(this);
    }

    [Server]
    private void OnTriggerStay(Collider other)
    {
        // Du moment ou un joueur se trouve dans la zone
        if(other.tag == "Player")
        {
            // Active les spawns qui lui sont attribués
            foreach (Spawners spawn in dedicatedSpawn)
                spawn.isActive = true;

            // Ajoute le joueur dans la liste des joueurs présent dans la zone si il ne s'y trouve pas déjà
            if(!playersInArea.Contains(other.gameObject))
                playersInArea.Add(other.gameObject);
        }
    }

    [Server]
    private void OnTriggerExit(Collider other)
    {
        // Du moment ou un joueur quitte la zone
        if (other.tag == "Player")
        {
            // Retire le joueur de la liste des joueurs actuellement dans la zone
            if (playersInArea.Contains(other.gameObject))
                playersInArea.Remove(other.gameObject);

            // Si il s'agissait du dernier joueur sur place, désactive les spawns associés
            if (playersInArea.Count == 0)
            {
                foreach (Spawners spawn in dedicatedSpawn)
                    spawn.isActive = false;
            }
        }
    }
}
