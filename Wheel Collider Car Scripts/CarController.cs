using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class CarController : MonoBehaviour
{
    public SpinWheels spinWheels;
    public ShakeTransform carBodyShake;
    public Transform carBodyTransform;

    private Rigidbody playerRB;
    private float steeringAngle;
    private float brakeInput;
    private float speed;
    private float speedClamped;

    public float slipAngle;
    public WheelColliders wheelCollider;
    public MeshRenderers meshRenderer;
    public WheelParticles wheelParticles;
    public GameObject smokePrefab;
    public float gasInput;
    public float steeringInput;
    public float motorPower;
    //public float steeringAngle;
    public float brakePower;
    public float movingDirection;
    public float carSpeed;
    public float maxSpeed;
    public AnimationCurve steeringCurve;
    public int isEngineRunning;
    public WheelCollider[] wheelColliders = new WheelCollider[4];

    // Start is called before the first frame update
    void Start()
    {
        playerRB = gameObject.GetComponent<Rigidbody>();
        InstantiateSmoke();
        StartCoroutine(CalculateSpeed());
    }

    // Update is called once per frame
    void Update()
    {
        speed = wheelCollider.RLWheel.rpm * wheelCollider.RLWheel.radius * 2f * Mathf.PI / 10;
        speedClamped = Mathf.Lerp(speedClamped, speed, Time.deltaTime);

        CarInput();
        CheckInput();
        CarMovement();
        CarSteering();
        BrakeInput();
        CarBraking();
        HandBrakeSystem();
        CarDrifting();
        SmokeAppearingOnHandbrake();
        FunctionForApplyWheelToCollider();
        //CheckParticles();
        //RearSuspensionLift();
    }

    public void CarInput()
    {
        gasInput = Input.GetAxis("Vertical");
        steeringInput = Input.GetAxis("Horizontal");

    }
    public void CheckInput()
    {
        if(Mathf.Abs(gasInput) > 0 && isEngineRunning == 0)
        {
            StartCoroutine(GetComponent<EngineAudio>().StartEngine());
        }
    }

    public void CarMovement()
    {
        if(isEngineRunning > 1)
        {
            if(speed < maxSpeed)
            {
                wheelCollider.RLWheel.motorTorque = gasInput * motorPower;
                wheelCollider.RRWheel.motorTorque = gasInput * motorPower;
            }

            else 
            {
                wheelCollider.RLWheel.motorTorque = 0;
                wheelCollider.RRWheel.motorTorque = 0;
            }
        }
    }

    public void CarSteering()
    {
        steeringAngle = steeringInput * steeringCurve.Evaluate(speed);

        slipAngle = Vector3.Angle(transform.forward, playerRB.velocity);

        wheelCollider.FLWheel.steerAngle = steeringAngle; //steeringInput * steeringAngle;
        wheelCollider.FRWheel.steerAngle = steeringAngle; //steeringInput * steeringAngle;
    }

    public float GetSpeedRatio()
    {
        var gas = Mathf.Clamp(gasInput, 0.5f, 1f);
        return speedClamped * gas / maxSpeed;
    }

    public void BrakeInput()
    {
        movingDirection = Vector3.Dot(transform.forward, playerRB.velocity);

        if (movingDirection < -0.5f && gasInput > 0)
        {
            brakeInput = Mathf.Abs(gasInput);
        }
        else if (movingDirection > 0.5f && gasInput < 0)
        {
            brakeInput = Mathf.Abs(gasInput);
        }
        else
        {
            brakeInput = 0;
        } 
    }

    public void CarBraking()
    {
        /* if (playerRB.velocity == Vector3.zero)
         {
             wheelCollider.FLWheel.brakeTorque = brakeInput * brakePower;
             wheelCollider.FRWheel.brakeTorque = brakeInput * brakePower;

             wheelCollider.RLWheel.brakeTorque = 0f;
             wheelCollider.RRWheel.brakeTorque = 0f;

             wheelCollider.RLWheel.motorTorque = gasInput * motorPower * 2000f;
             wheelCollider.RRWheel.motorTorque = gasInput * motorPower * 2000f;
         }*/

        // When Gas and Brake pedals are pressed together

        if ((Input.GetKey(KeyCode.W) && Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.UpArrow) && Input.GetKey(KeyCode.DownArrow)) || (Input.GetKey(KeyCode.W) && Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.UpArrow) && Input.GetKey(KeyCode.S)))
        {
            brakeInput = 1;

            wheelCollider.RLWheel.brakeTorque = brakeInput * brakePower;
            wheelCollider.RRWheel.brakeTorque = brakeInput * brakePower;
        }

        wheelCollider.FLWheel.brakeTorque = brakeInput * brakePower * 0.7f;
        wheelCollider.FRWheel.brakeTorque = brakeInput * brakePower * 0.7f;

        wheelCollider.RLWheel.brakeTorque = brakeInput * brakePower * 0.3f;
        wheelCollider.RRWheel.brakeTorque = brakeInput * brakePower * 0.3f;
    }

    public void HandBrakeSystem()
    {
        bool isDrifting = true;

        if(Input.GetKey(KeyCode.Space))
        {
            while(isDrifting)
            {
                if(slipAngle > 1f)
                {
                    WheelFrictionCurve sFriction;

                    sFriction = wheelCollider.RLWheel.sidewaysFriction;
                    sFriction.extremumValue = 0.6f;
                    wheelCollider.RLWheel.sidewaysFriction = sFriction;

                    sFriction = wheelCollider.RRWheel.sidewaysFriction;
                    sFriction.extremumValue = 0.6f;
                    wheelCollider.RRWheel.sidewaysFriction = sFriction;

                    //slipAngle += Vector3.Angle(transform.forward, playerRB.velocity);
                }

                isDrifting = false;
            }

            brakeInput = 1;

            wheelCollider.FLWheel.brakeTorque = brakeInput * brakePower;
            wheelCollider.FRWheel.brakeTorque = brakeInput * brakePower;
                                                                       
            wheelCollider.RLWheel.brakeTorque = brakeInput * brakePower;
            wheelCollider.RRWheel.brakeTorque = brakeInput * brakePower;
            

            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
            {
                if (carSpeed == 0f)
                {
                    carBodyShake.Begin();

                    //wheelParticles.RLWheel.Play();
                    //wheelParticles.RRWheel.Play();
                    //
                    //spinWheels.WheelMeshSpinning();
                }

                /* wheelCollider.RLWheel.brakeTorque = 0f;
                 wheelCollider.RRWheel.brakeTorque = 0f;

                 wheelCollider.RLWheel.motorTorque = gasInput * motorPower * 10000f;
                 wheelCollider.RRWheel.motorTorque = gasInput * motorPower * 10000f;*/
            }

            if(playerRB.velocity != Vector3.zero)
            {
                wheelParticles.RLWheel.Stop();
                wheelParticles.RRWheel.Stop();
            }
        }

        if (slipAngle < 1f)
        {
            WheelFrictionCurve slipFriction;

            slipFriction = wheelCollider.RLWheel.sidewaysFriction;
            slipFriction.extremumValue = 1f;
            wheelCollider.RLWheel.sidewaysFriction = slipFriction;

            slipFriction = wheelCollider.RRWheel.sidewaysFriction;
            slipFriction.extremumValue = 1f;
            wheelCollider.RRWheel.sidewaysFriction = slipFriction;
        }
    }

    public void InstantiateSmoke()
    {
        //wheelParticles.FLWheel = Instantiate(smokePrefab, wheelCollider.FLWheel.transform.position - Vector3.up * wheelCollider.FLWheel.radius,Quaternion.identity, wheelCollider.FLWheel.transform).GetComponent<ParticleSystem>();

        //wheelParticles.FRWheel = Instantiate(smokePrefab, wheelCollider.FRWheel.transform.position - Vector3.up * wheelCollider.FRWheel.radius, Quaternion.identity, wheelCollider.FRWheel.transform).GetComponent<ParticleSystem>();

        wheelParticles.RLWheel = Instantiate(smokePrefab, wheelCollider.RLWheel.transform.position - Vector3.up * wheelCollider.RLWheel.radius, Quaternion.Euler(90f, 0, 0), wheelCollider.RLWheel.transform).GetComponent<ParticleSystem>();

        wheelParticles.RRWheel = Instantiate(smokePrefab, wheelCollider.RRWheel.transform.position - Vector3.up * wheelCollider.RRWheel.radius, Quaternion.Euler(90f, 0, 0), wheelCollider.RRWheel.transform).GetComponent<ParticleSystem>();
    }

    public void SmokeAppearingOnHandbrake()
    {
        if(Input.GetKey(KeyCode.W) && Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.Space) && Input.GetKey(KeyCode.W))
        {
            if(carSpeed == 0f)
            {
                wheelParticles.RLWheel.Play();
                wheelParticles.RRWheel.Play();

                spinWheels.WheelMeshSpinning();
            }           
        }

        else
        {
            wheelParticles.RLWheel.Stop();
            wheelParticles.RRWheel.Stop();
        }
    }

    private IEnumerator CalculateSpeed()
    {
        bool isPlaying = true;

        while(isPlaying)
        {
            Vector3 prevPos = transform.position;

            yield return new WaitForFixedUpdate();

            carSpeed = Mathf.RoundToInt(Vector3.Distance(transform.position, prevPos) / Time.fixedDeltaTime);
        }
    }
    
    public void CarDrifting()
    {
        
            /*
            if(Input.GetKey(KeyCode.Space))
            {
                brakeInput = 1;

                wheelCollider.FLWheel.motorTorque = gasInput * motorPower;
                wheelCollider.FRWheel.motorTorque = gasInput * motorPower;

                wheelCollider.RLWheel.brakeTorque = brakeInput * brakePower;
                wheelCollider.RRWheel.brakeTorque = brakeInput * brakePower;
            }
            */

    


        /*
        WheelHit[] wheelHits = new WheelHit[4];

        brakeInput = 1;
        float slipAngle = 0.1f;

        wheelCollider.FLWheel.GetGroundHit(out wheelHits[0]);
        wheelCollider.FRWheel.GetGroundHit(out wheelHits[1]);

        wheelCollider.RLWheel.GetGroundHit(out wheelHits[2]);
        wheelCollider.RRWheel.GetGroundHit(out wheelHits[3]);

        if (carSpeed == 30f && Mathf.Abs(wheelCollider.FLWheel.steerAngle) > slipAngle)
        {
            wheelCollider.FLWheel.motorTorque = gasInput * motorPower;
            wheelCollider.FRWheel.motorTorque = gasInput * motorPower;

            wheelCollider.RLWheel.brakeTorque = brakeInput * brakePower;
            wheelCollider.RRWheel.brakeTorque = brakeInput * brakePower;

        }
        */
    }

    /*public void CheckParticles()
    {
        WheelHit[] wheelHits = new WheelHit[4];

        wheelCollider.FLWheel.GetGroundHit(out wheelHits[0]);
        wheelCollider.FRWheel.GetGroundHit(out wheelHits[1]);

        wheelCollider.RLWheel.GetGroundHit(out wheelHits[2]);
        wheelCollider.RRWheel.GetGroundHit(out wheelHits[3]);

        if (wheelHits[2].forwardSlip <= 0 && wheelHits[3].forwardSlip <=0)
        {
            carBodyShake.Begin();
        }
    }*/

    public void RearSuspensionLift()
    {
        // When turning the vehicle to the left side

        if((Input.GetKey(KeyCode.W) && Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.UpArrow) && Input.GetKey(KeyCode.LeftArrow)) || (Input.GetKey(KeyCode.W) && Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.UpArrow) && Input.GetKey(KeyCode.A)))
        {
            wheelCollider.FRWheel.suspensionDistance = 0.2f;
            wheelCollider.RLWheel.suspensionDistance = 0.6f;
        }

        else if(!(Input.GetKey(KeyCode.W) && Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.UpArrow) && Input.GetKey(KeyCode.LeftArrow)) || (Input.GetKey(KeyCode.W) && Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.UpArrow) && Input.GetKey(KeyCode.A)))
        {
            wheelCollider.FRWheel.suspensionDistance = 0.3f;
            wheelCollider.RLWheel.suspensionDistance = 0.3f;
        }

        // When turning the vehicle to the right side

        if ((Input.GetKey(KeyCode.W) && Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.UpArrow) && Input.GetKey(KeyCode.RightArrow)) || (Input.GetKey(KeyCode.W) && Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.UpArrow) && Input.GetKey(KeyCode.D)))
        {
            wheelCollider.FLWheel.suspensionDistance = 0.2f;
            wheelCollider.RRWheel.suspensionDistance = 0.6f;
        }

        else if (!(Input.GetKey(KeyCode.W) && Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.UpArrow) && Input.GetKey(KeyCode.RightArrow)) || (Input.GetKey(KeyCode.W) && Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.UpArrow) && Input.GetKey(KeyCode.D)))
        {
            wheelCollider.FLWheel.suspensionDistance = 0.3f;
            wheelCollider.RRWheel.suspensionDistance = 0.3f;
        }

    }

    public void FunctionForApplyWheelToCollider()
    {
        ApplyWheelToCollider(wheelCollider.FLWheel, meshRenderer.FLWheel);
        ApplyWheelToCollider(wheelCollider.FRWheel, meshRenderer.FRWheel);
        ApplyWheelToCollider(wheelCollider.RLWheel, meshRenderer.RLWheel);
        ApplyWheelToCollider(wheelCollider.RRWheel, meshRenderer.RRWheel);
    }

    public void ApplyWheelToCollider(WheelCollider collider, MeshRenderer mesh)
    {
        Vector3 position;
        Quaternion quat;

        collider.GetWorldPose(out position, out quat);

        mesh.transform.position = position;
        mesh.transform.rotation = quat;
    }
}

[System.Serializable]
public class WheelColliders
{
    public WheelCollider FLWheel;
    public WheelCollider FRWheel;
    public WheelCollider RLWheel;
    public WheelCollider RRWheel;
}

[System.Serializable]
public class MeshRenderers
{
    public MeshRenderer FLWheel;
    public MeshRenderer FRWheel;
    public MeshRenderer RLWheel;
    public MeshRenderer RRWheel;
}

[System.Serializable]
public class WheelParticles
{
    public ParticleSystem FLWheel;
    public ParticleSystem FRWheel;
    public ParticleSystem RLWheel;
    public ParticleSystem RRWheel;
}

