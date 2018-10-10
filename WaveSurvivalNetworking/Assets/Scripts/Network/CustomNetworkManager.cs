using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;

public class CustomNetworkManager : NetworkManager
{
    [SerializeField] int maxPlayer = 4;

    public struct GameInfo
    {
        public string gameName;
        public string gameIP;
        public int gamePort;
    }

    GameInfo gameInfo = new GameInfo();
    
    bool isHost = false;

    CustomDiscovery networkDiscovery;

    private void Start()
    {
        networkDiscovery = GetComponent<CustomDiscovery>();
    }

    #region Host Management
    // Lance la partie
    public void StartHosting(string gameName)
    {
        // Récupère les informations qui vont être envoyées sur le réseau
        gameInfo.gameName = gameName;
        gameInfo.gameIP = Network.player.ipAddress;
        gameInfo.gamePort = singleton.networkPort;
        
        // Lance la partie
        singleton.StartHost();

        // Indique que l'utilisateur est l'hôte de la partie
        isHost = true;
    }

    public override void OnStartHost()
    {
        base.OnStartHost();

        // Prépare les données a propager sur le réseau
        // Format : [Nom de la partie]|[Adresse IP]|[Port utilisé]
        string data = gameInfo.gameName + "|"  + gameInfo.gameIP + "|" + gameInfo.gamePort.ToString();

        // Lance le broadcast
        networkDiscovery.ServerBroadcast(data);
    }

    public override void OnServerConnect(NetworkConnection conn)
    {
        base.OnServerConnect(conn);

        Debug.Log("New client connected, total : " + numPlayers + "/" + maxConnections);

        // Si le nombre maximum de joueur est atteints, arrête le boradcast
        if (numPlayers == maxPlayer)
            networkDiscovery.StopBroadcast();

        // Déconnecte les joueur se connectant alors que le maximum de connexion est atteint
        if (numPlayers > maxPlayer)
            conn.Disconnect();
    }

    public void StopHosting()
    {
        singleton.StopHost();
    }

    public override void OnStopHost()
    {
        base.OnStopHost();

        // Retire le statut d'hôte
        isHost = false;

        // Réinitialise le broadcast et écoute le réseau
        networkDiscovery.ClientBroadcast();
    }
    #endregion 

    #region Client Management    
    public void StartClient(string ip, int port)
    {
        // Démarre le client et le connecte au serveur
        singleton.StartClient();
        singleton.client.Connect(ip, port);

        // Arrête l'écoute de broadcast
        networkDiscovery.StopBroadcast();
    }

    public void LeaveGame()
    {
        // Si il s'agit de l'hôte, l'arrète
        if (isHost)
        {
            StopHosting();
            return;
        }
       
        // Déconnecte et arrête le client
        singleton.client.Disconnect();
        singleton.StopClient();

        // Reprend l'écoute de broadcast
        networkDiscovery.ClientBroadcast();
    }
    #endregion
}
