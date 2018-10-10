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
            // Check if the game is over
            if (waveValues.gameStatus != -1)
            {
                // Define on which camera the canvas have to be rendered
                endCanvas.worldCamera = Camera.current;

                // Fade in of the panel and its children
                if (endPanel.alpha < 1)
                    endPanel.alpha += fadeSpeed;

                // Set the message to show
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

                // Check if the time passed on this screen has reached his end
                if (waittingTime >= waitBeforeLeaving)
                {
                    // Leave the game
                    FindObjectOfType<CustomNetworkManager>().LeaveGame();
                    Destroy(this);
                }
            }
        }
        else
            waveValues = FindObjectOfType<WaveValues>();
    }
}
