using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Gère les points de passage sur la minimap.
/// Fournit des fonctionnalités pour définir, suivre et supprimer des points de passage.
/// </summary>
public class MinimapWaypoints : MonoBehaviour
{
    public static MinimapWaypoints Instance { get; private set; }

    [Header("Paramètres du Point de Passage")]
    [SerializeField]
    private Sprite waypointSprite;
    [SerializeField]
    private Color waypointColor = Color.yellow;
    [SerializeField]
    private Vector2 waypointSize = new Vector2(30f, 30f);

    [Header("Indicateur de Point de Passage")]
    [SerializeField]
    private bool showDistanceText = true;
    [SerializeField]
    private float reachDistance = 35f;

    private Vector3 waypointPosition;
    private bool hasWaypoint = false;
    private RectTransform waypointIcon;
    private Image waypointImage;
    private Text distanceText;
    private RectTransform minimapRect;

    public bool HasWaypoint => hasWaypoint;
    public Vector3 WaypointPosition => waypointPosition;

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
    }

    private void Start()
    {
        CreateWaypointIcon();
    }

    private void CreateWaypointIcon()
    {
        if (Minimap.Instance == null)
        {
            Invoke(nameof(CreateWaypointIcon), 0.5f);
            return;
        }

        // Trouver le parent de la minimap
        RawImage minimapImage = Minimap.Instance.GetComponentInChildren<RawImage>();
        if (minimapImage == null)
            return;

        minimapRect = minimapImage.rectTransform.parent as RectTransform;
        if (minimapRect == null)
            minimapRect = minimapImage.rectTransform;

        // Créer l'icône du point de passage
        GameObject iconObj = new GameObject("WaypointIcon");
        iconObj.transform.SetParent(minimapRect, false);

        waypointIcon = iconObj.AddComponent<RectTransform>();
        waypointIcon.sizeDelta = waypointSize;

        waypointImage = iconObj.AddComponent<Image>();
        waypointImage.color = waypointColor;
        waypointImage.raycastTarget = false;

        if (waypointSprite != null)
        {
            waypointImage.sprite = waypointSprite;
        }
        else
        {
            waypointImage.sprite = CreateWaypointSprite();
        }

        // Créer le texte de distance
        if (showDistanceText)
        {
            GameObject textObj = new GameObject("DistanceText");
            textObj.transform.SetParent(iconObj.transform, false);

            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchoredPosition = new Vector2(0, -20);
            textRect.sizeDelta = new Vector2(100, 20);

            distanceText = textObj.AddComponent<Text>();
            distanceText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            distanceText.fontSize = 12;
            distanceText.color = waypointColor;
            distanceText.alignment = TextAnchor.MiddleCenter;
            distanceText.raycastTarget = false;
        }

        waypointIcon.gameObject.SetActive(false);
    }

    private Sprite CreateWaypointSprite()
    {
        // Créer un sprite marqueur/épingle
        int size = 32;
        Texture2D tex = new Texture2D(size, size);
        Color[] pixels = new Color[size * size];

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                pixels[y * size + x] = Color.clear;
            }
        }

        // Dessiner une forme de marqueur simple (losange)
        int center = size / 2;
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                int dx = Mathf.Abs(x - center);
                int dy = Mathf.Abs(y - center);
                if (dx + dy <= center - 2)
                {
                    pixels[y * size + x] = Color.white;
                }
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();

        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
    }

    private void LateUpdate()
    {
        if (!hasWaypoint || waypointIcon == null || Minimap.Instance == null)
            return;

        UpdateWaypointPosition();
        UpdateDistanceText();
        CheckWaypointReached();
    }

    private void UpdateWaypointPosition()
    {
        Vector2 minimapPos = Minimap.Instance.WorldToMinimapPosition(waypointPosition);
        bool isInRange = Minimap.Instance.IsPositionVisible(waypointPosition);

        if (isInRange)
        {
            waypointIcon.anchoredPosition = minimapPos;
        }
        else
        {
            // Limiter au bord
            float radius = (minimapRect.rect.width / 2f) - 15f;
            Vector2 clampedPos = minimapPos.normalized * radius;
            waypointIcon.anchoredPosition = clampedPos;
        }
    }

    private void UpdateDistanceText()
    {
        if (distanceText == null)
            return;

        PlayerController player = FindFirstObjectByType<PlayerController>();
        if (player == null)
            return;

        float distance = Vector3.Distance(player.transform.position, waypointPosition);
        
        if (distance >= 1000)
        {
            distanceText.text = $"{distance / 1000f:F1}km";
        }
        else
        {
            distanceText.text = $"{distance:F0}m";
        }
    }

    private void CheckWaypointReached()
    {
        PlayerController player = FindFirstObjectByType<PlayerController>();
        if (player == null)
            return;

        float distance = Vector3.Distance(player.transform.position, waypointPosition);
        if (distance <= reachDistance)
        {
            Debug.Log("Point de passage atteint !");
            ClearWaypoint();
        }
    }

    /// <summary>
    /// Définir un point de passage à la position monde spécifiée
    /// </summary>
    public void SetWaypoint(Vector3 position)
    {
        waypointPosition = position;
        hasWaypoint = true;

        if (waypointIcon != null)
        {
            waypointIcon.gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// Définir un point de passage depuis une position écran (raycast vers le monde)
    /// </summary>
    public void SetWaypointFromScreen(Vector2 screenPosition, Camera camera, LayerMask groundLayer)
    {
        Ray ray = camera.ScreenPointToRay(screenPosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 1000f, groundLayer))
        {
            SetWaypoint(hit.point);
        }
    }

    /// <summary>
    /// Effacer le point de passage actuel
    /// </summary>
    public void ClearWaypoint()
    {
        hasWaypoint = false;

        if (waypointIcon != null)
        {
            waypointIcon.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Basculer le point de passage - s'il existe, l'effacer; sinon rien
    /// </summary>
    public void ToggleWaypoint(Vector3 position)
    {
        if (hasWaypoint && Vector3.Distance(waypointPosition, position) < 10f)
        {
            ClearWaypoint();
        }
        else
        {
            SetWaypoint(position);
        }
    }

    /// <summary>
    /// Obtenir la direction vers le point de passage depuis le joueur
    /// </summary>
    public Vector3 GetDirectionToWaypoint(Vector3 fromPosition)
    {
        if (!hasWaypoint)
            return Vector3.zero;

        Vector3 direction = waypointPosition - fromPosition;
        direction.y = 0;
        return direction.normalized;
    }

    /// <summary>
    /// Obtenir la distance jusqu'au point de passage depuis une position
    /// </summary>
    public float GetDistanceToWaypoint(Vector3 fromPosition)
    {
        if (!hasWaypoint)
            return -1f;

        return Vector3.Distance(fromPosition, waypointPosition);
    }

    private void OnDestroy()
    {
        if (waypointIcon != null)
        {
            Destroy(waypointIcon.gameObject);
        }

        if (Instance == this)
        {
            Instance = null;
        }
    }
}
