using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlashlightFlicker : MonoBehaviour
{
    public Light flashlight;

    //Coroutines to toggle the lights of the players flashlight and the lights in the carousel

    public IEnumerator Flicker(float duration)
    {
        flashlight.enabled = false;
        yield return new WaitForSeconds(duration);
        flashlight.enabled = true;
        yield return new WaitForSeconds(duration);
        flashlight.enabled = false;
        yield return new WaitForSeconds(duration);
        flashlight.enabled = true;
        yield return new WaitForEndOfFrame();
    }

    public IEnumerator CarouselFlicker()
    {
        flashlight.enabled = false;
        yield return new WaitForSeconds(2f);
        flashlight.enabled = true;
        yield return new WaitForSeconds(1f);
        flashlight.enabled = false;
        yield return new WaitForSeconds(2f);
        flashlight.enabled = true;
        yield return new WaitForSeconds(1f);
        flashlight.enabled = false;
        yield return new WaitForSeconds(2f);
        flashlight.enabled = true;
        yield return new WaitForEndOfFrame();
    }
}
