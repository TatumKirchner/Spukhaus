using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpenKeypad : MonoBehaviour
{
    private KeypadInput keypadInput;
    private int highlightLayer;
    private int defaultLayer;
    [SerializeField] private GameObject interactText;

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
            interactText.SetActive(true);
            keypadInput.canInteract = true;
        }   
    }

    private void OnTriggerExit(Collider other)
    {
        //When the player leaves the trigger set the keypad back to normal.
        if (other.CompareTag("Player"))
        {
            gameObject.layer = defaultLayer;
            interactText.SetActive(false);
            keypadInput.canInteract = false;
            keypadInput.isActive = false;
        }
    }
}
