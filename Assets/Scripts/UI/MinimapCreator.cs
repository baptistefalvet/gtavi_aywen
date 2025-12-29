using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Configure automatiquement le système de minimap à l'exécution.
/// Ajoutez ceci à n'importe quel GameObject de votre scène (comme le Joueur).
/// </summary>
public class MinimapCreator : MonoBehaviour
{
    [Header("Cible")]
    [SerializeField]
    private Transform target;
    [SerializeField]
    private bool autoFindPlayer = true;

    [Header("Paramètres de la Minimap")]
    [SerializeField]
    private Vector2 minimapSize = new Vector2(200f, 200f);
    [SerializeField]
    private Vector2 minimapPosition = new Vector2(-20f, -20f);
    [SerializeField]
    private float defaultZoom = 50f;
    [SerializeField]
    private float cameraHeight = 100f;

    [Header("Apparence")]
    [SerializeField]
    private Color borderColor = new Color(0.2f, 0.2f, 0.2f, 1f);
    [SerializeField]
    private float borderWidth = 4f;
    [SerializeField]
    private Color playerIconColor = Color.white;
    [SerializeField]
    private Vector2 playerIconSize = new Vector2(20f, 20f);

    [Header("Layers")]
    [SerializeField]
    private LayerMask minimapLayers = ~0; // Tout par défaut

    private void Start()
    {
        // Trouver le joueur si nécessaire
        if (autoFindPlayer && target == null)
        {
            PlayerController player = FindFirstObjectByType<PlayerController>();
            if (player != null)
            {
                target = player.transform;
            }
        }

        // Créer la minimap
        CreateMinimap();
    }

    private void CreateMinimap()
    {
        // === CANVAS ===
        GameObject canvasObj = new GameObject("MinimapCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        canvasObj.AddComponent<GraphicRaycaster>();

        // === CONTENEUR (avec bordure) ===
        GameObject containerObj = new GameObject("MinimapContainer");
        containerObj.transform.SetParent(canvasObj.transform, false);

        RectTransform containerRect = containerObj.AddComponent<RectTransform>();
        containerRect.anchorMin = new Vector2(1, 0);
        containerRect.anchorMax = new Vector2(1, 0);
        containerRect.pivot = new Vector2(1, 0);
        containerRect.anchoredPosition = minimapPosition;
        containerRect.sizeDelta = minimapSize;

        Image borderImage = containerObj.AddComponent<Image>();
        borderImage.color = borderColor;
        borderImage.sprite = CreateCircleSprite(128);
        borderImage.raycastTarget = false;

        // === MASQUE ===
        GameObject maskObj = new GameObject("MinimapMask");
        maskObj.transform.SetParent(containerObj.transform, false);

        RectTransform maskRect = maskObj.AddComponent<RectTransform>();
        maskRect.anchorMin = Vector2.zero;
        maskRect.anchorMax = Vector2.one;
        maskRect.offsetMin = new Vector2(borderWidth, borderWidth);
        maskRect.offsetMax = new Vector2(-borderWidth, -borderWidth);

        Image maskImage = maskObj.AddComponent<Image>();
        maskImage.sprite = CreateCircleSprite(128);
        maskImage.raycastTarget = false;

        Mask mask = maskObj.AddComponent<Mask>();
        mask.showMaskGraphic = false;

        // === AFFICHAGE DE LA MINIMAP ===
        GameObject displayObj = new GameObject("MinimapDisplay");
        displayObj.transform.SetParent(maskObj.transform, false);

        RectTransform displayRect = displayObj.AddComponent<RectTransform>();
        displayRect.anchorMin = Vector2.zero;
        displayRect.anchorMax = Vector2.one;
        displayRect.offsetMin = Vector2.zero;
        displayRect.offsetMax = Vector2.zero;

        RawImage minimapImage = displayObj.AddComponent<RawImage>();
        minimapImage.raycastTarget = false;

        // === ICÔNE DU JOUEUR ===
        GameObject playerIconObj = new GameObject("PlayerIcon");
        playerIconObj.transform.SetParent(maskObj.transform, false);

        RectTransform playerIconRect = playerIconObj.AddComponent<RectTransform>();
        playerIconRect.anchoredPosition = Vector2.zero;
        playerIconRect.sizeDelta = playerIconSize;

        Image playerIcon = playerIconObj.AddComponent<Image>();
        playerIcon.sprite = CreateCircleSprite(32);
        playerIcon.color = playerIconColor;
        playerIcon.raycastTarget = false;

        // === CAMÉRA MINIMAP ===
        GameObject camObj = new GameObject("MinimapCamera");
        Camera minimapCam = camObj.AddComponent<Camera>();
        minimapCam.orthographic = true;
        minimapCam.orthographicSize = defaultZoom;
        minimapCam.cullingMask = minimapLayers;
        minimapCam.clearFlags = CameraClearFlags.SolidColor;
        minimapCam.backgroundColor = new Color(0.15f, 0.18f, 0.22f, 1f);
        minimapCam.depth = -10;

        // Créer la render texture
        RenderTexture renderTexture = new RenderTexture(512, 512, 16);
        renderTexture.antiAliasing = 2;
        minimapCam.targetTexture = renderTexture;
        minimapImage.texture = renderTexture;

        // Positionner la caméra au-dessus de la cible
        if (target != null)
        {
            camObj.transform.position = target.position + Vector3.up * cameraHeight;
        }
        camObj.transform.rotation = Quaternion.Euler(90f, 0f, 0f);

        // === AJOUTER LA MINIMAP ===
        Minimap minimap = containerObj.AddComponent<Minimap>();

        // Utiliser la réflexion pour définir les champs privés
        SetField(minimap, "target", target);
        SetField(minimap, "minimapCamera", minimapCam);
        SetField(minimap, "minimapImage", minimapImage);
        SetField(minimap, "playerIcon", playerIconRect);
        SetField(minimap, "minimapMask", maskRect);
        SetField(minimap, "zoomLevel", defaultZoom);
        SetField(minimap, "cameraHeight", cameraHeight);
        SetField(minimap, "minimapLayers", minimapLayers);

        // === AJOUTER LES POINTS DE PASSAGE ===
        containerObj.AddComponent<MinimapWaypoints>();

        Debug.Log("Minimap créée avec succès !");
    }

    private void SetField(object obj, string fieldName, object value)
    {
        var field = obj.GetType().GetField(fieldName,
            System.Reflection.BindingFlags.NonPublic |
            System.Reflection.BindingFlags.Instance);

        if (field != null)
        {
            field.SetValue(obj, value);
        }
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
}
