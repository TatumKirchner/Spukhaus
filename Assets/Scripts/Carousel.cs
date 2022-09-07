using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Carousel : MonoBehaviour
{
    #region Parameters
    [SerializeField] private GameObject door;
    private GameObject currentTarget;

    [SerializeField] private float doorCloseSpeed = 35f;
    [SerializeField] private float doorOpenSpeed = 35f;
    [SerializeField] private float interactionRange = 50f;

    [SerializeField] private AudioClip[] wrongDoorClips;
    [SerializeField] private AudioClip doorOpenClip;
    [SerializeField] private AudioClip doorCloseClip;

    [SerializeField] private Animator wallAnim;
    [SerializeField] private Animator lightAnim;
    [SerializeField] private GameObject interactText;
    private AudioSource source;

    private bool startSpin = false;
    private bool doorClosed = false;
    private bool correctDoor = false;
    private bool doorOpening = false;
    private bool canInteractWithDoor = false;
    private bool spinComplete = false;
    private bool doorFinishedOpening = false;

    private int highlightLayer;
    private int doorLayer;

    private Vector3 screenCenter;

    private FlashlightFlicker flicker;
    private FieldOfView fieldOfView;
    #endregion

    private void Start()
    {
        source = GetComponent<AudioSource>();
        highlightLayer = LayerMask.NameToLayer("Highlight");
        doorLayer = LayerMask.NameToLayer("Doors");
        screenCenter = new Vector3(Screen.width >> 1, Screen.height >> 1);
        flicker = FindObjectOfType<FlashlightFlicker>();
        fieldOfView = FindObjectOfType<FieldOfView>();
    }

    //When the player enters the trigger start the animation
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !spinComplete)
        {
            startSpin = true;
            //Set this to stop it from spinning again if the player walks into the trigger again
            spinComplete = true;
        }
    }

    private void Update()
    {
        PlayAnimation();
        InteractWithDoor();
        HighlightDoors();
        OpenDoor();
    }

    void PlayAnimation()
    {
        if (startSpin)
        {
            //Rotate the door
            if (!doorClosed)
                door.transform.Rotate(doorCloseSpeed * Time.deltaTime * -Vector3.up);
            //When the door is closed stop rotating, play an audio clip, and start the carousel animation
            if (Quaternion.Angle(door.transform.localRotation, Quaternion.Euler(0, -90, 0)) < 1f)
            {
                doorClosed = true;
                source.clip = doorCloseClip;
                source.Play();
                wallAnim.SetBool("Spin", true);
                lightAnim.SetBool("playAnim", true);
                StartCoroutine(flicker.CarouselFlicker());
                startSpin = false;
            }
        }
    }

    void HighlightDoors()
    {
        if (doorClosed)
        {
            //Raycast to detect doors and highlightable objects
            if (Physics.Raycast(Camera.main.ScreenPointToRay(screenCenter), out RaycastHit hit, interactionRange, LayerMask.GetMask("Doors", "Highlight")))
            {
                GameObject target = hit.collider.gameObject;

                if (currentTarget != target)
                {
                    //Switch its layer to the object highlight layer
                    currentTarget = target;
                    currentTarget.layer = highlightLayer;
                    interactText.SetActive(true);
                    canInteractWithDoor = true;
                    //If the player highlights the correct door set the bool
                    if (currentTarget.CompareTag("Exit Door"))
                    {
                        correctDoor = true;
                    }
                    else
                    {
                        correctDoor = false;
                    }
                }
            }
            else if (currentTarget != null)
            {
                //Set the objects layer back to normal so its not highlighted anymore
                currentTarget.layer = doorLayer;
                interactText.SetActive(false);
                currentTarget = null;
                canInteractWithDoor = false;
                correctDoor = false;
            }
        }
    }

    void InteractWithDoor()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (canInteractWithDoor)
            {
                if (!correctDoor)
                {
                    //If the door the player interacted with is wrong play a clip. Else Open the door.
                    int i = Random.Range(0, wrongDoorClips.Length);
                    source.clip = wrongDoorClips[i];
                    source.Play();
                }
                else
                {
                    doorOpening = true;
                }
            }
        }
    }

    void OpenDoor()
    {
        //When the player interacts with the correct door open it
        if (doorOpening && !doorFinishedOpening)
        {
            //Increase the field of view of the flashlight so it will trigger the end at a better distance.
            //Then open the door to the exit.
            fieldOfView.viewRadius = 5f;
            door.transform.Rotate(Vector3.up * doorOpenSpeed * Time.deltaTime);
            source.clip = doorOpenClip;
            source.Play();
            if (Quaternion.Angle(door.transform.localRotation, Quaternion.Euler(0, 0, 0)) <= .3f)
            {
                doorOpening = false;
                doorClosed = false;
                doorFinishedOpening = true;
            }
        }
    }
}
