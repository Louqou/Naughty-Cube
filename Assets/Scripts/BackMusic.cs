using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackMusic : MonoBehaviour
{
    public AudioClip[] music;

    public void StartMusic()
    {
        if (music.Length == 9) {
            AudioSource aS = GetComponent<AudioSource>();
            aS.clip = music[LevelLoader.stage - 1];
            aS.Play();
        }
    }
}
