using TMPro;
using UnityEngine;

public class PlayerWeaponController : MonoBehaviour
{
    [Header("Componants")]
    [SerializeField]
    Animator playerAnimator;
    [SerializeField]
    Transform PlayerModel;
    [SerializeField]
    ThirdPersonCam playerCamera;
    PlayerController playerController;
    PlayerAimIK playerAimIk;
    PlayerAim playerAim;
    Rigidbody playerRb;

    [Header("Equipement Settings")]
    [SerializeField]
    KeyCode EquipeKey;
    [SerializeField]
    WeaponController[] Weapons;
    [SerializeField]
    Vector3[] WeaponsScales;
    [SerializeField]
    Vector3 WeponSpawnSpeed;

    [Header("Ammo Settings")]
    [SerializeField]
    KeyCode ReloadKey;
    [SerializeField]
    CanvasGroup AmmoGroup;
    [SerializeField]
    TextMeshProUGUI AmmoCount;
    [SerializeField]
    TextMeshProUGUI ShadowAmmoCount;

    [Header("Shooting Panic")]
    [SerializeField]
    float PanicRange;
    [SerializeField]
    LayerMask NpcLayer;

    WeaponController ActiveWeapon;

    [HideInInspector]
    public bool isWeaponEquiped;

    int activeWeaponIndex;

    private void Awake()
    {
        playerController = GetComponent<PlayerController>();
        playerAimIk = GetComponent<PlayerAimIK>();
        playerAim = GetComponent<PlayerAim>();
        playerRb = GetComponent<Rigidbody>();
    }

    public void EquipeWeapon(WeaponController weapon)
    {
        playerAimIk.ResetWeights();
        playerAimIk.EquipeWeapon(weapon);
        playerAnimator.SetBool("CloseHands", true);
        ActiveWeapon = weapon;
        isWeaponEquiped = true;
    }

    public void UnequipeWeapon()
    {
        playerAimIk.UnequipeWeapon();
        playerAnimator.SetBool("CloseHands", false);
        ActiveWeapon = null;
        isWeaponEquiped = false;
    }

    void SetClosedHands()
    {
        playerAnimator.SetBool("CloseHands", isWeaponEquiped || playerController.state == MouvementState.Walk);
    }

    private void Update()
    {
        DetectEquipement();
        UpdateCurrentWeapon();
        SetClosedHands();
        UpdateWeaponAbility();
        AmmoDisplay();
    }

    void DetectEquipement()
    {
        if (Input.GetKeyDown(EquipeKey))
        {
            activeWeaponIndex += 1;
            activeWeaponIndex = activeWeaponIndex % (Weapons.Length + 1);

            if (activeWeaponIndex > 0)
            {
                EquipeWeapon(Weapons[activeWeaponIndex - 1]);
            }
            else
            {
                UnequipeWeapon();
            }
        }
    }

    private void UpdateCurrentWeapon()
    {
        for (int i = 0; i < Weapons.Length; i++)
        {
            WeaponController weapon = Weapons[i];
            if (isWeaponEquiped && weapon == ActiveWeapon)
            {
                weapon.transform.localScale = new Vector3(Mathf.Lerp(weapon.transform.localScale.x, WeaponsScales[i].x, Time.deltaTime * WeponSpawnSpeed.x), weapon.transform.localScale.y, weapon.transform.localScale.z);
                weapon.transform.localScale = new Vector3(weapon.transform.localScale.x, Mathf.Lerp(weapon.transform.localScale.y, WeaponsScales[i].y, Time.deltaTime * WeponSpawnSpeed.y), weapon.transform.localScale.z);
                weapon.transform.localScale = new Vector3(weapon.transform.localScale.x, weapon.transform.localScale.y, Mathf.Lerp(weapon.transform.localScale.z, WeaponsScales[i].z, Time.deltaTime * WeponSpawnSpeed.z));
            }
            else
            {
                weapon.transform.localScale = Vector3.zero;
            }
        }

    }

    private void UpdateWeaponAbility()
    {
        if (isWeaponEquiped)
        {
            if (ActiveWeapon != null)
            {
                if (ActiveWeapon.TryGetComponent<GunController>(out GunController gun))
                {
                    bool shootAuto = gun.GunData.isAutomatic && Input.GetMouseButton(0);
                    bool shootManual = Input.GetMouseButtonDown(0);

                    if (gun.CanShoot && (shootAuto || shootManual) && gun.buletsLeft > 0 && !gun.reloading)
                    {
                        gun.buletsShot = 0;

                        Vector3 shootPoint = playerAim.IsAiming ? playerAim.AimPoint.position : (transform.position + (-PlayerModel.right) * 20.0f);

                        Vector3 recoilDir = -(shootPoint - transform.position);

                        playerRb.AddForce(recoilDir.normalized * gun.GunData.recoil, ForceMode.Impulse);

                        gun.Shoot(shootPoint);
                        gun.ShootEffects();
                        AmmoDisplayShootEffect();

                        playerCamera.CameraShake(gun.GunData.camShake.Amplitude, gun.GunData.camShake.Frequency);


                        Collider[] npcs = Physics.OverlapSphere(transform.position, PanicRange, NpcLayer);

                        foreach (Collider c in npcs)
                        {
                            if(c.gameObject.TryGetComponent<NpcController>(out NpcController npc))
                            {
                                npc.SetPanic();
                            }
                        }

                    }

                    bool reload = Input.GetKeyDown(ReloadKey);

                    if (reload && gun.buletsLeft < gun.GunData.MagazinSize && !gun.reloading)
                    {
                        gun.Reload();
                    }

                    if (gun.CanShoot && (shootAuto || shootManual) && gun.buletsLeft <= 0 && !gun.reloading)
                    {
                        gun.Reload();
                    }
                }
            }
        }
    }


    void AmmoDisplay()
    {
        if (isWeaponEquiped && ActiveWeapon != null)
        {
            if (ActiveWeapon.TryGetComponent<GunController>(out GunController gun))
            {
                AmmoCount.text = gun.buletsLeft + "/" + gun.GunData.MagazinSize;
                ShadowAmmoCount.text = gun.buletsLeft + "/" + gun.GunData.MagazinSize;
            }
            else
            {
                AmmoCount.text = "1";
                ShadowAmmoCount.text = "1";
            }

            AmmoGroup.alpha = Mathf.Lerp(AmmoGroup.alpha, 1.0f, Time.deltaTime * 5.0f);
        }
        else
        {
            AmmoGroup.alpha = Mathf.Lerp(AmmoGroup.alpha, 0.0f, Time.deltaTime * 5.0f);
        }

        AmmoGroup.GetComponent<RectTransform>().rotation =Quaternion.Slerp(AmmoGroup.GetComponent<RectTransform>().rotation, Quaternion.identity, Time.deltaTime * 4.0f);
        AmmoGroup.GetComponent<RectTransform>().localScale = Vector3.Slerp(AmmoGroup.GetComponent<RectTransform>().localScale, Vector3.one, Time.deltaTime * 4.0f);
    }

    void AmmoDisplayShootEffect()
    {
        AmmoGroup.GetComponent<RectTransform>().rotation = Quaternion.Euler(0, 0, Random.Range(-25f, 25f));
        AmmoGroup.GetComponent<RectTransform>().localScale = new Vector3(Random.Range(0.5f, 2.0f), Random.Range(0.5f, 2.0f), Random.Range(0.5f, 2.0f));
    }
}
