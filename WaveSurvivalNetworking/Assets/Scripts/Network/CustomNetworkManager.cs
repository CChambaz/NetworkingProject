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
    public void StartHosting(string gameName)
    {
        // Set the datas that will be send on the network
        gameInfo.gameName = gameName;
        gameInfo.gameIP = Network.player.ipAddress;
        gameInfo.gamePort = singleton.networkPort;
        
        // Launch the game
        singleton.StartHost();

        // Set the host status
        isHost = true;
    }

    public override void OnStartHost()
    {
        base.OnStartHost();

        // Prepare the datas
        // Format : [game name]|[IP address]|[Port in use]
        string data = gameInfo.gameName + "|"  + gameInfo.gameIP + "|" + gameInfo.gamePort.ToString();

        // Start broadcasting the data on the network
        networkDiscovery.ServerBroadcast(data);
    }

    public override void OnServerConnect(NetworkConnection conn)
    {
        base.OnServerConnect(conn);

        Debug.Log("New client connected, total : " + numPlayers + "/" + maxConnections);

        // Check if the maximum amount of players is reached
        if (numPlayers >= maxPlayer)
            networkDiscovery.StopBroadcast();
        // If not, check if the broadcast is stopped
        else if (!networkDiscovery.running)
        {
            // Prepare the datas
            string data = gameInfo.gameName + "|" + gameInfo.gameIP + "|" + gameInfo.gamePort.ToString();

            // Start broadcasting
            networkDiscovery.ServerBroadcast(data);
        }

        // Disconnect the incoming player if the maximum amount of player is reached
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

        // Unset the host status
        isHost = false;

        // Restart listening for broadcast
        networkDiscovery.ClientBroadcast();
    }
    #endregion 

    #region Client Management    
    public void StartClient(string ip, int port)
    {
        // Start the client and connect it to the server
        singleton.StartClient();
        singleton.client.Connect(ip, port);

        // Stop listening for broadcast
        networkDiscovery.StopBroadcast();
    }

    public void LeaveGame()
    {
        // If it's the host, stop the game
        if (isHost)
        {
            StopHosting();
            return;
        }
       
        // Disconnect and stop the client
        singleton.client.Disconnect();
        singleton.StopClient();

        // Restart listening for broadcast
        networkDiscovery.ClientBroadcast();
    }
    #endregion
}
