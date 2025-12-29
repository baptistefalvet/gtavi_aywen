using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Système de police basique : gère le niveau de recherche, l'apparition des policiers et la poursuite du joueur.
/// </summary>
public class PoliceSystem : MonoBehaviour
{
    [Header("Paramètres de recherche")]
    [Range(0,5)]
    public int wantedLevel = 0;
    public int maxWantedLevel = 5;
    public float timeToLoseLevel = 20f;
    private float loseTimer;

    [Header("Police")]
    public GameObject policePrefab;
    public int[] policeCountPerLevel = {0, 1, 2, 3, 5, 8};
    public float spawnRadius = 40f;
    public float respawnDelay = 10f;
    private float respawnTimer;
    private List<GameObject> activePolice = new List<GameObject>();

    [Header("Référence Joueur")]
    public Transform player;

    void Start()
    {
        if (player == null)
            player = FindFirstObjectByType<PlayerController>()?.transform;
        loseTimer = timeToLoseLevel;
    }

    void Update()
    {
        if (wantedLevel > 0)
        {
            loseTimer -= Time.deltaTime;
            if (loseTimer <= 0)
            {
                DecreaseWantedLevel();
            }
            HandlePolice();
        }
        else
        {
            loseTimer = timeToLoseLevel;
            RemoveAllPolice();
        }
    }

    // NOTE : Appeler cette fonction pour augmenter le niveau de recherche (ex : après un crime)
    public void AddWantedLevel(int amount = 1)
    {
        wantedLevel = Mathf.Clamp(wantedLevel + amount, 0, maxWantedLevel);
        loseTimer = timeToLoseLevel;
    }

    // NOTE : Appeler cette fonction pour baisser le niveau de recherche (ex : si le joueur se cache)
    public void DecreaseWantedLevel(int amount = 1)
    {
        wantedLevel = Mathf.Clamp(wantedLevel - amount, 0, maxWantedLevel);
        loseTimer = timeToLoseLevel;
    }

    void HandlePolice()
    {
        int target = policeCountPerLevel[Mathf.Clamp(wantedLevel, 0, policeCountPerLevel.Length-1)];
        // Supprimer les policiers en trop
        while (activePolice.Count > target)
        {
            GameObject p = activePolice[activePolice.Count-1];
            activePolice.RemoveAt(activePolice.Count-1);
            if (p) Destroy(p);
        }
        // Ajouter des policiers si besoin
        respawnTimer -= Time.deltaTime;
        while (activePolice.Count < target && respawnTimer <= 0)
        {
            Vector3 pos = player.position + Random.onUnitSphere * spawnRadius;
            pos.y = player.position.y;
            GameObject police = Instantiate(policePrefab, pos, Quaternion.identity);
            activePolice.Add(police);
            respawnTimer = respawnDelay;
        }
    }

    void RemoveAllPolice()
    {
        foreach (var p in activePolice)
            if (p) Destroy(p);
        activePolice.Clear();
    }

    // NOTE: À utiliser pour signaler qu'un crime a été commis.
    // NOTE: Utiliser avec CrimeSeverityManager pour déterminer la gravité
    public void CrimeCommitted(int gravité = 1)
    {
        AddWantedLevel(gravité);
    }
}
