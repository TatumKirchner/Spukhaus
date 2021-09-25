using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class Pause : MonoBehaviour
{
    public bool isPaused = false;
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject optionsPanel;
    [SerializeField] private GameObject creditsPanel;
    [SerializeField] private GameObject endPanel;

    [SerializeField] private AudioMixer musicMixer;
    [SerializeField] private AudioMixer sfxMixer;

    [SerializeField] MouseLook mouseLook;

    private bool endGame = false;


    private void Start()
    {
        pausePanel.SetActive(false);
        optionsPanel.SetActive(false);
    }

    private void Update()
    {
        //Toggle paused bool
        if (Input.GetKeyDown(KeyCode.Escape) && !endGame)
        {
            isPaused = !isPaused;
        }

        //When the game is paused stop time and any audio that is playing and set the pause UI to active.
        if (isPaused)
        {
            Time.timeScale = 0;
            pausePanel.SetActive(true);
            Cursor.lockState = CursorLockMode.Confined;
            Cursor.visible = true;
            AudioListener.pause = true;
        }
        else
        {
            Time.timeScale = 1;
            pausePanel.SetActive(false);
            optionsPanel.SetActive(false);
            creditsPanel.SetActive(false);
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            AudioListener.pause = false;
        }
    }

    //Methods for the buttons on the pause menu.

    public void Resume()
    {
        isPaused = false;
    }

    public void Quit()
    {
        Application.Quit();
    }

    public void Options()
    {
        pausePanel.SetActive(false);
        optionsPanel.SetActive(true);
    }

    //Sets the mixers volume. Using Log10 makes it able to go to 0 so there is no sound. Without it was unable to get the audio to be completely off.
    public void MusicSlider(float value)
    {
        musicMixer.SetFloat("MusicVol", Mathf.Log10(value) * 20);
    }

    public void SfxSlider(float value)
    {
        sfxMixer.SetFloat("SfxVol", Mathf.Log10(value) * 20);
    }

    public void Back()
    {
        pausePanel.SetActive(true);
        optionsPanel.SetActive(false);
    }

    public void OpenCreditsPanel()
    {
        pausePanel.SetActive(false);
        creditsPanel.SetActive(true);
    }

    public void CloseCreditsPanel()
    {
        creditsPanel.SetActive(false);
        pausePanel.SetActive(true);
    }

    public void Mousesensitivity(float value)
    {
        mouseLook.mouseSensitivity = value;
    }

    public void EndScreen()
    {
        endGame = true;
        isPaused = true;
        pausePanel.SetActive(false);
        endPanel.SetActive(true);
    }

    //Loads the main menu
    public void EndScreenExit()
    {
        SceneManager.LoadScene(0);
    }
}
