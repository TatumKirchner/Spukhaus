using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpenKeypad : MonoBehaviour
{
    private KeypadInput keypadInput;
    private int highlightLayer;
    private int defaultLayer;

    private void Start()
    {
        defaultLayer = LayerMask.NameToLayer("Default");
        highlightLayer = LayerMask.NameToLayer("Highlight");
        keypadInput = GetComponent<KeypadInput>();
    }

    private void OnTriggerEnter(Collider other)
    {
        //When the player enters the trigger set the keypad objects layer to the highlight layer and make it intractable.
        if (other.CompareTag("Player"))
        {
            gameObject.layer = highlightLayer;
            keypadInput.canInteract = true;
        }   
    }

    private void OnTriggerExit(Collider other)
    {
        //When the player leaves the trigger set the keypad back to normal.
        if (other.CompareTag("Player"))
        {
            gameObject.layer = defaultLayer;
            keypadInput.canInteract = false;
            keypadInput.isActive = false;
        }
    }
}
