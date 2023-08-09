using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpinWheels : MonoBehaviour
{
    private float speedUp = 0f;

    // Update is called once per frame
    void FixedUpdate()
    {
        WheelMeshSpinning();
    }

    public void WheelMeshSpinning()
    {
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
        {
            speedUp += 0.1f;
            transform.Rotate(Vector3.right * speedUp);
        }

        else if(Input.GetKeyUp(KeyCode.W) || Input.GetKeyUp(KeyCode.UpArrow)) 
        {
            //rigidbody.AddTorque( Vector3.forward);
            transform.Rotate(Vector3.right * speedUp);
            speedUp /= speedUp * 1.5f;
        }

        else
        {
            transform.Rotate(Vector3.right * speedUp);
            speedUp -= 0.1f;
        }

        if (speedUp <= 0f)
        {
            speedUp = 0f;
        }
    }
}
