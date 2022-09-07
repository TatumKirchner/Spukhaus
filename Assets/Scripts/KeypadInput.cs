using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;

public class KeypadInput : MonoBehaviour
{
    [SerializeField] private GameObject textBox;
    [SerializeField] private GameObject doorToOpen;
    [SerializeField] private AudioSource source;
    [SerializeField] private AudioClip[] toneClips;
    [SerializeField] private GameObject interactText;
    private InputField textInput;

    private readonly string code = "1408";

    [HideInInspector] public bool isActive = false;
    [HideInInspector] public bool canInteract = false;

    [SerializeField] private float doorOpenSpeed = 5f;

    private bool openDoor;

    private void Start()
    {
        textInput = textBox.GetComponent<InputField>();
        textInput.contentType = InputField.ContentType.Pin;
        textBox.SetActive(false);
        InputField.SubmitEvent submitEvent = new InputField.SubmitEvent();
        submitEvent.AddListener(CompareString);
        textInput.onEndEdit = submitEvent;
    }

    private void Update()
    {
        GetInput();
        ActivateTextBox();
        OpenDoor();
    }

    //When the keypad is intractable test if the player presses E to bring up the text box.
    void GetInput()
    {
        if (canInteract)
            if (Input.GetKeyDown(KeyCode.E))
                isActive = !isActive;
    }

    //Turn the text box on and set it as the currently selected object.
    void ActivateTextBox()
    {
        if (isActive)
        {
            textBox.SetActive(true);
            interactText.SetActive(false);
            EventSystem.current.SetSelectedGameObject(textBox);
        }
        else
        {
            textBox.SetActive(false);
        }
    }

    //Tests the players input to check if it is the right code.
    void CompareString(string input)
    {
        if (String.Equals(input, code))
        {
            isActive = false;
            openDoor = true;
        }
        else
        {
            textInput.text = null;
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(textBox);
            if (!textBox.activeInHierarchy)
                EventSystem.current.SetSelectedGameObject(null);
        }
    }

    //When the player gets the right code open the door
    void OpenDoor()
    {
        if (openDoor)
        {
            doorToOpen.transform.Rotate(doorOpenSpeed * Time.deltaTime * Vector3.up);
            if (doorToOpen.transform.localEulerAngles.y >= 90)
            {
                openDoor = false;
            }
        }            
    }

    //Plays a sound when the player enters a number
    public void PlayKeyTone()
    {
        int clip = UnityEngine.Random.Range(0, toneClips.Length);
        source.clip = toneClips[clip];
        source.Play(); 
    }
}
