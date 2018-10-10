using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class WaveValues : NetworkBehaviour
{
    public struct PlayerInfo
    {
        public NetworkIdentity playerNetID;
    }

    public class SyncListPlayerInfo : SyncListStruct<PlayerInfo> { }

    public SyncListPlayerInfo players = new SyncListPlayerInfo();

    [SyncVar(hook = "OnWaveActiveChanged")] public bool isWaveActive = false;
    [SyncVar(hook = "OnWaveNumChanged")] public int waveNumber = 0;
    [SyncVar(hook = "OnMobsLeftChanged")] public int mobsLeft = 0;
    [SyncVar(hook = "OnPlayerAliveChanged")] public int playerAlive = 0;
    // Statut du jeu, valeur : -1 => en cours; 0 => Défaite; 1 => Victoire
    [SyncVar(hook = "OnGameStatusChanged")] public int gameStatus = -1;

    public void AddPlayer(PlayerInfo player)
    {
        if (!players.Contains(player))
            players.Add(player);
    }

    public void RemovePlayer(NetworkIdentity playerNetID)
    {
        if (!isServer)
            return;

        PlayerInfo playerToRemove = new PlayerInfo();

        playerToRemove.playerNetID = playerNetID;

        players.Remove(playerToRemove);
    }

    void OnWaveActiveChanged(bool isActive)
    {
        isWaveActive = isActive;
    }

    void OnWaveNumChanged(int number)
    {
        waveNumber = number;
    }

    void OnMobsLeftChanged(int left)
    {
        mobsLeft = left;
    }

    void OnPlayerAliveChanged(int number)
    {
        playerAlive = number;

        // Vérifie qu'il reste des joueurs en vie, sinon place la partie dans l'état défaite
        if (playerAlive <= 0)
            gameStatus = 0;
    }

    void OnGameStatusChanged(int number)
    {
        gameStatus = number;
    }
}
