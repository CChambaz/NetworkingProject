using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EndGameCanvas : MonoBehaviour {
    [SerializeField] CanvasGroup endPanel;
    [SerializeField] Text endMessage;

    [SerializeField] float fadeSpeed;

    [SerializeField] float waitBeforeLeaving;

    string winMessage = "You survived !";
    string loseMessage = "You're all dead...";

    float waittingTime = 0;

    Canvas endCanvas;

    WaveValues waveValues;
    
	// Use this for initialization
	void Start () {
        waveValues = FindObjectOfType<WaveValues>();
        endCanvas = GetComponent<Canvas>();
	}
	
	// Update is called once per frame
	void Update () {
        if (waveValues != null)
        {
            if (waveValues.gameStatus != -1)
            {
                endCanvas.worldCamera = Camera.current;

                if (endPanel.alpha < 1)
                    endPanel.alpha += fadeSpeed;

                switch (waveValues.gameStatus)
                {
                    case 1:
                        endMessage.text = winMessage;
                        break;
                    case 0:
                        endMessage.text = loseMessage;
                        break;
                }

                waittingTime += Time.deltaTime;

                if (waittingTime >= waitBeforeLeaving)
                {
                    FindObjectOfType<CustomNetworkManager>().LeaveGame();
                    Destroy(this);
                }
            }
        }
        else
            waveValues = FindObjectOfType<WaveValues>();
    }

    void FadeUIEllements()
    {
        endPanel.alpha += fadeSpeed;
    }

    void EndGame()
    {
        endCanvas.worldCamera = Camera.current;

        switch(waveValues.gameStatus)
        {
            case 1:
                endMessage.text = winMessage;
                break;
            case 0:
                endMessage.text = loseMessage;
                break;
        }

        waittingTime += Time.deltaTime;

        if (waittingTime >= waitBeforeLeaving)
        {
            FindObjectOfType<CustomNetworkManager>().LeaveGame();
            Destroy(this);
        }
    }
}
