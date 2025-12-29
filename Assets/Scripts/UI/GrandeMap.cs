using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Système de grande carte (plein écran) qui peut être ouverte/fermée.
/// Affiche une vue complète du monde avec marqueurs et points de passage.
/// </summary>
public class GrandeMap : MonoBehaviour
{
    public static GrandeMap Instance { get; private set; }

    [Header("Contrôles")]
    [SerializeField]
    private KeyCode toggleKey = KeyCode.M;
    [SerializeField]
    private bool pauseGameWhenOpen = true;

    [Header("Paramètres de Caméra")]
    [SerializeField]
    private float defaultZoom = 200f;
    [SerializeField]
    private float minZoom = 50f;
    [SerializeField]
    private float maxZoom = 500f;
    [SerializeField]
    private float zoomSpeed = 50f;
    [SerializeField]
    private float panSpeed = 100f;
    [SerializeField]
    private float cameraHeight = 500f;

    [Header("Paramètres UI")]
    [SerializeField]
    private Color backgroundColor = new Color(0.1f, 0.12f, 0.15f, 0.95f);
    [SerializeField]
    private Color borderColor = new Color(0.3f, 0.3f, 0.3f, 1f);

    [Header("Icône du Joueur")]
    [SerializeField]
    private Color playerIconColor = Color.white;
    [SerializeField]
    private Vector2 playerIconSize = new Vector2(30f, 30f);

    // Composants UI
    private GameObject mapPanel;
    private RawImage mapImage;
    private Image playerIcon;
    private RectTransform playerIconRect;
    private Text coordsText;
    private Text zoomText;

    // Caméra
    private Camera mapCamera;
    private RenderTexture mapRenderTexture;

    // État
    private bool isOpen = false;
    private float currentZoom;
    private Vector3 cameraOffset = Vector3.zero;
    private Transform playerTransform;

    public bool IsOpen => isOpen;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            Debug.Log("GrandeMap: Initialisé avec succès");
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        currentZoom = defaultZoom;
    }

    private void Start()
    {
        // Trouver le joueur
        PlayerController player = FindFirstObjectByType<PlayerController>();
        if (player != null)
        {
            playerTransform = player.transform;
        }

        CreateMapUI();
        CreateMapCamera();
        
        // Masquer au démarrage
        mapPanel.SetActive(false);
        
        Debug.Log($"GrandeMap: Prêt ! Appuyez sur [{toggleKey}] pour ouvrir la carte");
    }

    private void CreateMapUI()
    {
        // Créer un Canvas dédié pour la grande carte (toujours au-dessus)
        GameObject canvasObj = new GameObject("GrandeMapCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999; // Toujours au-dessus
        
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        
        canvasObj.AddComponent<GraphicRaycaster>();

        // === PANNEAU PRINCIPAL ===
        mapPanel = new GameObject("GrandeMapPanel");
        mapPanel.transform.SetParent(canvas.transform, false);

        RectTransform panelRect = mapPanel.AddComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        Image panelBg = mapPanel.AddComponent<Image>();
        panelBg.color = backgroundColor;

        // === CONTENEUR DE LA CARTE ===
        GameObject mapContainer = new GameObject("MapContainer");
        mapContainer.transform.SetParent(mapPanel.transform, false);

        RectTransform containerRect = mapContainer.AddComponent<RectTransform>();
        containerRect.anchorMin = new Vector2(0.05f, 0.1f);
        containerRect.anchorMax = new Vector2(0.95f, 0.9f);
        containerRect.offsetMin = Vector2.zero;
        containerRect.offsetMax = Vector2.zero;

        Image containerBorder = mapContainer.AddComponent<Image>();
        containerBorder.color = borderColor;

        // === MASQUE ===
        GameObject maskObj = new GameObject("MapMask");
        maskObj.transform.SetParent(mapContainer.transform, false);

        RectTransform maskRect = maskObj.AddComponent<RectTransform>();
        maskRect.anchorMin = Vector2.zero;
        maskRect.anchorMax = Vector2.one;
        maskRect.offsetMin = new Vector2(4, 4);
        maskRect.offsetMax = new Vector2(-4, -4);

        Image maskImage = maskObj.AddComponent<Image>();
        maskImage.color = Color.white;

        Mask mask = maskObj.AddComponent<Mask>();
        mask.showMaskGraphic = false;

        // === AFFICHAGE DE LA CARTE ===
        GameObject mapDisplay = new GameObject("MapDisplay");
        mapDisplay.transform.SetParent(maskObj.transform, false);

        RectTransform displayRect = mapDisplay.AddComponent<RectTransform>();
        displayRect.anchorMin = Vector2.zero;
        displayRect.anchorMax = Vector2.one;
        displayRect.offsetMin = Vector2.zero;
        displayRect.offsetMax = Vector2.zero;

        mapImage = mapDisplay.AddComponent<RawImage>();
        mapImage.raycastTarget = false;

        // === ICÔNE DU JOUEUR ===
        GameObject playerIconObj = new GameObject("PlayerIcon");
        playerIconObj.transform.SetParent(maskObj.transform, false);

        playerIconRect = playerIconObj.AddComponent<RectTransform>();
        playerIconRect.sizeDelta = playerIconSize;

        playerIcon = playerIconObj.AddComponent<Image>();
        playerIcon.sprite = CreateCircleSprite(32);
        playerIcon.color = playerIconColor;
        playerIcon.raycastTarget = false;

        // === TEXTE COORDONNÉES ===
        GameObject coordsObj = new GameObject("CoordsText");
        coordsObj.transform.SetParent(mapPanel.transform, false);

        RectTransform coordsRect = coordsObj.AddComponent<RectTransform>();
        coordsRect.anchorMin = new Vector2(0.05f, 0.02f);
        coordsRect.anchorMax = new Vector2(0.4f, 0.08f);
        coordsRect.offsetMin = Vector2.zero;
        coordsRect.offsetMax = Vector2.zero;

        coordsText = coordsObj.AddComponent<Text>();
        coordsText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        coordsText.fontSize = 18;
        coordsText.color = Color.white;
        coordsText.alignment = TextAnchor.MiddleLeft;

        // === TEXTE ZOOM ===
        GameObject zoomObj = new GameObject("ZoomText");
        zoomObj.transform.SetParent(mapPanel.transform, false);

        RectTransform zoomRect = zoomObj.AddComponent<RectTransform>();
        zoomRect.anchorMin = new Vector2(0.6f, 0.02f);
        zoomRect.anchorMax = new Vector2(0.95f, 0.08f);
        zoomRect.offsetMin = Vector2.zero;
        zoomRect.offsetMax = Vector2.zero;

        zoomText = zoomObj.AddComponent<Text>();
        zoomText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        zoomText.fontSize = 18;
        zoomText.color = Color.white;
        zoomText.alignment = TextAnchor.MiddleRight;

        // === TITRE ===
        GameObject titleObj = new GameObject("Title");
        titleObj.transform.SetParent(mapPanel.transform, false);

        RectTransform titleRect = titleObj.AddComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.3f, 0.92f);
        titleRect.anchorMax = new Vector2(0.7f, 0.98f);
        titleRect.offsetMin = Vector2.zero;
        titleRect.offsetMax = Vector2.zero;

        Text titleText = titleObj.AddComponent<Text>();
        titleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        titleText.fontSize = 28;
        titleText.color = Color.white;
        titleText.alignment = TextAnchor.MiddleCenter;
        titleText.text = "CARTE";

        // === INSTRUCTIONS ===
        GameObject instructionsObj = new GameObject("Instructions");
        instructionsObj.transform.SetParent(mapPanel.transform, false);

        RectTransform instrRect = instructionsObj.AddComponent<RectTransform>();
        instrRect.anchorMin = new Vector2(0.05f, 0.92f);
        instrRect.anchorMax = new Vector2(0.3f, 0.98f);
        instrRect.offsetMin = Vector2.zero;
        instrRect.offsetMax = Vector2.zero;

        Text instrText = instructionsObj.AddComponent<Text>();
        instrText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        instrText.fontSize = 14;
        instrText.color = new Color(0.7f, 0.7f, 0.7f);
        instrText.alignment = TextAnchor.MiddleLeft;
        instrText.text = "ZQSD: Déplacer | Molette: Zoom | Echap/M: Fermer";
    }

    private void CreateMapCamera()
    {
        GameObject camObj = new GameObject("GrandeMapCamera");
        mapCamera = camObj.AddComponent<Camera>();
        mapCamera.orthographic = true;
        mapCamera.orthographicSize = currentZoom;
        mapCamera.clearFlags = CameraClearFlags.SolidColor;
        mapCamera.backgroundColor = new Color(0.15f, 0.18f, 0.22f, 1f);
        mapCamera.depth = -20;
        mapCamera.enabled = false;

        // Créer la render texture
        mapRenderTexture = new RenderTexture(1024, 1024, 16);
        mapRenderTexture.antiAliasing = 2;
        mapCamera.targetTexture = mapRenderTexture;
        mapImage.texture = mapRenderTexture;
    }

    private void Update()
    {
        // Touche pour ouvrir/fermer
        if (Input.GetKeyDown(toggleKey))
        {
            Debug.Log("GrandeMap: Touche M détectée !");
            Toggle();
        }

        // Echap pour fermer
        if (isOpen && Input.GetKeyDown(KeyCode.Escape))
        {
            Close();
        }

        if (isOpen)
        {
            HandleInput();
            UpdateCamera();
            UpdateUI();
        }
    }

    private void HandleInput()
    {
        // Zoom avec la molette
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0)
        {
            currentZoom -= scroll * zoomSpeed * 10f;
            currentZoom = Mathf.Clamp(currentZoom, minZoom, maxZoom);
        }


        // Déplacement avec ZQSD ou flèches (toujours actif même si UI focus)
        float horizontal = 0f;
        float vertical = 0f;
        if (Input.GetKey(KeyCode.Q) || Input.GetKey(KeyCode.LeftArrow)) horizontal -= 1f;
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) horizontal += 1f;
        if (Input.GetKey(KeyCode.Z) || Input.GetKey(KeyCode.UpArrow)) vertical += 1f;
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) vertical -= 1f;

        // Ajout support axes Unity (pour compatibilité manette ou config custom)
        horizontal += Input.GetAxisRaw("Horizontal");
        vertical += Input.GetAxisRaw("Vertical");

        if (horizontal != 0 || vertical != 0)
        {
            cameraOffset.x += horizontal * panSpeed * Time.unscaledDeltaTime;
            cameraOffset.z += vertical * panSpeed * Time.unscaledDeltaTime;
        }

        // Recentrer sur le joueur avec Espace
        if (Input.GetKeyDown(KeyCode.Space))
        {
            cameraOffset = Vector3.zero;
        }

        // Clic pour placer un waypoint
        if (Input.GetMouseButtonDown(0))
        {
            PlaceWaypointAtClick();
        }

        // Clic droit pour effacer le waypoint
        if (Input.GetMouseButtonDown(1))
        {
            if (MinimapWaypoints.Instance != null)
            {
                MinimapWaypoints.Instance.ClearWaypoint();
            }
        }
    }

    private void PlaceWaypointAtClick()
    {
        if (MinimapWaypoints.Instance == null || mapImage == null)
            return;

        // Convertir la position de la souris en position sur la carte
        RectTransform mapRect = mapImage.rectTransform;
        Vector2 localPoint;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            mapRect, Input.mousePosition, null, out localPoint))
        {
            // Normaliser la position (-0.5 à 0.5)
            Vector2 normalizedPos = new Vector2(
                localPoint.x / mapRect.rect.width,
                localPoint.y / mapRect.rect.height
            );

            // Convertir en position monde
            Vector3 worldPos = GetCameraCenter();
            worldPos.x += normalizedPos.x * currentZoom * 2f;
            worldPos.z += normalizedPos.y * currentZoom * 2f;
            worldPos.y = 0;

            MinimapWaypoints.Instance.SetWaypoint(worldPos);
        }
    }

    private Vector3 GetCameraCenter()
    {
        if (playerTransform != null)
        {
            return playerTransform.position + cameraOffset;
        }
        return cameraOffset;
    }

    private void UpdateCamera()
    {
        if (mapCamera == null)
            return;

        Vector3 center = GetCameraCenter();
        mapCamera.transform.position = new Vector3(center.x, cameraHeight, center.z);
        mapCamera.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        mapCamera.orthographicSize = currentZoom;
    }

    private void UpdateUI()
    {
        // Mettre à jour la position de l'icône du joueur
        if (playerTransform != null && playerIconRect != null && mapImage != null)
        {
            Vector3 center = GetCameraCenter();
            Vector3 playerOffset = playerTransform.position - center;

            RectTransform mapRect = mapImage.rectTransform;
            float scaleX = mapRect.rect.width / (currentZoom * 2f);
            float scaleY = mapRect.rect.height / (currentZoom * 2f);

            Vector2 iconPos = new Vector2(
                playerOffset.x * scaleX,
                playerOffset.z * scaleY
            );

            playerIconRect.anchoredPosition = iconPos;

            // Rotation de l'icône
            playerIconRect.localRotation = Quaternion.Euler(0, 0, -playerTransform.eulerAngles.y);
        }

        // Mettre à jour les textes
        if (coordsText != null && playerTransform != null)
        {
            coordsText.text = $"Position: X:{playerTransform.position.x:F0} Z:{playerTransform.position.z:F0}";
        }

        if (zoomText != null)
        {
            zoomText.text = $"Zoom: {currentZoom:F0}m | [Espace] Recentrer";
        }
    }

    /// <summary>
    /// Ouvrir la grande carte
    /// </summary>
    public void Open()
    {
        if (isOpen)
            return;

        isOpen = true;
        mapPanel.SetActive(true);
        mapCamera.enabled = true;
        cameraOffset = Vector3.zero;

        if (pauseGameWhenOpen)
        {
            Time.timeScale = 0f;
        }

        // Afficher le curseur
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    /// <summary>
    /// Fermer la grande carte
    /// </summary>
    public void Close()
    {
        if (!isOpen)
            return;

        isOpen = false;
        mapPanel.SetActive(false);
        mapCamera.enabled = false;

        if (pauseGameWhenOpen)
        {
            Time.timeScale = 1f;
        }

        // Masquer le curseur
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = false;
    }

    /// <summary>
    /// Basculer l'état de la carte
    /// </summary>
    public void Toggle()
    {
        if (isOpen)
            Close();
        else
            Open();
    }

    private Sprite CreateCircleSprite(int size)
    {
        Texture2D tex = new Texture2D(size, size);
        Color[] pixels = new Color[size * size];

        float center = size / 2f;
        float radius = size / 2f - 1f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                pixels[y * size + x] = dist <= radius ? Color.white : Color.clear;
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();
        tex.filterMode = FilterMode.Bilinear;

        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
    }

    private void OnDestroy()
    {
        if (mapRenderTexture != null)
        {
            mapRenderTexture.Release();
            Destroy(mapRenderTexture);
        }

        if (Instance == this)
        {
            Instance = null;
        }
    }
}
