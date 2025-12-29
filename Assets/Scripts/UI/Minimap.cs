using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Système principal de minimap qui affiche une vue du monde en plongée.
/// Attachez ceci à un canvas UI et configurez une caméra avec render texture.
/// </summary>
public class Minimap : MonoBehaviour
{
    public static Minimap Instance { get; private set; }

    [Header("Paramètres de Cible")]
    [SerializeField]
    private Transform target;
    [SerializeField]
    private bool followTargetRotation = true;

    [Header("Paramètres de Caméra")]
    [SerializeField]
    private Camera minimapCamera;
    [SerializeField]
    private float cameraHeight = 100f;
    [SerializeField]
    private float zoomLevel = 50f;
    [SerializeField]
    private float minZoom = 20f;
    [SerializeField]
    private float maxZoom = 150f;
    [SerializeField]
    private float zoomSpeed = 10f;

    [Header("Paramètres UI")]
    [SerializeField]
    private RawImage minimapImage;
    [SerializeField]
    private RectTransform playerIcon;
    [SerializeField]
    private RectTransform minimapMask;

    [Header("Paramètres de Render Texture")]
    [SerializeField]
    private int renderTextureSize = 512;
    [SerializeField]
    private LayerMask minimapLayers;

    private RenderTexture minimapRenderTexture;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        SetupMinimapCamera();
    }

    private void Start()
    {
        // Essayer de trouver le joueur si non assigné
        if (target == null)
        {
            PlayerController player = FindFirstObjectByType<PlayerController>();
            if (player != null)
            {
                target = player.transform;
            }
        }
    }

    private void SetupMinimapCamera()
    {
        if (minimapCamera == null)
        {
            // Créer la caméra minimap si non assignée
            GameObject camObj = new GameObject("MinimapCamera");
            camObj.transform.SetParent(transform);
            minimapCamera = camObj.AddComponent<Camera>();
        }

        // Configurer la caméra
        minimapCamera.orthographic = true;
        minimapCamera.orthographicSize = zoomLevel;
        minimapCamera.cullingMask = minimapLayers;
        minimapCamera.clearFlags = CameraClearFlags.SolidColor;
        minimapCamera.backgroundColor = new Color(0.1f, 0.1f, 0.15f, 1f);

        // Créer la render texture
        minimapRenderTexture = new RenderTexture(renderTextureSize, renderTextureSize, 16);
        minimapRenderTexture.antiAliasing = 2;
        minimapCamera.targetTexture = minimapRenderTexture;

        // Assigner à l'UI
        if (minimapImage != null)
        {
            minimapImage.texture = minimapRenderTexture;
        }
    }

    private void LateUpdate()
    {
        if (target == null || minimapCamera == null)
            return;

        UpdateCameraPosition();
        UpdateCameraRotation();
        UpdatePlayerIcon();
    }

    private void UpdateCameraPosition()
    {
        Vector3 newPosition = target.position;
        newPosition.y = target.position.y + cameraHeight;
        minimapCamera.transform.position = newPosition;
    }

    private void UpdateCameraRotation()
    {
        if (followTargetRotation)
        {
            // La caméra regarde vers le bas, tournée selon la rotation Y de la cible
            float targetYRotation = target.eulerAngles.y;
            minimapCamera.transform.rotation = Quaternion.Euler(90f, targetYRotation, 0f);

            // L'icône du joueur reste fixe (pointe vers le haut) quand la carte tourne
            if (playerIcon != null)
            {
                playerIcon.localRotation = Quaternion.identity;
            }
        }
        else
        {
            // La caméra reste alignée au nord
            minimapCamera.transform.rotation = Quaternion.Euler(90f, 0f, 0f);

            // L'icône du joueur tourne pour montrer la direction
            if (playerIcon != null)
            {
                playerIcon.localRotation = Quaternion.Euler(0f, 0f, -target.eulerAngles.y);
            }
        }
    }

    private void UpdatePlayerIcon()
    {
        // L'icône du joueur est toujours centrée en mode carte rotative
        // En mode carte fixe, on pourrait la décaler mais on garde centré pour simplifier
        if (playerIcon != null)
        {
            playerIcon.anchoredPosition = Vector2.zero;
        }
    }

    /// <summary>
    /// Définir le niveau de zoom de la minimap
    /// </summary>
    public void SetZoom(float zoom)
    {
        zoomLevel = Mathf.Clamp(zoom, minZoom, maxZoom);
        if (minimapCamera != null)
        {
            minimapCamera.orthographicSize = zoomLevel;
        }
    }

    /// <summary>
    /// Zoomer sur la minimap
    /// </summary>
    public void ZoomIn()
    {
        SetZoom(zoomLevel - zoomSpeed);
    }

    /// <summary>
    /// Dézoomer sur la minimap
    /// </summary>
    public void ZoomOut()
    {
        SetZoom(zoomLevel + zoomSpeed);
    }

    /// <summary>
    /// Définir la cible à suivre
    /// </summary>
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    /// <summary>
    /// Basculer entre le mode carte rotative et carte fixe
    /// </summary>
    public void ToggleRotationMode()
    {
        followTargetRotation = !followTargetRotation;
    }

    /// <summary>
    /// Convertir une position monde en position UI minimap
    /// </summary>
    public Vector2 WorldToMinimapPosition(Vector3 worldPosition)
    {
        if (minimapCamera == null || minimapMask == null)
            return Vector2.zero;

        Vector3 offset = worldPosition - target.position;
        float mapScale = minimapMask.rect.width / (zoomLevel * 2f);

        Vector2 minimapPos;
        if (followTargetRotation)
        {
            // Tourner le décalage pour correspondre à la rotation de la caméra
            float angle = -target.eulerAngles.y * Mathf.Deg2Rad;
            float cos = Mathf.Cos(angle);
            float sin = Mathf.Sin(angle);
            minimapPos = new Vector2(
                offset.x * cos - offset.z * sin,
                offset.x * sin + offset.z * cos
            ) * mapScale;
        }
        else
        {
            minimapPos = new Vector2(offset.x, offset.z) * mapScale;
        }

        return minimapPos;
    }

    /// <summary>
    /// Vérifier si une position monde est visible sur la minimap
    /// </summary>
    public bool IsPositionVisible(Vector3 worldPosition)
    {
        if (minimapMask == null)
            return false;

        Vector2 minimapPos = WorldToMinimapPosition(worldPosition);
        float radius = minimapMask.rect.width / 2f;
        return minimapPos.magnitude <= radius;
    }

    private void OnDestroy()
    {
        if (minimapRenderTexture != null)
        {
            minimapRenderTexture.Release();
            Destroy(minimapRenderTexture);
        }

        if (Instance == this)
        {
            Instance = null;
        }
    }
}
