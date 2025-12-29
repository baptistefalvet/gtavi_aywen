using UnityEngine;

public class GunController : WeaponController
{
    public Transform GunTip;

    [Header("Shooting")]
    public GunObject GunData;
    [SerializeField]
    GameObject BulletPrefab;
    [SerializeField]
    GameObject[] FlashsPrefabs;
    [SerializeField]
    float WiggleMaxAngle;
    [SerializeField]
    float ReloadMaxAngle;
    [HideInInspector]
    public bool CanShoot = true;

    Vector3 defaultRot;
    Vector3 reloadRot;

    [HideInInspector]
    public int buletsLeft;
    [HideInInspector]
    public int buletsShot;

    [HideInInspector]
    public bool reloading = false;
    bool allowInvoke = true;

    private void Awake()
    {
        CanShoot = true;
        buletsLeft = GunData.MagazinSize;
        defaultRot = transform.localEulerAngles;
    }

    public void ShootEffects()
    {
        foreach (var f in FlashsPrefabs)
        {
            GameObject particle = Instantiate(f,GunTip.position,GunTip.rotation, transform);
            particle.GetComponent<ParticleSystem>().Play();
            Destroy(particle,3.0f);
        }
    }

    void GunWiggle()
    {
        float zRot = Random.Range(0.5f, 1.0f) * WiggleMaxAngle;
        transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, transform.localEulerAngles.y, zRot);
    }

    public void Shoot(Vector3 point)
    {
        CanShoot = false;
        GunWiggle();

        buletsShot += 1;
        buletsLeft -= 1;

        Vector3 dir = point - GunTip.position;

        Vector2 rand = Random.insideUnitCircle * GunData.spread;

        dir += new Vector3(rand.x, rand.y, 0);

        GameObject bullet = Instantiate(BulletPrefab, GunTip.position, Quaternion.identity);
        bullet.transform.forward = dir.normalized;
        bullet.GetComponent<BulletController>().data = GunData;
        bullet.GetComponent<BulletController>().ShootBullet(dir.normalized, transform.up,GunData.ShootForce,GunData.UpwardForce);

        if(allowInvoke)
        {
            Invoke("ResetCanShoot", GunData.cooldown);
            allowInvoke = false;
        }
        
    }

    void ResetCanShoot()
    {
        allowInvoke = true;
        CanShoot = true;
    }

    public void Reload()
    {
        reloading = true;
        SelectRandomReloadRot();
        Invoke("FinishReloading", GunData.ReloadTime);
    }

    void SelectRandomReloadRot()
    {
        float xRot = Random.Range(0.15f, 1.0f) * ReloadMaxAngle;
        float yRot = -Random.Range(0.25f, 0.7f) * ReloadMaxAngle;
        float zRot = -Random.Range(-0.25f, 0.25f) * ReloadMaxAngle;
        reloadRot = new Vector3(xRot, yRot, zRot);
    }

    void FinishReloading()
    {
        buletsLeft = GunData.MagazinSize;
        reloading = false;
    }

    void RotataeGun()
    {
        if (reloading)
        {
            if(Mathf.Floor(Time.time * 10.0f) % 5 == 0)
            SelectRandomReloadRot();
            transform.localRotation = Quaternion.Slerp(transform.localRotation, Quaternion.Euler(reloadRot), Time.deltaTime * 13.0f);
        }
        else
        {
            transform.localRotation = Quaternion.Slerp(transform.localRotation, Quaternion.Euler(defaultRot), Time.deltaTime * 8.0f);
        }
            
    }

    private void Update()
    {
        RotataeGun();
    }
}
