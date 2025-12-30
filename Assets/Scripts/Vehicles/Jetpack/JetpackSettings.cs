using UnityEngine;

namespace GD3.GtaviAywen
{
    /// <summary>
    /// ScriptableObject containing all tunable jetpack parameters.
    /// Create asset via: Assets > Create > Vehicles > JetpackSettings
    /// </summary>
    [CreateAssetMenu(fileName = "JetpackSettings", menuName = "Vehicles/JetpackSettings")]
    public class JetpackSettings : ScriptableObject
    {
        #region Phase 1 - Thrust Settings

        [Header("Thrust")]
        [Tooltip("Base thrust power in m/s^2")]
        [SerializeField] private float m_ThrustPower = 15f;

        [Tooltip("Vertical movement damping")]
        [SerializeField] private float m_VerticalDamping = 2f;

        [Tooltip("Multiplier for gravity compensation (1.0 = full compensation)")]
        [SerializeField, Range(0.5f, 1.5f)] private float m_GravityCompensationFactor = 1.0f;

        #endregion

        #region Public Properties

        public float ThrustPower => m_ThrustPower;
        public float VerticalDamping => m_VerticalDamping;
        public float GravityCompensationFactor => m_GravityCompensationFactor;

        #endregion
    }
}
