using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayMusic : MonoBehaviour
{
    private AudioSource source;
    [SerializeField] AudioClip[] clips;

    // Start is called before the first frame update
    void Start()
    {
        source = GetComponent<AudioSource>();
        source.ignoreListenerPause = true;
        source.playOnAwake = true;
        source.loop = false;
    }

    // Update is called once per frame
    void Update()
    {
        //When the clip is done playing pick a new one to play.
        if (!source.isPlaying)
        {
            int i = Random.Range(0, clips.Length);
            source.clip = clips[i];
            source.Play();
        }
    }
}
