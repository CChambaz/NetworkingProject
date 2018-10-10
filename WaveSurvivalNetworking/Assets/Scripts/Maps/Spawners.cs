using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Spawners : NetworkBehaviour
{
    [SerializeField] public GameObject[] mobsArray;

    public bool isActive;

    private void Start()
    {
        if (!isServer)
            Destroy(this);
    }

    [Server]
    public int Spawn(int mobID)
    {
        // Prepare the spawn of the enemie
        GameObject mob = Instantiate(mobsArray[mobID], transform.position, transform.rotation);

        // Spawn the enemie over the network
        NetworkServer.Spawn(mob);

        // Return the ID of the enemie
        return mob.GetInstanceID();
    }
}
