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

        // Récupère la caméra passive
        defaultCamera = Camera.main.gameObject;

        // Active le joueur
        EnablePlayer();

        waveValues = FindObjectOfType<WaveValues>();

        if (!isServer)
            return;

        // Ajoute le joueur à la liste globale des joueurs
        AddPlayer();

        // Indique qu'un joueur vivant supplémentaire est présent
        waveValues.playerAlive++;
	}

    void DisablePlayer()
    {
        // Active la caméra passive si c'est le joueur locale
        if (isLocalPlayer)
            defaultCamera.SetActive(true);

        // Désactive les éléments partagés
        OnToogleShared.Invoke(false);

        // Si c'est le joueur local
        if (isLocalPlayer)
            // Désactive les éléments locaux
            OnToogleLocal.Invoke(false);
        else
            // Désactive les éléments vu par les autres clients
            OnToogleRemote.Invoke(false);
    }

    void EnablePlayer()
    {
        // Désactive la caméra passive si c'est le joueur locale
        if (isLocalPlayer)
            defaultCamera.SetActive(false);

        // Active les éléments partagés
        OnToogleShared.Invoke(true);

        // Si c'est le joueur local
        if (isLocalPlayer)
            // Active les éléments locaux
            OnToogleLocal.Invoke(true);
        else
            // Active les éléments vu par les autres clients
            OnToogleRemote.Invoke(true);
    }

    void AddPlayer()
    {
        // Récupère l'objet contenant la liste globale des joueurs
        WaveValues waveValues = FindObjectOfType<WaveValues>();

        // Prépare l'ajout du joueur à la liste
        WaveValues.PlayerInfo thisPlayer = new WaveValues.PlayerInfo();

        // Ajoute l'ID réseau du joueur à ses informations
        thisPlayer.playerNetID = GetComponent<NetworkIdentity>();

        // Si l'objet éxiste
        if (waveValues != null)
            // Ajoute le joueur à la liste globale des joueurs
            waveValues.AddPlayer(thisPlayer);
        else
            // Retente d'ajouter le joueur
            AddPlayer();
    }

    public void PlayerDie()
    {
        // Désactive le joueur
        DisablePlayer();

        // Indique qu'il y a un joueur vivant en moins
        waveValues.playerAlive--;

        // Déplace le joueur à la zone de mort
        transform.position = GameObject.FindGameObjectWithTag("Finish").transform.position;
    }

    public void PlayerRespawn()
    {
        // Récupère le composant HealthValues du joueur
        HealthValues healthValues = GetComponent<HealthValues>();

        // Reset son nombre de points de vie au maximum
        healthValues.actualHealth = healthValues.maxHealth;

        // Indique qu'un joueur supplémentaire est vivant
        waveValues.playerAlive++;

        // Déplace le joueur à un point de respawn
        transform.position = GameObject.FindGameObjectWithTag("Respawn").transform.position;

        // Active le joueur
        EnablePlayer();
    }

    private void OnDestroy()
    {
        Debug.Log("On destroy...");

        WaveValues waveValues = FindObjectOfType<WaveValues>();
        NetworkIdentity networkIdentity = GetComponent<NetworkIdentity>();

        // Retire les informations concernant le joueur
        if (waveValues != null && networkIdentity != null)
            waveValues.RemovePlayer(networkIdentity);
    }
}
