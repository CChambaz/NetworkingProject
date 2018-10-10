using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DeathCanvas : MonoBehaviour {
    [Header("Death UI attributs")]
    [SerializeField] GameObject deathMenuPanel;
    [SerializeField] Text deathMobLeft;
    [SerializeField] Text playerStillAlive;

    WaveValues waveValues;
    Canvas deathCanvas;

    // Use this for initialization
    void Start () {
        deathCanvas = GetComponent<Canvas>();

        waveValues = FindObjectOfType<WaveValues>();
	}
	
	// Update is called once per frame
	void Update () {
        // Check if the active camera is the camera of this canvas
        if (Camera.current == deathCanvas.worldCamera)
        {
            // Enable the cursor
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;

            // Enable or disable the death menu
            if (Input.GetKeyUp(KeyCode.Escape))
            {
                Debug.Log("Activate death menu");
                deathMenuPanel.SetActive(!deathMenuPanel.activeSelf);
            }

            // Update the mobs and player alive left texts
            deathMobLeft.text = "Mobs left : " + waveValues.mobsLeft.ToString();
            playerStillAlive.text = "Player alive : " + waveValues.playerAlive.ToString();
        }
    }
}
