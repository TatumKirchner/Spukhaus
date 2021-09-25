using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpScare : MonoBehaviour
{
    private AudioSource source;
    [SerializeField] AudioClip[] audioClips;
    private bool audioPlayed = false;
    private bool playAudio = false;
    public AudioClip clip;

    private void Start()
    {
        source = transform.GetChild(0).GetComponent<AudioSource>();
        int i = Random.Range(0, audioClips.Length);
        source.clip = audioClips[i];
        clip = audioClips[i];
    }

    private void Update()
    {
        PlayAudio();
    }

    //When player enters the trigger play a clip
    private void OnTriggerEnter(Collider other)
    {
        if (!audioPlayed)
        {
            if (other.CompareTag("Player"))
            {
                audioPlayed = true;
                playAudio = true;
            }
        }
    }

    void PlayAudio()
    {
        if (playAudio)
        {
            playAudio = false;
            source.Play();
        }
    }
}
