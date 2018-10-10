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
        // Prépare le spawn de l'ennemis selon l'ID transmis
        GameObject mob = Instantiate(mobsArray[mobID], transform.position, transform.rotation);

        // Spawn l'ennemis sur tout le résaux
        NetworkServer.Spawn(mob);

        // Retourne l'ID de l'ennemis
        return mob.GetInstanceID();
    }
}
