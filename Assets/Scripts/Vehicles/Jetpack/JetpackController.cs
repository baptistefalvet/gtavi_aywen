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

        [Header("Camera (Phase 4)")]
        [Tooltip("Main camera for strafe mode. If null, uses Camera.main")]
        [SerializeField] private Camera m_MainCamera;

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

        // Phase 4 - Strafe Mode
        private bool m_IsStrafeModeActive;
        private Vector3 m_TargetStrafeVelocity;
        private float m_CurrentYawVelocity;  // For SmoothDampAngle

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            CacheComponents();
            ValidateReferences();
            ConfigureRigidbody();
            CacheMainCamera();
        }

        private void OnEnable()
        {
            m_IsActive = true;

            // Reset all state variables to prevent stale values from previous activation
            m_CurrentThrust = 0f;
            m_TargetPitch = 0f;
            m_TargetRoll = 0f;
            m_IsStrafeModeActive = false;
            m_TargetStrafeVelocity = Vector3.zero;
            m_CurrentYawVelocity = 0f;

            // Reset Rigidbody velocities to prevent inherited momentum (only if not kinematic)
            if (m_Rigidbody != null && !m_Rigidbody.isKinematic)
            {
                m_Rigidbody.linearVelocity = Vector3.zero;
                m_Rigidbody.angularVelocity = Vector3.zero;
            }

            LogDebug("Jetpack activated - state reset");
        }

        private void OnDisable()
        {
            m_IsActive = false;
            LogDebug("Jetpack deactivated");
        }

        private void FixedUpdate()
        {
            if (!m_IsActive || m_Settings == null) return;

            UpdateStrafeModeState();

            ApplyThrust();
            ApplyVerticalDamping();

            if (m_IsStrafeModeActive)
            {
                ApplyStrafeModePhysics();
            }
            else
            {
                ApplyHorizontalDamping();
            }

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

        private void CacheMainCamera()
        {
            if (m_MainCamera == null)
            {
                m_MainCamera = Camera.main;
                if (m_MainCamera == null)
                {
                    Debug.LogWarning("[JetpackController] Camera not found! Strafe mode disabled.");
                }
            }
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

        private void ApplyHorizontalDamping()
        {
            // Dampen horizontal velocity to prevent drift
            Vector3 horizontalVelocity = new Vector3(m_Rigidbody.linearVelocity.x, 0f, m_Rigidbody.linearVelocity.z);
            Vector3 dampingForce = -horizontalVelocity * m_Settings.HorizontalDamping;

            m_Rigidbody.AddForce(dampingForce, ForceMode.Acceleration);
        }

        #endregion

        #region Phase 2 - Attitude Control

        /// <summary>
        /// Updates target pitch and roll based on player input.
        /// Phase 3: W/S keyboard controls pitch, A/D controls roll (Normal mode).
        /// Phase 4: Strong upright stabilization in strafe mode.
        /// </summary>
        private void UpdateTargetAttitude()
        {
            if (m_IsStrafeModeActive)
            {
                // Strafe mode: force upright with aggressive auto-level
                float autoLevelRate = m_Settings.AutoLevelSpeed * m_Settings.StrafeStabilizationMultiplier;

                m_TargetPitch = Mathf.MoveTowards(m_TargetPitch, 0f, autoLevelRate * Time.fixedDeltaTime);
                m_TargetRoll = Mathf.MoveTowards(m_TargetRoll, 0f, autoLevelRate * Time.fixedDeltaTime);

                if (Mathf.Abs(m_TargetPitch) < m_Settings.AutoLevelDeadzone)
                    m_TargetPitch = 0f;
                if (Mathf.Abs(m_TargetRoll) < m_Settings.AutoLevelDeadzone)
                    m_TargetRoll = 0f;
            }
            else
            {
                // Normal mode: existing Phase 3 behavior
                float pitchInput = m_InputHandler != null ? m_InputHandler.PitchInput : 0f;
                float rollInput = m_InputHandler != null ? m_InputHandler.RollInput : 0f;
                Vector2 moveInput = m_InputHandler != null ? m_InputHandler.MoveInput : Vector2.zero;

                // Phase 3: W/S keyboard input controls pitch target
                if (Mathf.Abs(moveInput.y) > 0.01f)
                {
                    // Negate moveInput.y: W key = -1 → negative pitch (forward tilt)
                    m_TargetPitch = -moveInput.y * m_Settings.MaxPitchDegrees;
                }
                else
                {
                    // Auto-level pitch when no W/S input
                    if (m_Settings.AutoLevelSpeed > 0f)
                    {
                        m_TargetPitch = Mathf.MoveTowards(m_TargetPitch, 0f,
                            m_Settings.AutoLevelSpeed * Time.fixedDeltaTime);
                        if (Mathf.Abs(m_TargetPitch) < m_Settings.AutoLevelDeadzone)
                        {
                            m_TargetPitch = 0f;
                        }
                    }
                }

                // Roll control from A/D keys
                if (Mathf.Abs(rollInput) > 0.01f)
                {
                    m_TargetRoll = -rollInput * m_Settings.MaxRollDegrees;
                }
                else if (m_Settings.AutoLevelSpeed > 0f)
                {
                    m_TargetRoll = Mathf.MoveTowards(m_TargetRoll, 0f,
                        m_Settings.AutoLevelSpeed * Time.fixedDeltaTime);
                    if (Mathf.Abs(m_TargetRoll) < m_Settings.AutoLevelDeadzone)
                    {
                        m_TargetRoll = 0f;
                    }
                }

                m_TargetPitch = Mathf.Clamp(m_TargetPitch, -m_Settings.MaxPitchDegrees, m_Settings.MaxPitchDegrees);
                m_TargetRoll = Mathf.Clamp(m_TargetRoll, -m_Settings.MaxRollDegrees, m_Settings.MaxRollDegrees);
            }
        }

        /// <summary>
        /// Applies PD controller torques to achieve target pitch and roll.
        /// Uses local-space torques for predictable behavior.
        /// Phase 4: Increased stabilization gains in strafe mode.
        /// </summary>
        private void ApplyAttitudeControl()
        {
            Vector3 currentEuler = transform.localEulerAngles;
            float currentPitch = NormalizeAngle(currentEuler.x);
            float currentRoll = NormalizeAngle(currentEuler.z);

            Vector3 localAngularVelocity = transform.InverseTransformDirection(m_Rigidbody.angularVelocity);

            // Phase 4: Apply stronger gains in strafe mode
            float pGain = m_Settings.StabilizationP;
            float dGain = m_Settings.StabilizationD;

            if (m_IsStrafeModeActive)
            {
                pGain *= m_Settings.StrafeStabilizationMultiplier;
                dGain *= m_Settings.StrafeStabilizationMultiplier;
            }

            float pitchError = m_TargetPitch - currentPitch;
            float pitchTorque = pitchError * pGain - localAngularVelocity.x * dGain;

            float rollError = m_TargetRoll - currentRoll;
            float rollTorque = rollError * pGain - localAngularVelocity.z * dGain;

            m_Rigidbody.AddRelativeTorque(pitchTorque, 0f, rollTorque, ForceMode.Acceleration);
        }

        /// <summary>
        /// Applies yaw control (heading rotation).
        /// Uses rate-based control with damping.
        /// Phase 4: Disabled in strafe mode (facing follows camera instead).
        /// </summary>
        private void ApplyYawControl()
        {
            // Disable manual yaw control in strafe mode (camera controls facing)
            if (m_IsStrafeModeActive) return;

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

        #region Phase 4 - Strafe Mode

        /// <summary>
        /// Updates strafe mode activation state based on input.
        /// </summary>
        private void UpdateStrafeModeState()
        {
            bool previousState = m_IsStrafeModeActive;
            m_IsStrafeModeActive = m_InputHandler != null && m_InputHandler.StrafeModeActive;

            if (m_IsStrafeModeActive && !previousState)
            {
                LogDebug("Strafe mode ACTIVATED");
            }
            else if (!m_IsStrafeModeActive && previousState)
            {
                LogDebug("Strafe mode DEACTIVATED");
            }
        }

        /// <summary>
        /// Applies camera-relative velocity control and facing direction in strafe mode.
        /// </summary>
        private void ApplyStrafeModePhysics()
        {
            if (m_MainCamera == null) return;

            CalculateStrafeTargetVelocity();
            ApplyStrafeVelocityControl();
            UpdateStrafeFacingDirection();
        }

        /// <summary>
        /// Calculates target horizontal velocity from camera-relative input.
        /// </summary>
        private void CalculateStrafeTargetVelocity()
        {
            Vector2 moveInput = m_InputHandler != null ? m_InputHandler.MoveInput : Vector2.zero;

            // Project camera directions onto horizontal plane
            Vector3 cameraForward = Vector3.ProjectOnPlane(m_MainCamera.transform.forward, Vector3.up).normalized;
            Vector3 cameraRight = Vector3.ProjectOnPlane(m_MainCamera.transform.right, Vector3.up).normalized;

            // Calculate target velocity from camera-relative input
            m_TargetStrafeVelocity = (cameraForward * moveInput.y + cameraRight * moveInput.x)
                                      * m_Settings.StrafeMaxSpeed;
        }

        /// <summary>
        /// Applies forces to reach target strafe velocity with high accel/braking.
        /// </summary>
        private void ApplyStrafeVelocityControl()
        {
            // Get current horizontal velocity
            Vector3 currentHorizontalVel = new Vector3(m_Rigidbody.linearVelocity.x, 0f, m_Rigidbody.linearVelocity.z);

            // Calculate velocity error
            Vector3 velocityError = m_TargetStrafeVelocity - currentHorizontalVel;

            // Determine acceleration: high accel when input present, high braking when released
            Vector2 moveInput = m_InputHandler != null ? m_InputHandler.MoveInput : Vector2.zero;
            bool hasInput = moveInput.magnitude > 0.01f;
            float accelRate = hasInput ? m_Settings.StrafeAccel : m_Settings.StrafeBraking;

            // Clamp force to acceleration limit (prevents overshoot)
            Vector3 controlForce = Vector3.ClampMagnitude(velocityError, accelRate);

            // Apply acceleration force
            m_Rigidbody.AddForce(controlForce, ForceMode.Acceleration);

            // Apply additional damping for stability
            Vector3 dampingForce = -currentHorizontalVel * m_Settings.StrafeDamping;
            m_Rigidbody.AddForce(dampingForce, ForceMode.Acceleration);
        }

        /// <summary>
        /// Smoothly rotates jetpack facing to match camera yaw direction.
        /// </summary>
        private void UpdateStrafeFacingDirection()
        {
            // Get camera yaw (horizontal rotation only)
            float cameraYaw = m_MainCamera.transform.eulerAngles.y;

            // Smoothly rotate toward camera yaw
            float currentYaw = transform.eulerAngles.y;
            float smoothedYaw = Mathf.SmoothDampAngle(
                currentYaw,
                cameraYaw,
                ref m_CurrentYawVelocity,
                m_Settings.StrafeFacingSmoothTime
            );

            // Apply rotation (preserve pitch/roll from attitude control)
            Vector3 currentEuler = transform.eulerAngles;
            transform.eulerAngles = new Vector3(currentEuler.x, smoothedYaw, currentEuler.z);
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

            // Phase 4: Target strafe velocity (white)
            if (m_IsStrafeModeActive)
            {
                Gizmos.color = Color.white;
                Gizmos.DrawRay(transform.position, m_TargetStrafeVelocity * 0.5f);
            }
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

            Vector2 moveInput = m_InputHandler != null ? m_InputHandler.MoveInput : Vector2.zero;

            GUILayout.BeginArea(new Rect(10, 10, 300, 310));
            GUILayout.Label("JETPACK DEBUG");
            GUILayout.Label($"Vertical Speed: {verticalSpeed:F2} m/s");
            GUILayout.Label($"Horizontal Speed: {horizontalSpeed:F2} m/s");
            GUILayout.Label($"Current Thrust: {m_CurrentThrust:F2} m/s2");
            GUILayout.Label($"Altitude: {transform.position.y:F2} m");
            GUILayout.Label("--- ATTITUDE ---");
            GUILayout.Label($"Pitch: {currentPitch:F1}° (target: {m_TargetPitch:F1}°)");
            GUILayout.Label($"Roll: {currentRoll:F1}° (target: {m_TargetRoll:F1}°)");
            GUILayout.Label($"Yaw: {currentYaw:F1}°");
            GUILayout.Label("--- INPUT ---");
            GUILayout.Label($"Move: ({moveInput.x:F2}, {moveInput.y:F2})");
            GUILayout.Label("--- STRAFE MODE ---");
            GUILayout.Label($"Active: {(m_IsStrafeModeActive ? "YES" : "NO")}");
            if (m_IsStrafeModeActive)
            {
                GUILayout.Label($"Target Vel: {m_TargetStrafeVelocity.magnitude:F2} m/s");
                GUILayout.Label($"Camera Yaw: {(m_MainCamera != null ? m_MainCamera.transform.eulerAngles.y : 0f):F1}°");
            }
            GUILayout.EndArea();
        }

        #endregion
    }
}
