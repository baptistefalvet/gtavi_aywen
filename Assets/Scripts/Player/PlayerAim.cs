using UnityEngine;

public class PlayerAim : MonoBehaviour
{
    [Header("Componants")]
    public Transform AimPoint;
    [SerializeField]
    Camera MainCam;
    [SerializeField]
    ThirdPersonCam playerCam;
    PlayerCarControll playerCar;
    PlayerRagdoll playerRagdoll;
    PlayerController playerController;
    PlayerWeaponController playerWeapon;

    [Header("Aiming")]
    [SerializeField]
    float AimingFov;
    [SerializeField]
    float FallBackDis;
    [SerializeField]
    LayerMask HitLayer;

    [HideInInspector]
    public bool IsAiming;


    private void Awake()
    {
        playerCar = GetComponent<PlayerCarControll>();
        playerRagdoll = GetComponent<PlayerRagdoll>();
        playerController = GetComponent<PlayerController>();
        playerWeapon = GetComponent<PlayerWeaponController>();
    }

    void GetTargetPos()
    {
        if(playerCam.mode == CameraStyle.Fps)
        {
            AimPoint.position = MainCam.transform.position + MainCam.transform.forward * FallBackDis;
        }
        else
        {
            Ray ray = MainCam.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hitInfo, Mathf.Infinity, HitLayer))
            {
                AimPoint.position = hitInfo.point;
            }
            else
            {
                AimPoint.position = ray.origin + ray.direction * FallBackDis;
            }
        }
    }

    void CheckIfAiming()
    {
        if (!playerWeapon.isWeaponEquiped)
        {
            IsAiming = false;
        }
        else if (playerCam.mode == CameraStyle.Fps)
        {
            IsAiming = true;
        }
        else if (!playerCar.isInCar && playerController.isGrounded && !playerRagdoll.IsRagdoll)
        {
            if(Input.GetMouseButton(1))
            {
                IsAiming = true;
            }
            else
            {
                IsAiming = false;
            }
        }
        else
        {
            IsAiming = false;
        }
    }
    void ModifyAimingPoint()
    {
        if (playerWeapon.isWeaponEquiped && IsAiming)
        {
            AimPoint.localScale = Vector3.Slerp(AimPoint.localScale, Vector3.one, Time.deltaTime * 5.0f);
        }
        else
        {
            AimPoint.localScale = Vector3.Slerp(AimPoint.localScale, Vector3.zero, Time.deltaTime * 5.0f);
        }
    }

    private void Update()
    {
        ModifyAimingPoint();
        CheckIfAiming();

        if (IsAiming)
        {
            GetTargetPos();
            playerCam.ChangeFov(AimingFov);
        }
        else
        {
            playerCam.ResetFov();
        }
            
    }

}
