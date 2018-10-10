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
        // Remove points from the exceeding if possible
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
        // Apply the exceeding score to the actual score
        scoreValues.actualScore = scoreValues.exceedingScore;

        // Define the next cap
        scoreValues.scoreNeeded = (int)(scoreValues.scoreNeeded * scoreMultiplier);

        // Check if the actual score is greater than the score needed
        if(scoreValues.actualScore > scoreValues.scoreNeeded)
        {
            // Define and apply the exceeding score
            scoreValues.exceedingScore = scoreValues.actualScore - scoreValues.scoreNeeded;

            // Define the actual score to his maximum
            scoreValues.actualScore = scoreValues.scoreNeeded;
        }
        else
            // Set the exceeding score to 0
            scoreValues.exceedingScore = 0;
    }
}
