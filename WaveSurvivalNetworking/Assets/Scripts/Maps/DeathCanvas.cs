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
        // Vérifie si la camera utilisé est la caméra montrant cet élément
        if (Camera.current == deathCanvas.worldCamera)
        {
            // Active le curseur 
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;

            Debug.Log("Is death");

            // Active ou désactive le menu de la mort si la touche escape est utilisée
            if (Input.GetKeyUp(KeyCode.Escape))
            {
                Debug.Log("Activate death menu");
                deathMenuPanel.SetActive(!deathMenuPanel.activeSelf);
            }

            deathMobLeft.text = "Mobs left : " + waveValues.mobsLeft.ToString();
            playerStillAlive.text = "Player alive : " + waveValues.playerAlive.ToString();
        }
    }
}
