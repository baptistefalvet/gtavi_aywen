using UnityEngine;
using UnityEngine.InputSystem;

namespace GD3.GtaviAywen
{
    /// <summary>
    /// Handles input for the jetpack using Unity Input System.
    /// Wraps InputActions to provide clean API for JetpackController.
    /// </summary>
    public class JetpackInputHandler : MonoBehaviour
    {
        #region Input Actions Asset

        [Header("Input")]
        [SerializeField] private InputActionAsset m_InputActions;

        #endregion

        #region Cached Actions

        private InputAction m_AscendAction;
        private InputAction m_DescendAction;
        private InputAction m_MoveAction;
        private InputAction m_YawAction;
        private InputAction m_PitchAction;
        private InputAction m_StrafeModeAction;
        private InputAction m_BoostAction;
        private InputAction m_ToggleGearAction;
        private InputAction m_ExitAction;

        #endregion

        #region Input State (Phase 1)

        public float AscendInput { get; private set; }
        public float DescendInput { get; private set; }

        /// <summary>
        /// Computed property for collective thrust input (-1 to +1).
        /// Positive = ascending, Negative = descending.
        /// </summary>
        public float CollectiveInput => AscendInput - DescendInput;

        #endregion

        #region Input State (Phase 2)

        /// <summary>
        /// Pitch input from mouse Y-axis or keyboard (-1 to +1).
        /// Positive = pitch up (nose up), Negative = pitch down.
        /// </summary>
        public float PitchInput { get; private set; }

        /// <summary>
        /// Yaw input from Q/E keys (-1 to +1).
        /// Negative (Q) = yaw left, Positive (E) = yaw right.
        /// </summary>
        public float YawInput { get; private set; }

        /// <summary>
        /// Roll input derived from Move.x (strafe direction).
        /// Negative = roll left, Positive = roll right.
        /// </summary>
        public float RollInput { get; private set; }

        /// <summary>
        /// Movement input from WASD/Arrow keys.
        /// </summary>
        public Vector2 MoveInput { get; private set; }

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            CacheInputActions();
        }

        private void OnEnable()
        {
            if (m_InputActions == null) return;
            if (m_AscendAction == null) CacheInputActions();
            EnableInputActions();
        }

        private void OnDisable()
        {
            DisableInputActions();
        }

        private void Update()
        {
            ReadInputs();
        }

        #endregion

        #region Input Setup

        private void CacheInputActions()
        {
            if (m_InputActions == null)
            {
                Debug.LogError("[JetpackInputHandler] InputActionAsset not assigned!");
                return;
            }

            var jetpackMap = m_InputActions.FindActionMap("Jetpack");
            if (jetpackMap == null)
            {
                Debug.LogError("[JetpackInputHandler] 'Jetpack' action map not found!");
                return;
            }

            m_AscendAction = jetpackMap.FindAction("Ascend");
            m_DescendAction = jetpackMap.FindAction("Descend");
            m_MoveAction = jetpackMap.FindAction("Move");
            m_YawAction = jetpackMap.FindAction("Yaw");
            m_PitchAction = jetpackMap.FindAction("Pitch");
            m_StrafeModeAction = jetpackMap.FindAction("StrafeMode");
            m_BoostAction = jetpackMap.FindAction("Boost");
            m_ToggleGearAction = jetpackMap.FindAction("ToggleGear");
            m_ExitAction = jetpackMap.FindAction("Exit");
        }

        private void EnableInputActions()
        {
            m_AscendAction?.Enable();
            m_DescendAction?.Enable();
            m_MoveAction?.Enable();
            m_YawAction?.Enable();
            m_PitchAction?.Enable();
        }

        private void DisableInputActions()
        {
            m_AscendAction?.Disable();
            m_DescendAction?.Disable();
            m_MoveAction?.Disable();
            m_YawAction?.Disable();
            m_PitchAction?.Disable();
        }

        #endregion

        #region Input Reading

        private void ReadInputs()
        {
            AscendInput = m_AscendAction?.ReadValue<float>() ?? 0f;
            DescendInput = m_DescendAction?.ReadValue<float>() ?? 0f;

            MoveInput = m_MoveAction?.ReadValue<Vector2>() ?? Vector2.zero;
            YawInput = m_YawAction?.ReadValue<float>() ?? 0f;
            PitchInput = m_PitchAction?.ReadValue<float>() ?? 0f;
            RollInput = MoveInput.x;
        }

        #endregion
    }
}
