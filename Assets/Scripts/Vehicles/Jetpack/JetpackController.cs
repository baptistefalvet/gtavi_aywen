using UnityEngine;

namespace GD3.GtaviAywen
{
    /// <summary>
    /// Main jetpack controller handling physics-based flight.
    /// Requires Rigidbody component. Uses ScriptableObject for settings.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class JetpackController : MonoBehaviour
    {
        #region References

        [Header("Settings")]
        [SerializeField] private JetpackSettings m_Settings;

        [Header("Input")]
        [SerializeField] private JetpackInputHandler m_InputHandler;

        #endregion

        #region Cached Components

        private Rigidbody m_Rigidbody;

        #endregion

        #region Debug Settings

        [Header("Debug")]
        [SerializeField] private bool m_EnableDebugLogs = true;
        [SerializeField] private bool m_EnableGizmos = true;

        #endregion

        #region State

        private float m_CurrentThrust;
        private bool m_IsActive;

        private float m_TargetPitch;
        private float m_TargetRoll;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            CacheComponents();
            ValidateReferences();
            ConfigureRigidbody();
        }

        private void OnEnable()
        {
            m_IsActive = true;
            LogDebug("Jetpack activated");
        }

        private void OnDisable()
        {
            m_IsActive = false;
            LogDebug("Jetpack deactivated");
        }

        private void FixedUpdate()
        {
            if (!m_IsActive || m_Settings == null) return;

            ApplyThrust();
            ApplyVerticalDamping();
            UpdateTargetAttitude();
            ApplyAttitudeControl();
            ApplyYawControl();
        }

        #endregion

        #region Initialization

        private void CacheComponents()
        {
            m_Rigidbody = GetComponent<Rigidbody>();
        }

        private void ValidateReferences()
        {
            if (m_Settings == null)
            {
                Debug.LogError("[JetpackController] JetpackSettings not assigned!");
            }
            if (m_InputHandler == null)
            {
                Debug.LogError("[JetpackController] JetpackInputHandler not assigned!");
            }
        }

        private void ConfigureRigidbody()
        {
            m_Rigidbody.useGravity = true;
            m_Rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
            m_Rigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
            m_Rigidbody.constraints = RigidbodyConstraints.None;

            LogDebug($"Rigidbody configured: mass={m_Rigidbody.mass}, drag={m_Rigidbody.linearDamping}");
        }

        #endregion

        #region Phase 1 - Thrust System

        private void ApplyThrust()
        {
            float collectiveInput = m_InputHandler != null ? m_InputHandler.CollectiveInput : 0f;

            float targetThrust = collectiveInput * m_Settings.ThrustPower;

            float gravityCompensation = CalculateGravityCompensation(collectiveInput);

            m_CurrentThrust = targetThrust + gravityCompensation;

            Vector3 thrustForce = transform.up * m_CurrentThrust;
            m_Rigidbody.AddForce(thrustForce, ForceMode.Acceleration);

            LogDebug($"Thrust: input={collectiveInput:F2}, thrust={targetThrust:F2}, " +
                     $"gravComp={gravityCompensation:F2}, total={m_CurrentThrust:F2}");
        }

        private float CalculateGravityCompensation(float collectiveInput)
        {
            float gravityMagnitude = Physics.gravity.magnitude;
            float compensationFactor = m_Settings.GravityCompensationFactor;

            if (collectiveInput >= 0f)
            {
                return gravityMagnitude * compensationFactor;
            }
            else
            {
                float descendFactor = 1f + collectiveInput;
                return gravityMagnitude * compensationFactor * descendFactor;
            }
        }

        private void ApplyVerticalDamping()
        {
            float verticalVelocity = m_Rigidbody.linearVelocity.y;
            float dampingForce = -verticalVelocity * m_Settings.VerticalDamping;

            m_Rigidbody.AddForce(Vector3.up * dampingForce, ForceMode.Acceleration);
        }

        #endregion

        #region Phase 2 - Attitude Control

        /// <summary>
        /// Updates target pitch and roll based on player input.
        /// Handles mouse delta accumulation and keyboard held inputs differently.
        /// </summary>
        private void UpdateTargetAttitude()
        {
            float pitchInput = m_InputHandler != null ? m_InputHandler.PitchInput : 0f;
            float rollInput = m_InputHandler != null ? m_InputHandler.RollInput : 0f;

            // TODO: Re-enable mouse pitch control later
            // Pitch control temporarily disabled
            /*
            if (Mathf.Abs(pitchInput) > 0.01f)
            {
                if (Mathf.Abs(pitchInput) < 0.5f)
                {
                    m_TargetPitch -= pitchInput * m_Settings.MousePitchSensitivity;
                }
                else
                {
                    m_TargetPitch = -pitchInput * m_Settings.MaxPitchDegrees;
                }
            }
            else if (m_Settings.AutoLevelSpeed > 0f)
            {
                m_TargetPitch = Mathf.MoveTowards(m_TargetPitch, 0f, m_Settings.AutoLevelSpeed * Time.fixedDeltaTime);
                if (Mathf.Abs(m_TargetPitch) < m_Settings.AutoLevelDeadzone)
                {
                    m_TargetPitch = 0f;
                }
            }
            */

            // Auto-level pitch when no input
            if (m_Settings.AutoLevelSpeed > 0f)
            {
                m_TargetPitch = Mathf.MoveTowards(m_TargetPitch, 0f, m_Settings.AutoLevelSpeed * Time.fixedDeltaTime);
                if (Mathf.Abs(m_TargetPitch) < m_Settings.AutoLevelDeadzone)
                {
                    m_TargetPitch = 0f;
                }
            }

            if (Mathf.Abs(rollInput) > 0.01f)
            {
                m_TargetRoll = -rollInput * m_Settings.MaxRollDegrees;
            }
            else if (m_Settings.AutoLevelSpeed > 0f)
            {
                m_TargetRoll = Mathf.MoveTowards(m_TargetRoll, 0f, m_Settings.AutoLevelSpeed * Time.fixedDeltaTime);
                if (Mathf.Abs(m_TargetRoll) < m_Settings.AutoLevelDeadzone)
                {
                    m_TargetRoll = 0f;
                }
            }

            m_TargetPitch = Mathf.Clamp(m_TargetPitch, -m_Settings.MaxPitchDegrees, m_Settings.MaxPitchDegrees);
            m_TargetRoll = Mathf.Clamp(m_TargetRoll, -m_Settings.MaxRollDegrees, m_Settings.MaxRollDegrees);
        }

        /// <summary>
        /// Applies PD controller torques to achieve target pitch and roll.
        /// Uses local-space torques for predictable behavior.
        /// </summary>
        private void ApplyAttitudeControl()
        {
            Vector3 currentEuler = transform.localEulerAngles;
            float currentPitch = NormalizeAngle(currentEuler.x);
            float currentRoll = NormalizeAngle(currentEuler.z);

            Vector3 localAngularVelocity = transform.InverseTransformDirection(m_Rigidbody.angularVelocity);

            float pitchError = m_TargetPitch - currentPitch;
            float pitchTorque = pitchError * m_Settings.StabilizationP - localAngularVelocity.x * m_Settings.StabilizationD;

            float rollError = m_TargetRoll - currentRoll;
            float rollTorque = rollError * m_Settings.StabilizationP - localAngularVelocity.z * m_Settings.StabilizationD;

            m_Rigidbody.AddRelativeTorque(pitchTorque, 0f, rollTorque, ForceMode.Acceleration);
        }

        /// <summary>
        /// Applies yaw control (heading rotation).
        /// Uses rate-based control with damping.
        /// </summary>
        private void ApplyYawControl()
        {
            float yawInput = m_InputHandler != null ? m_InputHandler.YawInput : 0f;

            Vector3 localAngularVelocity = transform.InverseTransformDirection(m_Rigidbody.angularVelocity);
            float currentYawRate = localAngularVelocity.y * Mathf.Rad2Deg;

            float desiredYawRate = yawInput * m_Settings.YawRate;
            float yawTorque;

            if (Mathf.Abs(yawInput) > 0.01f)
            {
                float yawError = desiredYawRate - currentYawRate;
                yawTorque = yawError * m_Settings.StabilizationP * 0.5f;
            }
            else
            {
                yawTorque = -currentYawRate * m_Settings.YawDamping;
            }

            m_Rigidbody.AddRelativeTorque(0f, yawTorque * Mathf.Deg2Rad, 0f, ForceMode.Acceleration);
        }

        /// <summary>
        /// Normalizes an angle to the range [-180, 180] degrees.
        /// </summary>
        private float NormalizeAngle(float angle)
        {
            while (angle > 180f) angle -= 360f;
            while (angle < -180f) angle += 360f;
            return angle;
        }

        #endregion

        #region Debug

        private void LogDebug(string message)
        {
            if (m_EnableDebugLogs)
            {
                Debug.Log($"[JetpackController] {message}");
            }
        }

        private void OnDrawGizmos()
        {
            if (!m_EnableGizmos) return;
            if (m_Rigidbody == null) return;

            Gizmos.color = Color.red;
            Gizmos.DrawRay(transform.position, transform.up * 3f);

            Gizmos.color = Color.blue;
            Gizmos.DrawRay(transform.position, m_Rigidbody.linearVelocity * 0.5f);

            float verticalSpeed = m_Rigidbody.linearVelocity.y;
            Gizmos.color = verticalSpeed > 0 ? Color.green : Color.yellow;
            Gizmos.DrawRay(transform.position, Vector3.up * verticalSpeed * 0.5f);

            Gizmos.color = Color.cyan;
            Gizmos.DrawRay(transform.position, transform.forward * 2f);

            Gizmos.color = Color.magenta;
            Quaternion targetRotation = Quaternion.Euler(m_TargetPitch, transform.eulerAngles.y, m_TargetRoll);
            Gizmos.DrawRay(transform.position, targetRotation * Vector3.up * 2f);
        }

        private void OnGUI()
        {
            if (!m_EnableDebugLogs) return;
            if (m_Rigidbody == null) return;

            float verticalSpeed = m_Rigidbody.linearVelocity.y;
            float horizontalSpeed = new Vector3(m_Rigidbody.linearVelocity.x, 0, m_Rigidbody.linearVelocity.z).magnitude;

            float currentPitch = NormalizeAngle(transform.localEulerAngles.x);
            float currentRoll = NormalizeAngle(transform.localEulerAngles.z);
            float currentYaw = transform.localEulerAngles.y;

            GUILayout.BeginArea(new Rect(10, 10, 300, 220));
            GUILayout.Label("JETPACK DEBUG");
            GUILayout.Label($"Vertical Speed: {verticalSpeed:F2} m/s");
            GUILayout.Label($"Horizontal Speed: {horizontalSpeed:F2} m/s");
            GUILayout.Label($"Current Thrust: {m_CurrentThrust:F2} m/s2");
            GUILayout.Label($"Altitude: {transform.position.y:F2} m");
            GUILayout.Label("--- ATTITUDE ---");
            GUILayout.Label($"Pitch: {currentPitch:F1} (target: {m_TargetPitch:F1})");
            GUILayout.Label($"Roll: {currentRoll:F1} (target: {m_TargetRoll:F1})");
            GUILayout.Label($"Yaw: {currentYaw:F1}");
            GUILayout.EndArea();
        }

        #endregion
    }
}
