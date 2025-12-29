using UnityEngine;

/// <summary>
/// Gère la gravité des crimes et le niveau de recherche associé.
/// </summary>
public class CrimeSeverityManager : MonoBehaviour
{
    public enum CrimeType
    {
        Aucun,
        Vol,
        Agression,
        Meurtre,
        Explosion,
        FuitePolice,
        TirArmeFeu,
        VolVoiture,
        Massacre
    }

    /// <summary>
    /// Retourne le niveau de recherche (étoiles) associé à un crime.
    /// </summary>
    public int GetWantedLevelForCrime(CrimeType crime)
    {
        switch (crime)
        {
            case CrimeType.Vol:
                return 1;
            case CrimeType.Agression:
                return 1;
            case CrimeType.TirArmeFeu:
                return 2;
            case CrimeType.VolVoiture:
                return 2;
            case CrimeType.FuitePolice:
                return 2;
            case CrimeType.Explosion:
                return 3;
            case CrimeType.Meurtre:
                return 4;
            case CrimeType.Massacre:
                return 5;
            default:
                return 0;
        }
    }

    /// <summary>
    /// Utilitaire pour déclencher un crime et retourner la gravité.
    /// </summary>
    public int CrimeCommis(CrimeType crime)
    {
        int niveau = GetWantedLevelForCrime(crime);
        // Ici, tu peux appeler PoliceSystem.Instance.AddWantedLevel(niveau);
        Debug.Log($"Crime commis: {crime} → Niveau de recherche: {niveau}");
        return niveau;
    }
}
