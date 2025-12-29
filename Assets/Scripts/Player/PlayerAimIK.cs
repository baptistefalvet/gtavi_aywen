using UnityEngine;
using UnityEngine.Animations.Rigging;

public class PlayerAimIK : MonoBehaviour
{
    [Header("Componants")]
    [SerializeField]
    ThirdPersonCam playerCam;
    [SerializeField]
    TwoBoneIKConstraint LeftHandIK;
    [SerializeField]
    TwoBoneIKConstraint RightHandIK;
    [SerializeField]
    Transform LeftHandIKTarget;
    [SerializeField]
    Transform RightHandIKTarget;
    [SerializeField]
    MultiAimConstraint SplineAim;
    [SerializeField]
    MultiAimConstraint WeaponAim;
    PlayerAim playerAim;

    [Header("Hand IK Settings")]
    [SerializeField]
    float stepSize;
    [SerializeField]
    float EquipeSmoothTime;

    [Header("Aim IK Settings")]
    [SerializeField,Range(0f,1f)]
    float SplineAimWeight;
    [SerializeField, Range(0f, 1f)]
    float WeaponAimWeight;

    WeaponController activeWeapon;


    float leftHandTargetWeight = 0.0f;
    float rightHandTargetWeight = 0.0f;
    float weaponAimTargetWeight = 0.0f;
    float splineAimTargetWeight = 0.0f;

    float leftHandWeight = 0.0f;
    float rightHandWeight = 0.0f;

    private void Awake()
    {
        playerAim = GetComponent<PlayerAim>();
    }

    public void ResetWeights()
    {
        LeftHandIK.weight = 0.0f;
        RightHandIK.weight = 0.0f;
    }

    public void EquipeWeapon(WeaponController weapon)
    {
        activeWeapon = weapon;
        leftHandTargetWeight = weapon.LeftHandWeight;
        rightHandTargetWeight = weapon.RightHandWeight;
        weaponAimTargetWeight = WeaponAimWeight;
        splineAimTargetWeight = SplineAimWeight;
    }

    public void UnequipeWeapon()
    {
        activeWeapon = null;
        leftHandTargetWeight = 0.0f;
        rightHandTargetWeight = 0.0f;
        weaponAimTargetWeight = 0.0f;
        splineAimTargetWeight = 0.0f;
    }

    private void Update()
    {
        UpdateHandIK();
    }

    private void UpdateHandIK()
    {
       leftHandWeight = Mathf.Lerp(leftHandWeight, leftHandTargetWeight, Time.deltaTime * EquipeSmoothTime );
       rightHandWeight = Mathf.Lerp(rightHandWeight, rightHandTargetWeight, Time.deltaTime * EquipeSmoothTime);

       LeftHandIK.weight = Mathf.Round(leftHandWeight * stepSize) / stepSize;
       RightHandIK.weight = Mathf.Round(rightHandWeight * stepSize) / stepSize;

       WeaponAim.weight = Mathf.Lerp(WeaponAim.weight, weaponAimTargetWeight, Time.deltaTime * EquipeSmoothTime );
       SplineAim.weight = Mathf.Lerp(SplineAim.weight, splineAimTargetWeight, Time.deltaTime * EquipeSmoothTime );
        

        if (activeWeapon != null)
        {
            LeftHandIKTarget.position = activeWeapon.LeftHandWeaponPos.position;
            LeftHandIKTarget.rotation = activeWeapon.LeftHandWeaponPos.rotation;
            RightHandIKTarget.position = activeWeapon.RightHandWeaponPos.position;
            RightHandIKTarget.rotation = activeWeapon.RightHandWeaponPos.rotation;

            if(!playerAim.IsAiming)
            {
                weaponAimTargetWeight = 0.0f;
                splineAimTargetWeight = 0.0f;
            }
            else if (playerCam.mode == CameraStyle.Fps)
            {
                weaponAimTargetWeight = WeaponAimWeight * 0.3f;
                splineAimTargetWeight = 0.0f;
            }
            else
            {
                weaponAimTargetWeight = WeaponAimWeight;
                splineAimTargetWeight = SplineAimWeight;
            }

        }
        
    }

    
}
