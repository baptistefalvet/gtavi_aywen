using System;
using UnityEngine;
using UnityEngine.Rendering;

public class TimeCycleController : MonoBehaviour
{
    [Header("Componants")]
    [SerializeField]
    Light MainLight;
    [SerializeField]
    Volume RainVolume;
    [SerializeField]
    Volume NightVolume;
    [SerializeField]
    ParticleSystem Rain1;
    [SerializeField]
    ParticleSystem Rain2;
    [SerializeField]
    Transform PlayerTarget;

    [Header("Day/Night")]
    [SerializeField]
    float SunYRotation;
    [SerializeField]
    float TimeSpeed;
    [SerializeField, ColorUsage(false, true)]
    Color SunColor;
    [SerializeField,ColorUsage(false,true)]
    Color MoonColor;
    [SerializeField]
    float SunIntensity;
    [SerializeField]
    float MoonIntensity;
    [SerializeField]
    float SunColorTemperature;
    [SerializeField]
    float MoonColorTemperature;
    [SerializeField]
    float TransitionTime;

    [Header("Rain")]
    [SerializeField,Range(0f,1f)]
    float RainProbability;
    [SerializeField, ColorUsage(false, true)]
    Color RainLightColor;
    [SerializeField]
    float RainLightIntensity;
    [SerializeField]
    float RainTemperature;
    [SerializeField]
    Vector3 RainOffset;

    [Header("Schedule")]
    [SerializeField,Range(0,59)]
    int StartMinute;
    [SerializeField, Range(0, 23)]
    int StartHour;
    [SerializeField]
    int StartDay;

    [HideInInspector]
    public int minutes;
    [HideInInspector]
    public int hours;
    [HideInInspector]
    public int days;

    float tempSeconds;

    bool isNight;

    bool isRaining;

    int seed;

    private void Awake()
    {
        seed = UnityEngine.Random.Range(0,999999);
    }

    void SetUpSchedule()
    {
        minutes = StartMinute;
        hours = StartHour;
        days = StartDay;
    }

    private void OnValidate()
    {
        SetUpSchedule();
        UpdateTimeCycle();
    }

    private void Update()
    {
        UpdateTimeCycle();
    }

    void UpdateTimeCycle()
    {
        UpdateTimeSchedule();
        UpdateLightRotation();
        UpdateLightSettings();
        UpdateVolumeSettings();
        UpdateRain();
    }

    void UpdateTimeSchedule()
    {
        tempSeconds += Time.deltaTime * TimeSpeed;
        if(tempSeconds > 1)
        {
            minutes += 1;
            tempSeconds = 0;
        }

        if (minutes >= 60)
        {
            hours += 1;
            minutes = 0;
        }

        if (hours >= 24)
        {
            days += 1;
            hours = 0;
        }

        isNight = Mathf.Floor(((hours + (minutes / 60.0f) + 6) % 24.0f) / 12.0f) % 2 == 0;
    }

    void UpdateLightRotation()
    {
        float angle = 180.0f * (((hours + (minutes / 60.0f) + 6) % 24.0f) /12.0f);
        angle = angle % 180.0f;
        angle = Mathf.Clamp(angle, 5, 180);
        angle *= Mathf.Deg2Rad;
        float x = Mathf.Cos(angle);
        float y = Mathf.Sin(angle);
        Vector3 dir = new Vector3(x, y, 0);
        dir = Quaternion.Euler(0, SunYRotation, 0) * dir;

        if (Vector3.Dot(MainLight.transform.forward, -dir) > 0.5)
            MainLight.transform.forward = Vector3.Lerp(MainLight.transform.forward, -dir, Time.deltaTime * 5.0f);
        else 
            MainLight.transform.forward = -dir;
    }

    void UpdateLightSettings()
    {
        if (isRaining)
        {
            MainLight.color = Color.Lerp(MainLight.color, RainLightColor, Time.deltaTime * TransitionTime);
            MainLight.intensity = Mathf.Lerp(MainLight.intensity, RainLightIntensity, Time.deltaTime * TransitionTime);
            MainLight.colorTemperature = Mathf.Lerp(MainLight.colorTemperature, RainTemperature, Time.deltaTime * TransitionTime);
        }
        else if (isNight)
        {
            MainLight.color = Color.Lerp(MainLight.color, MoonColor, Time.deltaTime * TransitionTime);
            MainLight.intensity = Mathf.Lerp(MainLight.intensity, MoonIntensity, Time.deltaTime * TransitionTime);
            MainLight.colorTemperature = Mathf.Lerp(MainLight.colorTemperature, MoonColorTemperature, Time.deltaTime * TransitionTime);
        }
        else
        {
            MainLight.color = Color.Lerp(MainLight.color, SunColor, Time.deltaTime * TransitionTime);
            MainLight.intensity = Mathf.Lerp(MainLight.intensity, SunIntensity, Time.deltaTime * TransitionTime);
            MainLight.colorTemperature = Mathf.Lerp(MainLight.colorTemperature, SunColorTemperature, Time.deltaTime * TransitionTime);
        }

        float lensIntensity = 2.0f;
        float adjustedHour = Mathf.Round(hours / 6.0f);
        if (adjustedHour == 1 || adjustedHour == 3)
            lensIntensity = 0.0f;

        MainLight.GetComponent<LensFlareComponentSRP>().intensity = Mathf.Lerp(MainLight.GetComponent<LensFlareComponentSRP>().intensity, lensIntensity, Time.deltaTime);
    }

    void UpdateVolumeSettings()
    {
        if (isNight && !isRaining)
        {
            NightVolume.weight = Mathf.Lerp(NightVolume.weight, 1.0f, Time.deltaTime * TransitionTime);
        }
        else
        {
            NightVolume.weight = Mathf.Lerp(NightVolume.weight, 0.0f, Time.deltaTime * TransitionTime);
        }
    }

    void UpdateRain()
    {
        Rain1.transform.position = PlayerTarget.position + RainOffset;
        Rain2.transform.position = PlayerTarget.position + RainOffset;

        float adjustedHour = Mathf.Round(hours / 3.0f);
        isRaining = (Mathf.PerlinNoise1D((days + seed) * 2.456f + hours * 0.0513f) > RainProbability) && adjustedHour != 2 && adjustedHour != 6;

        if (isRaining)
        {
            Rain1.Play();
            Rain2.Play();
            RainVolume.weight = Mathf.Lerp(RainVolume.weight, 1.0f, Time.deltaTime * TransitionTime);
        }
        else
        {
            Rain1.Stop(false,ParticleSystemStopBehavior.StopEmitting);
            Rain2.Stop(false,ParticleSystemStopBehavior.StopEmitting);
            RainVolume.weight = Mathf.Lerp(RainVolume.weight, 0.0f, Time.deltaTime * TransitionTime);
        }
    }
}
