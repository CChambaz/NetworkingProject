using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityStandardAssets.Characters.FirstPerson;

public class UI : NetworkBehaviour
{
    [Header("Game UI attributs")]
    [SerializeField] Canvas uiCanvas;
    [SerializeField] Text uiWave;
    [SerializeField] Text uiAmmo;
    [SerializeField] Text uiCommonScoreText;
    [SerializeField] Image uiCommonScoreImage;
    [SerializeField] Image uiKeyPanel;
    [SerializeField] Image uiHealthState;

    [Header("Pause menu attributs")]
    [SerializeField] Canvas pauseCanvas;

    [Header("Fading attributs")]
    [SerializeField] float minOpacity;
    [SerializeField] float fadeDuration;
    [SerializeField] float fillSpeed;

    Gun playerGun;
    WaveValues waveValues;
    ScoreValues scoreValues;
    PlayerAction playerAction;
    HealthValues playerHealth;

    bool isPause = false;

	// Use this for initialization
	void Start ()
    {
        playerGun = GetComponent<Gun>();
        playerAction = GetComponent<PlayerAction>();
        playerHealth = GetComponent<HealthValues>();
        waveValues = FindObjectOfType<WaveValues>();
        scoreValues = FindObjectOfType<ScoreValues>();
    }
	
	// Update is called once per frame
	void Update ()
    {
        if (waveValues.gameStatus != -1)
        {
            uiCanvas.enabled = false;
            pauseCanvas.enabled = false;
            return;
        }

        if (playerHealth.isAlive)
        {
            if (Input.GetKeyUp(KeyCode.Escape))
                SwitchActiveUI();

            UpdateWave();
            UpdateHealth();
            UpdateAmmo();
            UpdateScore();
            UpdateIndication();
        }

        
	}

    public void SwitchActiveUI()
    {
        if (isPause)
        {
            isPause = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
        else
        {
            isPause = true;
            Cursor.lockState = CursorLockMode.None;
        }

        Cursor.visible = isPause;

        GetComponent<CharacterController>().enabled = !isPause;
        GetComponent<FirstPersonController>().enabled = !isPause;
        GetComponent<Gun>().enabled = !isPause;

        uiCanvas.gameObject.SetActive(!isPause);

        pauseCanvas.gameObject.SetActive(isPause);
    }

    public void DisconnectAction(int sceneID)
    {
        FindObjectOfType<CustomNetworkManager>().LeaveGame();

        GameManager.gmInstance.LoadScene(sceneID);
    }

    void UpdateWave()
    {
        if (waveValues == null)
            waveValues = FindObjectOfType<WaveValues>();

        uiWave.text = "Wave " + waveValues.waveNumber.ToString();

        // Fade the wave part of the UI beetween waves
        if (!waveValues.isWaveActive)
        {
            if (uiWave.canvasRenderer.GetAlpha() <= minOpacity)
                uiWave.CrossFadeAlpha(1, fadeDuration, true);
            else if(uiWave.canvasRenderer.GetAlpha() >= 1)               
                uiWave.CrossFadeAlpha(minOpacity, fadeDuration, true);
        }
        else if(waveValues.isWaveActive && uiWave.canvasRenderer.GetAlpha() < 1)
            uiWave.CrossFadeAlpha(1, fadeDuration, true);
    }

    void UpdateHealth()
    {
        if (playerHealth == null)
            playerHealth = GetComponent<HealthValues>();

        if (playerHealth.isAlive)
        {
            float actualHealth = (float)playerHealth.actualHealth;
            float totalHealth = (float)playerHealth.maxHealth;

            uiHealthState.fillAmount = Mathf.Lerp(uiHealthState.fillAmount, actualHealth / totalHealth, fillSpeed);
        }
    }

    void UpdateAmmo()
    {
        if (playerGun == null)
            playerGun = GetComponent<Gun>();

        uiAmmo.text = playerGun.magazineState + "/" + playerGun.magazineCapacity;

        // Fade ammo part of the UI when reloading
        if (playerGun.isReloading)
        {
            if (uiAmmo.canvasRenderer.GetAlpha() <= minOpacity)
                uiAmmo.CrossFadeAlpha(1, fadeDuration, true);
            else if (uiAmmo.canvasRenderer.GetAlpha() >= 1)
                uiAmmo.CrossFadeAlpha(minOpacity, fadeDuration, true);
        }
        else if(!playerGun.isReloading && uiAmmo.canvasRenderer.GetAlpha() < 1)
            uiAmmo.CrossFadeAlpha(1, fadeDuration, true);
    }

    void UpdateScore()
    {
        if (scoreValues == null)
            scoreValues = FindObjectOfType<ScoreValues>();

        // Apply score text and exceeding score if necessary
        if(scoreValues.exceedingScore == 0)
            uiCommonScoreText.text = scoreValues.actualScore + "/" + scoreValues.scoreNeeded;
        else
            uiCommonScoreText.text = scoreValues.actualScore + "/" + scoreValues.scoreNeeded + " (+" + scoreValues.exceedingScore + ")";

        float score = (float)scoreValues.actualScore;
        float score2 = (float)scoreValues.scoreNeeded;

        uiCommonScoreImage.fillAmount = Mathf.Lerp(uiCommonScoreImage.fillAmount, score / score2, fillSpeed);
    }

    void UpdateIndication()
    {
        if (playerAction == null)
            playerAction = GetComponent<PlayerAction>();

        if(playerAction.actionableObject != null)
            uiKeyPanel.gameObject.SetActive(true);
        else
            uiKeyPanel.gameObject.SetActive(false);
    }
}
