using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarAntiRoll : MonoBehaviour
{
    public Rigidbody carRigidBody;
    public WheelCollider RLWheel;
    public WheelCollider RRWheel;
    public float antiRoll = 5000.0f;

    public void FixedUpdate()
    {
        WheelHit hit; 
        float travelL = 1.0f;
        float travelR = 1.0f;

        bool groundedL = RLWheel.GetGroundHit(out hit);

        if (groundedL)
        {
            travelL = (-RLWheel.transform.InverseTransformPoint(hit.point).y - RLWheel.radius) / RLWheel.suspensionDistance;
        } 

        bool groundedR = RRWheel.GetGroundHit(out hit);

        if (groundedR)
        {
            travelR = (-RRWheel.transform.InverseTransformPoint(hit.point).y - RRWheel.radius) / RRWheel.suspensionDistance;
        }

        float antiRollForce = (travelL - travelR) * antiRoll;

        if (groundedL)
        {
            carRigidBody.AddForceAtPosition(RLWheel.transform.up * -antiRollForce,
            RLWheel.transform.position);
        }
        
        if (groundedR)
        {
            carRigidBody.AddForceAtPosition(RRWheel.transform.up * antiRollForce,
            RRWheel.transform.position);
        }

            

    }


}
