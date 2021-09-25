using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class Flashlight : MonoBehaviour
{
    #region Parameters

    [Header("Parameters")]
    public int maxBatteries = 4;
    public int currentBatteries;

    public bool useBatteryLife = true;
    public bool useUi;
    public float drainSpeed = 1f;

    public bool recharge = true;
    public float rechargeSpeed = 0.5f;

    [Range(0, 1)] [Tooltip("Minimum amount of chagarge needed for the flashlight to turn back on")]
    public float minChargePercentage = 0.05f; // Minimum amount of chagarge needed for the flashlight to turn back on

    public const float minBatteryCharge = 0f;
    public float maxBatteryCharge = 10f;

    public float followSpeed = 5f;
    public Quaternion offset = Quaternion.identity;


    #endregion

    #region References

    [Header("References")]
    public AudioClip onClip;
    public AudioClip offClip;
    public AudioClip newBatteryClip;
    public AudioMixerGroup sfxMixer;

    public Image stateImage;
    public Slider batteryChargeSlider;
    public Image batteryChargeSliderFill;
    public Text newBatteryText;
    public Text batteryCountText;
    public CanvasGroup holder;

    public Color fullChargeColor = Color.green;
    public Color noChargeColor = Color.red;

    public Camera mainCamera;
    public GameObject flashlight;


    #endregion

    #region Stats

    [Header("Stats")]
    public float batteryLife = 0.0f;
    public bool flashlightOn = false;
    private bool batteryDead = false;

    #endregion

    #region Private Properties

    private IEnumerator IE_updateBatteryLife;

    private Light spotLight;

    float defaultIntensity = 0.0f;

    AudioSource flashlightAudioSource;

    #endregion

    private void Start()
    {
        Init();
    }

    private void Update()
    {
        //Toggle Flashlight
        if (Input.GetKeyDown(KeyCode.F))
            ToggleFlashLight(!flashlightOn, true);
        //Insert a new battery
        if (Input.GetKeyDown(KeyCode.R) && CanReload() && useUi)
            Reload();
        //Aim the flashlight where the player is looking
        if (flashlightOn)
        {
            transform.localRotation = Quaternion.Slerp(transform.localRotation, mainCamera.transform.localRotation * offset, followSpeed * Time.deltaTime);
            flashlight.transform.rotation = transform.rotation;
        }
    }

    //Insert a new battery if reload key is pressed
    private void Reload()
    {
        batteryLife = maxBatteryCharge;
        spotLight.intensity = LightIntensity();
        currentBatteries--;

        UpdateCountText();
        UpdateSlider();

        UpdateBatteryState(false);
        UpdateBatteryLifeProcess();

        PlaySFX(newBatteryClip);
    }

    //Turn the flashlight on and off
    void ToggleFlashLight(bool state, bool playSound)
    {
        flashlightOn = state;
        flashlight.SetActive(state);

        state = (!batteryDead || !flashlightOn) && flashlightOn;

        ToggleObject(state);

        if (useUi)
        {
            if (holder)
            {
                switch (flashlightOn)
                {
                    case true:
                        holder.alpha = 1.0f;
                        break;
                    case false:
                        holder.alpha = 0.0f;
                        break;
                }
            }
        }

        if (playSound)
        {
            PlaySFX(flashlightOn ? onClip : offClip);
        }
        UpdateBatteryLifeProcess();
    }

    //Starts a coroutine to increase or decrease the battery charge
    void UpdateBatteryLifeProcess()
    {
        if (IE_updateBatteryLife != null)
            StopCoroutine(IE_updateBatteryLife);

        if (flashlightOn && !batteryDead)
        {
            if (useBatteryLife)
            {
                IE_updateBatteryLife = ReduceBattery();
                StartCoroutine(IE_updateBatteryLife);
            }
            return;
        }
        if (recharge)
        {
            IE_updateBatteryLife = IncreaseBattery();
            StartCoroutine(IE_updateBatteryLife);
        }
    }

    //If the battery is dead the battery will recharge
    private IEnumerator IncreaseBattery()
    {
        while (batteryLife < maxBatteryCharge)
        {
            float newValue = batteryLife + rechargeSpeed * Time.deltaTime;
            batteryLife = Mathf.Clamp(newValue, minChargePercentage, maxBatteryCharge);
            spotLight.intensity = LightIntensity();

            BatteryLifeCheck();

            UpdateSlider();

            yield return new WaitForEndOfFrame();
        }
    }

    //Checks if the battery is dead and if the flashlight can be reloaded
    private void BatteryLifeCheck()
    {
        if (ReloadReady() && batteryDead)
        {
            UpdateBatteryState(false);
            UpdateBatteryLifeProcess();
        }
    }

    //Reduces charge while the battery is above 0
    private IEnumerator ReduceBattery()
    {
        while (batteryLife > 0.0f)
        {
            float newValue = batteryLife - drainSpeed * Time.deltaTime;
            batteryLife = Mathf.Clamp(newValue, minBatteryCharge, maxBatteryCharge);

            spotLight.intensity = LightIntensity();

            UpdateSlider();
            yield return new WaitForEndOfFrame();
        }
        UpdateBatteryState(true);
        UpdateBatteryLifeProcess();
    }

    //Enables the new battery text and changes the state image color
    private void UpdateBatteryState(bool isDead)
    {
        batteryDead = isDead;

        if (newBatteryText)
            newBatteryText.enabled = isDead;
        if (stateImage)
            stateImage.color = isDead ? new Color(1, 1, 1, .5f) : Color.white;

        bool state = !batteryDead && flashlightOn;
        ToggleObject(state);
    }

    //Play a sound clip when the flashlight turns on, off, or a new battery is inserted
    private void PlaySFX(AudioClip clip)
    {
        //if (clip == null)
        //    return;
        flashlightAudioSource.clip = clip;
        flashlightAudioSource.Play();
    }

    //Turns the flashlight light on/off
    void ToggleObject(bool state)
    {
        spotLight.enabled = state;
    }

    //Updates the slider value
    void UpdateSlider()
    {
        if (batteryChargeSlider)
            batteryChargeSlider.value = batteryLife;
        if (batteryChargeSliderFill)
            batteryChargeSliderFill.color = Color.Lerp(noChargeColor, fullChargeColor, BatteryLifeNormalized());
    }

    //Updates the current and max amount of batteries
    void UpdateCountText()
    {
        if (batteryCountText)
        {
            StringBuilder countString = new StringBuilder("Batteries: ");
            countString.Append(currentBatteries);
            countString.Append(" / ");
            countString.Append(maxBatteries);

            batteryCountText.text = countString.ToString();
        }            
    }

    //Checks if the flashlight is on, the player has batteries, and the current battery has some charge
    private bool CanReload()
    {
        return flashlightOn && (currentBatteries > 0 && batteryLife < maxBatteryCharge);
    }

    //Checks if the battery percentage is above the minimum charge percentage
    private bool ReloadReady()
    {
        return BatteryLifeNormalized() >= minChargePercentage;
    }

    //Returns the current battery life normalized
    private float BatteryLifeNormalized()
    {
        return batteryLife / maxBatteryCharge;
    }

    //Returns the intensity based on the current battery life
    private float LightIntensity()
    {
        return defaultIntensity * BatteryLifeNormalized();
    }

    //Initializes members on start
    void Init()
    {
        if (spotLight == null)
        {
            spotLight = GetComponent<Light>();
            if (spotLight == null)
                spotLight = gameObject.AddComponent<Light>();
            spotLight.type = LightType.Spot;
        }

        if (flashlightAudioSource == null)
        {
            flashlightAudioSource = GetComponent<AudioSource>();
            if (flashlightAudioSource == null)
            {
                flashlightAudioSource = gameObject.AddComponent<AudioSource>();
            }
            flashlightAudioSource.playOnAwake = false;
            flashlightAudioSource.outputAudioMixerGroup = sfxMixer;
        }

        if (!mainCamera)
            mainCamera = Camera.main;

        defaultIntensity = spotLight.intensity;
        batteryLife = maxBatteryCharge;

        UpdateBatteryState(false);

        ToggleFlashLight(true, false);

        if (useUi)
        {
            UpdateCountText();

            batteryChargeSlider.minValue = minBatteryCharge;
            batteryChargeSlider.maxValue = maxBatteryCharge;
            batteryChargeSlider.value = batteryLife;
            UpdateSlider();

            newBatteryText.text = "RELOAD (R)";
        }
        else
        {
            holder.alpha = 0.0f;
        }
        
    }
}
