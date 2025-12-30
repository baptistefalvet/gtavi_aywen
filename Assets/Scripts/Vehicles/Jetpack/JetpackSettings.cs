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

        #region Phase 2 - Attitude Control Settings

        [Header("Attitude Limits")]
        [Tooltip("Maximum pitch angle in degrees (nose up/down)")]
        [SerializeField, Range(10f, 45f)] private float m_MaxPitchDegrees = 30f;

        [Tooltip("Maximum roll angle in degrees (bank left/right)")]
        [SerializeField, Range(10f, 45f)] private float m_MaxRollDegrees = 25f;

        [Header("Yaw Control")]
        [Tooltip("Yaw rotation rate in degrees per second")]
        [SerializeField] private float m_YawRate = 90f;

        [Tooltip("Yaw angular velocity damping")]
        [SerializeField] private float m_YawDamping = 3f;

        [Header("Angular Stabilization (PD Gains)")]
        [Tooltip("Proportional gain - higher = faster response, may oscillate")]
        [SerializeField] private float m_StabilizationP = 10f;

        [Tooltip("Derivative gain - higher = more damping, prevents overshoot")]
        [SerializeField] private float m_StabilizationD = 5f;

        [Header("Auto-Level")]
        [Tooltip("Speed at which the jetpack returns to level when no input (0 = disabled)")]
        [SerializeField, Range(0f, 10f)] private float m_AutoLevelSpeed = 3f;

        [Tooltip("Deadzone angle below which auto-level stops (prevents jitter)")]
        [SerializeField, Range(0.1f, 5f)] private float m_AutoLevelDeadzone = 1f;

        [Header("Mouse Control")]
        [Tooltip("Mouse pitch sensitivity (degrees per mouse delta unit)")]
        [SerializeField] private float m_MousePitchSensitivity = 50f;

        #endregion

        #region Public Properties

        public float ThrustPower => m_ThrustPower;
        public float VerticalDamping => m_VerticalDamping;
        public float GravityCompensationFactor => m_GravityCompensationFactor;

        public float MaxPitchDegrees => m_MaxPitchDegrees;
        public float MaxRollDegrees => m_MaxRollDegrees;
        public float YawRate => m_YawRate;
        public float YawDamping => m_YawDamping;
        public float StabilizationP => m_StabilizationP;
        public float StabilizationD => m_StabilizationD;
        public float AutoLevelSpeed => m_AutoLevelSpeed;
        public float AutoLevelDeadzone => m_AutoLevelDeadzone;
        public float MousePitchSensitivity => m_MousePitchSensitivity;

        #endregion
    }
}
