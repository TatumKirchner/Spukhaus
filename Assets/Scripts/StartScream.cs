using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartScream : MonoBehaviour
{
    private AudioSource source;
    public float playDelay = 5f;

    // At the start of the game play the scream clip to signal that the player needs to enter the building.
    void Start()
    {
        source = GetComponent<AudioSource>();
        source.PlayDelayed(playDelay);
    }    
}
