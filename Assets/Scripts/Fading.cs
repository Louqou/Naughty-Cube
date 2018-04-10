using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// Created with reference to https://www.youtube.com/watch?v=0HwZQt94uHQ
public class Fading : MonoBehaviour
{
    public Texture2D fadeOutTexture;
    public float fadeSpeed = 0.8f;

    private int drawDepth = -1000;
    private float alpha = 1.0f;
    private int fadeDir = -1;

    private void Awake()
    {
        SceneManager.sceneLoaded += this.SceneLoaded;
    }

    private void OnGUI()
    {
        alpha += fadeDir * fadeSpeed * Time.deltaTime;
        alpha = Mathf.Clamp01(alpha);
        GUI.color = new Color(
            GUI.color.r,
            GUI.color.g,
            GUI.color.b,
            alpha);
        GUI.depth = drawDepth;
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), fadeOutTexture);
    }

    public float BeginFade(int dir)
    {
        fadeDir = dir;
        return 1 / fadeSpeed;
    }

    private void SceneLoaded(Scene scene, LoadSceneMode mode)
    {
        BeginFade(-1);
    }
}
