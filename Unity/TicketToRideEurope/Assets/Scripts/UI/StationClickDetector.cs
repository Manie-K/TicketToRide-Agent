using UnityEngine;

public class StationClickDetector : MonoBehaviour
{
    private bool isStationPlaced = false;
    private SpriteRenderer spriteRenderer;

    [Header("Tablica grafik dworców dla graczy (0 = Gracz 1, 1 = Gracz 2 itd.)")]
    public Sprite[] playerStationSprites;

    void Start()
    {
        // Automatycznie pobieramy SpriteRenderer z tego obiektu miasteczka
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void OnMouseDown()
    {
        if (isStationPlaced) return;

        int currentActivePlayerNumber = TurnUIController.currentActivePlayer;

        PlaceStationForPlayer(currentActivePlayerNumber);
    }

    public void PlaceStationForPlayer(int playerNumber)
    {
        if (isStationPlaced) return;

        int arrayIndex = playerNumber - 1; // Indeksy w tablicach odliczamy od 0

        if (playerStationSprites == null || playerStationSprites.Length <= arrayIndex || playerStationSprites[arrayIndex] == null)
        {
            Debug.LogError($"Brak przypisanej grafiki dworca dla gracza numer {playerNumber}!");
            return;
        }

        isStationPlaced = true;

        if (spriteRenderer != null)
        {
            // Podmieniamy grafikę na kolor właściwy dla tego gracza!
            spriteRenderer.sprite = playerStationSprites[arrayIndex];
            spriteRenderer.color = Color.white; // Resetujemy kolor do czystej bieli
        }

        Debug.Log($"[SUKCES] Gracz {playerNumber} postawił swój dworzec w mieście: {gameObject.name}!");
    }
}