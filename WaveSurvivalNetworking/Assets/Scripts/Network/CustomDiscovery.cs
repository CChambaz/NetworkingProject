using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class CustomDiscovery : NetworkDiscovery
{
    float activeGamesTimeOut = 5; 
    
    // Dictionnary storing the games discovered with the time at which they are considered as expired
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
        // Define the data to send
        broadcastData = data;

        // Stop the coroutine checking the dictionary
        StopCoroutine(coroutineExpiredGames);

        // Clear the games dictionary
        activeGames.Clear();

        // Check if it is needed to stop the broadcast
        if (running)
            StopBroadcast();

        // Restart the NetworkTransport
        NetworkTransport.Shutdown();
        NetworkTransport.Init();

        // Reinitialize the component
        Initialize();

        // Start broadcasting over the network
        StartAsServer();
    }

    public void ClientBroadcast()
    {
        Debug.Log("Stop broadcasting...");

        // Check if it is needed to stop the broadcast
        if (running)
            StopBroadcast();

        // Restart the NetworkTransport
        NetworkTransport.Shutdown();
        NetworkTransport.Init();

        // Reinitialize the component
        Initialize();

        // Sart listening for broadcast
        StartAsClient();

        // Start the coroutine checking the dictionary
        coroutineExpiredGames = StartCoroutine(CleanExpiredGame());
    }

    IEnumerator CleanExpiredGame()
    {
        while(true)
        {
            // Bool used to check if a game has been removed
            bool expiredGameRemoved = false;

            // Get all games discovered
            var keys = new List<CustomNetworkManager.GameInfo>(activeGames.Keys);

            foreach(var key in keys)
            {
                // Check if the game has expired
                if(activeGames[key] <= Time.time)
                {
                    activeGames.Remove(key);
                    expiredGameRemoved = true;
                }
            }

            LobbyMenu lobby = FindObjectOfType<LobbyMenu>();

            // Update the lobby game list if at least one as been removed
            if (expiredGameRemoved && lobby != null)
                lobby.UpdateGameList();

            yield return new WaitForSeconds(activeGamesTimeOut);
        }
    }

    public override void OnReceivedBroadcast(string fromAddress, string data)
    {
        base.OnReceivedBroadcast(fromAddress, data);

        Debug.Log("Received boradcast!");
        
        // Prepare the datas
        string[] datas = data.Split('|');
   
        CustomNetworkManager.GameInfo info = new CustomNetworkManager.GameInfo();

        // Make sure the datas array has the desired number of elements
        try
        {
            // Set the values
            info.gameName = datas[0];
            info.gameIP = datas[1];
            info.gamePort = int.Parse(datas[2]);
        }
        // Otherwise the datas received are invalid
        catch
        {
            return;
        }

        // Check if the dictionary does not already contain this game
        if (!activeGames.ContainsKey(info))
        {            
            // Add the game to the dictionary
            activeGames.Add(info, Time.time + activeGamesTimeOut);

            // Update the lobby game list
            FindObjectOfType<LobbyMenu>().UpdateGameList();
        }
        else
            // Update the game timeout
            activeGames[info] = Time.time + activeGamesTimeOut;
    }
}
