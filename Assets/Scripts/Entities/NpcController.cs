using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

[System.Serializable]
public class NpcCloth
{
    public float minSaturation;
    public float maxSaturation;
    public float minBightness;
    public float maxBightness;
}

public class NpcController : MonoBehaviour
{
    [Header("Componants")]
    [SerializeField]
    Transform ZoneCenter;
    [SerializeField]
    float ZoneCenterRadius;
    [SerializeField]
    SkinnedMeshRenderer meshRenderer;
    [SerializeField]
    Animator animator;
    NpcRagdoll ragdoll;
    NavMeshAgent agent;


    [Header("Npc Randomization")]
    [SerializeField]
    NpcCloth ShirtCloth;
    [SerializeField]
    NpcCloth PantsCloth;
    [SerializeField]
    NpcCloth ShoesCloth;

    Material shirtMat, pantsMat,shoesMat;

    [Header("Npc Deplacement")]
    [SerializeField]
    LayerMask GroundLayer;
    [SerializeField]
    Transform Player;
    [SerializeField]
    float PatrolingSpeed;
    [SerializeField]
    float PatrolingRange;
    [SerializeField,Range(-1f,1f)]
    float PatrolingMaxAngle;
    [SerializeField]
    float PanicSpeed;
    [SerializeField]
    float PanicTime;
    [SerializeField]
    float MinDisToPlayerPanic;


    Vector3 walkPoint;
    bool walkPointSet;

    Vector3 panicPoint;
    bool panicPointSet;

    bool isPanic = false;


    [Header("Npc Health")]
    [SerializeField]
    float NpcMaxHealth;
    [SerializeField]
    float sleepMinTime;
    [SerializeField]
    float sleepMaxTime;
    [SerializeField]
    float DeathRagdollForceMul;
    [SerializeField]
    float RagdollHitForce;

    [Header("Npc Auto Ragdoll")]
    [SerializeField]
    float VelocityTreshold;
    [SerializeField]
    float MassTreshold;


    float health;
    bool isDead = false;

    Coroutine wakeUpCoroutine;
    Vector3 PanicStartPos;

    float seachTime = 0.0f;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        ragdoll = GetComponent<NpcRagdoll>();
        health = NpcMaxHealth;
    }


    public void SetPanic()
    {
        isPanic = true;
        PanicStartPos = transform.position;
        Invoke("ResetPanic", PanicTime + Random.Range(0.0f, 1.5f));
    }

    void ResetPanic()
    {
        isPanic = false;
    }

    void RandomizeNPC()
    {
        float size = Random.Range(0f,1f);
        size = Mathf.Pow(size, 4.0f) * 70.0f;

        Color col = Random.ColorHSV(0f,1f,1.0f,1.0f,1.0f,1.0f);

        float h = 0.0f;
        float s = 0.0f;
        float v = 0.0f;

        Color.RGBToHSV(col,out h,out s,out v);

        float shirtS = s *Random.Range(ShirtCloth.minSaturation, ShirtCloth.maxSaturation);
        float shirtV = v *Random.Range(ShirtCloth.minBightness, ShirtCloth.maxBightness);

        float pantsS = s * Random.Range(PantsCloth.minSaturation, PantsCloth.maxSaturation);
        float pantsV = v * Random.Range(PantsCloth.minBightness, PantsCloth.maxBightness);

        float shoesS = s * Random.Range(ShoesCloth.minSaturation, ShoesCloth.maxSaturation);
        float shoesV = v * Random.Range(ShoesCloth.minBightness, ShoesCloth.maxBightness);

        Color shirtCol = Color.HSVToRGB(h, shirtS, shirtV);
        Color pantsCol = Color.HSVToRGB(h, pantsS, pantsV);
        Color shoesCol = Color.HSVToRGB(h, shoesS, shoesV);

        Material[] mats = meshRenderer.materials;

        shirtMat = Instantiate(mats[0]);
        shirtMat.color = shirtCol;

        pantsMat = Instantiate(mats[1]);
        pantsMat.color = pantsCol;

        shoesMat = Instantiate(mats[2]);
        shoesMat.color = shoesCol;

        mats[0] = shirtMat;
        mats[1] = pantsMat;
        mats[2] = shoesMat;

        meshRenderer.SetMaterials(mats.ToList());
        

        meshRenderer.SetBlendShapeWeight(0, size);
        meshRenderer.SetBlendShapeWeight(1, size);
    }

    private void Start()
    {
        RandomizeNPC();
    }

    private void Update()
    {
        if (!ragdoll.IsRagdoll && agent.enabled)
        {
            if (!isPanic)
            {
                agent.speed = PatrolingSpeed;
                Patroling();
            }
            else
            {
                agent.speed = PanicSpeed;
                Panicking();
            }
        }

        animator.SetBool("Walking", !ragdoll.IsRagdoll && !isPanic);
        animator.SetBool("Running", !ragdoll.IsRagdoll && isPanic);
    }


    void Patroling()
    {

        if (!walkPointSet)
        {
            SearchWalkPoint();
            seachTime = 0.0f;
        }
        else
        {
            agent.SetDestination(walkPoint);
            seachTime += Time.deltaTime;
        }


        float disToWalkPoint = Vector3.Distance(transform.position, walkPoint);

        if (disToWalkPoint < 1 || seachTime > 5.0f)
            walkPointSet = false;

    }

    void SearchWalkPoint()
    {
        Vector2 randomOffset = Random.insideUnitCircle * PatrolingRange;

        walkPoint = new Vector3(transform.position.x + randomOffset.x, transform.position.y, transform.position.z + randomOffset.y);

        bool grounded = Physics.Raycast(walkPoint, -transform.up, 2.0f, GroundLayer);

        Vector3 dirToWalkPoint = (walkPoint - transform.position).normalized;

        bool angle = Vector3.Dot(transform.forward, dirToWalkPoint) > PatrolingMaxAngle;

        bool isInCenter = Vector3.Distance(walkPoint, ZoneCenter.position) < ZoneCenterRadius;

        if (grounded && angle && isInCenter)
            walkPointSet = true;
    }


    void Panicking()
    {
        if (!panicPointSet)
        {
            SearchPanicPoint();
        }
        else
        {
            agent.SetDestination(panicPoint);
        }


        float disToPanicPoint = Vector3.Distance(transform.position, panicPoint);

        if (disToPanicPoint < 1)
            panicPointSet = false;
    }


    void SearchPanicPoint()
    {
        Vector3 dirToPlayer = (transform.position - new Vector3(Player.position.x, transform.position.y, Player.position.z));

        Vector2 offset = Random.insideUnitCircle * 4.0f;

        panicPoint = transform.position + dirToPlayer.normalized * Random.Range(5.0f,2.0f) + new Vector3(offset.x,0, offset.y);

        bool grounded = Physics.Raycast(panicPoint, -transform.up, 2.0f, GroundLayer);

        if (grounded)
            panicPointSet = true;
    }










    public void HitNpc(Vector3 dir,float damages,float knockback)
    {
        Debug.Log(health);

        if (isDead)
            return;

        if(wakeUpCoroutine != null)
            StopCoroutine(wakeUpCoroutine);

        health -= damages;

        if(health <= 0)
        {
            health = 0;

            if (!ragdoll.IsRagdoll)
                ragdoll.EnableRagdoll(dir * knockback * DeathRagdollForceMul);
            else
                ragdoll.ApplyForceToRagdoll(dir * knockback * DeathRagdollForceMul);

            if (wakeUpCoroutine != null)
                StopCoroutine(wakeUpCoroutine);

            KillNpc();
        }
        else
        {
            if(!ragdoll.IsRagdoll)
                ragdoll.EnableRagdoll(dir * knockback);
            else
                ragdoll.ApplyForceToRagdoll(dir * knockback);

            wakeUpCoroutine = StartCoroutine(RagdollWakeUpCoroutine());
        }
    }



    IEnumerator RagdollWakeUpCoroutine()
    {
        float sleepTime = Random.Range(sleepMinTime,sleepMaxTime);

        yield return new WaitForSeconds(sleepTime);

        if (ragdoll.IsRagdoll && !isDead)
            ragdoll.DisableRagdoll();
    }


    void KillNpc()
    {
        isDead = true;
        ragdoll.FreezeParts(4.0f);
    }


    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.TryGetComponent<Rigidbody>(out Rigidbody rb) && !ragdoll.IsRagdoll)
        {
            if (rb.mass > MassTreshold && collision.relativeVelocity.magnitude > VelocityTreshold)
            {
                ragdoll.EnableRagdoll(collision.relativeVelocity * RagdollHitForce);
                rb.AddForce(collision.relativeVelocity ,ForceMode.VelocityChange);

                if (wakeUpCoroutine != null)
                    StopCoroutine(wakeUpCoroutine);

                wakeUpCoroutine = StartCoroutine(RagdollWakeUpCoroutine());
            }
        }
    }
}
