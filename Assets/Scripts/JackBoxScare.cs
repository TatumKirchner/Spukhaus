using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JackBoxScare : MonoBehaviour
{
    private AudioSource source;
    private bool isLooping = true;
    [SerializeField] private AudioClip clip;

    private void Start()
    {
        source = GetComponent<AudioSource>();
    }

    private void OnTriggerEnter(Collider other)
    {
        //When the player enters the trigger turn looping off
        if (other.CompareTag("Player"))
        {
            isLooping = false;
        }
    }

    private void Update()
    {
        PrepareJumpScare();
    }

    //When the current clip is done playing. Play a new one.
    void PrepareJumpScare()
    {
        if (!isLooping)
        {
            source.loop = false;
            if (!source.isPlaying)
            {
                isLooping = true;
                source.clip = clip;
                source.Play();
            }
        }
    }
}
