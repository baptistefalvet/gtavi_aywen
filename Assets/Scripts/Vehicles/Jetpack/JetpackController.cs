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
        }

        private void OnGUI()
        {
            if (!m_EnableDebugLogs) return;
            if (m_Rigidbody == null) return;

            float verticalSpeed = m_Rigidbody.linearVelocity.y;
            float horizontalSpeed = new Vector3(m_Rigidbody.linearVelocity.x, 0, m_Rigidbody.linearVelocity.z).magnitude;

            GUILayout.BeginArea(new Rect(10, 10, 300, 150));
            GUILayout.Label("JETPACK DEBUG");
            GUILayout.Label($"Vertical Speed: {verticalSpeed:F2} m/s");
            GUILayout.Label($"Horizontal Speed: {horizontalSpeed:F2} m/s");
            GUILayout.Label($"Current Thrust: {m_CurrentThrust:F2} m/s2");
            GUILayout.Label($"Altitude: {transform.position.y:F2} m");
            GUILayout.EndArea();
        }

        #endregion
    }
}
