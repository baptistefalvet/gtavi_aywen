using System.Collections;
using UnityEngine;
public class PlayerCarControll : MonoBehaviour
{
    [Header("Componant")]
    [SerializeField]
    Animator animator;
    [SerializeField]
    ThirdPersonCam playerCam;
    [SerializeField]
    Collider playerCollider;
    [SerializeField]
    GameObject playerObject;
    PlayerController playerController;
    PlayerRagdoll playerRagdoll;
    Rigidbody rb;

    [Header("Car Detection")]
    [SerializeField]
    LayerMask VehicleLayer;
    [SerializeField]
    string VehicleTag;
    [SerializeField]
    float maxCarDetectionRadius;
    [SerializeField]
    KeyCode EnterCarKey;

    [Header("Car Entering")]
    [SerializeField]
    float EnterDuration;
    [SerializeField]
    float ExitCooldown;

    [HideInInspector]
    public bool isInCar;

    bool canExit = false;

    CarController currentCar;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        playerController = GetComponent<PlayerController>();
        playerRagdoll = GetComponent<PlayerRagdoll>();
    }

    private void LateUpdate()
    {
        CheckForCar();
    }

    void CheckForCar()
    {
        if (Input.GetKeyDown(EnterCarKey) && !playerRagdoll.IsRagdoll)
        {
            if (isInCar)
            {
                if (currentCar != null)
                    StartCoroutine(ExitCar());
            }
            else
            {
                Collider[] cols = Physics.OverlapSphere(transform.position, maxCarDetectionRadius, VehicleLayer);
                float minDis = -1;
                GameObject car = null;

                foreach (Collider col in cols)
                {
                    if (col.transform.parent != null)
                    {
                        if (col.transform.parent.gameObject.tag == VehicleTag)
                        {
                            float dis = Vector3.Distance(transform.position, col.transform.parent.position);
                            if (dis < minDis || minDis == -1)
                            {
                                minDis = dis;
                                car = col.transform.parent.gameObject;
                            }
                        }
                    }

                }

                if (car != null)
                {
                    if (car.TryGetComponent<CarController>(out CarController carControll))
                        StartCoroutine(EnterCar(carControll));
                }
            }
        }
    }

    IEnumerator EnterCar(CarController car)
    {
        currentCar = car;
        isInCar = true;
        playerController.CanMove = false;
        playerRagdoll.enabled = false;
        rb.isKinematic = true;
        playerCollider.enabled = false;

        animator.SetBool("Grounded", true);
        animator.SetTrigger("EnterCar");

        yield return null;

        float dL = Vector3.Distance(car.LeftDoor.position, transform.position);
        float dR = Vector3.Distance(car.RightDoor.position, transform.position);

        Transform nearestDoor = dL < dR ? car.LeftDoor : car.RightDoor;

        transform.position = nearestDoor.position + nearestDoor.forward * 0.8f;
        playerObject.transform.right = nearestDoor.forward;

        yield return new WaitForSeconds(EnterDuration);

        playerObject.SetActive(false);
        transform.parent = car.transform;
        transform.localPosition = new Vector3(0f,1f,0f);

        car.enabled = true;
        playerCam.enabled = false;
        car.StartCar();
        
        yield return new WaitForSeconds(ExitCooldown);

        canExit = true;
    }


    IEnumerator ExitCar()
    {
        playerObject.SetActive(true);
        transform.parent = null;

        canExit = false;

        playerCam.enabled = true;

        currentCar.StopCar();
        currentCar.enabled = false;
        isInCar = false;
        playerCollider.enabled = true;
        rb.isKinematic = false;

        yield return null;

        transform.position = currentCar.LeftDoor.position + currentCar.LeftDoor.forward * 1.5f + Vector3.up * 1.25f;
        transform.rotation = Quaternion.identity;

        currentCar = null;

        playerController.CanMove = true;
        playerRagdoll.enabled = true; 
    }
}
