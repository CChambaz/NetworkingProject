using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class HealthValues : NetworkBehaviour
{
    [SerializeField] public int maxHealth;

    [SyncVar(hook = "OnHealthChanged")] public int actualHealth;
    [SyncVar(hook = "OnAliveChanged")] public bool isAlive = true;

    // Use this for initialization
    void Start()
    {
        actualHealth = maxHealth;
    }

    void OnHealthChanged(int health)
    {
        actualHealth = health;
    }

    void OnAliveChanged(bool alive)
    {
        isAlive = alive;

        // Détermine l'action à entreprendre selon la valeur transmise
        if (isAlive)
            GetComponent<Player>().PlayerRespawn();
        else
            GetComponent<Player>().PlayerDie();
    }
}
