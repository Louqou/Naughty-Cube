using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Pause : MonoBehaviour
{
    public bool Paused { get; private set; }

    public GameObject mainCamera;
    public GameObject pauseLines, playArrow;
    public GameObject pauseScreen;

    public GameObject demoUI;

    private void Update()
    {
        if (Input.GetKeyDown("p") || Input.GetKeyDown(KeyCode.Escape)) {
            PauseGame();
        }
    }

    public void PauseGame()
    {
        if (demoUI != null) {
            demoUI.SetActive(Paused);
        }
        pauseScreen.SetActive(!Paused);
        playArrow.SetActive(!Paused);
        pauseLines.SetActive(Paused);

        if (Paused) {
            Time.timeScale = 1;
            mainCamera.GetComponent<Camera>().cullingMask = -1;
        }
        else {
            Time.timeScale = 0;
            mainCamera.GetComponent<Camera>().cullingMask = 0;
            mainCamera.GetComponent<Camera>().cullingMask |= 1 << LayerMask.NameToLayer("UI");
            GameObject levelCompleteText = GameObject.FindGameObjectWithTag("EndStage");
            if (levelCompleteText) {
                levelCompleteText.SetActive(false);
            }
        }

        Paused = !Paused;
    }

    public void PauseExit()
    {
        Time.timeScale = 1;
        pauseScreen.SetActive(false);
        mainCamera.GetComponent<Camera>().cullingMask = -1;
        StartCoroutine(ShowGameOverScreen());
    }

    public IEnumerator ShowGameOverScreen()
    {
        Fading fading = GetComponent<Fading>();
        fading.fadeSpeed = 0.2f;
        float time = fading.BeginFade(1);
        yield return new WaitForSeconds(time);
        GetComponent<PPEffects>().RestoreDefault();

        if (demoUI == null) {
            SceneManager.LoadScene("Scenes/GameOver", LoadSceneMode.Single);
        }
        else {
            SceneManager.LoadScene("Scenes/Menu", LoadSceneMode.Single);
        }
    }
}
