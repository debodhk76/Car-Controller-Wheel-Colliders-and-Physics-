using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ShakeTransform : MonoBehaviour
{
    public Transform carPrefab;

    [Header("Info")]
    private Vector3 startPos;
    private float timer;
    private Vector3 randomPos;

    [Header("Settings")]
    [Range(0f, 2f)]
    public float time = 0.2f;
    [Range(0f, 2f)]
    public float distance = 0.01f;
    [Range(0f, 100f)]
    public float delayBetweenShakes = 50f;

    public void Awake()
    {
        startPos = transform.position;
    }

    public void Update()
    {
        startPos = carPrefab.transform.position;
    }

    /*private void OnValidate()
    {
        if (delayBetweenShakes > time)
            delayBetweenShakes = time;
    }*/

    public void Begin()
    {
        //StopAllCoroutines();
        StartCoroutine(Shake());
    }

    public IEnumerator Shake()
    {
        timer = 0f;

            while (timer < time)
            {
                timer += Time.deltaTime;

                randomPos = startPos + (new Vector3(Random.insideUnitSphere.x, Random.insideUnitSphere.y, 0) * distance);

                transform.position = randomPos;
                
                if (delayBetweenShakes > 0f)
                {
                    yield return new WaitForSeconds(delayBetweenShakes);
                }
                else
                {
                    yield return null;
                }
            }

            transform.position = startPos;
    }
    

}
