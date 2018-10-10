using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Events;

[System.Serializable] public class ToogleEvent : UnityEvent<bool> {}

public class Player : NetworkBehaviour
{
    [SerializeField] ToogleEvent OnToogleShared;
    [SerializeField] ToogleEvent OnToogleLocal;
    [SerializeField] ToogleEvent OnToogleRemote;

    GameObject defaultCamera;

    WaveValues waveValues;

    // Use this for initialization
    void Start () {
        Debug.Log("Player initialized...");

        // Get the default camera
        defaultCamera = Camera.main.gameObject;

        EnablePlayer();

        waveValues = FindObjectOfType<WaveValues>();

        if (!isServer)
            return;

        // Add the player to the global player list
        AddPlayer();

        // Add a player to the player alive count
        waveValues.playerAlive++;
	}

    void DisablePlayer()
    {
        // Set active the default camera if it is the local player
        if (isLocalPlayer)
            defaultCamera.SetActive(true);

        // Disable the shared elements
        OnToogleShared.Invoke(false);

        if (isLocalPlayer)
            // Disable the local elements
            OnToogleLocal.Invoke(false);
        else
            // Disable the remote elements
            OnToogleRemote.Invoke(false);
    }

    void EnablePlayer()
    {
        // Disable the default camera if it is the local player
        if (isLocalPlayer)
            defaultCamera.SetActive(false);

        // Enable the shared elements
        OnToogleShared.Invoke(true);
        
        if (isLocalPlayer)
            // Enable the local elements
            OnToogleLocal.Invoke(true);
        else
            // enable the remote elements
            OnToogleRemote.Invoke(true);
    }

    void AddPlayer()
    {
        WaveValues waveValues = FindObjectOfType<WaveValues>();

        // Prepare the addition of the player
        WaveValues.PlayerInfo thisPlayer = new WaveValues.PlayerInfo();

        // Add the NetID to the information of the player
        thisPlayer.playerNetID = GetComponent<NetworkIdentity>();

        if (waveValues != null)
            // Add the player to the global list of the players
            waveValues.AddPlayer(thisPlayer);
        else
            // Retry to add the player
            AddPlayer();
    }

    public void PlayerDie()
    {
        // Disable the player
        DisablePlayer();

        // Remove the player of the player alive count
        waveValues.playerAlive--;

        // Move the player to the deadzone
        transform.position = GameObject.FindGameObjectWithTag("Finish").transform.position;
    }

    public void PlayerRespawn()
    {
        HealthValues healthValues = GetComponent<HealthValues>();

        // Reset the amount of health to his maximum
        healthValues.actualHealth = healthValues.maxHealth;

        // Add the player to the player alive count
        waveValues.playerAlive++;

        // Move the player to the respawn position
        transform.position = GameObject.FindGameObjectWithTag("Respawn").transform.position;

        // Enable the player
        EnablePlayer();
    }

    private void OnDestroy()
    {
        Debug.Log("On destroy...");

        WaveValues waveValues = FindObjectOfType<WaveValues>();
        NetworkIdentity networkIdentity = GetComponent<NetworkIdentity>();

        // Remove the information of the player
        if (waveValues != null && networkIdentity != null)
        {
            waveValues.RemovePlayer(networkIdentity);

            if (GetComponent<HealthValues>().isAlive)
                waveValues.playerAlive--;
        }
    }
}
