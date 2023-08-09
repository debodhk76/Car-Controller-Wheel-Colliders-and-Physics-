using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EngineAudio : MonoBehaviour
{
    private CarController carcontroller;
    private float speedRatio;
    private float revLimiter;

    public Transform carTransform;
    public float carSpeed;
    public AudioSource runningSound;
    public float runningMaxVolume;
    public float runningMaxPitch;
    public AudioSource idleSound;
    public float idleMaxVolume;
    public float idleMinVolume;
    public float limiterSound = 1f;
    public float limiterFrequency = 3f;
    public float limiterEngage = 0.8f;
    public AudioSource startingSound;
    public bool isEngineRunning = false;

    // Start is called before the first frame update
    void Start()
    {
        carcontroller = GetComponent<CarController>();
        idleSound.volume = 0;
        runningSound.volume = 0;

        StartCoroutine(CalculateSpeed());
    }

    // Update is called once per frame
    void Update()
    {
        if(carcontroller)
        {
            speedRatio = carcontroller.GetSpeedRatio();
        }

        if(speedRatio > limiterEngage)
        {
            revLimiter = (Mathf.Sin(Time.time * limiterFrequency) + 1f) * limiterSound * (speedRatio - limiterEngage);
        }

        if(isEngineRunning)
        {
            if (carSpeed == 0)
            {
                idleSound.volume = 1f;
                idleSound.pitch = 0.5f;
                runningSound.volume = 0f;
                runningSound.pitch = 0.5f;
            }

            if(carSpeed > 0)
            {
                startingSound.Stop();
                idleSound.volume = 0f;
                runningSound.volume = Mathf.Lerp(1f, runningMaxVolume, speedRatio);
                runningSound.pitch = Mathf.Lerp(runningSound.pitch, Mathf.Lerp(2f, runningMaxPitch, speedRatio) + revLimiter, Time.deltaTime);
                

            }
        }
        /*
        else
        {
            idleSound.volume = 0;
            runningSound.volume = 0;
        }*/

    }
    public IEnumerator StartEngine()
    {
        startingSound.pitch = 0.5f;
        startingSound.Play();
        carcontroller.isEngineRunning = 1;
        yield return new WaitForSeconds(5f);
        isEngineRunning = true;
        carcontroller.isEngineRunning = 2;
    }
    private IEnumerator CalculateSpeed()
    {
        bool isPlaying = true;

        while (isPlaying)
        {
            Vector3 prevPos = carTransform.transform.position;

            yield return new WaitForFixedUpdate();

            carSpeed = Mathf.RoundToInt(Vector3.Distance(carTransform.transform.position, prevPos) / Time.fixedDeltaTime);
        }
    }
}
