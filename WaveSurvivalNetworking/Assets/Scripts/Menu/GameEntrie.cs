using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameEntrie : MonoBehaviour
{
    [SerializeField] Text textGameName;
    [SerializeField] Text textGameIP;
    [SerializeField] Button buttonJoinGame;

    public string gameName;
    public string gameIP;
    public int gamePort;

	// Use this for initialization
	void Start () {
        textGameName.text = gameName;
        textGameIP.text = gameIP;

        buttonJoinGame.onClick.AddListener(JoinGame); 
	}
	
	void JoinGame()
    {
        FindObjectOfType<CustomNetworkManager>().StartClient(gameIP, gamePort);
    }
}
