using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class CustomDiscovery : NetworkDiscovery
{
    float activeGamesTimeOut = 5; 

    // Dictionnaire regroupant les parties découvertes avec le temps à partie du quel elles sont éxpirées
    public Dictionary<CustomNetworkManager.GameInfo, float> activeGames = new Dictionary<CustomNetworkManager.GameInfo, float>();

    Coroutine coroutineExpiredGames = null;

	// Use this for initialization
	void Start () {
        Debug.Log("Initialized discovery...");
        Initialize();
        StartAsClient();
        coroutineExpiredGames = StartCoroutine(CleanExpiredGame());
	}
	
    public void ServerBroadcast(string data)
    {
        Debug.Log("Start broadcasting...");
        // Défini les données à envoyées
        broadcastData = data;

        // Arrète la coroutine de vérification des parties éxpirées
        StopCoroutine(coroutineExpiredGames);

        // Vide la liste des parties détectées
        activeGames.Clear();

        // Vérifie que le composant écoute ou émet des broadcast et l'arrête si nécessaire
        if (running)
            StopBroadcast();

        // Réinitialise la couche de transmission
        NetworkTransport.Shutdown();
        NetworkTransport.Init();

        // Réinitialise le composant
        Initialize();

        // Commence l'envoie de broadcast
        StartAsServer();
    }

    public void ClientBroadcast()
    {
        Debug.Log("Stop broadcasting...");
        
        // Vérifie que le composant écoute ou émet des broadcast et l'arrête si nécessaire
        if (running)
            StopBroadcast();

        // Réinitialise la couche de transmission
        NetworkTransport.Shutdown();
        NetworkTransport.Init();

        // Réinitialise le composant
        Initialize();

        // Commence l'écoute de broadcast
        StartAsClient();

        // Lance la coroutine de vérification des parties éxpirées
        coroutineExpiredGames = StartCoroutine(CleanExpiredGame());
    }

    IEnumerator CleanExpiredGame()
    {
        while(true)
        {
            // Booléen utilisé pour déterminé si une partie a été retirée
            bool expiredGameRemooved = false;

            // Récupère toutes les parties découvertes
            var keys = new List<CustomNetworkManager.GameInfo>(activeGames.Keys);

            foreach(var key in keys)
            {
                // Vérifie que n'a pas éxpirée et la retire si besoin
                if(activeGames[key] <= Time.time)
                {
                    activeGames.Remove(key);
                    expiredGameRemooved = true;
                }
            }

            LobbyMenu lobby = FindObjectOfType<LobbyMenu>();

            // Met à jour la liste des entrées du lobby si nécessaire
            if (expiredGameRemooved && lobby != null)
                lobby.UpdateGameList();

            yield return new WaitForSeconds(activeGamesTimeOut);
        }
    }

    public override void OnReceivedBroadcast(string fromAddress, string data)
    {
        base.OnReceivedBroadcast(fromAddress, data);

        Debug.Log("Received boradcast!");
        
        // Prépare les données reçues
        string[] datas = data.Split('|');
   
        CustomNetworkManager.GameInfo info = new CustomNetworkManager.GameInfo();

        // Met en forme les données
        info.gameName = datas[0];
        info.gameIP = datas[1];
        info.gamePort = int.Parse(datas[2]);

        // Vérifie que la liste des partie ne contient pas déjà cette partie
        if (!activeGames.ContainsKey(info))
        {            
            // Ajoute la partie à la liste et met à jour la liste des parties du lobby
            activeGames.Add(info, Time.time + activeGamesTimeOut);
            FindObjectOfType<LobbyMenu>().UpdateGameList();
        }
        else
            // Met à jour le temps à partir du quel la partie est éxpirée
            activeGames[info] = Time.time + activeGamesTimeOut;
    }
}
