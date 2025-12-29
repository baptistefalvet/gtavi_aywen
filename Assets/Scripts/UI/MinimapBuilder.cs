using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Composant d'aide pour configurer facilement une interface minimap complète.
/// Attachez à un GameObject vide et appelez SetupMinimap() depuis l'inspecteur ou le code.
/// </summary>
[ExecuteInEditMode]
public class MinimapBuilder : MonoBehaviour
{
    [Header("Configuration de la Minimap")]
    [SerializeField]
    private Vector2 minimapSize = new Vector2(200f, 200f);
    [SerializeField]
    private Vector2 minimapPosition = new Vector2(-20f, -20f);
    [SerializeField]
    private MinimapShape minimapShape = MinimapShape.Circle;
    [SerializeField]
    private Color borderColor = Color.white;
    [SerializeField]
    private float borderWidth = 3f;

    [Header("Icône du Joueur")]
    [SerializeField]
    private Sprite playerIconSprite;
    [SerializeField]
    private Color playerIconColor = Color.white;
    [SerializeField]
    private Vector2 playerIconSize = new Vector2(24f, 24f);

    [Header("Paramètres de Caméra")]
    [SerializeField]
    private float defaultZoom = 50f;
    [SerializeField]
    private float cameraHeight = 100f;
    [SerializeField]
    private LayerMask minimapLayers = ~0;

    public enum MinimapShape
    {
        Circle,
        Square,
        Rounded
    }

    [ContextMenu("Setup Minimap")]
    public void SetupMinimap()
    {
        // Créer le Canvas si nécessaire
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("MinimapCanvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
        }

        // Créer le conteneur de la minimap
        GameObject minimapContainer = new GameObject("MinimapContainer");
        minimapContainer.transform.SetParent(canvas.transform, false);

        RectTransform containerRect = minimapContainer.AddComponent<RectTransform>();
        containerRect.anchorMin = new Vector2(1, 0);
        containerRect.anchorMax = new Vector2(1, 0);
        containerRect.pivot = new Vector2(1, 0);
        containerRect.anchoredPosition = minimapPosition;
        containerRect.sizeDelta = minimapSize;

        // Ajouter l'arrière-plan/bordure
        Image borderImage = minimapContainer.AddComponent<Image>();
        borderImage.color = borderColor;
        borderImage.raycastTarget = false;

        // Créer le masque
        GameObject maskObj = new GameObject("MinimapMask");
        maskObj.transform.SetParent(minimapContainer.transform, false);

        RectTransform maskRect = maskObj.AddComponent<RectTransform>();
        maskRect.anchorMin = Vector2.zero;
        maskRect.anchorMax = Vector2.one;
        maskRect.offsetMin = new Vector2(borderWidth, borderWidth);
        maskRect.offsetMax = new Vector2(-borderWidth, -borderWidth);

        Image maskImage = maskObj.AddComponent<Image>();
        maskImage.color = Color.white;
        maskImage.raycastTarget = false;

        Mask mask = maskObj.AddComponent<Mask>();
        mask.showMaskGraphic = false;

        // Appliquer la forme du masque
        if (minimapShape == MinimapShape.Circle)
        {
            maskImage.sprite = CreateCircleSprite();
            borderImage.sprite = CreateCircleSprite();
        }
        else if (minimapShape == MinimapShape.Rounded)
        {
            // Utiliser le sprite par défaut avec coins arrondis si disponible
        }

        // Créer l'affichage de la minimap
        GameObject displayObj = new GameObject("MinimapDisplay");
        displayObj.transform.SetParent(maskObj.transform, false);

        RectTransform displayRect = displayObj.AddComponent<RectTransform>();
        displayRect.anchorMin = Vector2.zero;
        displayRect.anchorMax = Vector2.one;
        displayRect.offsetMin = Vector2.zero;
        displayRect.offsetMax = Vector2.zero;

        RawImage minimapImage = displayObj.AddComponent<RawImage>();
        minimapImage.raycastTarget = false;

        // Créer l'icône du joueur
        GameObject playerIconObj = new GameObject("PlayerIcon");
        playerIconObj.transform.SetParent(maskObj.transform, false);

        RectTransform playerIconRect = playerIconObj.AddComponent<RectTransform>();
        playerIconRect.anchoredPosition = Vector2.zero;
        playerIconRect.sizeDelta = playerIconSize;

        Image playerIcon = playerIconObj.AddComponent<Image>();
        playerIcon.color = playerIconColor;
        playerIcon.raycastTarget = false;

        if (playerIconSprite != null)
        {
            playerIcon.sprite = playerIconSprite;
        }
        else
        {
            playerIcon.sprite = CreateCircleSprite();
        }

        // Ajouter le composant Minimap
        Minimap minimap = minimapContainer.AddComponent<Minimap>();

        // Configurer via réflexion ou champs sérialisés
        SetPrivateField(minimap, "minimapImage", minimapImage);
        SetPrivateField(minimap, "playerIcon", playerIconRect);
        SetPrivateField(minimap, "minimapMask", maskRect);
        SetPrivateField(minimap, "zoomLevel", defaultZoom);
        SetPrivateField(minimap, "cameraHeight", cameraHeight);
        SetPrivateField(minimap, "minimapLayers", minimapLayers);

        // Ajouter le système de points de passage
        minimapContainer.AddComponent<MinimapWaypoints>();

        Debug.Log("Configuration de la minimap terminée ! Assignez une cible joueur au composant Minimap.");
    }

    private void SetPrivateField(object obj, string fieldName, object value)
    {
        var field = obj.GetType().GetField(fieldName, 
            System.Reflection.BindingFlags.NonPublic | 
            System.Reflection.BindingFlags.Instance);
        
        if (field != null)
        {
            field.SetValue(obj, value);
        }
    }

    private Sprite CreateCircleSprite()
    {
        int size = 128;
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

#if UNITY_EDITOR
    [ContextMenu("Créer un Layer Minimap")]
    public void CreateMinimapLayer()
    {
        Debug.Log("Pour créer un layer Minimap :\n" +
                  "1. Allez dans Edit > Project Settings > Tags and Layers\n" +
                  "2. Ajoutez un nouveau layer appelé 'Minimap'\n" +
                  "3. Assignez le terrain et les bâtiments à ce layer\n" +
                  "4. Définissez le culling mask de Minimap sur ce layer");
    }
#endif
}
