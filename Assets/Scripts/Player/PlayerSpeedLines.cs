using Effects;
using UnityEngine;
using UnityEngine.Rendering;

public class PlayerSpeedLines : MonoBehaviour
{
    [Header("SpeedLinesValues")]
    [SerializeField]
    float TransitionSpeed;
    [SerializeField]
    float SpeedLinesEnabledEdges;
    [SerializeField]
    float SpeedLinesDisabledEdges;

    Volume PlayerVolume;
    SpeedLines speedLines;

    [HideInInspector]
    public bool EnableSpeedLines;

    private void Awake()
    {
        PlayerVolume = GetComponent<Volume>();
        PlayerVolume.profile.TryGet<SpeedLines>(out speedLines);
    }

    void UpdateSpeedLines()
    {
        if (EnableSpeedLines)
        {
            speedLines.LinesEdges.value = Mathf.Lerp(speedLines.LinesEdges.value, SpeedLinesEnabledEdges, Time.deltaTime * TransitionSpeed * 3.0f);
        }
        else
        {
            speedLines.LinesEdges.value = Mathf.Lerp(speedLines.LinesEdges.value, SpeedLinesDisabledEdges, Time.deltaTime * TransitionSpeed);
        }
    }
    private void Update()
    {
        UpdateSpeedLines();
    }
}
