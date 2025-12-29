using UnityEngine;
using UnityEngine.Animations.Rigging;

public class PlayerIKController : MonoBehaviour
{
    [Header("Bones")]
    [SerializeField]
    Transform LeftFootBone;
    [SerializeField]
    Transform RightFootBone;
    [SerializeField]
    Transform BodyBone;

    [Header("Componants")]
    [SerializeField]
    Transform PlayerOrientation;
    [SerializeField]
    Animator playerAnimator;
    [SerializeField]
    Transform leftFootIKTarget;
    [SerializeField]
    TwoBoneIKConstraint leftFootIK;
    [SerializeField]
    Transform rightFootIKTarget;
    [SerializeField]
    TwoBoneIKConstraint rightFootIK;
    [SerializeField]
    Transform pelvisIKTarget;
    [SerializeField]
    MultiPositionConstraint pelvisConstaint;
    [SerializeField]
    MultiRotationConstraint leftFootRotationIKContraintTarget;
    [SerializeField]
    MultiRotationConstraint rightFootRotationIKContraintTarget;

    [Header("IK Placement")]
    [SerializeField]
    bool affectHip;
    [SerializeField]
    float distanceToGround;
    [SerializeField]
    float baseHipsPositionY;
    [SerializeField]
    LayerMask GroundLayer;
    [SerializeField]
    Vector3 leftFootRotationOffset;
    [SerializeField]
    Vector3 rightFootRotationOffset;

    bool isAllowedToUseFootIk;

    private void Start()
    {
        SetIsAllowedToUseFootIK(true);
    }

    public void SetIsAllowedToUseFootIK(bool boolVal)
    {
        isAllowedToUseFootIk = boolVal;
        if(!boolVal)
        {
            leftFootIK.weight = 0f;
            rightFootIK.weight = 0f;
            pelvisConstaint.weight = 0f;
        }
        else
        {
            leftFootIK.weight = playerAnimator.GetFloat("IK_LeftFootWeight");
            rightFootIK.weight = playerAnimator.GetFloat("IK_RightFootWeight");
        }
    }

    private void LateUpdate()
    {
        UpdateFootIK();
    }
    void UpdateFootIK()
    {
        Ray rayLeft = new Ray(LeftFootBone.position + Vector3.up * 0.5f, Vector3.down);
        bool isRayCasHitLeftFoot = Physics.Raycast(rayLeft, out RaycastHit leftFootHit, distanceToGround + 2, GroundLayer);

        Ray rayRight = new Ray(RightFootBone.position + Vector3.up * 0.5f, Vector3.down);
        bool isRayCasHitRightFoot = Physics.Raycast(rayRight, out RaycastHit rightFootHit, distanceToGround + 2, GroundLayer);

        if (isAllowedToUseFootIk)
        {
            SetWightOfConstraint(leftFootHit, rightFootHit);
            FootIK1(isRayCasHitLeftFoot, isRayCasHitRightFoot, leftFootHit, rightFootHit);
            HipsIK1(isRayCasHitLeftFoot, isRayCasHitRightFoot, leftFootHit, rightFootHit);
        }
    }

    private void OnDrawGizmos()
    {
        Ray rayLeft = new Ray(LeftFootBone.position + Vector3.up * 0.5f, Vector3.down);
        Ray rayRight = new Ray(RightFootBone.position + Vector3.up * 0.5f, Vector3.down);

        Gizmos.color = Color.yellow;

        Gizmos.DrawRay(rayLeft);
        Gizmos.DrawRay(rayRight);
    }

    float smoothedHipsWeight = 0f;
    float smoothedLeftFootWeight = 0f;
    float smoothedRightFootWeight = 0f;

    void SetWightOfConstraint(RaycastHit leftFootHit,RaycastHit rightFootHit)
    {
        float leftSlope = Vector3.Angle(Vector3.up, leftFootHit.normal);
        float rightSlope = Vector3.Angle(Vector3.up, rightFootHit.normal);
        float averageSlopeAngle = (rightSlope + leftSlope) * 0.5f;
        float slopNormalizedValue = averageSlopeAngle / 90f;

        float targetWeight = Mathf.Clamp01(Mathf.Abs(leftFootHit.point.y - rightFootHit.point.y) / 0.3f);

        if (targetWeight < 0.01f)
            targetWeight = 0.0f;

        smoothedHipsWeight = Mathf.Lerp(smoothedHipsWeight, targetWeight, Time.deltaTime * 10f);

        float currentLeftFootY = LeftFootBone.position.y;
        float currentRightFootY = RightFootBone.position.y;
        float leftFootWeight = Mathf.Clamp01(playerAnimator.GetFloat("IK_LeftFootWeight") + slopNormalizedValue);
        float rightFootWeight = Mathf.Clamp01(playerAnimator.GetFloat("IK_RightFootWeight") + slopNormalizedValue);

        if(currentLeftFootY < leftFootHit.point.y)
        {
            smoothedLeftFootWeight = Mathf.Lerp(smoothedLeftFootWeight, 1, Time.deltaTime * 20.0f);
        }
        else
        {
            smoothedLeftFootWeight = Mathf.Lerp(smoothedLeftFootWeight, leftFootWeight, Time.deltaTime * 20.0f);
        }

        if (currentRightFootY < rightFootHit.point.y)
        {
            smoothedRightFootWeight = Mathf.Lerp(smoothedRightFootWeight, 1, Time.deltaTime * 20.0f);
        }
        else
        {
            smoothedRightFootWeight = Mathf.Lerp(smoothedRightFootWeight, rightFootWeight, Time.deltaTime * 20.0f);
        }

        pelvisConstaint.weight = Mathf.Clamp(smoothedHipsWeight, 0, 0.95f);
        leftFootIK.weight = Mathf.Clamp(smoothedLeftFootWeight, 0, 0.95f);
        rightFootIK.weight = Mathf.Clamp(smoothedRightFootWeight, 0, 0.95f);

        leftFootRotationIKContraintTarget.weight = Mathf.Clamp(smoothedLeftFootWeight, 0, 0.95f);
        rightFootRotationIKContraintTarget.weight = Mathf.Clamp(smoothedRightFootWeight, 0, 0.95f);

    }

    void FootIK1(bool isRayCastHitLeftFoot,bool isRayCastHitRightFoot,RaycastHit leftFootHit,RaycastHit rightFootHit)
    {
        if (!playerAnimator)
            return;

        if(isRayCastHitLeftFoot)
        {
            float currentLeftFootY = LeftFootBone.position.y;
            Vector3 footPosition = leftFootHit.point;
            if(currentLeftFootY < leftFootHit.point.y)
            {
                footPosition.y += distanceToGround + 0.1f;
            }
            else
            {
                footPosition.y += distanceToGround;
            }
            leftFootIKTarget.position = footPosition;

            Quaternion footRotationOffset = Quaternion.Euler(leftFootRotationOffset);
            Vector3 footForward = Vector3.ProjectOnPlane(PlayerOrientation.forward, leftFootHit.normal).normalized;
            Quaternion footRotation = Quaternion.LookRotation(footForward, leftFootHit.normal) * footRotationOffset;

            leftFootRotationIKContraintTarget.transform.rotation = Quaternion.Slerp(leftFootRotationIKContraintTarget.transform.rotation, footRotation, Time.deltaTime * 10);
        }

        if (isRayCastHitRightFoot)
        {
            float currentRightFootY = RightFootBone.position.y;
            Vector3 footPosition = rightFootHit.point;
            if (currentRightFootY < rightFootHit.point.y)
            {
                footPosition.y += distanceToGround + 0.1f;
            }
            else
            {
                footPosition.y += distanceToGround;
            }
            rightFootIKTarget.position = footPosition;

            Quaternion footRotationOffset = Quaternion.Euler(rightFootRotationOffset);
            Vector3 footForward = Vector3.ProjectOnPlane(PlayerOrientation.forward, rightFootHit.normal).normalized;
            Quaternion footRotation = Quaternion.LookRotation(footForward, rightFootHit.normal) * footRotationOffset;

            rightFootRotationIKContraintTarget.transform.rotation = Quaternion.Slerp(rightFootRotationIKContraintTarget.transform.rotation, footRotation, Time.deltaTime * 10);
        }
    }

    float lowestFootY;
    float hipsTargetY;
    float hipsCurrentY;
    Vector3 currentHipsPosition;

    void HipsIK1(bool isRayCastHitLeftFoot, bool isRayCastHitRightFoot, RaycastHit leftFootHit, RaycastHit rightFootHit)
    {
        if (!affectHip)
            return;

        if (playerAnimator)
        {
            if (isRayCastHitLeftFoot && isRayCastHitRightFoot)
            {
                float leftY = leftFootHit.point.y;
                float rightY = rightFootHit.point.y;

                lowestFootY = Mathf.Min(leftY, rightY);
                hipsTargetY = baseHipsPositionY + lowestFootY;

                if(Mathf.Abs(hipsTargetY - hipsCurrentY) > 0.01f)
                {
                    hipsCurrentY = Mathf.Lerp(hipsCurrentY, hipsTargetY, Time.deltaTime * 15f);
                }

                pelvisIKTarget.position = new Vector3(pelvisIKTarget.position.x, hipsCurrentY,pelvisIKTarget.position.z);

                currentHipsPosition = BodyBone.position;
            }
        }
    }
}
