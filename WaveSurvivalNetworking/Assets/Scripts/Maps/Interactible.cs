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
            // Active ou désactive le signe de l'object avec lequel les joueurs peuvent intéragir si ils ont atteint le score nécessaire
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
        // Active ou désactive les colliders utilisés pour définir si un joueur se trouve a porté d'un objet interactif
        foreach(Collider col in collider)
            col.enabled = isActive;
        
        // Modifie l'ascpect des objets concernés
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
