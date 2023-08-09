using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WheelSuspension : MonoBehaviour
{
    public bool FLWheel;
    public bool FRWheel;
    public bool RLWheel;
    public bool RRWheel;

    private Rigidbody playerRB;

    [Header("Suspension")]
    public float restLength;
    public float springTravel;
    public float springStiffness;
    public float damperStiffness;

    private float minLength;
    private float maxLength;
    private float lastlength;
    public float springLength;
    private float springVelocity;
    public float springForce;
    private float damperForce;

    [Header("Wheel Steer Angle")]
    public float steerAngle;
    public float steerTime;
    
    private Vector3 suspensionForce;
    public Vector3 wheelVelocityLS; // Local Space 
    private float Fx;
    private float Fy;
    private float wheelAngle;

    [Header("Wheel")]
    public float wheelRadius;

    // Start is called before the first frame update
    void Start()
    {
        playerRB = transform.root.GetComponent<Rigidbody>();

        minLength = restLength - springTravel;
        maxLength = restLength + springTravel;
    }
    private void Update()
    {
        wheelAngle = Mathf.Lerp(wheelAngle, steerAngle, steerTime * Time.deltaTime);

        transform.localRotation = Quaternion.Euler(Vector3.up * wheelAngle);

        Debug.DrawRay(transform.position, -transform.up * (springLength + wheelRadius), Color.green);
    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        if(Physics.Raycast(transform.position, -transform.up, out RaycastHit hit, maxLength + wheelRadius))
        {
            lastlength = springLength;
            springLength = hit.distance - wheelRadius;
            springLength = Mathf.Clamp(springLength, minLength, maxLength);
            springVelocity = (lastlength - springLength) / Time.fixedDeltaTime;
            springForce = springStiffness * (restLength - springLength);
            damperForce = damperStiffness * springVelocity;

            suspensionForce = (springForce + damperForce) * transform.up;

            wheelVelocityLS = transform.InverseTransformDirection(playerRB.GetPointVelocity(hit.point));

            Fx = Input.GetAxis("Vertical") * 0.25f * springForce;
            Fy = wheelVelocityLS.x * springForce;

            playerRB.AddForceAtPosition(suspensionForce + (Fx * transform.forward) + (Fy * -transform.right), hit.point);
        }
    }
}
