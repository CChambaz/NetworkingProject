using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class ScoreManager : NetworkBehaviour
{
    [SerializeField] float scoreMultiplier;

    ScoreValues scoreValues;

    private void Start()
    {
        if (!isServer)
            Destroy(this);

        scoreValues = FindObjectOfType<ScoreValues>();
    }

    [Server]
    public void AddPoints(int points)
    {
        scoreValues.actualScore += points;

        if(scoreValues.actualScore > scoreValues.scoreNeeded)
        {
            scoreValues.actualScore = scoreValues.scoreNeeded;

            scoreValues.exceedingScore += points;
        }
    }

    [Server]
    public void RemovePoints(int points)
    {
        // Retire les points au score éxcédentaire si applicable
        if (scoreValues.exceedingScore > 0)
        {
            scoreValues.exceedingScore -= points;

            if (scoreValues.exceedingScore < 0)
                scoreValues.exceedingScore = 0;
        }
        else 
        {
            scoreValues.actualScore -= points;

            if (scoreValues.actualScore < 0)
                scoreValues.actualScore = 0;
        }
    }

    [Server]
    public void NextScoreLevel()
    {
        // Applique le score éxcedentaire en lieu et place au score actuel
        scoreValues.actualScore = scoreValues.exceedingScore;

        // Détermine le prochain pallier nécessaire
        scoreValues.scoreNeeded = (int)(scoreValues.scoreNeeded * scoreMultiplier);

        // Vérifie si le score actuel est supérieur au score nécessaire
        if(scoreValues.actualScore > scoreValues.scoreNeeded)
        {
            // Détermine et applique le score éxcédentaire
            scoreValues.exceedingScore = scoreValues.actualScore - scoreValues.scoreNeeded;

            // Défini le score actuel au maximum
            scoreValues.actualScore = scoreValues.scoreNeeded;
        }
        else
            // Réinitialise le score éxcédentaire
            scoreValues.exceedingScore = 0;
    }
}
