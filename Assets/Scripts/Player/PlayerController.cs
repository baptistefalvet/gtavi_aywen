using System.Collections;
using System.Security;
using UnityEngine;


public enum MouvementState { Idle,Walk,Sprint,Air}

public class PlayerController : MonoBehaviour
{
    [HideInInspector]
    public MouvementState state;

    [Header("Componants Settings")]
    [SerializeField]
    Transform Orientation;
    [SerializeField]
    Transform PlayerObj;
    [SerializeField]
    Animator animator;
    Rigidbody rb;

    [Header("Mouvement Settings")]
    [SerializeField]
    float WalkSpeed;
    [SerializeField]
    float SprintSpeed;
    [SerializeField]
    KeyCode SprintKey;

    float moveSpeed;


    [Header("Ground Settings")]
    [SerializeField, Range(0f, 1f)]
    float AirMultiplier;
    [SerializeField]
    float GroundDrag;
    [SerializeField]
    float IncreassedGravity;
    [SerializeField]
    float ConstantDownGravity;
    [SerializeField]
    LayerMask GroundLayer;
    [SerializeField]
    float PlayerHeight;
    [SerializeField]
    Vector3 CheckBoxSize;

    [HideInInspector]
    public bool isGrounded;

    [Header("Jump Settings")]
    [SerializeField]
    float JumpForce;
    [SerializeField]
    float JumpDelay;
    [SerializeField]
    float JumpCooldown;
    [SerializeField]
    KeyCode JumpKey;

    [Header("Slope Handle")]
    [SerializeField]
    float MaxSlopeAngle;
    RaycastHit SlopeHit;

    bool canJump = true;
    bool exitingSlope = false;

    float horiztonalInput;
    float verticalInput;

    [HideInInspector]
    public bool CanMove = true;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = false;
    }

    void GetInputs()
    {
        horiztonalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");
    }

    void CheckIfGrounded()
    {
        Ray ray = new Ray(transform.position, Vector3.down);
        isGrounded = Physics.BoxCast(transform.position, CheckBoxSize, Vector3.down, transform.rotation, PlayerHeight * 0.5f + 0.15f, GroundLayer.value);
    }

    void ManageStates()
    {
        bool sprinting = Input.GetKey(SprintKey);

        moveSpeed = sprinting ? SprintSpeed : WalkSpeed;

        if (isGrounded)
        {
            if (rb.linearVelocity.magnitude > 0.5)
            {
                if (sprinting)
                {
                    state = MouvementState.Sprint;
                }
                else
                {
                    state = MouvementState.Walk;
                }
            }
            else
            {
                state = MouvementState.Idle;
            }
        }
        else
        {
            state = MouvementState.Air;
        }
        
    }

    void UpdateAnimator()
    {
        animator.SetBool("Walking", state == MouvementState.Walk);
        animator.SetBool("Running", state == MouvementState.Sprint);
        animator.SetBool("Grounded",isGrounded || OnSlope());
    }

    void SpeedControll()
    {
        if (isGrounded)
            rb.linearDamping = GroundDrag;
        else
            rb.linearDamping = 0;


        if (OnSlope() && !exitingSlope)
        {
            if (rb.linearVelocity.magnitude > moveSpeed)
                rb.linearVelocity = rb.linearVelocity.normalized * moveSpeed;
        }
        else
        {
            Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
            if (flatVel.magnitude > moveSpeed)
            {
                flatVel = Vector3.ClampMagnitude(flatVel, moveSpeed);
                rb.linearVelocity = new Vector3(flatVel.x, rb.linearVelocity.y, flatVel.z);
            }
        }

    }

    private void Update()
    {
        if (CanMove)
        {
            GetInputs();
            CheckIfGrounded();
            ManageStates();
            SpeedControll();
            CheckForJumping();
            UpdateAnimator();
        } 
    }


    void MovePlayer()
    {
        
        Vector3 dir = Orientation.right * horiztonalInput + Orientation.forward * verticalInput;
        

        if (OnSlope() && !exitingSlope)
        {
            rb.AddForce(GetSlopeMoveDirection(dir.normalized) * moveSpeed);

            if (rb.linearVelocity.y > 0)
                rb.AddForce(Vector3.down * 30f);
            else if (rb.linearVelocity.y < 0)
                rb.AddForce(Vector3.down * 50f);
        }
        else
        {
            float mul = isGrounded ? 1 : AirMultiplier;
            rb.AddForce(dir.normalized * moveSpeed * mul);


            if (rb.linearVelocity.y < 0)
                rb.AddForce(Vector3.down * IncreassedGravity);
            else
                rb.AddForce(Vector3.down * ConstantDownGravity);

        }

        rb.useGravity = !OnSlope();
    }

    private void FixedUpdate()
    {
        if (CanMove)
        {
            MovePlayer();
        }
    }

    void CheckForJumping()
    {
        if (isGrounded && canJump && Input.GetKeyDown(JumpKey))
        {
            StartCoroutine(Jump());
        }
    }

    IEnumerator Jump()
    {
        canJump = false;
        exitingSlope = true;

        animator.SetTrigger("Jump");

        yield return new WaitForSeconds(JumpDelay);

        rb.linearVelocity = new Vector3(rb.linearVelocity.x,0,rb.linearVelocity.z);
        rb.AddForce(Vector3.up * JumpForce, ForceMode.Impulse);

        yield return new WaitForSeconds(JumpCooldown);

        canJump = true;
        exitingSlope = false;
    }

    public bool OnSlope()
    {
        if(Physics.BoxCast(transform.position, CheckBoxSize, Vector3.down,out SlopeHit, transform.rotation, PlayerHeight * 0.5f + 0.2f, GroundLayer.value))
        {
            float angle =  Vector3.Angle(Vector3.up,SlopeHit.normal);
            return angle > 4.0f && angle < MaxSlopeAngle;
        }
        return false;
    }

    public Vector3 GetSlopeMoveDirection(Vector3 moveDir)
    {
        return Vector3.ProjectOnPlane(moveDir, SlopeHit.normal);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position + Vector3.down * (PlayerHeight * 0.5f + 0.15f), CheckBoxSize);
    }
}
