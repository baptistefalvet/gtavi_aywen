using System.Collections;
using UnityEngine;

namespace GD3.GtaviAywen
{
    /// <summary>
    /// Handles player mounting/dismounting the jetpack.
    /// Attach to the Player GameObject (same as PlayerCarControll).
    /// </summary>
    public class JetpackMount : MonoBehaviour
    {
        #region Components

        [Header("Player Components")]
        [SerializeField] private Animator m_Animator;
        [SerializeField] private ThirdPersonCam m_PlayerCam;
        [SerializeField] private Collider m_PlayerCollider;
        [SerializeField] private GameObject m_PlayerObject;

        private PlayerController m_PlayerController;
        private PlayerRagdoll m_PlayerRagdoll;
        private PlayerCarControll m_PlayerCarControll;
        private PlayerAim m_PlayerAim;
        private PlayerWeaponController m_PlayerWeaponController;
        private Rigidbody m_Rigidbody;

        #endregion

        #region Jetpack Detection

        [Header("Jetpack Detection")]
        [SerializeField] private LayerMask m_JetpackLayer;
        [SerializeField] private string m_JetpackTag = "Jetpack";
        [SerializeField] private float m_DetectionRadius = 3f;
        [SerializeField] private KeyCode m_MountKey = KeyCode.F;

        #endregion

        #region Mount Settings

        [Header("Mount Settings")]
        [SerializeField] private float m_MountDuration = 0.5f;
        [SerializeField] private float m_DismountCooldown = 0.5f;
        [SerializeField] private Vector3 m_MountOffset = new Vector3(0f, 0.5f, 0f);

        #endregion

        #region State

        [HideInInspector]
        public bool IsInJetpack;

        private JetpackController m_CurrentJetpack;
        private Rigidbody m_JetpackRigidbody;
        private bool m_CanDismount;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            m_Rigidbody = GetComponent<Rigidbody>();
            m_PlayerController = GetComponent<PlayerController>();
            m_PlayerRagdoll = GetComponent<PlayerRagdoll>();
            m_PlayerCarControll = GetComponent<PlayerCarControll>();
            m_PlayerAim = GetComponent<PlayerAim>();
            m_PlayerWeaponController = GetComponent<PlayerWeaponController>();
        }

        private void Update()
        {
            CheckForJetpack();
        }

        #endregion

        #region Jetpack Detection

        private void CheckForJetpack()
        {
            if (!Input.GetKeyDown(m_MountKey)) return;
            if (m_PlayerRagdoll != null && m_PlayerRagdoll.IsRagdoll) return;

            if (IsInJetpack)
            {
                if (m_CanDismount && m_CurrentJetpack != null)
                {
                    StartCoroutine(DismountJetpack());
                }
            }
            else
            {
                TryMountNearbyJetpack();
            }
        }

        private void TryMountNearbyJetpack()
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, m_DetectionRadius, m_JetpackLayer);

            float minDistance = float.MaxValue;
            JetpackController nearestJetpack = null;

            foreach (Collider col in colliders)
            {
                GameObject obj = col.gameObject;

                if (obj.CompareTag(m_JetpackTag) || (obj.transform.parent != null && obj.transform.parent.CompareTag(m_JetpackTag)))
                {
                    JetpackController jetpack = obj.GetComponent<JetpackController>();
                    if (jetpack == null && obj.transform.parent != null)
                    {
                        jetpack = obj.transform.parent.GetComponent<JetpackController>();
                    }

                    if (jetpack != null)
                    {
                        float distance = Vector3.Distance(transform.position, jetpack.transform.position);
                        if (distance < minDistance)
                        {
                            minDistance = distance;
                            nearestJetpack = jetpack;
                        }
                    }
                }
            }

            if (nearestJetpack != null)
            {
                StartCoroutine(MountJetpack(nearestJetpack));
            }
        }

        #endregion

        #region Mount/Dismount

        private IEnumerator MountJetpack(JetpackController jetpack)
        {
            m_CurrentJetpack = jetpack;
            m_JetpackRigidbody = jetpack.GetComponent<Rigidbody>();
            IsInJetpack = true;
            m_CanDismount = false;

            m_PlayerController.CanMove = false;
            if (m_PlayerRagdoll != null)
            {
                m_PlayerRagdoll.enabled = false;
            }
            if (m_PlayerCarControll != null)
            {
                m_PlayerCarControll.enabled = false;
            }
            if (m_PlayerAim != null)
            {
                m_PlayerAim.enabled = false;
            }
            if (m_PlayerWeaponController != null)
            {
                m_PlayerWeaponController.enabled = false;
            }
            m_Rigidbody.isKinematic = true;
            m_Rigidbody.interpolation = RigidbodyInterpolation.None;
            m_PlayerCollider.enabled = false;

            if (m_Animator != null)
            {
                m_Animator.SetBool("Grounded", false);
            }

            // Disable ThirdPersonCam BEFORE yield to prevent it from rotating player on next frame
            m_PlayerCam.enabled = false;

            yield return null;

            transform.SetParent(jetpack.transform);
            transform.localPosition = m_MountOffset;
            transform.localRotation = Quaternion.identity;

            if (m_JetpackRigidbody != null)
            {
                m_JetpackRigidbody.isKinematic = false;
            }

            jetpack.enabled = true;

            Debug.Log("[JetpackMount] Player mounted jetpack. Press F to dismount, Space to ascend, Ctrl to descend.");

            yield return new WaitForSeconds(m_MountDuration + m_DismountCooldown);

            m_CanDismount = true;
        }

        private IEnumerator DismountJetpack()
        {
            m_CanDismount = false;

            m_CurrentJetpack.enabled = false;

            if (m_JetpackRigidbody != null)
            {
                m_JetpackRigidbody.isKinematic = true;
            }

            transform.SetParent(null);

            Vector3 dismountPosition = m_CurrentJetpack.transform.position + Vector3.down * 2f;
            transform.position = dismountPosition;
            transform.rotation = Quaternion.identity;

            m_PlayerCollider.enabled = true;
            m_Rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
            m_Rigidbody.isKinematic = false;
            m_Rigidbody.linearVelocity = Vector3.zero;

            IsInJetpack = false;

            yield return null;

            m_CurrentJetpack = null;
            m_JetpackRigidbody = null;

            m_PlayerController.CanMove = true;
            if (m_PlayerRagdoll != null)
            {
                m_PlayerRagdoll.enabled = true;
            }
            if (m_PlayerCarControll != null)
            {
                m_PlayerCarControll.enabled = true;
            }
            if (m_PlayerAim != null)
            {
                m_PlayerAim.enabled = true;
            }
            if (m_PlayerWeaponController != null)
            {
                m_PlayerWeaponController.enabled = true;
            }
            if (m_PlayerCam != null)
            {
                m_PlayerCam.enabled = true;
            }

            Debug.Log("[JetpackMount] Player dismounted jetpack.");
        }

        #endregion

        #region Debug

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, m_DetectionRadius);
        }

        #endregion
    }
}
