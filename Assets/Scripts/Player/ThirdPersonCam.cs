using Unity.Cinemachine;
using UnityEngine;

public enum CameraStyle { Near = 0,Far = 1,Fps = 2}

public class ThirdPersonCam : MonoBehaviour
{
    [HideInInspector]
    public CameraStyle mode = CameraStyle.Near;

    [Header("Componants")]
    [SerializeField]
    Rigidbody playerRb;
    [SerializeField]
    Transform player;
    [SerializeField]
    Transform playerOrientation;
    [SerializeField]
    Transform lookAtTarget;
    [SerializeField]
    PlayerController playerController;
    [SerializeField]
    PlayerAim playerAim;


    [Header("Rotation Settings")]
    [SerializeField]
    int FrameDelay;
    [SerializeField]
    float RotationSpeed;

    [Header("Camera Style Settings")]
    [SerializeField]
    float DefaultFOV;
    [SerializeField]
    KeyCode CamChangeModeKey;
    [SerializeField]
    GameObject fpsCam, nearCam, farCam;

    [Header("Fps Camera Settings")]
    [SerializeField]
    float SensitivityX;
    [SerializeField]
    float SensitivityY;
    [SerializeField]
    Vector2 xRotClamp;
    float xRot, yRot;

    [Header("Camera Shake Settings")]
    [SerializeField]
    float DefaultAmplitude;
    [SerializeField]
    float DefaultFrequency;
    [SerializeField]
    float ShakeRecoverySpeed;

    float fov;
    float shakeAmplitude;
    float shakeFrequency;

    private void Start()
    {
        ChangeFov(DefaultFOV);
    }

    public void UpdateDamping(float damping)
    {
        nearCam.GetComponent<CinemachineOrbitalFollow>().TrackerSettings.PositionDamping = new Vector3(damping * 0.25f, damping * 0.25f, damping * 0.25f);
        farCam.GetComponent<CinemachineOrbitalFollow>().TrackerSettings.PositionDamping = new Vector3(damping * 0.25f, damping * 0.25f, damping * 0.25f);
    }

    void SwitchCameraStyle()
    {
        if (Input.GetKeyDown(CamChangeModeKey))
        {
            mode = (CameraStyle)(((int)mode + 1) % 3);
        }

        nearCam.SetActive(mode == CameraStyle.Near);
        farCam.SetActive(mode == CameraStyle.Far);
        fpsCam.SetActive(mode == CameraStyle.Fps);
    }

    public void CameraShake(float Amplitude, float Frequency)
    {
        shakeAmplitude = Amplitude;
        shakeFrequency = Frequency;
    }

    void UpdateCamShake()
    {
        nearCam.GetComponent<CinemachineBasicMultiChannelPerlin>().AmplitudeGain = shakeAmplitude;
        farCam.GetComponent<CinemachineBasicMultiChannelPerlin>().AmplitudeGain = shakeAmplitude;
        fpsCam.GetComponent<CinemachineBasicMultiChannelPerlin>().AmplitudeGain = shakeAmplitude;

        nearCam.GetComponent<CinemachineBasicMultiChannelPerlin>().FrequencyGain = shakeFrequency;
        farCam.GetComponent<CinemachineBasicMultiChannelPerlin>().FrequencyGain = shakeFrequency;
        fpsCam.GetComponent<CinemachineBasicMultiChannelPerlin>().FrequencyGain = shakeFrequency;

        shakeAmplitude = Mathf.Lerp(shakeAmplitude, DefaultAmplitude,Time.deltaTime * ShakeRecoverySpeed);
        shakeFrequency = Mathf.Lerp(shakeFrequency, DefaultFrequency, Time.deltaTime * ShakeRecoverySpeed);
    }

    public void ChangeFov(float val)
    {
        fov = val;
    }

    public void ResetFov()
    {
        fov = DefaultFOV;
    }


    void UpdateFov()
    {
        nearCam.GetComponent<CinemachineCamera>().Lens.FieldOfView= Mathf.Lerp(nearCam.GetComponent<CinemachineCamera>().Lens.FieldOfView, fov,Time.deltaTime * 8.0f);
        farCam.GetComponent<CinemachineCamera>().Lens.FieldOfView = Mathf.Lerp(farCam.GetComponent<CinemachineCamera>().Lens.FieldOfView ,fov,Time.deltaTime * 8.0f);
        fpsCam.GetComponent<CinemachineCamera>().Lens.FieldOfView = Mathf.Lerp(fpsCam.GetComponent<CinemachineCamera>().Lens.FieldOfView ,fov,Time.deltaTime * 8.0f);
    }

    private void Update()
    {
        UpdateCamShake();
        UpdateFov();
        SwitchCameraStyle();

        if(mode == CameraStyle.Fps)
        {
            float mouseX = Input.GetAxisRaw("Mouse X") * SensitivityX * Time.deltaTime;
            float mouseY = Input.GetAxisRaw("Mouse Y") * SensitivityY * Time.deltaTime;

            xRot -= mouseY;
            yRot += mouseX;

            xRot = Mathf.Clamp(xRot, xRotClamp.x, xRotClamp.y);

            fpsCam.transform.rotation = Quaternion.Euler(xRot, yRot, 0);
            playerOrientation.rotation = Quaternion.Euler(0, yRot, 0);
            player.rotation = Quaternion.Euler(0, yRot + 90, 0);
        }
        else
        {
            Transform target = mode == CameraStyle.Near ? lookAtTarget : player;

            Vector3 viewDir = target.position - new Vector3(transform.position.x, target.position.y, transform.position.z);
            playerOrientation.forward = viewDir;

            if (playerAim.IsAiming)
            {
                Transform point = playerAim.AimPoint;

                Vector3 dir = -(point.position - new Vector3(player.position.x, point.position.y, player.position.z));
                float dot = Vector3.Dot(dir.normalized, player.right);

                if (dot < 0.33)
                    player.right = Vector3.Slerp(player.right, dir.normalized, RotationSpeed * Time.deltaTime * 0.1f);
            }
            else
            {
                bool slope = playerController.OnSlope();

                float horiztonalInput = -Input.GetAxisRaw("Horizontal");
                float verticalInput = Input.GetAxisRaw("Vertical");
                Vector3 inputDir = horiztonalInput * playerOrientation.forward + verticalInput * playerOrientation.right;

                if (slope)
                    inputDir = playerController.GetSlopeMoveDirection(inputDir).normalized;

                if (inputDir != Vector3.zero && (Mathf.Floor(Time.time * 12) % FrameDelay == 0))
                    player.forward = Vector3.Slerp(player.forward, inputDir.normalized, RotationSpeed * Time.deltaTime);
            }
            
        }

    }

    private void OnDrawGizmos()
    {
        Transform point = playerAim.AimPoint;

        Vector3 dir = -(point.position - new Vector3(player.position.x, point.position.y, player.position.z));
        
        Gizmos.color = Color.green;

        Gizmos.DrawRay(player.position, dir);
        Gizmos.DrawRay(player.position, player.right);
    }
}
