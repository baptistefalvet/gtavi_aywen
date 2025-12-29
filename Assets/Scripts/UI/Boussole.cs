using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Boussole affichant les directions cardinales (N, S, E, O) en haut de l'écran.
/// Se met à jour en fonction de la direction de la caméra.
/// </summary>
public class Boussole : MonoBehaviour
{
    [Header("Paramètres")]
    [SerializeField]
    private float largeur = 400f;
    [SerializeField]
    private float hauteur = 30f;
    [SerializeField]
    private Color couleurFond = new Color(0f, 0f, 0f, 0.5f);
    [SerializeField]
    private Color couleurTexte = Color.white;
    [SerializeField]
    private Color couleurNord = Color.red;

    private Transform cameraTransform;
    private RectTransform containerRect;
    private RectTransform markersParent;
    
    // Marqueurs de direction
    private Text[] directionTexts;
    private float[] directionAngles = { 0f, 45f, 90f, 135f, 180f, 225f, 270f, 315f };
    private string[] directionLabels = { "N", "NE", "E", "SE", "S", "SO", "O", "NO" };

    private void Start()
    {
        // Utiliser la caméra principale pour la direction
        if (Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }

        CreateBoussoleUI();
    }

    private void CreateBoussoleUI()
    {
        // Trouver ou créer un Canvas
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("BoussoleCanvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            canvasObj.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasObj.AddComponent<GraphicRaycaster>();
        }

        // === CONTENEUR PRINCIPAL ===
        GameObject container = new GameObject("Boussole");
        container.transform.SetParent(canvas.transform, false);

        containerRect = container.AddComponent<RectTransform>();
        containerRect.anchorMin = new Vector2(0.5f, 1f);
        containerRect.anchorMax = new Vector2(0.5f, 1f);
        containerRect.pivot = new Vector2(0.5f, 1f);
        containerRect.anchoredPosition = new Vector2(0f, -10f);
        containerRect.sizeDelta = new Vector2(largeur, hauteur);

        Image bgImage = container.AddComponent<Image>();
        bgImage.color = couleurFond;
        bgImage.raycastTarget = false;

        // === MASQUE ===
        GameObject maskObj = new GameObject("Mask");
        maskObj.transform.SetParent(container.transform, false);

        RectTransform maskRect = maskObj.AddComponent<RectTransform>();
        maskRect.anchorMin = Vector2.zero;
        maskRect.anchorMax = Vector2.one;
        maskRect.offsetMin = new Vector2(5, 2);
        maskRect.offsetMax = new Vector2(-5, -2);

        Image maskImage = maskObj.AddComponent<Image>();
        maskImage.color = Color.white;
        Mask mask = maskObj.AddComponent<Mask>();
        mask.showMaskGraphic = false;

        // === PARENT DES MARQUEURS ===
        GameObject markersObj = new GameObject("Markers");
        markersObj.transform.SetParent(maskObj.transform, false);

        markersParent = markersObj.AddComponent<RectTransform>();
        markersParent.anchorMin = new Vector2(0.5f, 0.5f);
        markersParent.anchorMax = new Vector2(0.5f, 0.5f);
        markersParent.sizeDelta = new Vector2(largeur * 4f, hauteur);

        // === CRÉER LES MARQUEURS ===
        directionTexts = new Text[directionLabels.Length * 2]; // Dupliquer pour le défilement

        for (int i = 0; i < directionLabels.Length * 2; i++)
        {
            int labelIndex = i % directionLabels.Length;
            
            GameObject textObj = new GameObject($"Dir_{directionLabels[labelIndex]}_{i}");
            textObj.transform.SetParent(markersParent, false);

            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0.5f, 0.5f);
            textRect.anchorMax = new Vector2(0.5f, 0.5f);
            textRect.sizeDelta = new Vector2(40, hauteur);

            Text text = textObj.AddComponent<Text>();
            text.text = directionLabels[labelIndex];
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 16;
            text.alignment = TextAnchor.MiddleCenter;
            text.raycastTarget = false;

            // Nord en rouge, autres en blanc
            if (directionLabels[labelIndex] == "N")
            {
                text.color = couleurNord;
                text.fontStyle = FontStyle.Bold;
            }
            else
            {
                text.color = couleurTexte;
            }

            directionTexts[i] = text;
        }

        // === INDICATEUR CENTRAL ===
        GameObject indicatorObj = new GameObject("CenterIndicator");
        indicatorObj.transform.SetParent(container.transform, false);

        RectTransform indicatorRect = indicatorObj.AddComponent<RectTransform>();
        indicatorRect.anchorMin = new Vector2(0.5f, 1f);
        indicatorRect.anchorMax = new Vector2(0.5f, 1f);
        indicatorRect.pivot = new Vector2(0.5f, 1f);
        indicatorRect.anchoredPosition = Vector2.zero;
        indicatorRect.sizeDelta = new Vector2(2f, hauteur);

        Image indicatorImage = indicatorObj.AddComponent<Image>();
        indicatorImage.color = Color.white;
        indicatorImage.raycastTarget = false;
    }

    private void LateUpdate()
    {
        if (cameraTransform == null || markersParent == null)
            return;

        // Récupérer l'angle de la caméra (0-360)
        float cameraAngle = cameraTransform.eulerAngles.y;

        // Calculer la position de chaque marqueur
        float pixelsPerDegree = largeur / 90f; // 90 degrés visibles

        for (int i = 0; i < directionTexts.Length; i++)
        {
            int labelIndex = i % directionLabels.Length;
            float markerAngle = directionAngles[labelIndex];
            
            // Ajouter 360 pour la deuxième série
            if (i >= directionLabels.Length)
            {
                markerAngle += 360f;
            }

            // Calculer la différence d'angle
            float angleDiff = markerAngle - cameraAngle;
            
            // Normaliser entre -180 et 540 pour gérer le wraparound
            while (angleDiff < -180f) angleDiff += 360f;
            while (angleDiff > 540f) angleDiff -= 360f;

            // Position horizontale
            float xPos = angleDiff * pixelsPerDegree;
            
            directionTexts[i].rectTransform.anchoredPosition = new Vector2(xPos, 0);
        }
    }
}
