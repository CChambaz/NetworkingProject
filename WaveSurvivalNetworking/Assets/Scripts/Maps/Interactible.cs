using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Interactible : NetworkBehaviour {
    [SerializeField] Material offMaterial;
    [SerializeField] Material OnMaterial;
    [SerializeField] MeshRenderer[] objectsComposing;

    Collider[] collider;

    ScoreValues scoreValues;
    ScoreManager scoreManager;
        
    private void Start()
    {
        scoreValues = FindObjectOfType<ScoreValues>();
        collider = GetComponents<Collider>();
    }

    private void Update()
    {
        if (scoreValues != null)
        {
            // Enable or disable the object sign if they have reached the score needed
            if (scoreValues.scoreNeeded == scoreValues.actualScore)
                ActiveSign(true);
            else if (scoreValues.scoreNeeded > scoreValues.actualScore)
                ActiveSign(false);
        }
        else
        {
            scoreValues = FindObjectOfType<ScoreValues>();
        }
    }

    void ActiveSign(bool isActive)
    {
        // Enable or disable the colliders used to define if the player is near an interactible object
        foreach(Collider col in collider)
            col.enabled = isActive;
        
        // Modifie the aspect of the concerned objects
        foreach(MeshRenderer mesh in objectsComposing)
        {
            if (isActive)
                mesh.material = OnMaterial;
            else
                mesh.material = offMaterial;
        }       
    }

    [Server]
    public void Action()
    {
        // Détruit l'objet
        NetworkServer.Destroy(gameObject);

        if (scoreManager == null)
            scoreManager = FindObjectOfType<ScoreManager>();

        // Fait passer le score au prochain seuil
        scoreManager.NextScoreLevel();
    }
}
