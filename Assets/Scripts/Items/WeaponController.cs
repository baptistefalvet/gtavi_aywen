using UnityEngine;

public class WeaponController : MonoBehaviour
{
    [Header("Componant")]
    public Transform LeftHandWeaponPos;
    public Transform RightHandWeaponPos;
    [Range(0f, 1f)]
    public float LeftHandWeight;
    [Range(0f, 1f)]
    public float RightHandWeight;
}
