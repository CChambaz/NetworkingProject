using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Health : NetworkBehaviour
{
    public enum Type
    {
        PLAYER,
        MOB
    }

    [SerializeField] public Type type;
    [SerializeField] bool isRegenerating;
    [SerializeField] int regenerationRate;
    [SerializeField] float regenerationCoolDown;
    [SerializeField] GameObject deathEffect;

    float hasNotRegenerateSince = 0f;

    HealthValues healthValues;

    // Use this for initialization
    void Start() {
        if (!isServer)
            Destroy(this);

        healthValues = GetComponent<HealthValues>();
    }

    // Update is called once per frame
    [Server]
    void Update() {
        if (isRegenerating && healthValues.isAlive)
        {
            hasNotRegenerateSince += Time.deltaTime;

            if (hasNotRegenerateSince > regenerationCoolDown)
                Regenerate();
        }
    }

    [Server]
    void Regenerate()
    {
        hasNotRegenerateSince = 0;

        if (healthValues.actualHealth < healthValues.maxHealth)
        {
            if (healthValues.actualHealth + regenerationRate >= healthValues.maxHealth)
                healthValues.actualHealth = healthValues.maxHealth;
            else
                healthValues.actualHealth += regenerationRate;
        }
    }

    [Server]
    public void TakeDamage(int damage)
    {
        healthValues.actualHealth -= damage;

        if (healthValues.actualHealth <= 0)
        {
            GameObject death = Instantiate(deathEffect, transform.position, transform.rotation);

            NetworkServer.Spawn(death);

            switch (type)
            {
                case Type.PLAYER:
                    healthValues.isAlive = false;
                    break;
                case Type.MOB:
                    gameObject.GetComponent<Mobs>().Die();
                    break;
                default:
                    break;
            }
        }
    }
}
