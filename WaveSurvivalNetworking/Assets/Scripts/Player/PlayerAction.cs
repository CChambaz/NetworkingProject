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

        // Prepare raycast
        Ray ray = new Ray(transform.position, transform.forward);

        // Do the raycast and get the object hitted
        if (Physics.Raycast(ray, out hit, interactibleRange))
        {
            // Try to get the Interactible component
            Interactible hitted = hit.transform.GetComponent<Interactible>();

            // If it exists, get the object
            if (hitted != null)
                actionableObject = hitted;
        }
        else
            actionableObject = null;

        // Do the action of the interactible object
        if (Input.GetKeyDown(KeyCode.E) && actionableObject != null && scoreValues.actualScore >= scoreValues.scoreNeeded)
        {
            CmdAction();

            actionableObject = null;
        }
	}

    [Command]
    // Send command to the server to do the interactible object action
    void CmdAction()
    {
        actionableObject.Action();
    }
}
