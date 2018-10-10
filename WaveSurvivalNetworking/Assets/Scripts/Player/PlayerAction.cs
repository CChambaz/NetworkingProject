using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PlayerAction : NetworkBehaviour {
    [SerializeField] float interactibleRange;

    public Interactible actionableObject;

    ScoreValues scoreValues;

	// Use this for initialization
	void Start () {
        scoreValues = FindObjectOfType<ScoreValues>();
        actionableObject = null;
    }
	
	// Update is called once per frame
	void Update () {
        RaycastHit hit;

        // Prépare le raycast
        Ray ray = new Ray(transform.position, transform.forward);

        // Effectu le raycast et récupère l'objet rencontré
        if (Physics.Raycast(ray, out hit, interactibleRange))
        {
            // Récupère le composant Interactible
            Interactible hitted = hit.transform.GetComponent<Interactible>();

            // Si il existe, récupère l'objet
            if (hitted != null)
                actionableObject = hitted;
        }
        else
            actionableObject = null;

        // Effectu l'action de l'objet interactif
        if (Input.GetKeyDown(KeyCode.E) && actionableObject != null && scoreValues.actualScore >= scoreValues.scoreNeeded)
        {
            CmdAction();

            actionableObject = null;
        }
	}

    [Command]
    // Envoi la commande au serveur pour effectuer l'action de l'objet interactif
    void CmdAction()
    {
        actionableObject.Action();
    }
}
