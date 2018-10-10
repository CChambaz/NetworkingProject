using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

public class LobbyMenu : MonoBehaviour {
    [SerializeField] Text textHostGameName;
    [SerializeField] Text textIPAdress;

    [SerializeField] GameObject entriesList;
    [SerializeField] GameObject baseEntrieModal;
    [SerializeField] GameObject gameEntriesPrefab;

    [SerializeField] Text messageBox;

    List<GameObject> gameEntries = new List<GameObject>();

    CustomDiscovery customDiscovery;
    CustomNetworkManager networkManager;

	// Use this for initialization
	void Start () {
        customDiscovery = FindObjectOfType<CustomDiscovery>();
        networkManager = FindObjectOfType<CustomNetworkManager>();

        // Make sure that no client is currently active
        if (networkManager.client != null)
            networkManager.StopClient();

        // Make sure that the cursor is visible dans usable
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }
	
	// Update is called once per frame
	void Update () {
        // Make sure that the customDiscovery is listening to broadcast
        if (!customDiscovery.running)
            customDiscovery.StartAsClient();
	}

    public void UpdateGameList()
    {
        Debug.Log("Updating game list...");

        // Destroy every active entries
        foreach(GameObject entrie in gameEntries)
        {
            Destroy(entrie);
        }

        // Clean the gameEntries list
        gameEntries.Clear();

        // Get all the games discovers by the listening of broadcast
        var keys = new List<CustomNetworkManager.GameInfo>(customDiscovery.activeGames.Keys);
        
        // Counter used to separate each entries
        int cpt = 0;

        foreach (var key in keys)
        {
            // Creat an entrie
            GameObject newEntrie = Instantiate(gameEntriesPrefab);

            GameEntrie entrie = newEntrie.GetComponent<GameEntrie>();
            
            // Assign the values of the actual key to the entrie
            entrie.gameName = key.gameName;
            entrie.gameIP = key.gameIP;
            entrie.gamePort = key.gamePort;

            // Define the parent of the new entrie
            newEntrie.transform.SetParent(entriesList.transform);            
            
            // Define the position of the entrie depending on how many entries allready exists
            Vector3 entriePosition = new Vector3(0, (baseEntrieModal.GetComponent<RectTransform>().rect.size.y / 4) * cpt, 0);
            
            // Adjust the size of the entry
            newEntrie.GetComponent<RectTransform>().localScale = baseEntrieModal.GetComponent<RectTransform>().localScale;

            // Apply the position to the entry
            newEntrie.GetComponent<Transform>().position = baseEntrieModal.GetComponent<Transform>().position - entriePosition;

            // Add the entry to the entries list
            gameEntries.Add(newEntrie);

            // Increments the counter
            cpt++;
        }
    }

    public void CreatGame()
    {
        // Check if a game name has been entered before starting a new game
        if (textHostGameName.text != "")
            FindObjectOfType<CustomNetworkManager>().StartHosting(textHostGameName.text);
        else
            DisplayMessage("You must enter a name before starting a game");
    }

    public void JoinGameByIP()
    {
        // Check if a valid IP address has been entered before joining the game
        if (Regex.Match(textIPAdress.text, "[0-9]{1,3}.[0-9]{1,3}.[0-9]{1,3}.[0-9]{1,3}").Success)
            FindObjectOfType<CustomNetworkManager>().StartClient(textIPAdress.text, FindObjectOfType<CustomNetworkManager>().networkPort);
        else
            DisplayMessage("Invalide IP adress format");
    }

    public void DisplayMessage(string message)
    {
        // Show the message in the dedicated space
        messageBox.text = message + "\n" + messageBox.text;
    }
}
