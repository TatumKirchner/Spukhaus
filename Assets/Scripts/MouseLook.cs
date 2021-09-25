using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseLook : MonoBehaviour
{
    public float mouseSensitivity = 100f;
    public Transform player;
    private float xRotation = 0f;

    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        MoveRotation();
    }

    void MoveRotation()
    {
        //Get input
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        //Rotate player body left and right
        player.Rotate(Vector3.up * mouseX);

        //Rotate camera up and down
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -60f, 50f);
        transform.localRotation = Quaternion.Slerp(transform.localRotation, Quaternion.Euler(xRotation, 0f, 0f), mouseSensitivity * Time.deltaTime);
    }
}
