using System.Collections;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class PlayerRagdoll : MonoBehaviour
{
    [Header("Componants")]
    [SerializeField]
    Rigidbody[] bodyParts;
    [SerializeField]
    Animator animator;
    [SerializeField]
    Rigidbody PlayerRb;
    [SerializeField]
    PlayerController playerController;
    [SerializeField]
    ThirdPersonCam camController;
    [SerializeField]
    Transform Targets;
    [SerializeField]
    Transform Player;
    [SerializeField]
    Transform Hips;
    PlayerWeaponController playerWeapon;

    [Header("GetUp")]
    [SerializeField]
    string _standUpStateName;
    [SerializeField]
    float standUpDelay;
    [SerializeField]
    LayerMask GroundLayer;
    [SerializeField]
    float MaxGetUpHeight;
    [SerializeField]
    float StandUpOffset;

    bool canGetUp = true;

    [HideInInspector]
    public bool IsRagdoll;


    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.E) && canGetUp)
        {
            if(!IsRagdoll)
            {
                EnableRagdoll(Random.insideUnitSphere * Random.Range(0.5f,3.0f));
            }
            else
            {
                DisableRagdoll();
            }
        }
    }

    private void Start()
    {
        canGetUp = true;
        IsRagdoll = false;

        for (int i = 0; i < bodyParts.Length; i++)
        {
            Rigidbody part = bodyParts[i];

            part.isKinematic = true;

            Collider collider = part.GetComponent<Collider>();
            collider.enabled = false;
        }

        animator.enabled = true;

        playerWeapon = GetComponent<PlayerWeaponController>();
    }


    public void EnableRagdoll(Vector3 force)
    {
        playerWeapon.UnequipeWeapon();

        Targets.parent = Hips;
        Targets.localPosition = Vector3.zero;

        IsRagdoll = true;
        animator.enabled = false;
        playerController.enabled = false;
        camController.UpdateDamping(5f);
        camController.enabled = false;
        PlayerRb.isKinematic = true;

        for (int i = 0; i < bodyParts.Length; i++)
        {
            Rigidbody part = bodyParts[i];

            if (part.TryGetComponent<Collider>(out Collider collider))
                collider.enabled = true;

            part.isKinematic = false;
            part.AddForce(force, ForceMode.Impulse);
        }
    }

    public void DisableRagdoll()
    {
        if(canGetUp)
            StartCoroutine(DisableRagdollCoroutine());
    }

    IEnumerator DisableRagdollCoroutine()
    {
        animator.SetBool("StandUp", true);
        
        canGetUp = false;
        IsRagdoll = false;

        for (int i = 0; i < bodyParts.Length; i++)
        {
            Rigidbody part = bodyParts[i];

            part.isKinematic = true;

            Collider collider = part.GetComponent<Collider>();
            collider.enabled = false;
        }

        transform.position = GetGroundPos();
        transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, Hips.rotation.y, transform.rotation.eulerAngles.z);


        animator.enabled = true;

        animator.Play(_standUpStateName);

        yield return null;

        Vector3 currentPos = Targets.position;
        Vector3 targetPos = GetGroundPos();

        Targets.parent = null;

        float elapsedTime = 0.0f;

        while (elapsedTime < standUpDelay)
        {
            Targets.position = Vector3.Lerp(currentPos, targetPos, elapsedTime/standUpDelay);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        Targets.parent = Player;
        Targets.localPosition = Vector3.zero;


        yield return null;


        playerController.enabled = true;
        camController.enabled = true;
        camController.UpdateDamping(1f);
        PlayerRb.isKinematic = false;

        canGetUp = true;

        animator.SetBool("StandUp", false);
    }


    Vector3 GetGroundPos()
    {
        Ray ray = new Ray(Hips.position + Vector3.up * 1.0f, Vector3.down);
        if(Physics.Raycast(ray, out RaycastHit hitInfo, MaxGetUpHeight,GroundLayer))
        {
            return hitInfo.point + Vector3.up * StandUpOffset;
        }
        else
        {
            return Hips.position + Vector3.up * (StandUpOffset + 0.5f);
        }
    }
}
