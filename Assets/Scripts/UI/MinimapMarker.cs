using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Types de marqueurs pouvant être affichés sur la minimap
/// </summary>
public enum MarkerType
{
    Default,
    Player,
    Enemy,
    NPC,
    Vehicle,
    Objective,
    Shop,
    Mission,
    Waypoint,
    Custom
}

/// <summary>
/// Attachez ce composant à tout objet devant apparaître sur la minimap.
/// Le marqueur suivra automatiquement la position de l'objet.
/// </summary>
public class MinimapMarker : MonoBehaviour
{
    [Header("Paramètres du Marqueur")]
    [SerializeField]
    private MarkerType markerType = MarkerType.Default;
    [SerializeField]
    private Sprite iconSprite;
    [SerializeField]
    private Color iconColor = Color.white;
    [SerializeField]
    private Vector2 iconSize = new Vector2(20f, 20f);

    [Header("Paramètres de Comportement")]
    [SerializeField]
    private bool rotateWithObject = true;
    [SerializeField]
    private bool showWhenOutOfRange = true;
    [SerializeField]
    private bool clampToEdge = true;
    [SerializeField]
    private float edgeOffset = 10f;

    [Header("Paramètres de Visibilité")]
    [SerializeField]
    private bool alwaysVisible = false;
    [SerializeField]
    private float visibilityRange = 200f;

    private RectTransform iconRectTransform;
    private Image iconImage;
    private RectTransform minimapRect;
    private bool isInitialized = false;

    public MarkerType Type => markerType;
    public bool IsVisible { get; private set; }

    private void Start()
    {
        CreateIcon();
    }

    private void CreateIcon()
    {
        if (Minimap.Instance == null)
        {
            // Réessayer plus tard si la minimap n'est pas prête
            Invoke(nameof(CreateIcon), 0.5f);
            return;
        }

        // Trouver le parent UI de la minimap
        RawImage minimapImage = FindMinimapImage();
        if (minimapImage == null)
        {
            Debug.LogWarning("MinimapMarker: Impossible de trouver l'image minimap dans la scène");
            return;
        }

        minimapRect = minimapImage.rectTransform.parent as RectTransform;
        if (minimapRect == null)
        {
            minimapRect = minimapImage.rectTransform;
        }

        // Créer l'icône UI
        GameObject iconObj = new GameObject($"MinimapIcon_{gameObject.name}");
        iconObj.transform.SetParent(minimapRect, false);

        iconRectTransform = iconObj.AddComponent<RectTransform>();
        iconRectTransform.sizeDelta = iconSize;
        iconRectTransform.anchoredPosition = Vector2.zero;

        iconImage = iconObj.AddComponent<Image>();
        iconImage.sprite = iconSprite;
        iconImage.color = iconColor;
        iconImage.raycastTarget = false;

        // Définir le sprite par défaut si aucun fourni
        if (iconSprite == null)
        {
            iconImage.sprite = CreateDefaultSprite();
        }

        // Appliquer les couleurs par défaut selon le type d'icône
        if (iconColor == Color.white)
        {
            ApplyDefaultColor();
        }

        isInitialized = true;
    }

    private RawImage FindMinimapImage()
    {
        // Essayer de trouver via Minimap
        Minimap minimap = Minimap.Instance;
        if (minimap != null)
        {
            RawImage[] images = minimap.GetComponentsInChildren<RawImage>();
            if (images.Length > 0)
                return images[0];
        }

        // Solution de secours: chercher dans la scène
        return FindFirstObjectByType<RawImage>();
    }

    private Sprite CreateDefaultSprite()
    {
        // Créer un sprite cercle simple
        int size = 32;
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

        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
    }

    private void ApplyDefaultColor()
    {
        switch (markerType)
        {
            case MarkerType.Player:
                iconColor = Color.white;
                break;
            case MarkerType.Enemy:
                iconColor = Color.red;
                break;
            case MarkerType.NPC:
                iconColor = new Color(0.5f, 0.8f, 1f); // Bleu clair
                break;
            case MarkerType.Vehicle:
                iconColor = Color.cyan;
                break;
            case MarkerType.Objective:
                iconColor = Color.yellow;
                break;
            case MarkerType.Shop:
                iconColor = Color.green;
                break;
            case MarkerType.Mission:
                iconColor = new Color(1f, 0.5f, 0f); // Orange
                break;
            case MarkerType.Waypoint:
                iconColor = Color.magenta;
                break;
            default:
                iconColor = Color.white;
                break;
        }

        if (iconImage != null)
        {
            iconImage.color = iconColor;
        }
    }

    private void LateUpdate()
    {
        if (!isInitialized || Minimap.Instance == null)
            return;

        UpdateIconPosition();
        UpdateIconRotation();
        UpdateVisibility();
    }

    private void UpdateIconPosition()
    {
        Vector2 minimapPos = Minimap.Instance.WorldToMinimapPosition(transform.position);
        bool isInRange = Minimap.Instance.IsPositionVisible(transform.position);

        if (isInRange)
        {
            iconRectTransform.anchoredPosition = minimapPos;
        }
        else if (clampToEdge && showWhenOutOfRange)
        {
            // Limiter au bord de la minimap
            float radius = (minimapRect.rect.width / 2f) - edgeOffset;
            Vector2 clampedPos = minimapPos.normalized * radius;
            iconRectTransform.anchoredPosition = clampedPos;
        }

        IsVisible = isInRange || showWhenOutOfRange;
    }

    private void UpdateIconRotation()
    {
        if (!rotateWithObject)
        {
            iconRectTransform.localRotation = Quaternion.identity;
            return;
        }

        // Tourner l'icône selon la rotation Y de l'objet
        float rotation = -transform.eulerAngles.y;
        iconRectTransform.localRotation = Quaternion.Euler(0f, 0f, rotation);
    }

    private void UpdateVisibility()
    {
        if (iconImage == null)
            return;

        bool shouldShow = alwaysVisible || IsVisible;

        // Vérifier la portée de visibilité
        if (!alwaysVisible && Minimap.Instance != null)
        {
            PlayerController player = FindFirstObjectByType<PlayerController>();
            if (player != null)
            {
                float distance = Vector3.Distance(transform.position, player.transform.position);
                shouldShow = shouldShow && distance <= visibilityRange;
            }
        }

        iconImage.enabled = shouldShow;
    }

    /// <summary>
    /// Définir le sprite de l'icône à l'exécution
    /// </summary>
    public void SetSprite(Sprite sprite)
    {
        iconSprite = sprite;
        if (iconImage != null)
        {
            iconImage.sprite = sprite;
        }
    }

    /// <summary>
    /// Définir la couleur de l'icône à l'exécution
    /// </summary>
    public void SetColor(Color color)
    {
        iconColor = color;
        if (iconImage != null)
        {
            iconImage.color = color;
        }
    }

    /// <summary>
    /// Définir la taille de l'icône à l'exécution
    /// </summary>
    public void SetSize(Vector2 size)
    {
        iconSize = size;
        if (iconRectTransform != null)
        {
            iconRectTransform.sizeDelta = size;
        }
    }

    /// <summary>
    /// Définir la visibilité de l'icône
    /// </summary>
    public void SetVisible(bool visible)
    {
        alwaysVisible = visible;
        if (iconImage != null)
        {
            iconImage.enabled = visible;
        }
    }

    private void OnDestroy()
    {
        if (iconRectTransform != null)
        {
            Destroy(iconRectTransform.gameObject);
        }
    }

    private void OnDisable()
    {
        if (iconImage != null)
        {
            iconImage.enabled = false;
        }
    }

    private void OnEnable()
    {
        if (iconImage != null && isInitialized)
        {
            iconImage.enabled = true;
        }
    }
}
