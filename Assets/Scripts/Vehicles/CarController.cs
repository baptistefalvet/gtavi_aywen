using UnityEngine;
using UnityEngine.Rendering;

public class CarController : MonoBehaviour
{
    [Header("Layers")]
    [SerializeField]
    int currentLayer;
    [SerializeField]
    int playerVehicleLayer;

    [Header("Componants")]
    [SerializeField]
    Rigidbody carRb;
    [SerializeField]
    Transform[] RayPoints;
    [SerializeField]
    LayerMask GroundLayer;
    [SerializeField]
    Transform accelerationPoint;

    [Header("Suspention Settings")]
    [SerializeField]
    float springStiffness;
    [SerializeField]
    float damperStiffness;
    [SerializeField]
    float restLength;
    [SerializeField]
    float springTravel;
    [SerializeField]
    float wheelRadius;

    [Header("Drag Settings")]
    [SerializeField]
    float GroundDrag;
    [SerializeField]
    float IncreassedGravity;

    [Header("Inputs Settings")]
    [SerializeField]
    float acceleration;
    [SerializeField]
    float reverseAcceleration;
    [SerializeField]
    float maxSpeed;
    [SerializeField]
    float deceleration;
    [SerializeField]
    float steerStrength;
    [SerializeField]
    AnimationCurve turningCurve;
    [SerializeField]
    float dragCoefficient;

    [Header("Visual Wheel Settings")]
    [SerializeField]
    Transform[] Wheels;
    [SerializeField]
    Transform[] SteeringWheels;
    [SerializeField]
    float WheelRotationSpeed;
    [SerializeField]
    float WheelSteerAngle;

    [Header("Visual Effects")]
    [SerializeField]
    PlayerSpeedLines playerSpeedLines;
    [SerializeField]
    ParticleSystem[] SmokeParticles;
    [SerializeField]
    TrailRenderer[] MarksRenderer;
    [SerializeField]
    ParticleSystem[] SteeringParticles;
    [SerializeField]
    float effectsTreshold;

    [Header("Car Starting")]
    [SerializeField]
    GameObject DownCol;
    [SerializeField]
    GameObject CarCinemachineCam;
    [SerializeField]
    GameObject Lights;

    [Header("Car Doors")]
    public Transform LeftDoor;
    public Transform RightDoor;

    Vector3 currentCarLocalVelocity = Vector3.zero;
    float carVelocityRatio = 0;

    float moveInput;
    float steerInput;

    private int[] wheelsIsGrounded = new int[4];
    bool isGrounded = false;

    private void Awake()
    {
        carRb = GetComponent<Rigidbody>();
    }

    public void StartCar()
    {
        gameObject.layer = playerVehicleLayer;
        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.layer = playerVehicleLayer;
        }
        CarCinemachineCam.SetActive(true);
        Lights.SetActive(true);
        DownCol.SetActive(false);
    }

    public void StopCar()
    {
        gameObject.layer = currentLayer;
        for(int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.layer = currentLayer;
        }

        CarCinemachineCam.SetActive(false);
        Lights.SetActive(false);
        DownCol.SetActive(true);

        carRb.linearDamping = 0.0f;
        ToggleMarks(false);
        ToggleParticles(false);
    }

    private void FixedUpdate()
    {
        Suspension();
        GroundCheck();
        ApplyDrag();
        CalculateCarVelocity();
        Movement();
    }

    void Suspension()
    {
        float maxLength = restLength + springTravel;

        for (int i = 0; i < RayPoints.Length; i++)
        {
            Transform point = RayPoints[i];
            RaycastHit hit;
            
            if(Physics.Raycast(point.position,-point.up, out hit, maxLength + wheelRadius, GroundLayer))
            {
                wheelsIsGrounded[i] = 1;

                float currentSpringLength = hit.distance - wheelRadius;
                float springCompression = (restLength - currentSpringLength)/springTravel;

                float springVelocity =Vector3.Dot(carRb.GetPointVelocity(point.position),point.up);
                float dampForce = springVelocity * damperStiffness;

                float springForce = springStiffness * springCompression;

                float netForce = springForce - dampForce;

                carRb.AddForceAtPosition(netForce * point.up, point.position);

                Debug.DrawLine(point.position, hit.point,Color.red);
            }
            else
            {
                wheelsIsGrounded[i] = 0;

                Debug.DrawLine(point.position, point.position + (wheelRadius + maxLength) * -point.up, Color.green);
            }

            
        }
    }

    void ApplyDrag()
    {
        for (int i = 0; i < RayPoints.Length; i++)
        {
            Transform point = RayPoints[i];
            carRb.AddForceAtPosition(IncreassedGravity * -point.up, point.position);
        }

        if (isGrounded)
            carRb.linearDamping = GroundDrag;
        else
            carRb.linearDamping = 0.0f;
    }


    void Movement()
    {
        if(isGrounded)
        {
            Acceleration();
            Deceleration();
            Turn();
            SidewaysDrag();
        }
    }

    void Acceleration()
    {
        float accel = moveInput > 0 ? acceleration : reverseAcceleration;
        carRb.AddForceAtPosition(accel * moveInput * transform.forward,accelerationPoint.position, ForceMode.Acceleration);
    }

    void Deceleration()
    {
        carRb.AddForceAtPosition(deceleration * moveInput * -transform.forward, accelerationPoint.position, ForceMode.Acceleration);
    }

    void Turn()
    {
        carRb.AddTorque(steerStrength * steerInput * turningCurve.Evaluate(Mathf.Abs(carVelocityRatio)) * Mathf.Sign(carVelocityRatio) * transform.up, ForceMode.Acceleration);
    }

    void SidewaysDrag()
    {
        float currentSidewaysSpeed = currentCarLocalVelocity.x;

        float dragMagnitude = -currentSidewaysSpeed * dragCoefficient;

        Vector3 dragForce = transform.right * dragMagnitude;

        carRb.AddForceAtPosition(dragForce, carRb.worldCenterOfMass, ForceMode.Acceleration);
    }

    void GroundCheck()
    {
        int tempGroundedWheels = 0;

        for (int i = 0; i < wheelsIsGrounded.Length; i++)
        {
            tempGroundedWheels += wheelsIsGrounded[i];
        }

        isGrounded = tempGroundedWheels > 2;
    }

    void CalculateCarVelocity()
    {
        currentCarLocalVelocity = transform.InverseTransformDirection(carRb.linearVelocity);
        carVelocityRatio = currentCarLocalVelocity.z / maxSpeed;
    }

    private void Update()
    {
        GetInputs();
        AdapteWheelPlacement();
        Effects();
    }

    void GetInputs()
    {
        moveInput = Input.GetAxis("Vertical");
        steerInput = Input.GetAxis("Horizontal");
    }


    void AdapteWheelPlacement()
    {
        float maxLength = restLength + springTravel;

        float steeringAngle = steerInput * WheelSteerAngle;
        for (int i = 0; i < SteeringWheels.Length; i++)
        {
            if(steeringAngle != 0)
                SteeringWheels[i].localRotation = Quaternion.Euler(new Vector3(SteeringWheels[i].localEulerAngles.x, steeringAngle,0));
        }

        for (int i = 0; i < Wheels.Length; i++)
        {
            Transform point = RayPoints[i];
            RaycastHit hit;

            Vector3 targetPos;

            if (Physics.Raycast(point.position, -point.up, out hit, maxLength + wheelRadius, GroundLayer))
            {
                targetPos = hit.point + point.up * wheelRadius * 2.0f;
            }
            else
            {
                targetPos = point.position;
            }

            Wheels[i].position = Vector3.Lerp(Wheels[i].position,targetPos,Time.deltaTime * 80.0f);
            Wheels[i].Rotate(Vector3.right, carVelocityRatio * WheelRotationSpeed * Time.deltaTime);
        }

    }


    void Effects()
    {
        if(isGrounded && Mathf.Abs(currentCarLocalVelocity.x) > effectsTreshold)
        {
            playerSpeedLines.EnableSpeedLines = true;
            ToggleMarks(true);
            ToggleParticles(true);
        }
        else
        {
            playerSpeedLines.EnableSpeedLines = false;
            ToggleMarks(false);
            ToggleParticles(false);
        }


        float steeringAngle = steerInput * WheelSteerAngle;
        for (int i = 0; i < SteeringParticles.Length; i++)
        {
            SteeringParticles[i].transform.rotation = SteeringParticles[i].transform.parent.rotation * Quaternion.Euler(SteeringParticles[i].transform.localEulerAngles.x, -steeringAngle,0);
        }
    }

    void ToggleMarks(bool togle)
    {
        foreach(TrailRenderer marks in MarksRenderer)
        {
            marks.emitting = togle;
        }
    }

    void ToggleParticles(bool togle)
    {
        for(int i = 0;i < SteeringParticles.Length;i++)
        {
            ParticleSystem particles = SteeringParticles[i];

            if (!particles.isPlaying)
                particles.Play();

            var emmission = particles.emission;

            bool steering = (i == 0 && steerInput > 0) || (i == 1 && steerInput < 0);

            if (togle && steering)
            {
                emmission.enabled = true;
            }
            else
                emmission.enabled = false;
        }

        foreach (ParticleSystem particles in SmokeParticles)
        {
            if(!particles.isPlaying)
                particles.Play();

            var emmission = particles.emission;

            if (togle)
            {
                emmission.enabled = true;
            }  
            else
                emmission.enabled = false;
        }
    }


}
