using UnityEngine;

[System.Serializable]
public class CamShake
{
    public float Amplitude;
    public float Frequency;
}

[CreateAssetMenu(fileName = "GunObject", menuName = "Scriptable Objects/GunObject")]
public class GunObject : ScriptableObject
{
    [Header("Shooting")]
    public bool isAutomatic;
    public float cooldown;
    public float spread;
    public float recoil;
    public CamShake camShake;

    [Header("Atributes")]
    public float Damages;
    public float Knockback;

    [Header("Bulets Settings")]
    public float ShootForce;
    public float UpwardForce;

    [Header("Reload Settings")]
    public int MagazinSize;
    public float ReloadTime;
}
