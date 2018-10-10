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

        // S'assure que l'utilisateur n'a pas de client actif
        if (networkManager.client != null)
            networkManager.StopClient();

        // S'assure que le curseur est visible et utilisable
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }
	
	// Update is called once per frame
	void Update () {
        // S'assure que l'écoute de broadcast est active
        if (!customDiscovery.running)
            customDiscovery.StartAsClient();
	}

    public void UpdateGameList()
    {
        Debug.Log("Updating game list...");

        // Détruit chaque entrées précedemment crées
        foreach(GameObject entrie in gameEntries)
        {
            Destroy(entrie);
        }

        // Retire toutes les entrées de la liste
        gameEntries.Clear();

        // Récupère toutes les parties découverte par le composant customDiscovery
        var keys = new List<CustomNetworkManager.GameInfo>(customDiscovery.activeGames.Keys);

        // Compteur utilisé pour espacé les entrées l'une des autres
        int cpt = 0;

        foreach (var key in keys)
        {
            // Crée une entrée
            GameObject newEntrie = Instantiate(gameEntriesPrefab);

            // Récupère le composant GameEntrie
            GameEntrie entrie = newEntrie.GetComponent<GameEntrie>();
            
            // Assigne les valeurs correspondantes à la clé active
            entrie.gameName = key.gameName;
            entrie.gameIP = key.gameIP;
            entrie.gamePort = key.gamePort;

            // Défini le parent de la nouvelle entrée
            newEntrie.transform.SetParent(entriesList.transform);            

            // Défini la position de l'entrée selon le nombre d'entrée éxistante
            Vector3 entriePosition = new Vector3(0, (baseEntrieModal.GetComponent<RectTransform>().rect.size.y / 4) * cpt, 0);
            
            // Ajuste la taille de l'entrée
            newEntrie.GetComponent<RectTransform>().localScale = baseEntrieModal.GetComponent<RectTransform>().localScale;

            // Applique la position de l'entrée
            newEntrie.GetComponent<Transform>().position = baseEntrieModal.GetComponent<Transform>().position - entriePosition;

            // Ajoute l'entrée à la liste des entrées
            gameEntries.Add(newEntrie);

            // Incrémente le compteur
            cpt++;
        }
    }

    public void CreatGame()
    {
        // Vérifie qu'un nom de partie à bien été entré avant de crée une partie
        if (textHostGameName.text != "")
            FindObjectOfType<CustomNetworkManager>().StartHosting(textHostGameName.text);
        else
            DisplayMessage("You must enter a name before starting a game");
    }

    public void JoinGameByIP()
    {
        // Vérifie qu'une adresse IP au format valide a été entrée avant de rejoindre la partie correspondante
        if (Regex.Match(textIPAdress.text, "[0-9]{1,3}.[0-9]{1,3}.[0-9]{1,3}.[0-9]{1,3}").Success)
            FindObjectOfType<CustomNetworkManager>().StartClient(textIPAdress.text, FindObjectOfType<CustomNetworkManager>().networkPort);
        else
            DisplayMessage("Invalide IP adress format");
    }

    public void DisplayMessage(string message)
    {
        // Affiche le message transmis dans la zone dédiée
        messageBox.text = message + "\n" + messageBox.text;
    }
}
