using UnityEngine;
using TMPro;

public class LocalPlayerUIController : MonoBehaviour
{
    public static LocalPlayerUIController instance;

    [Header("Referencje do Tekstów interfejsu")]
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI carriagesText;
    public TextMeshProUGUI stationsText;

    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    public void UpdateLocalPlayerStats(string newName, int currentCarriages, int currentStations)
    {
        if (nameText != null) nameText.text = newName;
        if (carriagesText != null) carriagesText.text = "Carriages: " + currentCarriages;
        if (stationsText != null) stationsText.text = "Stations: " + currentStations;
    }
}
