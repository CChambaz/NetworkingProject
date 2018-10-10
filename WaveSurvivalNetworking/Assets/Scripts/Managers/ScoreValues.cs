using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class ScoreValues : NetworkBehaviour
{
    [SerializeField] [SyncVar(hook = "OnScoreNeededChanged")] public int scoreNeeded;

    [SyncVar(hook = "OnScoreChanged")] public int actualScore = 0;
    [SyncVar(hook = "OnExceedingChanged")] public int exceedingScore = 0;

    void OnScoreNeededChanged(int value)
    {
        scoreNeeded = value;
    }

    void OnScoreChanged(int score)
    {
        actualScore = score;
    }

    void OnExceedingChanged(int exceeding)
    {
        exceedingScore = exceeding;
    }
}
