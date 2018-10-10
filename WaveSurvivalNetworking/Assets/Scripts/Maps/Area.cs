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
        // While a player is in the area
        if(other.tag == "Player")
        {
            // Set active the spawn of this area
            foreach (Spawners spawn in dedicatedSpawn)
                spawn.isActive = true;

            // Add the player to the list of the player actually in the area if he's not already registered
            if(!playersInArea.Contains(other.gameObject))
                playersInArea.Add(other.gameObject);
        }
    }

    [Server]
    private void OnTriggerExit(Collider other)
    {
        // When a player leave the area
        if (other.tag == "Player")
        {
            // Remove the player from the list of the player actually in the area
            if (playersInArea.Contains(other.gameObject))
                playersInArea.Remove(other.gameObject);

            // If it was the last, disable the spawn of this area
            if (playersInArea.Count == 0)
            {
                foreach (Spawners spawn in dedicatedSpawn)
                    spawn.isActive = false;
            }
        }
    }
}
