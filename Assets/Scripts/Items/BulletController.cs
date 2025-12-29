using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;
using UnityEngine.UI;

public class BulletController : MonoBehaviour
{
    [Header("Bullet Settings")]
    [SerializeField]
    float lifeTime;
    [SerializeField]
    LayerMask HitLayer;
    [SerializeField]
    GameObject HitFlashEffect;

    [HideInInspector]
    public GunObject data;

    Rigidbody rb;
    float elapsedTime;
    bool isDestroyed;

    Vector3 dir;

    public void ShootBullet(Vector3 shootDir, Vector3 upDir, float shootForce,float upForce)
    {
        dir = shootDir;
        isDestroyed = false;
        elapsedTime = 0.0f;

        rb = GetComponent<Rigidbody>();

        rb.AddForce(shootDir * shootForce,ForceMode.Impulse);
        rb.AddForce(upDir * upForce, ForceMode.Impulse);
    }

    public static bool IsInLayerMask(GameObject obj, LayerMask mask) => (mask.value & (1 << obj.layer)) != 0;

    private void OnCollisionEnter(Collision collision)
    {
        if (IsInLayerMask(collision.gameObject,HitLayer) && !isDestroyed)
        {
            Debug.Log(collision.gameObject.tag);

            if(collision.gameObject.tag == "NPC")
            {
                collision.transform.root.GetComponent<NpcController>().HitNpc(dir, data.Damages, data.Knockback);
            }

            DestroyBullet();
        }
    }

    private void Update()
    {
        elapsedTime += Time.deltaTime;
        if (elapsedTime >= lifeTime && !isDestroyed)
        {
            DestroyBullet();
        }
    }

    void DestroyBullet()
    {
        isDestroyed = true;
        rb.isKinematic = true;

        GameObject hit = Instantiate(HitFlashEffect,transform.position,Quaternion.identity);

        Destroy(gameObject, 0.1f);
        Destroy(hit, 4.0f);
    }
}
