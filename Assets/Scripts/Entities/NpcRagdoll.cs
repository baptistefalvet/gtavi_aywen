using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class NpcRagdoll : MonoBehaviour
{
    [Header("Componants")]
    [SerializeField]
    Rigidbody[] bodyParts;
    [SerializeField]
    Animator animator;
    [SerializeField]
    Transform Npc;
    [SerializeField]
    Transform Hips;
    NpcController controller;
    Collider col;
    NavMeshAgent agent;

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

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        controller = GetComponent<NpcController>();
        col = GetComponent<Collider>();
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
    }


    public void EnableRagdoll(Vector3 force)
    {
        if (!canGetUp)
            return;

        IsRagdoll = true;
        animator.enabled = false;
        controller.enabled = false;
        col.enabled = false;
        agent.enabled = false;

        for (int i = 0; i < bodyParts.Length; i++)
        {
            Rigidbody part = bodyParts[i];

            if (part.TryGetComponent<Collider>(out Collider collider))
                collider.enabled = true;

            part.isKinematic = false;
            part.AddForce(force, ForceMode.Impulse);
        }
    }

    public void ApplyForceToRagdoll(Vector3 force)
    {
        for (int i = 0; i < bodyParts.Length; i++)
        {
            Rigidbody part = bodyParts[i];

            if (part.TryGetComponent<Collider>(out Collider collider))
                collider.enabled = true;

            part.AddForce(force, ForceMode.Impulse);
        }
    }

    public void DisableRagdoll()
    {
        if (canGetUp)
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

        yield return new WaitForSeconds(standUpDelay);


        controller.enabled = true;
        col.enabled = true;
        agent.enabled = true;

        canGetUp = true;

        animator.SetBool("StandUp", false);
    }

    Vector3 GetGroundPos()
    {
        Ray ray = new Ray(Hips.position + Vector3.up * 1.0f, Vector3.down);
        if (Physics.Raycast(ray, out RaycastHit hitInfo, MaxGetUpHeight, GroundLayer))
        {
            return hitInfo.point + Vector3.up * StandUpOffset;
        }
        else
        {
            return Hips.position + Vector3.up * (StandUpOffset + 0.5f);
        }
    }

    public void FreezeParts(float delay)
    {
        StartCoroutine(FreezePartsCoroutine(delay));
    }

    IEnumerator FreezePartsCoroutine(float delay)
    {
        yield return new WaitForSeconds(delay);

        for (int i = 0; i < bodyParts.Length; i++)
        {
            Rigidbody part = bodyParts[i];

            part.isKinematic = true;

            Collider collider = part.GetComponent<Collider>();
            collider.enabled = false;
        }
    }
}
