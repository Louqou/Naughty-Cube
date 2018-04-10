using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    public AudioClip startSound;

    private IEnumerator StartGame()
    {
        float fadeTime = gameObject.GetComponent<Fading>().BeginFade(1);
        yield return new WaitForSeconds(fadeTime);
        SceneManager.LoadScene("Scenes/Stage", LoadSceneMode.Single);
    }

    private IEnumerator StartHowTo()
    {
        LevelLoader.stage = 1;
        float fadeTime = gameObject.GetComponent<Fading>().BeginFade(1);
        yield return new WaitForSeconds(fadeTime);
        SceneManager.LoadScene("Scenes/DemoStage", LoadSceneMode.Single);
    }

    public void Play()
    {
        GetComponent<AudioSource>().PlayOneShot(startSound);
        LevelLoader.stage = 1;
        StartCoroutine(StartGame());
    }

    public void Instructions()
    {
        GetComponent<AudioSource>().PlayOneShot(startSound);
        StartCoroutine(StartHowTo());
    }

    public void Quit()
    {
        Application.Quit();
    }
}
