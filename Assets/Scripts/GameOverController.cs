using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOverController : MonoBehaviour
{
    private bool started = false;
    public Text scoreText;
    public Text highScoreText;
    public GameObject newHighScoreText;

    public Text betterLuck;

    public GameObject fireworks;

    private void Start()
    {
        int highScore = PlayerPrefs.GetInt("Highscore");
        if (Stage.score > highScore) {
            newHighScoreText.SetActive(true);
            PlayerPrefs.SetInt("Highscore", Stage.score);
        }

        scoreText.text = "Score: " + Stage.score.ToString("N0");
        highScoreText.text = "High Score: " + highScore.ToString("N0");
        Stage.score = 0;

        if (LevelLoader.stage == 10) {
            betterLuck.text = "Congratulations!\nFinal stage complete.";
            fireworks.SetActive(true);
        }
    }

    private void Update()
    {
        if (!started && Input.GetButton("Fire1")) {
            StartCoroutine(StartGame());
            started = true;
        }
    }

    private IEnumerator StartGame()
    {
        float fadeTime = gameObject.GetComponent<Fading>().BeginFade(1);
        yield return new WaitForSeconds(fadeTime);
        SceneManager.LoadScene("Scenes/Menu", LoadSceneMode.Single);
    }
}
